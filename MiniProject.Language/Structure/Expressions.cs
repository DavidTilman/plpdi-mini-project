using MiniProject.Language.TypeChecking;

namespace MiniProject.Language.Structure;

public interface IExpr
{
    public Value Evaluate(Env Env);
    public Type TypeCheck(TypingContext context);
}
public sealed record E_Lit(object Value) : IExpr
{
    public Value Evaluate(Env Env) => Value switch
    {
        ulong ul => new V_Nat(ul),
        double d => new V_Real(d),
        bool b   => new V_Bool(b),
        _ => throw new Exception("Invalid Literal")
    };

    public Type TypeCheck(TypingContext context) => Value switch
    {
        ulong ul => new T_Nat(),
        double d => new T_Real(),
        bool b   => new T_Bool(),
        _ => throw new Exception("Invalid Literal")
    };
}

public sealed record E_UnaryNegate(IExpr Expr) : IExpr
{
    // E-Neg: negating a Nat gives an Int, since the result can be below zero.
    public Value Evaluate(Env Env) => Expr.Evaluate(Env) switch
    {
        V_Nat n  => new V_Int(-(long)n.Value),
        V_Int i  => new V_Int(-i.Value),
        V_Real r => new V_Real(-r.Value),
        _ => throw new Exception("Invalid operation.")
    };

    public Type TypeCheck(TypingContext context)
    {
        Type eT = Expr.TypeCheck(context);
        return TypeChecker.Neg(eT) ?? throw new TypeCheckerException($"Cannot negate {eT}.");
    }
}
public sealed record E_Var(string Name) : IExpr
{
    public Value Evaluate(Env Env)
    {
        return Env.GetRef(Name).Value;
    }

    public Type TypeCheck(TypingContext context)
    {
        return context.Lookup(Name);
    }
}
public sealed record E_Op(IExpr E1, Operation Op, IExpr E2) : IExpr
{
    public Value Evaluate(Env Env)
    {
        return Op.Evaluate(E1.Evaluate(Env), E2.Evaluate(Env));
    }

    public Type TypeCheck(TypingContext context)
    {
        Type t1 = E1.TypeCheck(context);
        Type t2 = E2.TypeCheck(context);
        return TypeChecker.Sig(Op, t1, t2) ?? throw new TypeCheckerException($"{Op} is not defined on {t1}, {t2}.");
    }
}

public sealed record E_Appl(IExpr Target, IReadOnlyList<IExpr> Args) : IExpr
{
    public Value Evaluate(Env Env)
    {
        Value val = Target.Evaluate(Env);
        if (val is not V_Closure clo) throw new Exception("Attempted application on non-closure.");
        Value[] values = new Value[Args.Count];
        for(int i = 0; i < Args.Count; i++) 
        {
            values[i] = Args[i].Evaluate(Env);
        }
        if (values.Length != clo.Params.Count) throw new Exception("Arguments do not match function parameters."); 
        Env callEnv = clo.Env;
        for (int i = 0; i < values.Length; i++)
            callEnv = callEnv.Bind(clo.Params[i].Name, values[i]);
        Outcome outcome = clo.Body.Execute(callEnv);
        if (outcome is not O_Ret ret) throw new Exception("Function did not return a value.");
        return ret.Value;
    }

    public Type TypeCheck(TypingContext context)
    {
        Type fT = Target.TypeCheck(context);
        if (fT is not T_Func func) throw new TypeCheckerException("Attempted application on non-func.");
        if (func.ParamTypes.Count != Args.Count)
            throw new TypeCheckerException("Args count does not match function parameter types.");
        for (int i = 0; i < Args.Count; i++)
        {
            Type argT = Args[i].TypeCheck(context);
            if (!TypeChecker.Assignable(argT, func.ParamTypes[i]))
                throw new TypeCheckerException($"Argument {i + 1} has type {argT}, expected {func.ParamTypes[i]}.");
        }
        return func.ReturnType;
    }
}

public sealed record E_Lambda(IReadOnlyList<Param> Params, Type ReturnType, IExpr Body) : IExpr
{
    public Value Evaluate(Env Env) => new V_Closure(Params, new B_Block([new S_Return(Body)]), Env);

    public Type TypeCheck(TypingContext context)
    {
        TypingContext cloCon = context;
        foreach (Param p in Params) 
            cloCon = cloCon.Bind(p.Name, p.Type);
        Type bodyT = Body.TypeCheck(cloCon);                      
        if (!TypeChecker.Assignable(bodyT, ReturnType))
            throw new TypeCheckerException($"Lambda body has type {bodyT}, expected {ReturnType}.");
        return new T_Func([.. Params.Select(p => p.Type)], ReturnType);   
    }
}
public sealed record E_Cond(IExpr Cond, IExpr OnTrue, IExpr OnFalse) : IExpr
{
    public Value Evaluate(Env Env)
    {
        if (Cond.Evaluate(Env) is not V_Bool cond) throw new Exception("Expression did not evaluate to a boolean.");
        return cond.Value ? OnTrue.Evaluate(Env) : OnFalse.Evaluate(Env);
    }

    public Type TypeCheck(TypingContext context)
    {   
        Type conT = Cond.TypeCheck(context);
        if (conT is not T_Bool) 
            throw new TypeCheckerException($"Attempted condition on non-bool ({conT}).");
        Type trueT = OnTrue.TypeCheck(context);
        Type falseT = OnFalse.TypeCheck(context);
        return TypeChecker.Widest(trueT, falseT)
               ?? throw new TypeCheckerException($"Type mismatch in condtional: {trueT}, {falseT}.");
    }
}
