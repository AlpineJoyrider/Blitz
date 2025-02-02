namespace Blitz.Compiler;

public class Tokenizer
{
    private string _src = "";
    private string[]? _lines;
    private int _pos;
    private int _line = 1;
    private int _col = 1;
    
    public List<TokenizerError> Errors { get; } = new();

    public List<Token> Tokenize(string source)
    {
        _src = source;
        _lines = source.Split('\n');
        _pos = 0;
        _line = 1;
        _col = 1;
        Errors.Clear();

        var tokens = new List<Token>();
        while (!IsAtEnd)
        {
            tokens.Add(ScanToken());
        }
        
        tokens.Add(CreateToken(TokenType.Eof, "End of file"));
        return tokens;
    }

    private Token ScanToken()
    {
        SkipWhitespace();

        Token token;
        switch (Current)
        {
            case '(': token = CreateToken(TokenType.OpenParen, "("); break;
            case ')': token = CreateToken(TokenType.CloseParen, ")"); break;
            case '{': token = CreateToken(TokenType.OpenBrace, "{"); break;
            case '}': token = CreateToken(TokenType.CloseBrace, "}"); break;
            case '[': token = CreateToken(TokenType.OpenBracket, "["); break;
            case ']': token = CreateToken(TokenType.CloseBracket, "]"); break;
            case ';': token = CreateToken(TokenType.Semicolon, ";"); break;
            case ',': token = CreateToken(TokenType.Comma, ","); break;
            case '.': token = CreateToken(TokenType.Dot, "."); break;
            case '&': token = CreateToken(TokenType.Ampersand, "&"); break;
            case '?': token = CreateToken(TokenType.Question, "?"); break;
            case ':':
                token = Match(':')
                    ? CreateToken(TokenType.ColonColon, "::", _col - 1)
                    : CreateToken(TokenType.Colon, ":");
                break;
            case '*':
                token = Match('=')
                    ? CreateToken(TokenType.MultiplyEqual, "*=", _col - 1)
                    : CreateToken(TokenType.Asterisk, "*");
                break;
            case '!':
                token = Match('=')
                    ? CreateToken(TokenType.NotEqual, "!=", _col - 1)
                    : CreateToken(TokenType.Exclamation, "!");
                break;
            case '=':
                token = Match('=')
                    ? CreateToken(TokenType.Equal, "==", _col - 1)
                    : CreateToken(TokenType.Assign, "=");
                break;
            case '+':
                if (Match('+')) token = CreateToken(TokenType.Increment, "++", _col - 1);
                else
                    token = Match('=')
                        ? CreateToken(TokenType.PlusEqual, "+=", _col - 1)
                        : CreateToken(TokenType.Plus, "+");
                break;
            case '-':
                if (Match('-')) token = CreateToken(TokenType.Decrement, "--", _col - 1);
                else
                    token = Match('=')
                        ? CreateToken(TokenType.MinusEqual, "-=", _col - 1)
                        : CreateToken(TokenType.Minus, "-");
                break;
            default:
                if (IsLetter(Current)) token = ScanIdentifier();
                else if (IsDigit(Current)) token = ScanNumber();
                else
                {
                    Error("Unexpected token.");
                    token = CreateToken(TokenType.Error, Current.ToString());
                }
                break;
        }
        
        Advance();
        return token;
    }

    private Token ScanIdentifier()
    {
        int start = _pos;
        int colStart = _col;
        while (IsLetterOrDigit(Current)) Advance();

        string value = _src.Substring(start, _pos - start);
        Retreat();
        return Keywords.TryGetValue(value, out var type)
            ? CreateToken(type, value, colStart)
            : CreateToken(TokenType.Identifier, value, colStart);
    }

    private Token ScanNumber()
    {
        int start = _pos;
        int colStart = _col;
        var isFloat = false;
        while (IsDigit(Current)) Advance();

        if (Current is '.' && IsDigit(Peek))
        {
            isFloat = true;
            Advance();
            while (IsDigit(Current)) Advance();
        }

        string value = _src.Substring(start, _pos - start);
        Retreat();
        return isFloat
            ? CreateToken(TokenType.FloatLiteral, value, double.Parse(value), colStart)
            : CreateToken(TokenType.IntegerLiteral, value, long.Parse(value), colStart);
    }

    private void SkipWhitespace()
    {
        while (true)
        {
            switch (Current)
            {
                case ' ':
                case '\t':
                case '\r':
                    Advance();
                    break;
                case '\n':
                    _line++;
                    _col = 0;
                    Advance();
                    break;
                case '!':
                    if (Peek == '!')
                    {
                        while (Current != '\n' && !IsAtEnd) Advance();
                    }
                    else
                    {
                        return;
                    }
                    break;
                default:
                    return;
            }
        }
    }

    #region Helpers

    private bool IsAtEnd => _pos >= _src.Length;
    private char Current => IsAtEnd ? '\0' : _src[_pos];
    private char Peek => _pos + 1 >= _src.Length ? '\0' : _src[_pos + 1];

    private void Advance()
    {
        _pos++;
        _col++;
    }

    private void Retreat()
    {
        _pos--;
        _col--;
    }

    private bool Match(char expected)
    {
        if (_pos + 1 >= _src.Length) return false;
        if (Peek != expected) return false;
        
        Advance();
        return true;
    }

    private Token CreateToken(TokenType type, string lexeme) => CreateToken(type, lexeme, _col);

    private Token CreateToken(TokenType type, string lexeme, int col) => CreateToken(type, lexeme, null, col);

    private Token CreateToken(TokenType type, string lexeme, object? literal, int col) =>
        new(type, lexeme, literal, _line, col);

    private void Error(string message) => Errors.Add(new TokenizerError(_line, _col, message, _lines![_line - 1]));
    
    private static bool IsDigit(char c) => c is >= '0' and <= '9';
    private static bool IsLetter(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    private static bool IsLetterOrDigit(char c) => IsDigit(c) || IsLetter(c);

    #endregion

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "import", TokenType.Import },
        { "fn", TokenType.Fn },
        { "let", TokenType.Let },
        { "var", TokenType.Var },
        { "comp", TokenType.Comp },
        { "return", TokenType.Return },
        { "ret", TokenType.Return },
        { "i8", TokenType.I8 },
        { "i16", TokenType.I16 },
        { "i32", TokenType.I32 },
        { "i64", TokenType.I64 },
        { "u8", TokenType.U8 },
        { "u16", TokenType.U16 },
        { "u32", TokenType.U32 },
        { "u64", TokenType.U64 },
        { "f32", TokenType.F32 },
        { "f64", TokenType.F64 },
        { "char", TokenType.Char },
        { "bool", TokenType.Bool },
        { "void", TokenType.Void },
    };
}

public readonly record struct TokenizerError(int Line, int Column, string Message, string LineContent)
{
    public override string ToString() => $"[{Line}:{Column}] {Message}\n{LineContent}\n{new string(' ', Column - 1)}^";
}