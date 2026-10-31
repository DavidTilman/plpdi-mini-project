using System.Globalization;
using MiniProject.Language.TypeChecking;

namespace MiniProject.Language.Structure;

public abstract record Outcome;
public sealed record O_Normal : Outcome; 
public sealed record O_Ret(Value Value) : Outcome; 

public interface IStmt
{
    public (Outcome, Env) Execute(Env Env);
    public TypingContext TypeCheck(TypingContext context, Type? ret);
}
public sealed record S_Decl(string Variable, Type Type, IExpr Expr) : IStmt
{
    public (Outcome, Env) Execute(Env Env)
    {
        return (new O_Normal(), Env.Bind(Variable, Expr.Evaluate(Env)));
    }

    public TypingContext TypeCheck(TypingContext context, Type? _)
    {
        Type expT = Expr.TypeCheck(context);
        if (!TypeChecker.Assignable(expT, Type)) 
            throw new TypeCheckerException($"{expT} is not assignable to {Type}.");
        return context.Bind(Variable, Type);
    }
}

public sealed record S_Read(string Variable, Type Type) : IStmt
{
    public (Outcome, Env) Execute(Env Env)
    {
        Console.Write($"{Variable} := ");
        string input = Console.ReadLine() ?? throw new Exception("No input left to read.");
        Value v = Type switch
        {
            T_Nat => ulong.TryParse(input, CultureInfo.InvariantCulture, out ulong ul) ? new V_Nat(ul) : throw new Exception($"Unable to parse {Type}"),
            T_Int => long.TryParse(input, CultureInfo.InvariantCulture, out long l) ? new V_Int(l) : throw new Exception($"Unable to parse {Type}"),
            T_Real => double.TryParse(input, CultureInfo.InvariantCulture, out double d) ? new V_Real(d) : throw new Exception($"Unable to parse {Type}"),
            T_Bool => bool.TryParse(input, out bool b) ? new V_Bool(b) : throw new Exception($"Unable to parse {Type}"),
            _ => throw new Exception($"Unable to read to {Type}"),
        };
        return (new O_Normal(), Env.Bind(Variable, v));
    }

    public TypingContext TypeCheck(TypingContext context, Type? _)
        => context.Bind(Variable, Type);
}

public sealed record S_Assign(string Variable, IExpr Expr) : IStmt
{
    public (Outcome, Env) Execute(Env Env)
    {
        Value v = Expr.Evaluate(Env);
        Env.GetRef(Variable).Value = v;
        return (new O_Normal(), Env);
    }

    public TypingContext TypeCheck(TypingContext context, Type? _)
    {
        Type varT = context.Lookup(Variable);
        Type expT = Expr.TypeCheck(context);
        if (!TypeChecker.Assignable(expT, varT)) 
            throw new TypeCheckerException($"{expT} is not assignable to {varT}.");
        return context;
    }
}

public sealed record S_While(IExpr Cond, B_Block Body) : IStmt
{
    public (Outcome, Env) Execute(Env Env)
    {
        while(true)
        {
            Value cond = Cond.Evaluate(Env);
            if (cond is not V_Bool cond_result) throw new Exception("Attempted condition on non bool.");
            if (!cond_result.Value) return (new O_Normal(), Env); 
            Outcome block_out = Body.Execute(Env);
             if (block_out is O_Ret ret) return (ret, Env);
            
        }
    }

    public TypingContext TypeCheck(TypingContext context, Type? ret)
    {
        Type conT = Cond.TypeCheck(context);
        if (conT is not T_Bool) throw new TypeCheckerException($"Attempted condition on non bool ({conT})");
        Body.TypeCheck(context, ret);
        return context;
    }
}

public sealed record S_Cond(IExpr Cond, B_Block OnTrue, B_Block OnFalse) : IStmt
{
    public (Outcome, Env) Execute(Env Env)
    {
        Value cond = Cond.Evaluate(Env);
        if (cond is not V_Bool cond_result) throw new Exception("Attempted condition on non bool.");
        if(cond_result.Value)
        {
            return (OnTrue.Execute(Env), Env);
        }
        else
        {
            return (OnFalse.Execute(Env), Env);
        }
    }

    public TypingContext TypeCheck(TypingContext context, Type? ret)
    {
        Type conT = Cond.TypeCheck(context);
        if (conT is not T_Bool)
            throw new TypeCheckerException($"Attempted condition on non-bool ({conT}).");
        OnTrue.TypeCheck(context, ret);
        OnFalse.TypeCheck(context, ret);
        return context;
    }
}

public sealed record S_Return(IExpr Expr) : IStmt
{
    public (Outcome, Env) Execute(Env Env)
    {
        return (new O_Ret(Expr.Evaluate(Env)), Env);
    }

    public TypingContext TypeCheck(TypingContext context, Type? ret)
    {
        Type expT = Expr.TypeCheck(context);
        if (ret is null) throw new TypeCheckerException("Attempted return outside of block.");
        if (!TypeChecker.Assignable(expT, ret)) throw new TypeCheckerException($"Attempted return on {expT} in block returning {ret}.");
        return context;
    }
}

public sealed record S_Print(IExpr Expr) : IStmt
{
    public (Outcome, Env) Execute(Env Env)
    {
        Value v = Expr.Evaluate(Env);
        Console.WriteLine(v.Show());
        return (new O_Normal(), Env);
    }

    public TypingContext TypeCheck(TypingContext context, Type? ret)
    {
        Expr.TypeCheck(context);
        return context;
    }
}