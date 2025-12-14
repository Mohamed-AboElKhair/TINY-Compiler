using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


public enum Token_Class
{
    Undefined,
    Number,
    String,
    Identifier,
    Int, Float, Read, Write, Repeat, Until, If, ElseIf, Else, Then, Return, Endl,End,
    PlusOp, MinusOp, MultiplyOp, DivideOp,
    EqualOp, NotEqualOp, GreaterThanOp, LessThanOp,
    AndOp, OrOp,
    AssignOp,
    SemiColon, Comma, OpenParenthesis, CloseParenthesis, OpenBrace, CloseBrace,
    Main

}
namespace TINY_Compiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();

        Dictionary<string, Token_Class> reservedWords = new Dictionary<string, Token_Class>
        {
            { "int", Token_Class.Int },
            { "float", Token_Class.Float },
            { "string", Token_Class.String },
            { "read", Token_Class.Read },
            { "write", Token_Class.Write },
            { "repeat", Token_Class.Repeat },
            { "until", Token_Class.Until },
            { "if", Token_Class.If },
            { "elseif", Token_Class.ElseIf },
            { "else", Token_Class.Else },
            { "end", Token_Class.End },
            { "then", Token_Class.Then },
            { "return", Token_Class.Return },
            { "endl", Token_Class.Endl },
            {"main" , Token_Class.Main }
        };

        Dictionary<string, Token_Class> operators = new Dictionary<string, Token_Class>
        {
            { "+", Token_Class.PlusOp },
            { "-", Token_Class.MinusOp },
            { "*", Token_Class.MultiplyOp },
            { "/", Token_Class.DivideOp },
            { "=", Token_Class.EqualOp },
            { "<>", Token_Class.NotEqualOp },
            { ":=", Token_Class.AssignOp },
            { "<", Token_Class.LessThanOp },
            { ">", Token_Class.GreaterThanOp },
            { "&&", Token_Class.AndOp },
            { "||", Token_Class.OrOp }
        };

        Dictionary<string, Token_Class> seperators = new Dictionary<string, Token_Class>
        {
            { ";", Token_Class.SemiColon },
            { ",", Token_Class.Comma },
            { "(", Token_Class.OpenParenthesis },
            { ")", Token_Class.CloseParenthesis },
            { "{", Token_Class.OpenBrace },
            { "}", Token_Class.CloseBrace }
        };

        public void StartScanning(string sourceCode)
        {
            for (int i = 0; i < sourceCode.Length; i++)
            {
                if (char.IsWhiteSpace(sourceCode[i]))
                    continue;

                int j = i;
                string currentLexeme = sourceCode[i].ToString();

                if (char.IsLetter(sourceCode[i]) || char.IsDigit(sourceCode[i]))
                {
                    j = i + 1;
                    while (j < sourceCode.Length && (char.IsLetter(sourceCode[j]) || char.IsDigit(sourceCode[j]) || sourceCode[j] == '.'))
                        currentLexeme += sourceCode[j++];

                    i = j - 1;
                    FindTokenClass(currentLexeme);
                }
                else if (sourceCode[i] == '"')
                {
                    j = i + 1;
                    while (j < sourceCode.Length)
                    {
                        currentLexeme += sourceCode[j];
                        if (sourceCode[j] == '"') break;
                        j++;
                    }
                    i = j;
                    FindTokenClass(currentLexeme);
                }
                else if (sourceCode[i] == '/' && i + 1 < sourceCode.Length && sourceCode[i + 1] == '*')
                {
                    j = i + 2;
                    while (j + 1 < sourceCode.Length && !(sourceCode[j] == '*' && sourceCode[j + 1] == '/'))
                        j++;

                    i = j + 1;

                }
                else
                {
                    string twoChars = "";
                    if (i + 1 < sourceCode.Length)
                        twoChars = sourceCode[i].ToString() + sourceCode[i + 1].ToString();
                    if (operators.ContainsKey(twoChars))
                    {
                        currentLexeme = twoChars;
                        i++;
                        FindTokenClass(currentLexeme);
                    }
                    else
                    {
                        FindTokenClass(currentLexeme);
                    }
                }
            }

            TINY_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            Token Tok = new Token();
            Tok.lex = Lex;
            if (reservedWords.ContainsKey(Lex))
            {
                Tok.token_type = reservedWords[Lex];
            }
            else if (operators.ContainsKey(Lex))
            {
                Tok.token_type = operators[Lex];
            }
            else if (seperators.ContainsKey(Lex))
            {
                Tok.token_type = seperators[Lex];
            }
            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Identifier;
            }
            else if (isConstant(Lex))
            {
                Tok.token_type = Token_Class.Number;
            }
            else if (isString(Lex))
            {
                Tok.token_type = Token_Class.String;
            }
            else
            {
                Tok.token_type = Token_Class.Undefined;
                Errors.Error_List.Add("Unrecognized token: " + Lex);
                return;
            }

            Tokens.Add(Tok);
        }



        bool isIdentifier(string lex)
        {
            Regex reg = new Regex(@"^[A-Za-z][A-Za-z0-9]*$", RegexOptions.Compiled);
            bool isValid = reg.IsMatch(lex);
            return isValid;
        }


        bool isConstant(string lex)
        {
            Regex reg = new Regex(@"^[0-9]+(\.[0-9]+)?$", RegexOptions.Compiled);
            bool isValid = reg.IsMatch(lex);
            return isValid;
        }
        bool isString(string lex)
        {
            Regex reg = new Regex("^\"[^\"]*\"$", RegexOptions.Compiled);
            bool isValid = reg.IsMatch(lex);
            return isValid;
        }
    }
}