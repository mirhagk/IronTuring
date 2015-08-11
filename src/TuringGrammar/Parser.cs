using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Ast;

namespace TSharpCompiler
{
    [Language("Turing", "0.1", "A .NET implementation of Turing that compiles to CIL")]
    public class TuringGrammar : Irony.Parsing.Grammar
    {
        public TuringGrammar()
        {
            //Expr -> n | v | Expr BinOp Expr | UnOP Expr | ( Expr )
            //BinOp -> + | - | * | / | **
            //UnOp -> -
            //ExprLine -> Expr EOF
            
            //1. Terminals
            Terminal num = new NumberLiteral("number");
            Terminal iden = new IdentifierTerminal("identifier");
            Terminal stringLiteral = TerminalFactory.CreateCSharpString("stringLiteral");
            CommentTerminal SingleLineComment = new CommentTerminal("SingleLineComment", "%", "\r", "\n", "\u2085", "\u2028", "\u2029");
            CommentTerminal DelimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(DelimitedComment);

            //2. Non-Terminals
            var Expr = new NonTerminal("expr");
            var BinOp = new NonTerminal("binOp", "operator");
            var BinExpr = new NonTerminal("binExpr");
            var unOp = new NonTerminal("unOp");
            var unExpr = new NonTerminal("unExpr");
            var ParExpr = new NonTerminal("parExpr");
            var Statement = new NonTerminal("statement");
            var Program = new NonTerminal("program");
            var setEqual = new NonTerminal("setEqual");

            var varType = new NonTerminal("varType");
            var variableDeclaration = new NonTerminal("variableDeclaration");
            var varOrConst = new NonTerminal("varOrConst");
            var idenList = new NonTerminal("identifierList");
            var assignment = new NonTerminal("assignment");
            var typeSpecifier = new NonTerminal("typeSpecifier");

            var ifBlock = new NonTerminal("ifBlock");
            var elseIfBlock = new NonTerminal("elseIfBlock");
            var optElseBlock = new NonTerminal("optElseBlock");
            
            var functionCall = new NonTerminal("functionCall");
            var optArgs = new NonTerminal("optArgs");
            var args = new NonTerminal("args");
            var functionDefinition = new NonTerminal("functionDefinition");
            var optParams = new NonTerminal("optParams");
            var parameters = new NonTerminal("parameters");
            var parameter = new NonTerminal("parameter");

            var io = new NonTerminal("io");
            var optSameLine = new NonTerminal("optionalSameLine");

            var loop = new NonTerminal("loop");
            var forLoop = new NonTerminal("forLoop");
            var exitLoop = new NonTerminal("exitLoop");

            var and = new NonTerminal("and");
            var or = new NonTerminal("or");
            var not = new NonTerminal("not");
            var result = new NonTerminal("result");
            var range = new NonTerminal("range");
            var boolean = new NonTerminal("boolean");
            var ioArgs = new NonTerminal("ioArgs");
            var newer = new NonTerminal("new");

            //3. BNF rules
            varType.Rule = ToTerm("int") | "nat" | "string" | "real" | "boolean" | "array" + range + "of" + varType | ToTerm("flexible") + "array" + range + "of" + varType |
                "int1" | "int2" | "int4" | "real1" | "real2" | "real4" | "nat1" | "nat2" | "nat4" | ToTerm("char") + "(" + num + ")" | ToTerm("string") + "(" + num + ")" | iden;
            range.Rule = num + ".." + num;
            setEqual.Rule = ToTerm(":=") + Expr;
            typeSpecifier.Rule = ToTerm(":") + varType;
            Expr.Rule = num | iden | BinExpr | ParExpr | stringLiteral | unExpr | functionCall | boolean;
            BinOp.Rule = ToTerm("-") | "*" | "/" | "**" | "+" | "div"|"mod" | and | or | "=" | ">" | "<" | ">=" | "<=";
            unOp.Rule = not | "-";
            BinExpr.Rule = Expr + BinOp + Expr;
            unExpr.Rule = unOp + Expr;
            ParExpr.Rule = "(" + Expr + ")";
            boolean.Rule = ToTerm("true") | "false";

            assignment.Rule = iden + setEqual;

            optArgs.Rule = args | Empty;
            //args.Rule = MakeStarRule(Expr, ToTerm(","));
            args.Rule = Expr + "," + args | Expr;
            functionCall.Rule = iden + "(" + optArgs + ")";

            optSameLine.Rule = ToTerm("..") | Empty;
            io.Rule = ToTerm("put") + args + optSameLine | ToTerm("get") + args | ToTerm("put") + ":" + args + optSameLine | ToTerm("get") + ":" + args | ToTerm("open") + ":" + iden +","+ Expr +","+ ioArgs;
            ioArgs.Rule = ToTerm("get") | "put" | "write" | "read" | "seek" | "tell" | ioArgs + "," + ioArgs;

            newer.Rule = ToTerm("new") + iden + "," + Expr;

            optParams.Rule = ToTerm("(") + parameters + ")" | ToTerm("(") + ")" | Empty;
            parameters.Rule = parameter + "," + parameters | parameter;
            parameter.Rule = idenList + typeSpecifier | "var" + idenList + typeSpecifier;
            functionDefinition.Rule = "function" + iden + optParams + typeSpecifier + Program + "end" + iden
                                    | "fcn" + iden + optParams + typeSpecifier + Program + "end" + iden
                                    | "procedure" + iden + optParams + Program + "end" + iden
                                    | "proc" + iden + optParams + Program + "end" + iden;

            ifBlock.Rule = ToTerm("if") + Expr + ToTerm("then") + Program + elseIfBlock + optElseBlock + ToTerm("end") + "if";
            elseIfBlock.Rule = ToTerm("elsif") + Expr + ToTerm("then") + Program + elseIfBlock | Empty;
            optElseBlock.Rule = ToTerm("else") + Program | Empty;

            idenList.Rule = iden + "," + idenList | iden;
            varOrConst.Rule = ToTerm("var") | "const";
            variableDeclaration.Rule = varOrConst + idenList | varOrConst + idenList + setEqual | varOrConst + idenList + typeSpecifier | varOrConst + idenList + typeSpecifier + setEqual;

            loop.Rule = "loop" + Program + "end" + "loop";
            forLoop.Rule = "for" + iden + ":" + range + Program + "end" + "for" |
                            "for" + iden + ":" + iden + Program + "end" + "for";
            exitLoop.Rule = ToTerm("exit") | ToTerm("exit") + "when" + Expr;

            and.Rule = ToTerm("and") | "&";
            or.Rule = ToTerm("or") | "|";
            not.Rule = ToTerm("not") | "~" | "!";
            result.Rule = "result" + Expr | "return" + Expr;
            //memberCall.Rule = iden + "." + functionCall | iden + "." + iden | iden + "." + memberCall;

            Statement.Rule = Expr | variableDeclaration | ifBlock | functionCall | functionDefinition | io | assignment | result | loop | forLoop | newer | exitLoop;
            Program.Rule = Statement + Program |Empty;
            this.Root = Program;

            //4. Set operator precendence and associativity
            RegisterOperators(10, Associativity.Left, "+", "-");
            RegisterOperators(20, Associativity.Left, "*", "/", "div", "mod");
            RegisterOperators(30, Associativity.Right, "**");
            

            //5. Register Parenthesis as punctuation symbols so they will not appear in the syntax tree
            MarkPunctuation("(", ")", ",");
            RegisterBracePair("(", ")");
            MarkTransient(Expr, BinOp, ParExpr, optArgs);

            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF; 
        }
    }
}
