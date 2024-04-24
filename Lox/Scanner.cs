using System.Collections.Generic;
using UcciLox.Tokens;

namespace UcciLox;

internal class Scanner
{
    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private readonly Dictionary<string, TokenType> keywords = [];

    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    public Scanner(string source)
    {
        _source = source;

        keywords.Add("and", TokenType.AND);
        keywords.Add("else", TokenType.ELSE);
        keywords.Add("false", TokenType.FALSE);
        keywords.Add("for", TokenType.FOR);
        keywords.Add("function", TokenType.FUN);
        keywords.Add("if", TokenType.IF);
        keywords.Add("nil", TokenType.NIL);
        keywords.Add("or", TokenType.OR);
        keywords.Add("return", TokenType.RETURN);
        keywords.Add("this", TokenType.THIS);
        keywords.Add("true", TokenType.TRUE);
        keywords.Add("var", TokenType.VAR);
        keywords.Add("while", TokenType.WHILE);
    }

    internal List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, string.Empty, null, _line));
        return _tokens;
    }

    internal void ScanToken()
    {
        char nextCharacter = Advance();
        switch (nextCharacter)
        {
            case '(':
                AddToken(TokenType.LEFT_PAREN);
                break;

            case ')':
                AddToken(TokenType.RIGHT_PAREN);
                break;

            case '{':
                AddToken(TokenType.LEFT_BRACE);
                break;

            case '}':
                AddToken(TokenType.RIGHT_BRACE);
                break;

            case ',':
                AddToken(TokenType.COMMA);
                break;

            case '.':
                AddToken(TokenType.DOT);
                break;

            case '-':
                AddToken(TokenType.MINUS);
                break;

            case '+':
                AddToken(TokenType.PLUS);
                break;

            case ';':
                AddToken(TokenType.SEMICOLON);
                break;

            case '*':
                AddToken(TokenType.STAR);
                break;

            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;

            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;

            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;

            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;

            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(TokenType.SLASH);
                }
                break;

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;

            case '"':
                HandleString();
                break;

            default:
                if (IsDigit(nextCharacter))
                {
                    HandleNumber();
                }
                else if (IsAlpha(nextCharacter))
                {
                    Identifier();
                }
                else
                {
                    Program.Error(_line, $"Unrecognized character: {nextCharacter}. ");
                }
                break;
        }
    }

    private void HandleNumber()
    {
        while (IsDigit(Peek()))
            Advance();

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the "."
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        int length = _current - _start;
        AddToken(TokenType.NUMBER, double.Parse(_source.Substring(_start, length)));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        int length = _current - _start;
        string text = _source.Substring(_start, length);
        if (!keywords.TryGetValue(text, out TokenType tokenType))
            tokenType = TokenType.IDENTIFIER;
        AddToken(tokenType);
    }

    private static bool IsAlpha(char c) => c >= 'a' && c <= 'z' ||
               c >= 'A' && c <= 'Z' ||
                c == '_';

    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length)
            return '\0';
        return _source[_current + 1];
    }

    private static bool IsDigit(char c) => c >= '0' && c <= '9';

    private void HandleString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
                _line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Program.Error(_line, "Unterminated string.");
            return;
        }

        // The closing ".
        Advance();

        // Trim the surrounding quotes.
        int length = _current - 1 - _start + 1;
        string value = _source.Substring(_start, length);
        AddToken(TokenType.STRING, value);
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private bool IsAtEnd() => _current >= _source.Length;

    private void AddToken(TokenType type) => AddToken(type, null);

    private void AddToken(TokenType type, object? literal)
    {
        int length = _current - _start;
        string tokenText = _source.Substring(_start, length);
        _tokens.Add(new Token(type, tokenText, literal, _line));
    }

    private char Advance() => _source[_current++];
}