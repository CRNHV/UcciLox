namespace UcciLox.Statements;

internal interface IStatementVisitor<R>
{
    R? VisitBlockStatement(Block statement);

    R? VisitExpressionStatement(ExpressionStatement statement);

    R? VisitIfStatement(If statement);

    R? VisitVarStatement(Var statement);

    R? VisitWhileStatement(While statement);

    R? VisitBreakStatement(Break statement);

    R? VisitFunctionStatement(Function statement);

    R? VisitReturnStatement(Return statement);
}