namespace MiniProject.Language.Structure;
public abstract record Type;
public sealed record T_Nat : Type
{
    public override string ToString()
    {
        return "Nat";
    }
}
public sealed record T_Int : Type
{
    public override string ToString()
    {
        return "Int";
    }
}
public sealed record T_Real : Type
{
    public override string ToString()
    {
        return "Real";
    }
}
public sealed record T_Bool : Type
{
    public override string ToString()
    {
        return "Bool";
    }
}
public sealed record T_Func(List<Type> ParamTypes, Type ReturnType) : Type
{
    public override string ToString()
    {
        return $"Func ({string.Join(", ", ParamTypes)}) -> {ReturnType}";
    }
}
