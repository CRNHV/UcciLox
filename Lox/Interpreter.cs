using System.Collections.Generic;
using UcciLox.Exceptions;
using UcciLox.Expressions;
using UcciLox.Functions;
using UcciLox.Functions.BuiltIn;
using UcciLox.Statements;
using UcciLox.Tokens;

namespace UcciLox;

internal class Interpreter : IExpressionVisitor<object>, IStatementVisitor<object>
{
    public BlockEnvironment Global = _environment;
    private static BlockEnvironment _environment = new(null);

    public Interpreter()
    {
        Global.Define("clock", new Clock());
        Global.Define("print", new Print());
        Global.Define("GetModuleHandle", new GetModHandle());
        Global.Define("GetProcAddress", new GetProcAddr());
        Global.Define("WinAPIFunc", new WinAPIFunc());
    }

    public void Interpret(List<Statement> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError error)
        {
            Program.RuntimeError(error);
        }
    }

    private object? Execute(Statement statement)
    {
        if (statement == null)
        {
            Program.RuntimeError(new RuntimeError(null, "Statement is null"));
            return null;
        }

        return statement.Accept(this);
    }

    public object ExecuteBlock(List<Statement> statements, BlockEnvironment blockEnvironment)
    {
        BlockEnvironment previous = _environment;
        try
        {
            _environment = blockEnvironment;

            foreach (Statement statement in statements)
            {
                if (statement is Break)
                {
                    return false;
                }

                bool? result = Execute(statement) as bool?;

                if (result != null && !result.Value)
                {
                    return false;
                }
            }
        }
        finally
        {
            _environment = previous;
        }

        return true;
    }

    private object Evaluate(Expression expr) => expr.Accept(this);

    #region Expressions

    public object? VisitAssignExpression(Assign expr)
    {
        object value = Evaluate(expr.value);
        if (!_environment.Contains(expr.Token.Lexeme))
        {
            Program.RuntimeError(new RuntimeError(expr.Token, $"Undefined variable '{expr.Token.Lexeme}'"));
        }
        else
        {
            _environment.Assign(expr.Token, value);
        }

        return null;
    }

    public object? VisitBinaryExpression(Binary expr)
    {
        object left = Evaluate(expr.Left);
        object right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.MINUS:
                return (double)left - (double)right;

            case TokenType.SLASH:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left / (double)right;

            case TokenType.STAR:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left * (double)right;

            case TokenType.PLUS:
                if (left.GetType() == typeof(double) && right.GetType() == typeof(double))
                {
                    return (double)left + (double)right;
                }

                return $"{left}{right}";
            case TokenType.GREATER:
                return (double)left > (double)right;

            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left >= (double)right;

            case TokenType.LESS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left < (double)right;

            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left <= (double)right;

            case TokenType.BANG_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return !IsEqual(left, right);

            case TokenType.EQUAL_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return IsEqual(left, right);
        }

        return null;
    }

    public object? VisitGroupingExpression(Grouping expr) => Evaluate(expr);

    public object? VisitLiteralExpression(Literal expr) => expr.Value;

    public object? VisitLogicalExpression(Logical expr) => throw new System.NotImplementedException();

    public object? VisitUnaryExpression(Unary expr)
    {
        object right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.MINUS:
                CheckNumberOperand(expr.Operator, right);
                return -(double)right;

            case TokenType.BANG:
                CheckNumberOperand(expr.Operator, right);
                return !IsTruthy(right);
        }

        // Unreachable.
        return null;
    }

    public object? VisitVariableExpression(Variable expr) => _environment.Get(expr.Name);

    public object? VisitCallExpression(Call expr)
    {
        object callee = Evaluate(expr.Callee);

        if (callee is not ICallable)
        {
            throw new RuntimeError(expr.Paren, "Not a function.");
        }

        ICallable function = (ICallable)callee;

        if (expr.Arguments.Count != function.Arity && function.GetType() != typeof(WinAPIFunc))
        {
            throw new RuntimeError(expr.Paren, "Expected " + function.Arity + " arguments but got " + expr.Arguments.Count + ".");
        }

        List<object> arguments = [];
        foreach (Expression argument in expr.Arguments)
        {
            var evaluatedArg = Evaluate(argument);
            if (evaluatedArg.GetType() == typeof(string))
            {
                evaluatedArg = ((string)evaluatedArg).Replace("\"", "");
            }

            arguments.Add(evaluatedArg);
        }

        return function.Call(this, arguments);
    }

    #endregion Expressions

    #region Statements

    public object? VisitBlockStatement(Block statement) => ExecuteBlock(statement.statements, new BlockEnvironment(_environment));

    public object? VisitExpressionStatement(ExpressionStatement statement) => Evaluate(statement.expression);

    public object? VisitIfStatement(If statement)
    {
        object? result = null;
        if (IsTruthy(Evaluate(statement.condition)))
        {
            result = Execute(statement.thenBranch);
        }
        else if (statement.elseBranch != null)
        {
            result = Execute(statement.elseBranch);
        }

        return result;
    }

    public object? VisitVarStatement(Var statement)
    {
        if (_environment.Contains(statement.name.Lexeme))
        {
            Program.RuntimeError(new RuntimeError(statement.name, $"Variable {statement.name.Lexeme} has already been declared."));
        }

        if (statement.initializer != null)
        {
            object value = Evaluate(statement.initializer);
            _environment.Define(statement.name.Lexeme, value);
        }
        else
        {
            // Define unassigned variable
            _environment.Define(statement.name.Lexeme);
        }

        return null;
    }

    public object? VisitWhileStatement(While statement)
    {
        while (IsTruthy(Evaluate(statement.condition)))
        {
            bool? result = Execute(statement.body) as bool?;

            if (result != null && !result.Value)
                break;
        }
        return null;
    }

    public object? VisitBreakStatement(Break statement) => null;

    public object? VisitFunctionStatement(Function statement)
    {
        Callable function = new(statement);
        _environment.Define(statement.Name.Lexeme, function);
        return null;
    }

    public object? VisitReturnStatement(Statements.Return statement)
    {
        object? value = null;
        if (statement.Value != null)
            value = Evaluate(statement.Value);

        throw new Exceptions.Return(value);
    }

    #endregion Statements

    #region Helpers

    private static bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj.GetType() == typeof(bool)) return (bool)obj;
        return true;
    }

    private static void CheckNumberOperand(Token op, object operand)
    {
        if (operand.GetType() == typeof(string)) return;
        throw new RuntimeError(op, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token op, object left, object right)
    {
        if (left.GetType() == typeof(double) && right.GetType() == typeof(double))
            return;

        throw new RuntimeError(op, "Operands must be numbers.");
    }

    private static bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    #endregion Helpers
}