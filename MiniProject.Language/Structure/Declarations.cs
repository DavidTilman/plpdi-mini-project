namespace MiniProject.Language.Structure;

public abstract record Decl;
public sealed record D_Func(string Name, IReadOnlyList<Param> Params, Type ReturnType, B_Block Body) : Decl;