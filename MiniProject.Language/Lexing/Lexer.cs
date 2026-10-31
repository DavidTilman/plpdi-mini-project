namespace MiniProject.Language.Lexing;

using System.Text;
using static TokenType;

public class Lexer
{
    static readonly Dictionary<char, TokenType> single_symbols = new()
    {
        { '+', PLUS },
        { '-', SUB },
        { '*', MULT },
        { '/', DIV },
        { '%', MOD },
        { '<', LT },
        { '>', GT },
        { '(', L_PAREN },
        { ')', R_PAREN },
        { '{', L_BRACE },
        { '}', R_BRACE },
        { ',', COMMA },
        { '.', DOT },
        { ':', COLON },
        { ';', SEMICOLON },
        { '?', QUESTION },
        { '=', EQUALS },
    };

    static readonly Dictionary<string, TokenType> double_symbols = new()
    {
        { "<=", LEQ },
        { "==", EQ },
        { "!=", NEQ },
        { ">=", GEQ },
        { "||", OR },
        { "&&", AND },
        { "->", ARROW },
        { "=>", FAT_ARROW },
        { ":=", ASSIGN },
    };

    static readonly Dictionary<string, TokenType> keywords = new()
    {
        { "Nat", NAT_TYPE },
        { "Int", INT_TYPE },
        { "Real", REAL_TYPE },
        { "Bool", BOOL_TYPE },
        { "True", TRUE_LIT },
        { "False", FALSE_LIT },
        { "Lambda", LAMBDA },
        { "Var", VAR },
        { "Read", READ },
        { "While", WHILE },
        { "If", IF },
        { "Else", ELSE },
        { "Ret", RET },
        { "Print", PRINT },
        { "Func", FUNC },
    };
    

    StringBuilder scratchpad = new();
    List<Token> tokens = [];
    string text = "";
    int i = 0;
    char Current => text[i];
    char Next => i + 1 < text.Length ? text[i + 1] : '\0';
    bool AtEnd => i >= text.Length;

    public List<Token> Lex(string input) {
        tokens = [];
        text = input;
        i = 0;

        while (!AtEnd)
        {
            scratchpad.Clear();

            if (char.IsWhiteSpace(Current)) {
                i++;
                continue;
            }

            scratchpad.Append(Current);
            scratchpad.Append(Next);

            if (double_symbols.TryGetValue(scratchpad.ToString(), out TokenType doubleT)) {
                tokens.Add(new Token(doubleT, scratchpad.ToString()));
                i += 2;
                continue;
            }

            scratchpad.Remove(1,1);

            if (single_symbols.TryGetValue(scratchpad[0], out TokenType singleT)) {
                tokens.Add(new Token(singleT, scratchpad.ToString()));
                i++;
                continue;
            }

            if (char.IsDigit(Current)) {
                ConsumeNumber(); // sets i locally
                continue;
            }

            if (char.IsLetter(Current)) {
                ConsumeWord(); // sets i locally
                continue;
            }

            throw new Exception($"Unknown char {Current}.");
        }

        tokens.Add(new Token(EOF, ""));
        return tokens;
    }

    private void ConsumeNumber()
    {
        scratchpad.Clear();
        if (!char.IsDigit(Current)) throw new Exception($"Cannot consume {Current} as digit.");

        bool seenDot = false;
        while (!AtEnd && (char.IsDigit(Current) || (Current == '.' && !seenDot)))
        {
            if (Current == '.') seenDot = true;
            scratchpad.Append(Current);
            i++;
        }

        tokens.Add(new Token(seenDot ? REAL_LIT : INT_LIT, scratchpad.ToString()));
        scratchpad.Clear();
    }

    private void ConsumeWord()
    {
        scratchpad.Clear();

        while (!AtEnd && char.IsLetterOrDigit(Current))
        {
            scratchpad.Append(Current);
            i++;
        }

        string word = scratchpad.ToString();

        if (keywords.TryGetValue(word, out TokenType type))
            tokens.Add(new Token(type, scratchpad.ToString()));
        else
            tokens.Add(new Token(IDENT, word));
    }
}