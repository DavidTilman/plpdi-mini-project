using MiniProject.Language.Lexing;
using MiniProject.Language.Parsing;
using MiniProject.Language.Structure;
using MiniProject.Language.TypeChecking;

string addition = File.ReadAllText("/home/dtilm/Documents/Academic/Year_3/Modules/3.1_Programming_Language_Principles,_Design,_and_Implementation/0._Mini-Project/examplePrograms/addition.numa");
string half = File.ReadAllText("/home/dtilm/Documents/Academic/Year_3/Modules/3.1_Programming_Language_Principles,_Design,_and_Implementation/0._Mini-Project/examplePrograms/half.numa");
string factorial = File.ReadAllText("/home/dtilm/Documents/Academic/Year_3/Modules/3.1_Programming_Language_Principles,_Design,_and_Implementation/0._Mini-Project/examplePrograms/factorial.numa");
string fibonacci = File.ReadAllText("/home/dtilm/Documents/Academic/Year_3/Modules/3.1_Programming_Language_Principles,_Design,_and_Implementation/0._Mini-Project/examplePrograms/fibonacci.numa");

void RunProg(string progText)
{
    Lexer l = new();
    Parser p = new(l.Lex(progText));
    P_Prog prog = p.ParseProgram();
    prog.TypeCheck();
    prog.Execute();
}


RunProg(File.ReadAllText("/home/dtilm/Documents/Academic/Year_3/Modules/3.1_Programming_Language_Principles,_Design,_and_Implementation/0._Mini-Project/examplePrograms/lambdas.numa"));