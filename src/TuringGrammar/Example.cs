using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Ast;

namespace TSharpCompiler
{
    [Language("Example", "", "showcase problem with not=")]
    public class Example : Irony.Parsing.Grammar
    {
        public Example()
        {
            //1. Terminals
            Terminal num = new NumberLiteral("number");
            MarkReservedWords("not=");

            //2. Non-Terminals
            var Expr = new NonTerminal("expr");
            var BinOp = new NonTerminal("binOp", "operator");
            var unOp = new NonTerminal("unOp","operator");
            var BinExpr = new NonTerminal("binExpr");
            var unExpr = new NonTerminal("unExpr");
            var exitLoop = new NonTerminal("exitLoop");
            var program = new NonTerminal("program");

            //3. BNF rules
            
            Expr.Rule = num  | BinExpr | unExpr | "(" + Expr + ")";
            BinOp.Rule = ToTerm("=") | "!=" | "not=";
            BinOp.Precedence = 20;
            unOp.Rule = ToTerm("not") | "!";
            unOp.Precedence = 10;
            exitLoop.Rule = ToTerm("exit") + "when" + Expr;
            BinExpr.Rule = Expr + BinOp + Expr;
            unExpr.Rule = unOp + Expr;

            program.Rule = Expr + program | exitLoop + program | Empty;
            this.Root = program;

            //4. Set operator precendence and associativity
            RegisterOperators(20, Associativity.Left, "=", "not=", "!=");
            RegisterOperators(10, Associativity.Left, "not","!");


            //5. Register Parenthesis as punctuation symbols so they will not appear in the syntax tree
            MarkReservedWords("not");
            MarkPunctuation("(", ")", ",");
            RegisterBracePair("(", ")");
            MarkTransient(Expr, BinOp, unOp);

            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF | LanguageFlags.CreateAst;
        }
    }
}
