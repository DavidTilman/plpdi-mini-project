using MiniProject.Language.TypeChecking;

namespace MiniProject.Language.Structure;

public interface IBlock
{
    public Outcome Execute(Env Env);
    public void TypeCheck(TypingContext context, Type? ret);
}
public sealed record B_Block(IReadOnlyList<IStmt> Statements) : IBlock
{
    public Outcome Execute(Env Env)
    {
        if (Statements.Count == 0) return new O_Normal();
        IStmt cur = Statements[0];
        (Outcome o, Env e) = cur.Execute(Env);
        if (o is O_Ret) return o;
        B_Block newBlock = new([.. Statements.Skip(1)]);
        return newBlock.Execute(e);
    }

    public void TypeCheck(TypingContext context, Type? ret)
    {
        foreach (IStmt s in Statements)
            context = s.TypeCheck(context, ret);
    }
}
