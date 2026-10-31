using MiniProject.Language.Lexing;
using MiniProject.Language.Structure;
using static MiniProject.Language.Lexing.TokenType;
using Type = MiniProject.Language.Structure.Type;

namespace MiniProject.Language.Parsing;

// Nat | Int | Real | Bool  — one parselet registered for all four base-type tokens.
public class BaseTypeParselet : ITypeParselet
{
    public Type Parse(Parser parser)
    {
        Token t = parser.Advance();
        return t.Type switch
        {
            NAT_TYPE => new T_Nat(),
            INT_TYPE => new T_Int(),
            REAL_TYPE => new T_Real(),
            BOOL_TYPE => new T_Bool(),
            _ => throw new ParseException($"Attempted parse of {t.Type} to a base type.")
        };
    }
}

// (t1, ... , tn) -> t
public class FuncTypeParselet : ITypeParselet
{
    public Type Parse(Parser parser)
    {
        parser.Expect(L_PAREN);
        List<Type> paramTypes = [];
        while (!parser.Check(R_PAREN))
        {
            paramTypes.Add(parser.ParseType());
            if (!parser.Check(COMMA)) break;
            parser.Advance();
        }
        parser.Expect(R_PAREN);
        parser.Expect(ARROW);
        return new T_Func(paramTypes, parser.ParseType());
    }
}
