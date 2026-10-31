using MiniProject.Language.Lexing;
using MiniProject.Language.Structure;
using static MiniProject.Language.Lexing.TokenType;
using Type = MiniProject.Language.Structure.Type;

namespace MiniProject.Language.Parsing;

public class S_VarParselet : IStmtParselet
{
    public IStmt Parse(Parser parser)
    {
        parser.Expect(VAR);
        Token ident = parser.Expect(IDENT);
        parser.Expect(COLON);
        Type type = parser.ParseType();
        parser.Expect(EQUALS);
        if (parser.Check(READ))
        {
            parser.Expect(READ);
            return new S_Read(ident.Lexeme, type);
        }
        return new S_Decl(ident.Lexeme, type, parser.ParseExpression(0));
    }
}

public class S_WhileParselet : IStmtParselet
{
    public IStmt Parse(Parser parser)
    {
        parser.Expect(WHILE);
        parser.Expect(L_PAREN);
        IExpr cond = parser.ParseExpression(0);
        parser.Expect(R_PAREN);
        return new S_While(cond, parser.ParseBlock());
    }
}

public class S_CondParselet : IStmtParselet
{
    public IStmt Parse(Parser parser)
    {
        parser.Expect(IF);
        parser.Expect(L_PAREN);
        IExpr cond = parser.ParseExpression(0);
        parser.Expect(R_PAREN);
        B_Block onTrue = parser.ParseBlock();
        parser.Expect(ELSE);
        B_Block onFalse = parser.ParseBlock();
        return new S_Cond(cond, onTrue, onFalse);
    }
}

public class S_ReturnParselet : IStmtParselet
{
    public IStmt Parse(Parser parser)
    {
        parser.Expect(RET);
        return new S_Return(parser.ParseExpression(0));
    }
}

public class S_PrintParselet : IStmtParselet
{
    public IStmt Parse(Parser parser)
    {
        parser.Expect(PRINT);
        return new S_Print(parser.ParseExpression(0));
    }
}

public class S_AssignParselet : IStmtParselet
{
    public IStmt Parse(Parser parser)
    {
        Token ident = parser.Expect(IDENT);
        parser.Expect(ASSIGN);
        return new S_Assign(ident.Lexeme, parser.ParseExpression(0));
    }
}
