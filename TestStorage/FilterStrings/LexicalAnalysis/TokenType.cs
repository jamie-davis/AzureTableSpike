namespace TestStorage.FilterStrings.LexicalAnalysis
{
    public enum TokenType
    {
        Identifier,
        CompEq,
        CompGt,
        CompGe,
        CompLt,
        CompLe,
        CompNe,
        LogicalAnd,
        LogicalNot,
        LogicalOr,
        StringLiteral,
        IntLiteral,
        LongLiteral,
        DoubleLiteral,
        GuidLiteral,
        DateTimeLiteral,
        OpenParen,
        CloseParen,
        Error,
        Terminator
    }
}