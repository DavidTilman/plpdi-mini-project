using MiniProject.Language.Lexing;
using MiniProject.Language.Structure;
using static MiniProject.Language.Lexing.TokenType;
using Type = MiniProject.Language.Structure.Type;

namespace MiniProject.Language.Parsing;

[Serializable]
public class ParseException(string msg) : Exception(msg) { }

public class Parser
{
    readonly List<Token> tokens;
    int pos = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        RegisterParselets();
    }

    // --- token cursor ---
    public Token Advance() => tokens[pos++];
    public Token Peek() => tokens[pos];
    public bool Check(TokenType t) => Peek().Type == t;
    public Token Expect(TokenType t)
        => Check(t) ? Advance() : throw new ParseException($"Expected {t} got {Peek().Type}.");

    private int GetPrecedence()
        => infixParselets.TryGetValue(Peek().Type, out IInfixExpressionParselet? p) ? p.Bp : 0;

    public IExpr ParseExpression(int prec)
    {
        IPrefixExpressionParselet? pre = prefixParselets.GetValueOrDefault(Peek().Type) 
            ?? throw new ParseException($"No prefix parselet for {Peek().Type}.");
        IExpr left = pre.Parse(this);

        while (prec < GetPrecedence())
            left = infixParselets[Peek().Type].Parse(this, left);

        return left;
    }

    // --- statements ---
    public IStmt ParseStatement()
    {
        IStmtParselet? sp = stmtParselets.GetValueOrDefault(Peek().Type)
            ?? throw new ParseException($"No statement parselet for {Peek().Type}.");
        return sp.Parse(this);
    }

    // --- blocks:  { s1; ... ; sn; } ---
    public B_Block ParseBlock()
    {
        Expect(L_BRACE);
        List<IStmt> stmts = [];
        while (!Check(R_BRACE))
        {
            stmts.Add(ParseStatement());
            Expect(SEMICOLON);
        }
        Expect(R_BRACE);
        return new B_Block(stmts);
    }

    // --- types ---
    public Type ParseType()
    {
        ITypeParselet? tp = typeParselets.GetValueOrDefault(Peek().Type) 
            ?? throw new ParseException($"No type parselet for {Peek().Type}.");
        return tp.Parse(this);
    }

    // --- parameter lists:  x1:t1, ... , xn:tn   (the enclosing parens are the caller's job) ---
    public IReadOnlyList<Param> ParseParams()
    {
        List<Param> ps = [];
        while (!Check(R_PAREN))
        {
            Token ident = Expect(IDENT);
            Expect(COLON);
            ps.Add(new Param(ident.Lexeme, ParseType()));
            if (!Check(COMMA)) break;
            Advance();
        }
        return ps;
    }

    // --- declarations:  Func F (params) : t = b ---
    private D_Func ParseFunc()
    {
        Expect(FUNC);
        Token name = Expect(IDENT);
        Expect(L_PAREN);
        IReadOnlyList<Param> ps = ParseParams();
        Expect(R_PAREN);
        Expect(COLON);
        Type ret = ParseType();
        Expect(EQUALS);
        return new D_Func(name.Lexeme, ps, ret, ParseBlock());
    }

    // --- program:  c1; ... ; cn;   where c ::= d | s ---
    public P_Prog ParseProgram()
    {
        List<Com> coms = [];
        while (!Check(EOF))
        {
            Com c = Check(FUNC) ? new C_Decl(ParseFunc()) : new C_Stmt(ParseStatement());
            Expect(SEMICOLON);
            coms.Add(c);
        }
        return new P_Prog(coms);
    }

    // --- parselet registry ---
    readonly Dictionary<TokenType, IPrefixExpressionParselet> prefixParselets = [];
    readonly Dictionary<TokenType, IInfixExpressionParselet> infixParselets = [];
    readonly Dictionary<TokenType, IStmtParselet> stmtParselets = [];
    readonly Dictionary<TokenType, ITypeParselet> typeParselets = [];

    public void Register(TokenType t, IPrefixExpressionParselet p) => prefixParselets[t] = p;
    public void Register(TokenType t, IInfixExpressionParselet p) => infixParselets[t] = p;
    public void Register(TokenType t, IStmtParselet p) => stmtParselets[t] = p;
    public void Register(TokenType t, ITypeParselet p) => typeParselets[t] = p;

    private void RegisterParselets()
    {
        // prefix expressions
        E_LitParselet lit = new();
        Register(INT_LIT, lit); Register(REAL_LIT, lit); Register(TRUE_LIT, lit); Register(FALSE_LIT, lit);
        Register(IDENT, new E_VarParselet());
        Register(SUB, new E_UnaryNegateParselet());
        Register(L_PAREN, new E_GroupParselet());
        Register(LAMBDA, new E_LambdaParselet());

        // infix expressions (this block is the precedence table)
        Register(OR,  new BinaryParselet(2, new OP_OR()));
        Register(AND, new BinaryParselet(3, new OP_AND()));
        Register(EQ,  new BinaryParselet(4, new OP_EQ()));
        Register(NEQ, new BinaryParselet(4, new OP_NEQ()));
        Register(LT,  new BinaryParselet(5, new OP_LT()));
        Register(LEQ, new BinaryParselet(5, new OP_LEQ()));
        Register(GT,  new BinaryParselet(5, new OP_GT()));
        Register(GEQ, new BinaryParselet(5, new OP_GEQ()));
        Register(PLUS, new BinaryParselet(6, new OP_Plus()));
        Register(SUB,  new BinaryParselet(6, new OP_Sub()));   // SUB is also a prefix (negate)
        Register(MULT, new BinaryParselet(7, new OP_Mult()));
        Register(DIV,  new BinaryParselet(7, new OP_Div()));
        Register(MOD,  new BinaryParselet(7, new OP_Mod()));
        Register(QUESTION, new E_CondParselet());
        Register(L_PAREN, new E_ApplParselet());               // L_PAREN is also a prefix (group)

        // statements
        Register(VAR,   new S_VarParselet());
        Register(WHILE, new S_WhileParselet());
        Register(IF,    new S_CondParselet());
        Register(RET,   new S_ReturnParselet());
        Register(PRINT, new S_PrintParselet());
        Register(IDENT, new S_AssignParselet());               // IDENT leads an assignment statement

        // types
        BaseTypeParselet baseType = new();
        Register(NAT_TYPE, baseType); Register(INT_TYPE, baseType);
        Register(REAL_TYPE, baseType); Register(BOOL_TYPE, baseType);
        Register(L_PAREN, new FuncTypeParselet());
    }
}
