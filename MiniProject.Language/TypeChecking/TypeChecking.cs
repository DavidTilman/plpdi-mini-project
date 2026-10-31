using System.Collections.Immutable;
using MiniProject.Language.Structure;
using Type = MiniProject.Language.Structure.Type;

namespace MiniProject.Language.TypeChecking;

[Serializable]
public class TypeCheckerException(string msg) : Exception(msg) { }

// Gamma
public sealed record TypingContext (ImmutableDictionary<string, Type> Vars)
{
    // name : t in Gamma
    public Type Lookup(string name)
        => Vars.TryGetValue(name, out Type? t) 
            ? t 
            : throw new TypeCheckerException($"Unbound var: {name}");

    // Gamma, name : type
    public TypingContext Bind(string name, Type type)
        => this with { Vars = Vars.SetItem(name, type) };
}

public static class TypeChecker
{
    public static Type? Neg(Type t) => t switch
    {
        T_Nat  => new T_Int(),
        T_Int  => new T_Int(),
        T_Real => new T_Real(),
        _ => null
    };

    public static Type? Widest(Type t1, Type t2)
    {
        if (Equal(t1, t2)) return t1;
        
        return (t1, t2) switch
        {
            (T_Nat, T_Int)  => new T_Int(),  
            (T_Nat, T_Real) => new T_Real(),
            (T_Int, T_Nat)  => new T_Int(),  
            (T_Int, T_Real) => new T_Real(),  
            (T_Real, T_Nat) => new T_Real(),
            (T_Real, T_Int) => new T_Real(),
            _ => null,
        };
    }
    public static Type? Sig(Operation op, Type t1, Type t2) => (op, t1, t2) switch
    {
        (OP_Plus or OP_Mult or OP_Div or OP_Mod, T_Nat or T_Int or T_Real, T_Nat or T_Int or T_Real)
            => Widest(t1, t2),
        (OP_Sub, T_Nat or T_Int or T_Real, T_Nat or T_Int or T_Real)
            => Widest(Widest(t1, t2)!, new T_Int()),
        (OP_LT or OP_LEQ or OP_GEQ or OP_GT, T_Nat or T_Int or T_Real, T_Nat or T_Int or T_Real)
            => new T_Bool(),
        (OP_EQ or OP_NEQ, T_Nat or T_Int or T_Real, T_Nat or T_Int or T_Real) => new T_Bool(),
        (OP_EQ or OP_NEQ, T_Bool, T_Bool) => new T_Bool(),
        (OP_AND or OP_OR, T_Bool, T_Bool) => new T_Bool(),
        _ => null,
    };
    public static bool Assignable(Type from, Type to)
    {
        if ((from is T_Nat or T_Int or T_Real) && (to is T_Nat or T_Int or T_Real))
            return Rank(from) <= Rank(to);
        return Equal(from, to);
    }

    static bool Equal(Type a, Type b) => (a, b) switch
    {
        (T_Nat, T_Nat) => true,
        (T_Int, T_Int) => true,
        (T_Real, T_Real) => true,
        (T_Bool, T_Bool) => true,
        (T_Func f, T_Func g)
            => f.ParamTypes.Count == g.ParamTypes.Count
               && f.ParamTypes.Zip(g.ParamTypes).All(p => Equal(p.First, p.Second))
               && Equal(f.ReturnType, g.ReturnType),
        _ => false,
    };

    static int Rank(Type t) => t switch
    {
        T_Nat => 0,
        T_Int => 1,
        T_Real => 2,
        _ => -1,
    };
}