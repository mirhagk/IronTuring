using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.IO;

using System.Reflection.Emit;
using System.Reflection;

namespace IronTuring
{
    class Program
    {
        public delegate void Test1();
        static void Main(string[] args)
        {
            var rootNode = getRoot(System.IO.File.ReadAllText("test1.t"), new TSharpCompiler.TuringGrammarBroken());
            if (rootNode == null)
                Console.WriteLine("Parsing failed");
            else
            {
                var file = new StreamWriter("parseTree.txt");
                dispTree(rootNode, 0, file);
                file.Close();
                //Console.WriteLine(Compile(rootNode));
                CodeGen codeGen = new CodeGen(rootNode, "test.exe");
                Console.WriteLine("done");
            }

            /* This can be interesting, it allows to embed the assembly within the existing assembly, useful for an editor
            DynamicMethod meth = new DynamicMethod("", null, null);
            ILGenerator il = meth.GetILGenerator();
            il.EmitWriteLine("Hello, World!");
            
            il.Emit(OpCodes.Ret);

            Test1 t1 = (Test1)meth.CreateDelegate(typeof(Test1));
            t1();
            */
            Console.ReadKey();
        }

        public static ParseTreeNode getRoot(string sourceCode, Grammar grammar)
        {

            LanguageData language = new LanguageData(grammar);

            Parser parser = new Parser(language);

            ParseTree parseTree = parser.Parse(sourceCode);

            ParseTreeNode root = parseTree.Root;

            return root;
        }
        public static void dispTree(ParseTreeNode node, int level, TextWriter output)
        {

            for (int i = 0; i < level; i++)

                output.Write("  ");

            output.WriteLine(node);



            foreach (ParseTreeNode child in node.ChildNodes)

                dispTree(child, level + 1, output);

        }
        public static string getTypeOfNode(ParseTreeNode node, Dictionary<string, ParseTreeNode> symbolTable)
        {
            if (node == null)
            {
                return "unknown";
            }
            if (node.Term.Name == "varType")//regular variable tpye
            {
                if (node.ChildNodes[0].Term.Name == "array")//need to handle arrays seperately
                {
                    return String.Format("array {0} to {1} of {2}", node.ChildNodes[1].Token.ValueString, node.ChildNodes[3].Token.ValueString, getTypeOfNode(node.ChildNodes[5], symbolTable));
                }
                else
                {
                    switch (node.ChildNodes[0].Term.Name)
                    {
                        case "int":
                            return "int32";
                        default:
                            return node.ChildNodes[0].Term.Name;
                    }
                }
            }
            if (node.Term.Name == "setEqual")
            {
                return EvalGetType(node.ChildNodes[1]);
            }
            return "";
        }
        public static string EvalGetType(ParseTreeNode node)
        {
            return "unknown";
        }
        public static string Compile(ParseTreeNode node)
        {
            string output = @".assembly extern mscorlib {}
.assembly Program {}

.class public Program
{
.method public static void Main(string[] args)
{
.entrypoint
.maxstack 100"; 
            
            Dictionary<string, ParseTreeNode> symbolTable = new Dictionary<string, ParseTreeNode>();
            string innerOutput = (Compile(node, symbolTable));
            foreach (var symbol in symbolTable)
            {
                Console.WriteLine("{0}: {1}", symbol.Key, getTypeOfNode(symbol.Value, symbolTable));
            }
            var variablesSymbolTable=symbolTable.Where(x => getTypeOfNode(x.Value, symbolTable) != "function");
            output += "\n.locals init (";
            foreach (var symbol in variablesSymbolTable)
            {
                output += getTypeOfNode(symbol.Value, symbolTable) + " " + symbol.Key+",";
            }
            output += "\b)\n";
            output += innerOutput;
            output += "ret}";
            //function additions go here
            output += "}";
            return output;
        }
        public static string Compile(ParseTreeNode node,Dictionary<string,ParseTreeNode> symbolTable)
        {
            string output = "";
            switch (node.Term.Name)
            {
                case "program":
                    if (node.ChildNodes.Count > 0)
                    {
                        output += Compile(node.ChildNodes[0], symbolTable) + "\n";
                        output += Compile(node.ChildNodes[1], symbolTable) + "\n";
                    }
                    break;
                case "statement":
                    output += Compile(node.ChildNodes[0], symbolTable) + "\n";
                    break;
                case "variableDeclaration":
                    //symbolTable.Add(node.ChildNodes[1].Token.ValueString,null);//identifier name
                    if (node.ChildNodes[2].Term.Name == "typeSpecifier")//has a type specifier
                    {
                        symbolTable[node.ChildNodes[1].Token.ValueString] = node.ChildNodes[2].ChildNodes[1];
                    }
                    else//just have an expression. Grab the type from the expression
                    {
                        symbolTable[node.ChildNodes[1].Token.ValueString] = node.ChildNodes[2];
                    }
                    break;
                default:
                    output += node.Term.Name + ",";
                    break;

            }

            return output;
        }

 
    }
}
