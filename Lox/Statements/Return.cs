using UcciLox.Expressions;
using UcciLox.Tokens;

namespace UcciLox.Statements;

internal class Return(Token keyword, Expression value) : Statement
{
    public Token Keyword { get; set; } = keyword;
    public Expression Value { get; set; } = value;

    internal override R? Accept<R>(IStatementVisitor<R?> visitor) where R : default => visitor.VisitReturnStatement(this);
}