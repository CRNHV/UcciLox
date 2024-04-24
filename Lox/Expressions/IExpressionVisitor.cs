namespace UcciLox.Expressions;

internal interface IExpressionVisitor<R>
{
    R? VisitAssignExpression(Assign expr);

    R? VisitBinaryExpression(Binary expr);

    R? VisitGroupingExpression(Grouping expr);

    R? VisitLiteralExpression(Literal expr);

    R? VisitLogicalExpression(Logical expr);

    R? VisitUnaryExpression(Unary expr);

    R? VisitVariableExpression(Variable expr);

    R? VisitCallExpression(Call expr);
}