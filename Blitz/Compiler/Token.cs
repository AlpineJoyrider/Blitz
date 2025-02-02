namespace Blitz.Compiler;

public enum TokenType
{
    OpenParen, CloseParen,
    OpenBrace, CloseBrace,
    OpenBracket, CloseBracket,
    Colon, ColonColon, Semicolon,
    Comma, Dot, Asterisk, Ampersand,
    Exclamation, Question,
    FSlash, BSlash,
    
    Assign, Equal, NotEqual,
    Plus, PlusEqual, Increment,
    Minus, MinusEqual, Decrement,
    MultiplyEqual, DivideEqual,
    Greater, GreaterEqual, GreaterShift,
    Lesser, LesserEqual, LesserShift,
    
    Import, Fn,
    Let, Var, Comp,
    Return,
    True, False,
    
    I8, I16, I32, I64,
    U8, U16, U32, U64,
    F32, F64, Char,
    Bool, Void,
    
    Identifier,
    IntegerLiteral,
    FloatLiteral,
    StringLiteral,
    CharLiteral,
    
    Error,
    Eof
}

public record Token(TokenType Type, string Lexeme, object? Literal, int Line, int Column)
{
    public override string ToString() => $"[{Line}:{Column}] - [{Type}] -> [{Lexeme}][{Literal}]";
}