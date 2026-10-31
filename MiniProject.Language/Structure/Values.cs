namespace MiniProject.Language.Structure;
public abstract record Value
{
    public static bool ValueEquals(Value V1, Value V2) => Operation.Coerce(V1, V2) switch
    {
        (V_Nat a, V_Nat b) => a.Value == b.Value,
        (V_Int a, V_Int b) => a.Value == b.Value,
        (V_Real a, V_Real b) => a.Value == b.Value,
        (V_Bool a, V_Bool b) => a.Value == b.Value,
        _ => throw new Exception("Invalid operation.")
    };

    public abstract string Show();
}

public sealed record V_Placeholder : Value
{
    public override string Show()
    {
        throw new Exception("Congratulations! You did the impossible! (hopefully)");
    }
}
public sealed record V_Nat(ulong Value) : Value
{
    public override string Show()
    {
        return Value.ToString();
    }
}
public sealed record V_Int(long Value) : Value
{
    public override string Show()
    {
        return Value.ToString();
    }
}
public sealed record V_Real(double Value) : Value
{
    public override string Show()
    {
        return Value.ToString();
    }
}
public sealed record V_Bool(bool Value) : Value
{
    public override string Show()
    {
        return Value.ToString();
    }
}
public sealed record V_Closure(IReadOnlyList<Param> Params, B_Block Body, Env Env) : Value
{
    public override string Show()
    {
        return $"<function({string.Join(", ", Params.Select(x => $"{x.Name}: {x.Type}"))})>";
    }
}
