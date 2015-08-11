using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Ast;

namespace TSharpCompiler
{
    [Language("Turing", "0.5 Broken", "A .NET implementation of Turing that compiles to CIL")]
    public class TuringGrammarBroken2 : Irony.Parsing.Grammar
    {
        public TuringGrammarBroken2()
        {
            //Expr -> n | v | Expr BinOp Expr | UnOP Expr | ( Expr )
            //BinOp -> + | - | * | / | **
            //UnOp -> -
            //ExprLine -> Expr EOF
            //1. Terminals
            Terminal num = new NumberLiteral("number");
            Terminal iden = new IdentifierTerminal("identifier");
            Terminal stringLiteral = TerminalFactory.CreateCSharpString("stringLiteral");
            Terminal charLiteral = new StringLiteral("charLiteral", "'", StringOptions.IsChar);
            CommentTerminal SingleLineComment = new CommentTerminal("SingleLineComment", "%", "\r", "\n", "\u2085", "\u2028", "\u2029");
            CommentTerminal DelimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(DelimitedComment);


            //var expandedIdentifier = new RegexBasedTerminal("expandedIdentifer",@"[a-zA-Z][a-zA-Z\.]*");

            //2. Non-Terminals
            //var expandedIdentifier = new NonTerminal("expandedIndentifier");
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

            var caseBlock = new NonTerminal("caseBlock");
            var labelBlock = new NonTerminal("labelBlock");
            
            var functionCall = new NonTerminal("functionCall");
            var args = new NonTerminal("args");
            var functionDefinition = new NonTerminal("functionDefinition");
            var optParams = new NonTerminal("optParams");
            var parameters = new NonTerminal("parameters");
            var parameter = new NonTerminal("parameter");

            var io = new NonTerminal("io");
            var optSameLine = new NonTerminal("optionalSameLine");
            var putItem = new NonTerminal("putItem");
            var widthExpn = new NonTerminal("widthExpn");
            var fractionWidth = new NonTerminal("fractionWidth");
            var exponentWidth = new NonTerminal("exponentWidth");
            var streamNumber = new NonTerminal("streamNumber");
            var putItems = new NonTerminal("putItems");

            var loop = new NonTerminal("loop");
            var forLoop = new NonTerminal("forLoop");
            var optDecreasing = new NonTerminal("optDecreasing");
            var optIncrementBy = new NonTerminal("optIncrementBy");
            var exitLoop = new NonTerminal("exitLoop");
            var initExpr = new NonTerminal("initExpr");

            var and = new NonTerminal("and");
            var or = new NonTerminal("or");
            var not = new NonTerminal("not");
            var result = new NonTerminal("result");
            var recordList = new NonTerminal("recordList");
            var type = new NonTerminal("type");
            var memberCall = new NonTerminal("memberCall");
            var range = new NonTerminal("range");
            var boolean = new NonTerminal("boolean");
            var ioArgs = new NonTerminal("ioArgs");
            var newer = new NonTerminal("new");
            var assignmentOp = new NonTerminal("assignmentOp");

            var overrideName = new NonTerminal("overrideName");
            var externalOverrideName = new NonTerminal("externalOverrideName");
            var externalDeclaration = new NonTerminal("externalDeclaration");
            var functionHeader = new NonTerminal("functionHeader");

            var importOrInclude = new NonTerminal("importOrInclude");
            var importSection = new NonTerminal("importSection");
            var import = new NonTerminal("import");
            var include = new NonTerminal("include");
            var unit = new NonTerminal("unit");

            //3. BNF rules
            varType.Rule = ToTerm("int") | "nat" | "string" | "real" | "boolean" | "array" + range + "of" + varType | ToTerm("flexible") + "array" + range + "of" + varType | "record" + recordList + "end" + "record" |
                "int1" | "int2" | "int4" | "real1" | "real2" | "real4" | "nat1" | "nat2" | "nat4" | ToTerm("char") + "(" + num + ")" | ToTerm("string") + "(" + num + ")" | iden;
            range.Rule = Expr + ".." + Expr | "char" | iden;
            setEqual.Rule = assignmentOp + Expr;
            assignmentOp.Rule = ToTerm(":=") | "+=" | "-=" | "*=" | "/=" | "div="
                 | "mod=" | "rem=" | "shl=" | "shr=" | "xor=";
            typeSpecifier.Rule = ToTerm(":") + varType;
            Expr.Rule = num | iden | BinExpr | ParExpr | stringLiteral | charLiteral | unExpr | functionCall | memberCall | boolean | initExpr;
            initExpr.Rule = ToTerm("init") + "(" + args + ")";

            BinOp.Rule = ToTerm("-") | "*" | "/" | "**" | "+" | "div" | "mod" | and | or | "=" | ">" | "<" | ">=" | "<=" | "~=" | "not=";
            BinOp.Precedence = 1;
            unOp.Rule = not | "-";
            unOp.Precedence = 2;

            BinExpr.Rule = Expr + PreferShiftHere() + BinOp + Expr;
            unExpr.Rule = unOp + Expr;
            ParExpr.Rule = "(" + Expr + ")";
            boolean.Rule = ToTerm("true") | "false";

            assignment.Rule = iden + setEqual | functionCall + setEqual;

            args.Rule = MakeStarRule(args, ToTerm(","),Expr);
            //args.Rule = Expr + "," + args | Expr;
            functionCall.Rule = memberCall + "(" + args + ")";

            overrideName.Rule = stringLiteral;
            externalOverrideName.Rule = overrideName | Empty;
            externalDeclaration.Rule = "external" + externalOverrideName + functionHeader;
            
            optSameLine.Rule = ToTerm("..") | Empty;
            io.Rule = ToTerm("put") + ":" + streamNumber + "," + putItem + putItems + optSameLine | ToTerm("get") + args
                    | ToTerm("put") + putItem + putItems + optSameLine | ToTerm("get") + ":" + args
                    | ToTerm("open") + ":" + iden + "," + Expr + "," + ioArgs | ToTerm("close") + ":" + iden;
            
            putItem.Rule = Expr | Expr + ":" + widthExpn | Expr + ":" + widthExpn + ":" + fractionWidth
                | Expr + ":" + widthExpn + ":" + fractionWidth + ":" + exponentWidth | "skip";
            putItems.Rule = "," + putItem + putItems | Empty;
            ioArgs.Rule = ToTerm("get") | "put" | "write" | "read" | "seek" | "tell" | ioArgs + PreferShiftHere() + "," + ioArgs;
            streamNumber.Rule = widthExpn.Rule = fractionWidth.Rule = exponentWidth.Rule = Expr;
            
            newer.Rule = ToTerm("new") + iden + "," + Expr;

            optParams.Rule = ToTerm("(") + parameters + ")" | ToTerm("(") + ")" | Empty;
            parameters.Rule = parameter + "," + parameters | parameter;
            parameter.Rule = idenList + typeSpecifier | "var" + idenList + typeSpecifier;
            //functionHeader.Rule = (ToTerm("function") | "fcn") + iden + optParams + typeSpecifier | (ToTerm("procedure") | "proc") + iden + optParams;
            functionHeader.Rule = "function" + iden + optParams + typeSpecifier
                                | "fcn" + iden + optParams + typeSpecifier
                                | "procedure" + iden + optParams
                                | "proc" + iden + optParams;
            functionDefinition.Rule = functionHeader + Program + "end" + iden; //"function" + iden + optParams + typeSpecifier + Program + "end" + iden
                                    //| "fcn" + iden + optParams + typeSpecifier + Program + "end" + iden
                                    //| "procedure" + iden + optParams + Program + "end" + iden
                                    //| "proc" + iden + optParams + Program + "end" + iden;

            ifBlock.Rule = ToTerm("if") + Expr + ToTerm("then") + Program + elseIfBlock + optElseBlock + ToTerm("end") + "if";
            elseIfBlock.Rule = ToTerm("elsif") + Expr + ToTerm("then") + Program + elseIfBlock | Empty;
            optElseBlock.Rule = ToTerm("else") + Program | Empty;

            caseBlock.Rule = ToTerm("case") + iden + "of" + labelBlock +  "end case";
            labelBlock.Rule = ToTerm("label") + Expr + ":" + Program + labelBlock | ToTerm("label") + ":" + labelBlock | Empty;

            idenList.Rule = iden + "," + idenList | iden;
            varOrConst.Rule = ToTerm("var") | "const";
            variableDeclaration.Rule = varOrConst + idenList | varOrConst + idenList + setEqual | varOrConst + idenList + typeSpecifier | varOrConst + idenList + typeSpecifier + setEqual;

            loop.Rule = "loop" + Program + "end" + "loop";
            optDecreasing.Rule = "decreasing" | Empty;
            optIncrementBy.Rule = "by" + Expr | Empty;
            forLoop.Rule = "for" + optDecreasing + iden + ":" + range + optIncrementBy + Program + "end" + "for";
            exitLoop.Rule = ToTerm("exit") | ToTerm("exit") + "when" + Expr;

            and.Rule = ToTerm("and") | "&";
            or.Rule = ToTerm("or") | "|";
            not.Rule = ToTerm("not") | "~" | "!";
            result.Rule = "result" + Expr | "return" + Expr;
            recordList.Rule = iden + typeSpecifier + recordList | Empty;
            type.Rule = "type" + iden + typeSpecifier;
            memberCall.Rule = memberCall + PreferShiftHere() + "." + memberCall;
            //memberCall.Rule = iden + "." + functionCall | iden + "." + iden | iden + "." + memberCall;

            importOrInclude.Rule = import | include;
            import.Rule = "import" + memberCall | "import" + memberCall + "in" + stringLiteral;
            include.Rule = "include" + stringLiteral;
            importSection.Rule = MakeStarRule(importSection, importOrInclude);
            unit.Rule = "unit" + importSection + Program | importSection + Program;

            //expandedIdentifier.Rule = iden | functionCall | memberCall ;
            Statement.Rule = functionCall | memberCall | iden | variableDeclaration | ifBlock | caseBlock | functionCall | functionDefinition | io | assignment | result | loop | forLoop | type | newer | exitLoop | externalDeclaration;
            Program.Rule = Statement + Program | Empty;
            this.Root = unit;

            //4. Set operator precendence and associativity
            RegisterOperators(05, Associativity.Left, "not");
            RegisterOperators(10, Associativity.Left, "=", "not=", ">=", "<=", "<", ">", "!=", "~=");
            RegisterOperators(30, Associativity.Left, "+", "-");
            RegisterOperators(40, Associativity.Left, "*", "/", "div", "mod");
            RegisterOperators(50, Associativity.Right, "**");
            

            //5. Register Parenthesis as punctuation symbols so they will not appear in the syntax tree
            MarkPunctuation("(", ")", ",","external","if","end","then","elsif","else","init", "loop", "for", "by",":","..","import","include","in",".");
            RegisterBracePair("(", ")");
            //MarkTransient(Expr, BinOp, ParExpr, assignmentOp, varOrConst);
            MarkTransient(Expr, BinOp, ParExpr, assignmentOp, varOrConst, overrideName, externalOverrideName,importOrInclude);
            MarkReservedWords("not=","init","include","import");

            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF;//| LanguageFlags.CreateAst;
        }
    }
}
