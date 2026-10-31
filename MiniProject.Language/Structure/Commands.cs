namespace MiniProject.Language.Structure;
public abstract record Com;

public sealed record C_Decl(Decl Decl) : Com;
public sealed record C_Stmt(IStmt Stmt) : Com;