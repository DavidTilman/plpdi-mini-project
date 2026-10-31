using System.Collections.Immutable;
namespace MiniProject.Language.Structure;

public sealed record Param(string Name, Type Type);

public sealed class Ref(Value value)
{
    public Value Value { get; set; } = value;
}
public sealed record Env(ImmutableDictionary<string, Ref> Vars)
{
    public Ref GetRef(string name)
    {
        return Vars.TryGetValue(name, out Ref? r) ? r : throw new Exception("Undefined variable access.");
    }

    public Env Bind(string name, Value val)
        => this with { Vars = Vars.SetItem(name, new Ref(val)) };
}