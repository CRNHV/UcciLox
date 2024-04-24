namespace UcciLox.Tokens;

internal sealed class Token(TokenType type, string lexeme, object? literal, int line)
{
    public TokenType Type { get; init; } = type;
    public string Lexeme { get; init; } = lexeme;
    public object? Literal { get; init; } = literal;
    public int Line { get; init; } = line;
}