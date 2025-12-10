using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TINY_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public  Node root;
        
        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }
        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(ProgramFunctions());
            program.Children.Add(MainFunction());
            MessageBox.Show("Success");
            return program;
        }
        Node ProgramFunctions()
        {
            Node programFunctions = new Node("ProgramFunctions");
            while (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type != Token_Class.Main)
            {
                programFunctions.Children.Add(FunctionStatement());
            }
            return programFunctions;
        }
        Node FunctionStatement()
        {
            Node functionstatement = new Node("FunctionStatement");
            //functionstatement.Children.Add(FunctionDeclaration());
            //functionstatement.Children.Add(FunctionBody());
            return functionstatement;
        }
        Node MainFunction()
        {
            Node mainFunction = new Node("MainFunction");
            mainFunction.Children.Add(match(Token_Class.Int));
            mainFunction.Children.Add(match(Token_Class.Main));
            mainFunction.Children.Add(match(Token_Class.OpenParenthesis));
            mainFunction.Children.Add(match(Token_Class.CloseParenthesis));
            //functionstatement.Children.Add(FunctionBody());
            return mainFunction;
        }
        Node FunctionCall()
        {
            Node functionCall = new Node("FunctionCall");
            functionCall.Children.Add(match(Token_Class.Identifier));
            functionCall.Children.Add(match(Token_Class.OpenParenthesis));
            functionCall.Children.Add(ArgumentList());
            functionCall.Children.Add(match(Token_Class.CloseParenthesis));
            return functionCall;
        }

        Node ArgumentList()
        {
            Node argumentList = new Node("ArgumentList");
            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                return argumentList; // empty argument list
            }
            argumentList.Children.Add(match(Token_Class.Identifier));
            argumentList.Children.Add(Arguments());
            return argumentList;
        }

        Node Arguments()
        {
            Node arguments = new Node("Arguments");
            if (TokenStream[InputPointer].token_type != Token_Class.Comma)
            {
                return arguments; // no more arguments
            }
            arguments.Children.Add(match(Token_Class.Comma));
            arguments.Children.Add(ArgumentList());

            return arguments;
        }

        Node Term()
        {
            Node term = new Node("Term");
            //Number or Identifier or function call
            if (TokenStream[InputPointer].token_type == Token_Class.Number)
            {
                term.Children.Add(match(Token_Class.Number));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
            { 
                if (InputPointer + 1 < TokenStream.Count &&
                    TokenStream[InputPointer + 1].token_type == Token_Class.OpenParenthesis)
                {
                    term.Children.Add(FunctionCall());
                }
                else
                {
                    term.Children.Add(match(Token_Class.Identifier));
                }
            }
            else
            { 
                Errors.Error_List.Add("Parsing Error: Expected Number or Identifier at position " + InputPointer + "\r\n");
                InputPointer++;
            }
            
            return term;
        }

        Node Equation()
        {
            Node equation = new Node("Equation");
            equation.Children.Add(EquationStart());
            equation.Children.Add(EquationTail());
            return equation;
        }
        Node EquationStart()
        {
            Node equationStart = new Node("EquationStart");
            if (TokenStream[InputPointer].token_type == Token_Class.OpenParenthesis)
            {
                equationStart.Children.Add(match(Token_Class.OpenParenthesis));
                equationStart.Children.Add(Equation());
                equationStart.Children.Add(match(Token_Class.CloseParenthesis));
            }
            else
            {
                equationStart.Children.Add(Term());
            }
             return equationStart;
        }
        Node EquationTail()
        {
            Node equationtail = new Node("EquationTail");
            //equationtail.Children.Add(ArthimeticOperator());
            if(equationtail.Children.Count==0)
            {
                //no operator found return empty
                return equationtail;
            }
            equationtail.Children.Add(Equation());
            return equationtail;
        }

        Node Expression( )
        {
            Node expression = new Node("Expression");
            if (TokenStream[InputPointer].token_type == Token_Class.String)
            {
                expression.Children.Add(match(Token_Class.String));
                return expression;
            }
            //equation or term
            //eqiation can have a single term might need to revise later 
            expression.Children.Add(Equation());


            return expression;
        }
        // Implement your logic here

        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString()  + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
