using System.Collections.Immutable;
using MiniProject.Language.TypeChecking;

namespace MiniProject.Language.Structure;
public interface IProg
{
    public void Execute();
    public void TypeCheck();
}
public sealed record P_Prog(IReadOnlyList<Com> Commands) : IProg
{
    public IEnumerable<D_Func> Funcs => Commands.OfType<C_Decl>().Select(c => c.Decl).OfType<D_Func>();
    public IEnumerable<IStmt> Stmts => Commands.OfType<C_Stmt>().Select(c => c.Stmt);
    public Env Establish()
    {
        HashSet<string> seen = [];
        foreach (D_Func func in Funcs)
        {
            if (!seen.Add(func.Name))
                throw new Exception($"Function {func.Name} is defined more than once.");
            HashSet<string> seen_param = [];
            foreach (Param p in func.Params)
                if (!seen_param.Add(p.Name))
                    throw new Exception($"Function {func.Name} has duplicate parameters {p.Name}.");
        }

        Env global_env = new([]);
        
        foreach (D_Func func in Funcs)
        {
            global_env = global_env.Bind(func.Name, new V_Placeholder());
        }
        foreach (D_Func func in Funcs)
        {
            global_env.GetRef(func.Name).Value = new V_Closure(func.Params, func.Body, global_env);
        }

        return global_env;
    }
    public void Execute()
    {
        Env global_env = Establish();
        B_Block main = new(Stmts.ToList());
        Outcome o = main.Execute(global_env);
        if (o is not O_Normal) throw new Exception("Program did not exit normally.");
    }

    public void TypeCheck()
    {
        Dictionary<string, Type> vars = [];
        foreach (D_Func f in Funcs) {
            if (vars.ContainsKey(f.Name))
                throw new TypeCheckerException($"Duplicate parameter name {f.Name}.");
            vars.Add(f.Name, new T_Func([.. f.Params.Select(x => x.Type)], f.ReturnType));
        }
        TypingContext master = new(vars.ToImmutableDictionary());
        
        foreach (D_Func f in Funcs) {
            TypingContext funcContext = master;
            foreach(Param pm in f.Params) 
                funcContext = funcContext.Bind(pm.Name, pm.Type);
            
            f.Body.TypeCheck(funcContext, f.ReturnType);
        }

        B_Block topLevelBlock = new([..Stmts]);
        topLevelBlock.TypeCheck(master, null);
    }
}