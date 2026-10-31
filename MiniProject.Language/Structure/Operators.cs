namespace MiniProject.Language.Structure;

public abstract record Operation
{
    public abstract Value Evaluate(Value V1, Value V2);

    // Widening coercion over Nat <= Int <= Real: promote the narrower operand
    // to the wider tag so a mixed pair hits the matching case below. Non-numeric
    // operands (and same-tag pairs) are left untouched.
    internal static (Value, Value) Coerce(Value a, Value b)
    {
        int ra = Rank(a), rb = Rank(b);
        if (ra < 0 || rb < 0 || ra == rb) return (a, b);
        int t = Math.Max(ra, rb);
        return (Promote(a, t), Promote(b, t));
    }

    private static int Rank(Value v) => v switch
    {
        V_Nat => 0,
        V_Int => 1,
        V_Real => 2,
        _ => -1,
    };

    private static Value Promote(Value v, int target) => (v, target) switch
    {
        (V_Nat n, 1) => new V_Int((long)n.Value),
        (V_Nat n, 2) => new V_Real(n.Value),
        (V_Int i, 2) => new V_Real(i.Value),
        _ => v,
    };
}

public sealed record OP_Plus : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Nat(a.Value + b.Value),
            (V_Int a, V_Int b) => new V_Int(a.Value + b.Value),
            (V_Real a, V_Real b) => new V_Real(a.Value + b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_Sub : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Int((long)a.Value - (long)b.Value),
            (V_Int a, V_Int b) => new V_Int(a.Value - b.Value),
            (V_Real a, V_Real b) => new V_Real(a.Value - b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_Mult : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Nat(a.Value * b.Value),
            (V_Int a, V_Int b) => new V_Int(a.Value * b.Value),
            (V_Real a, V_Real b) => new V_Real(a.Value * b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_Div : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Nat(a.Value / b.Value),
            (V_Int a, V_Int b) => new V_Int(a.Value / b.Value),
            (V_Real a, V_Real b) => new V_Real(a.Value / b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_Mod : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Nat(a.Value % b.Value),
            (V_Int a, V_Int b) => new V_Int(a.Value % b.Value),
            (V_Real a, V_Real b) => new V_Real(a.Value % b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_LT : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Bool(a.Value < b.Value),
            (V_Int a, V_Int b) => new V_Bool(a.Value < b.Value),
            (V_Real a, V_Real b) => new V_Bool(a.Value < b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_LEQ : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Bool(a.Value <= b.Value),
            (V_Int a, V_Int b) => new V_Bool(a.Value <= b.Value),
            (V_Real a, V_Real b) => new V_Bool(a.Value <= b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_EQ : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        return new V_Bool(Value.ValueEquals(V1, V2));
    }
}

public sealed record OP_NEQ : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        return new V_Bool(!Value.ValueEquals(V1, V2));
    }
}

public sealed record OP_GEQ : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Bool(a.Value >= b.Value),
            (V_Int a, V_Int b) => new V_Bool(a.Value >= b.Value),
            (V_Real a, V_Real b) => new V_Bool(a.Value >= b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_GT : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Nat a, V_Nat b) => new V_Bool(a.Value > b.Value),
            (V_Int a, V_Int b) => new V_Bool(a.Value > b.Value),
            (V_Real a, V_Real b) => new V_Bool(a.Value > b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_OR : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Bool a, V_Bool b) => new V_Bool(a.Value || b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}

public sealed record OP_AND : Operation
{
    public override Value Evaluate(Value V1, Value V2)
    {
        (V1, V2) = Coerce(V1, V2);
        return (V1, V2) switch
        {
            (V_Bool a, V_Bool b) => new V_Bool(a.Value && b.Value),
            _ => throw new Exception("Invalid operation.")
        };
    }
}