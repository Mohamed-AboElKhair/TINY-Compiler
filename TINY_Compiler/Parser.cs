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
            Node programFunctions = null;//new Node("ProgramFunctions");
            if(InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type != Token_Class.Main) programFunctions = new Node("ProgramFunctions");
            while (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type != Token_Class.Main)
            {
                programFunctions.Children.Add(FunctionStatement());
            }
            return programFunctions;
        }
        Node FunctionStatement()
        {
            Node functionstatement = new Node("FunctionStatement");
            functionstatement.Children.Add(Function_Declaration());
            functionstatement.Children.Add(Function_body());
            return functionstatement;
        }
        Node MainFunction()
        {
            Node mainFunction = new Node("MainFunction");
            mainFunction.Children.Add(match(Token_Class.Int));
            mainFunction.Children.Add(match(Token_Class.Main));
            mainFunction.Children.Add(match(Token_Class.OpenParenthesis));
            mainFunction.Children.Add(match(Token_Class.CloseParenthesis));
            mainFunction.Children.Add(Function_body());
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
            if (InputPointer >= TokenStream.Count) return argumentList;
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
            if (InputPointer >= TokenStream.Count) return arguments;
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
            if (InputPointer >= TokenStream.Count) return term;
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
                //InputPointer++;
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
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.OpenParenthesis)
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

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.String)
            {
                expression.Children.Add(match(Token_Class.String));
                return expression;
            }
            //equation or term
            //eqiation can have a single term might need to revise later 
            expression.Children.Add(Equation());


            return expression;
        }


        Node AssignmentStmt()
        {
            Node assignment = new Node("AssignmentStmt");
            assignment.Children.Add(match(Token_Class.Identifier));
            assignment.Children.Add(match(Token_Class.AssignOp));
            assignment.Children.Add(Expression());
            assignment.Children.Add(match(Token_Class.SemiColon));
            return assignment;
        }

        Node DataType()
        {
            Node data_type = new Node("DataType");
            if (InputPointer >= TokenStream.Count)
            {
                Errors.Error_List.Add("Parsing Error: Expected DataType at position " + InputPointer + "\r\n");
                return data_type;
            }
            Token_Class type = TokenStream[InputPointer].token_type;
            if (type == Token_Class.Int || type == Token_Class.Float || type == Token_Class.String)
            {
                data_type.Children.Add(match(type));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected DataType at position " + InputPointer + "\r\n");
            }

            return data_type;
        }

        Node DeclarationStmt()
        {
            Node decl = new Node("DeclarationStmt");
            decl.Children.Add(DataType());
            decl.Children.Add(DeclList());
            decl.Children.Add(match(Token_Class.SemiColon));
            return decl;
        }

        Node DeclList()
        {
            Node list = new Node("DeclList");
            list.Children.Add(DeclItem());
            list.Children.Add(DeclListDach());
            return list;
        }

        Node DeclListDach()
        {
            Node listPrime = new Node("DeclListDach");
            if (InputPointer >= TokenStream.Count) return listPrime;
            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                listPrime.Children.Add(match(Token_Class.Comma));
                listPrime.Children.Add(DeclItem());
                listPrime.Children.Add(DeclListDach());
            }

            // else ε
            return listPrime;
        }

        Node DeclItem()
        {
            Node item = new Node("DeclItem");
            item.Children.Add(match(Token_Class.Identifier));
            item.Children.Add(OptInit());
            return item;
        }

        Node OptInit()
        {
            Node init = new Node("OptInit");
            if (InputPointer >= TokenStream.Count) return init;
            if (TokenStream[InputPointer].token_type == Token_Class.AssignOp)
            {
                init.Children.Add(match(Token_Class.AssignOp));
                init.Children.Add(Expression());
            }

            // else ε 
            return init;
        }

        Node WriteStmt()
        {
            Node Write_Stmt = new Node("WriteStmt");

            Write_Stmt.Children.Add(match(Token_Class.Write));

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Endl)
            {
                Write_Stmt.Children.Add(match(Token_Class.Endl));
            }
            else
            {
                Write_Stmt.Children.Add(Expression());
            }

            Write_Stmt.Children.Add(match(Token_Class.SemiColon));
            return Write_Stmt;

        }
        // Implement your logic here
        Node Read_Statement()
        {
            Node ReadS = new Node("Read_Statement");
            ReadS.Children.Add(match(Token_Class.Read));
            ReadS.Children.Add(match(Token_Class.Identifier));
            ReadS.Children.Add(match(Token_Class.SemiColon));
            return ReadS;
        }

        Node ReturnStat()
        {
            Node returnS = new Node("Return_Statement");
            returnS.Children.Add(match(Token_Class.Return));
            returnS.Children.Add(Expression());
            returnS.Children.Add(match(Token_Class.SemiColon));
            return returnS;


        }
        Node condition_operator()
        {
            Node conoperator = new Node("condition_operator");
            if (InputPointer >= TokenStream.Count) return conoperator;
            switch (TokenStream[InputPointer].token_type)
            {
                case Token_Class.EqualOp:
                    conoperator.Children.Add(match(Token_Class.EqualOp));
                    break;
                case Token_Class.NotEqualOp:
                    conoperator.Children.Add(match(Token_Class.NotEqualOp));
                    break;
                case Token_Class.GreaterThanOp:
                    conoperator.Children.Add(match(Token_Class.GreaterThanOp));
                    break;
                case Token_Class.LessThanOp:
                    conoperator.Children.Add(match(Token_Class.LessThanOp));
                    break;
            }

            return conoperator;
        }

        Node boolean_operator()
        {
            Node booloperator = new Node("BoolOp");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                booloperator.Children.Add(match(Token_Class.OrOp));
            }
            else
            {
                booloperator.Children.Add(match(Token_Class.AndOp));
            }
            return booloperator;
        }
        Node Condition()
        {
            Node cond = new Node("Condition");
            cond.Children.Add(match(Token_Class.Identifier));
            cond.Children.Add(condition_operator());
            cond.Children.Add(Term());
            return cond;
        }
        Node Condition_Statement()
        {
            Node conditionS = new Node("Condition_Statement");
            conditionS.Children.Add(Condition());
            conditionS.Children.Add(ConditionStatementTail());
            return conditionS;
        }

        Node ConditionStatementTail()
        {
            Node conditionST = new Node("Condition_Statement'");

            while (InputPointer < TokenStream.Count &&
                  (TokenStream[InputPointer].token_type == Token_Class.AndOp ||
                   TokenStream[InputPointer].token_type == Token_Class.OrOp))
            {
                conditionST.Children.Add(boolean_operator());
                conditionST.Children.Add(Condition());
            }

            return conditionST;
        }

        //ZAINAB
        //If_Statement ⟶ if Condition_Statement then Statements (Else_IF_Statement |Else_statement |end)
        Node If_statement()
        {
            Node ifstat = new Node("If_statement");
            ifstat.Children.Add(match(Token_Class.If));
            ifstat.Children.Add(Condition_Statement());
            ifstat.Children.Add(match(Token_Class.Then));
            ifstat.Children.Add(Statements());
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ElseIf)
            {
                ifstat.Children.Add(Else_IF_Statement());
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                ifstat.Children.Add(Else_Statement());
            }

           // ifstat.Children.Add(match(Token_Class.End));

            return ifstat;


        }
        //Else_IF_Statement ⟶ elseif Condition_Statement then Statements  end
        Node Else_IF_Statement()
        {
            Node elseifstat = new Node("Else_IF_Statement");
            elseifstat.Children.Add(match(Token_Class.ElseIf));
            elseifstat.Children.Add(Condition_Statement());
            elseifstat.Children.Add(match(Token_Class.Then));
            elseifstat.Children.Add(Statements());
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                elseifstat.Children.Add(Else_Statement());
            }
            //elseifstat.Children.Add(match(Token_Class.End));

            return elseifstat;



        }
        //Else_statement ⟶ else statements end    
        Node Else_Statement()
        {
            Node elsestat = new Node("Else_Statement");
            elsestat.Children.Add(match(Token_Class.Else));
            elsestat.Children.Add(Statements());
           // elsestat.Children.Add(match(Token_Class.End));
            return elsestat;


        }
        //Repeat_Statement ⟶ repeat Statements until Condition_Statement

        Node Repeat_Statement()
        {
            Node repstat = new Node("Repeat_Statement");
            repstat.Children.Add(match(Token_Class.Repeat));
            repstat.Children.Add(Statements());
            repstat.Children.Add(match(Token_Class.Until));
            repstat.Children.Add(Condition_Statement());

            return repstat;



        }
        //Statement → Assignment_Statement 1
        //| Write_Statement 1
        //| Read_Statement 1
        //| Return_Statement 1
        //| Condition_Statement
        //| Declaration_Statement 1 
        //| If_Statement 1
        //| Repeat_Statement1
        //| comment_statement 
        //| Function_Call 1 | ɛ
        Node Statement()
        {
            Node statementNode = new Node("Statement");

            if (InputPointer < TokenStream.Count)
            {
                var token = TokenStream[InputPointer].token_type;

                if (token == Token_Class.Write)
                {
                    statementNode.Children.Add(WriteStmt());
                    return statementNode;
                }
                else if (token == Token_Class.Read)
                {
                    statementNode.Children.Add(Read_Statement());
                    return statementNode;

                }
                else if (token == Token_Class.Return)
                {
                    statementNode.Children.Add(ReturnStat());
                    return statementNode;
                }
                else if (token == Token_Class.Repeat)
                {
                    statementNode.Children.Add(Repeat_Statement());
                    return statementNode;
                }
                else if (token == Token_Class.If)
                {
                    statementNode.Children.Add(If_statement());
                    return statementNode;
                }
                else if (token == Token_Class.Int || token == Token_Class.Float || token == Token_Class.String)
                {

                    statementNode.Children.Add(DeclarationStmt());
                    return statementNode;
                }
                else if (token == Token_Class.Identifier)
                {
                    if (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == Token_Class.AssignOp)
                        statementNode.Children.Add(AssignmentStmt());
                    else
                        statementNode.Children.Add(FunctionCall());
                    statementNode.Children.Add(match(Token_Class.SemiColon));
                    return statementNode;
                }
                else
                    return null;
            }

            return statementNode;
        }
        Node Statements()
        {
            Node statementsNode = new Node("Statements");

            while (true)
            {
                Node stat = Statement();
                if (stat == null) break;
                statementsNode.Children.Add(stat);
            }

            return statementsNode;
        }

        //SHOROUQ
        Node Function_Declaration()
        {
            // FuncDecl  → DataType FuncName(FuncParams)

            Node funcDec = new Node("Function_Declaration");
            funcDec.Children.Add(DataType());
            funcDec.Children.Add(Function_Name());
            funcDec.Children.Add(match(Token_Class.OpenParenthesis));
            funcDec.Children.Add(Parameters());
            funcDec.Children.Add(match(Token_Class.CloseParenthesis));

            return funcDec;
        }
        Node Function_Name()
        {
            Node funcName = new Node("Function_Name");
            funcName.Children.Add(match(Token_Class.Identifier));
            return funcName;
        }
        Node Function_body()
        {
            Node funcbody = new Node("Function_body");
            funcbody.Children.Add(match(Token_Class.OpenBrace));
            funcbody.Children.Add(Statement());
            funcbody.Children.Add(ReturnStat());
            funcbody.Children.Add(match(Token_Class.CloseBrace));
            return funcbody;

        }
        Node Parameter()
        {
            Node param = new Node("Parameter");
            param.Children.Add(DataType());
            param.Children.Add(match(Token_Class.Identifier));
            return param;
        }
        Node Parameters()
        {
            Node Params = new Node("Parameters");
            if (InputPointer >= TokenStream.Count) return Params;
            if (TokenStream[InputPointer].token_type == Token_Class.CloseParenthesis)
                return Params; // empty parameters

            Params.Children.Add(Parameter());

            while (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                Params.Children.Add(match(Token_Class.Comma));
                Params.Children.Add(Parameter());
            }

            return Params;
        }

        Node Parameters_rec()
        {
            Node paramsNode = new Node("Parameters");
            paramsNode.Children.Add(match(Token_Class.Comma));
            paramsNode.Children.Add(Parameters());
            return paramsNode;

        }
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
