using MiniProject.Language.Structure;
using Type = MiniProject.Language.Structure.Type;

namespace MiniProject.Language.Parsing;

// Each parselet consumes its own tokens, starting with its opening one.
// The driver only peeks to decide which parselet to dispatch to.
public interface IPrefixExpressionParselet
{
    IExpr Parse(Parser parser);
}

public interface IInfixExpressionParselet
{
    IExpr Parse(Parser parser, IExpr left);
    int Bp { get; }
}

public interface IStmtParselet
{
    IStmt Parse(Parser parser);
}

public interface ITypeParselet
{
    Type Parse(Parser parser);
}
