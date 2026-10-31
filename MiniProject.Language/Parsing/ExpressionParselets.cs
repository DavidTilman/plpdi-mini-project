using MiniProject.Language.Lexing;
using MiniProject.Language.Structure;
using static MiniProject.Language.Lexing.TokenType;

namespace MiniProject.Language.Parsing;

public class E_LitParselet : IPrefixExpressionParselet
{
    public IExpr Parse(Parser parser)
    {
        Token t = parser.Advance();
        return t.Type switch
        {
            INT_LIT => new E_Lit(ulong.Parse(t.Lexeme)),
            REAL_LIT => new E_Lit(double.Parse(t.Lexeme)),
            TRUE_LIT => new E_Lit(true),
            FALSE_LIT => new E_Lit(false),
            _ => throw new ParseException($"Attempted parse of {t.Type} to E_Lit.")
        };
    }
}

public class E_VarParselet : IPrefixExpressionParselet
{
    public IExpr Parse(Parser parser) => new E_Var(parser.Expect(IDENT).Lexeme);
}

public class E_UnaryNegateParselet : IPrefixExpressionParselet
{
    public IExpr Parse(Parser parser)
    {
        parser.Expect(SUB);
        return new E_UnaryNegate(parser.ParseExpression(8));
    }
}

public class E_GroupParselet : IPrefixExpressionParselet
{
    public IExpr Parse(Parser parser)
    {
        parser.Expect(L_PAREN);
        IExpr inner = parser.ParseExpression(0);
        parser.Expect(R_PAREN);
        return inner;
    }
}

public class E_LambdaParselet : IPrefixExpressionParselet
{
    public IExpr Parse(Parser parser)
    {
        parser.Expect(LAMBDA);
        parser.Expect(L_PAREN);
        IReadOnlyList<Param> _params = parser.ParseParams();
        parser.Expect(R_PAREN);
        parser.Expect(COLON);
        var retType = parser.ParseType();
        parser.Expect(FAT_ARROW);
        return new E_Lambda(_params, retType, parser.ParseExpression(0));
    }
}

public class BinaryParselet(int bp, Operation op, bool rightAssoc = false) : IInfixExpressionParselet
{
    public int Bp => bp;

    public IExpr Parse(Parser parser, IExpr left)
    {
        parser.Advance(); // consume the operator we were dispatched on
        return new E_Op(left, op, parser.ParseExpression(rightAssoc ? bp - 1 : bp));
    }
}

public class E_CondParselet : IInfixExpressionParselet
{
    public int Bp => 1;

    public IExpr Parse(Parser parser, IExpr left)
    {
        parser.Expect(QUESTION);
        IExpr then = parser.ParseExpression(0);
        parser.Expect(COLON);
        IExpr els = parser.ParseExpression(Bp - 1); // right-associative
        return new E_Cond(left, then, els);
    }
}

public class E_ApplParselet : IInfixExpressionParselet
{
    public int Bp => 9;

    public IExpr Parse(Parser parser, IExpr left)
    {
        parser.Expect(L_PAREN);
        List<IExpr> args = [];
        while (!parser.Check(R_PAREN))
        {
            args.Add(parser.ParseExpression(0));
            if (!parser.Check(COMMA)) break;
            parser.Advance();
        }
        parser.Expect(R_PAREN);
        return new E_Appl(left, args);
    }
}
