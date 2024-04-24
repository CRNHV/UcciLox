using System;
using System.Collections.Generic;
using UcciLox.Expressions;
using UcciLox.Statements;
using UcciLox.Tokens;

namespace UcciLox;

internal class Parser(List<Token> tokens)
{
    private class ParseError : Exception
    { }

    private readonly List<Token> tokens = tokens;
    private int current = 0;

    public List<Statement> Parse()
    {
        List<Statement> statements = [];
        while (!AtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Expression Expression() => Assignment();

    private Statement? Declaration()
    {
        try
        {
            if (Match(TokenType.VAR)) return VarDeclaration();
            if (Match(TokenType.FUN)) return Function("function");
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Function Function(string kind)
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");
        List<Token> parameters = [];
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(
                    Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
        List<Statement> body = Block();
        return new Function(name, parameters, body);
    }

    private Statement Statement()
    {
        if (Match(TokenType.FOR)) return ForStatement();
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.WHILE)) return WhileStatement();
        if (Match(TokenType.LEFT_BRACE)) return new Block(Block());
        if (Match(TokenType.BREAK)) return BreakStatement();
        if (Match(TokenType.RETURN)) return ReturnStatement();
        return ExpressionStatement();
    }

    private Return ReturnStatement()
    {
        Token keyword = Previous();
        Expression? value = null;
        if (!Check(TokenType.SEMICOLON))
        {
            value = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new Return(keyword, value);
    }

    private Statement ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

        Statement? initializer;
        if (Match(TokenType.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(TokenType.VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expression? condition = null;
        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        Expression? increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
        {
            increment = Expression();
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

        Statement body = Statement();

        if (increment != null)
        {
            body = new Block(
                [
                    body,
                    new ExpressionStatement(increment),
                ]);
        }

        condition ??= new Literal(true);
        body = new While(condition, body);

        if (initializer != null)
        {
            body = new Block([initializer, body]);
        }

        return body;
    }

    private If IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expression condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        Statement thenBranch = Statement();
        Statement? elseBranch = null;
        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }

        return new If(condition, thenBranch, elseBranch);
    }

    private ExpressionStatement ExpressionStatement()
    {
        Expression expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new ExpressionStatement(expr);
    }

    private List<Statement> Block()
    {
        List<Statement> statements = [];

        while (!Check(TokenType.RIGHT_BRACE) && !AtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Expression Assignment()
    {
        Expression expr = Or();

        if (Match(TokenType.EQUAL))
        {
            Token equals = Previous();
            Expression value = Assignment();

            Variable variable = (Variable)expr;
            if (variable != null)
            {
                Token name = variable.Name;
                return new Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expression Or()
    {
        Expression expr = And();

        while (Match(TokenType.OR))
        {
            Token op = Previous();
            Expression right = And();
            expr = new Logical(expr, op, right);
        }

        return expr;
    }

    private Expression And()
    {
        Expression expr = Equality();

        while (Match(TokenType.AND))
        {
            Token op = Previous();
            Expression right = Equality();
            expr = new Logical(expr, op, right);
        }

        return expr;
    }

    private Var VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expression? initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }

        if (initializer is null)
        {
            throw new Exception("Variable declaration's expression is null.");
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new Var(name, initializer);
    }

    private While WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expression condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        Statement body = Statement();

        return new While(condition, body);
    }

    private Break BreakStatement()
    {
        Consume(TokenType.SEMICOLON, "Expect ';' after break statement");

        return new Break();
    }

    private Expression Equality()
    {
        Expression expr = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token op = Previous();
            Expression right = Comparison();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expression Comparison()
    {
        Expression expr = Addition();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token op = Previous();
            Expression right = Addition();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expression Addition()
    {
        Expression expr = Multiplication();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token op = Previous();
            Expression right = Multiplication();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expression Multiplication()
    {
        Expression expr = Unary();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            Token op = Previous();
            Expression right = Unary();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expression Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            Token op = Previous();
            Expression right = Unary();
            return new Unary(op, right);
        }

        return Call();
    }

    private Expression Call()
    {
        Expression expr = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Call FinishCall(Expression callee)
    {
        List<Expression> arguments = [];
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }

        Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new Call(callee, paren, arguments);
    }

    private Expression Primary()
    {
        if (Match(TokenType.FALSE)) return new Literal(false);
        if (Match(TokenType.TRUE)) return new Literal(true);
        if (Match(TokenType.NIL)) return new Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            var previousToken = Previous() ?? throw new Exception("Previous token is null.");
            if (previousToken.Literal == null)
            {
                throw new Exception($"Previous token's literal is null.");
            }

            return new Literal(previousToken.Literal);
        }

        if (Match(TokenType.LEFT_PAREN))
        {
            Expression expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Grouping(expr);
        }

        if (Match(TokenType.IDENTIFIER))
        {
            return new Variable(Previous());
        }

        throw Error(Peek(), "Expect expression.");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType tokenType)
    {
        if (AtEnd()) return false;
        return Peek().Type == tokenType;
    }

    private Token Advance()
    {
        if (!AtEnd()) current++;
        return Previous();
    }

    private bool AtEnd() => Peek().Type == TokenType.EOF;

    private Token Peek() => tokens[current];

    private Token Previous() => tokens[current - 1];

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private static ParseError Error(Token token, string message)
    {
        Program.Error(token.Line, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!AtEnd())
        {
            if (Previous().Type == TokenType.SEMICOLON) return;

            switch (Peek().Type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            Advance();
        }
    }
}