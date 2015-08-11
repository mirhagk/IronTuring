using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Ast;

namespace TSharpCompiler
{
    [Language("Turing", "0.3 Based off of spec", "A .NET implementation of Turing that compiles to CIL")]
    public class SpecParser: Irony.Parsing.Grammar
    {
        public SpecParser()
        {
            //Expr -> n | v | Expr BinOp Expr | UnOP Expr | ( Expr )
            //BinOp -> + | - | * | / | **
            //UnOp -> -
            //ExprLine -> Expr EOF

            //1. Terminals
            //Terminal num = new NumberLiteral("number");
            Terminal id = new IdentifierTerminal("id");
            //Terminal stringLiteral = TerminalFactory.CreateCSharpString("stringLiteral");
            CommentTerminal SingleLineComment = new CommentTerminal("SingleLineComment", "%", "\r", "\n", "\u2085", "\u2028", "\u2029");
            CommentTerminal DelimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(DelimitedComment);

            //2. Non-Terminals
            //Programs and declarations
            var program = new NonTerminal("program");
            var declarationOrStatementsInMainProgram = new NonTerminal("declarationOrStatementsInMainProgram");
            var declaration = new NonTerminal("declaration");
            var constantDeclaration = new NonTerminal("constantDeclaration");
            var initializingValue = new NonTerminal("initializingValue");
            var variableDeclaration = new NonTerminal("variableDeclaration");
           
            //Types
            var typeDeclaration = new NonTerminal("typeDeclaration");
            var typeSpec = new NonTerminal("typeSpec");
            var standardType = new NonTerminal("standardType");
            var subrangeType = new NonTerminal("subrangeType");
            var arrayType = new NonTerminal("arrayType");
            var indexType = new NonTerminal("indexType");
            var recordType = new NonTerminal("recordType");
            var namedType = new NonTerminal("namedType");

            //subprograms
            var subprogramDeclaration = new NonTerminal("subprogramDeclaration");
            var subprogramHeader = new NonTerminal("subprogramHeader");
            var parameterDeclaration = new NonTerminal("parameterDeclaration");
            var parameterType = new NonTerminal("parameterType");
            var subprogramBody = new NonTerminal("subprogramBody");

            //statements and input/output
            var declarationsAndStatements = new NonTerminal("declarationsAndStatements");
            var declarationOrStatement = new NonTerminal("declarationOrStatement");
            var statement = new NonTerminal("statement");
            var procedureCall = new NonTerminal("procedureCall");
            var ifStatement = new NonTerminal("ifStatement");
            var loopStatement = new NonTerminal("loopStatement");
            var caseStatement = new NonTerminal("caseStatement");
            var forStatement = new NonTerminal("forStatement");
            var putStatement = new NonTerminal("putStatement");
            var putItem = new NonTerminal("putItem");
            var getStatement = new NonTerminal("getStatement");
            var getItem = new NonTerminal("getItem");
            var openStatement = new NonTerminal("openStatement");
            var capability = new NonTerminal("capability");
            var closeStatement = new NonTerminal("closeStatement");
            var streamNumber = new NonTerminal("streamNumber");
            var widthExpn = new NonTerminal("widthExpn");
            var fractionWidth = new NonTerminal("fractionWidth");
            var exponentWidth = new NonTerminal("exponentWidth");
            var fileNumber = new NonTerminal("fileNumber");
            var fileNumberVariable = new NonTerminal("fileNumberVariable");//this was not defined in the spec. The behaviour should conform to how the language works, but it's not guaranteed
            var fileName = new NonTerminal("fileName"); //above is a variable declared of int type, and this is either string constant or variable

            //References and Expressions
            var variableReference = new NonTerminal("variableReference");
            var reference = new NonTerminal("reference");
            var componentSelector = new NonTerminal("componentSelector");
            var booleanExpr = new NonTerminal("booleanExpr");
            var compileTimeExpn = new NonTerminal("compileTimeExpn");
            var expn = new NonTerminal("expn");
            var explicitConstant = new NonTerminal("explicitConstant");
            var infixOperator = new NonTerminal("infixOperator");
            var prefixOperator = new NonTerminal("prefixOperator");
            var substring = new NonTerminal("substring");
            var substringPosition = new NonTerminal("substringPosition");

            //Explicit Constants
            Terminal explicitStringConstant = TerminalFactory.CreateCSharpString("explicitStringConstant");//new StringLiteral("explicitStringConstant");//new NonTerminal("explicitStringConstant");
            Terminal explicitUnsignedRealConstant = new NumberLiteral("explicitUnsignedRealConstant", NumberOptions.Default);//new NonTerminal("explicitUnsignedRealConstant");
            Terminal explicitUnsignedIntegerConstant = new NumberLiteral("explicitUnsignedIntegerConstant", NumberOptions.IntOnly | NumberOptions.NoDotAfterInt);//new NonTerminal("explicitUnsignedIntegerConstant");
            


            //3. BNF Rules
            //Programs and declarations
            program.Rule = MakeStarRule(program, declarationOrStatementsInMainProgram);
            declarationOrStatementsInMainProgram.Rule = declaration | statement | subprogramDeclaration;
            declaration.Rule = constantDeclaration | variableDeclaration | typeDeclaration;
            constantDeclaration.Rule = ToTerm("const") + id + ":=" + expn 
                                       | "const" + id + ":" + typeSpec + ":=" + initializingValue;
            initializingValue.Rule = expn | "init" + "(" + MakePlusRule(initializingValue, ToTerm(","), initializingValue) + ")";
            variableDeclaration.Rule = ToTerm("var") + MakePlusRule(variableDeclaration, ToTerm(","), id) + ":=" + expn
                                       | "var" + MakePlusRule(variableDeclaration, ToTerm(","), id) + ":" + typeSpec + (":=" + initializingValue | Empty);

            //Types
            typeDeclaration.Rule = ToTerm("type") + id + ":" + typeSpec;
            typeSpec.Rule = standardType | subrangeType | arrayType | recordType | namedType;
            standardType.Rule = ToTerm("int") | "real" | "boolean" | "string" + ("(" + compileTimeExpn + ")" | Empty);
            subrangeType.Rule = compileTimeExpn + ".." + expn;
            arrayType.Rule = ToTerm("array") + MakePlusRule(arrayType, ToTerm(","), indexType) + "of" + typeSpec;
            indexType.Rule = subrangeType | namedType;
            recordType.Rule = ToTerm("record") + MakePlusRule(recordType, MakePlusRule(recordType, ToTerm(","), id) + ":" + typeSpec) + "end" + "record";
            namedType.Rule = id;

            //Subprograms
            subprogramDeclaration.Rule = subprogramHeader + subprogramBody;
            subprogramHeader.Rule = ToTerm("procedure") + id + ("(" + MakePlusRule(subprogramHeader, ToTerm(","), parameterDeclaration) + ")" | Empty)
                                  | "function" + id + ("(" + MakePlusRule(subprogramHeader, ToTerm(","), parameterDeclaration) + ")" | Empty) + ":" + typeSpec;
            parameterDeclaration.Rule = (ToTerm("var") | Empty) + MakePlusRule(parameterDeclaration, ToTerm(","), id) + ":" + parameterType;
            parameterType.Rule = typeSpec | "string" + "(" + "*" + ")"
                               | "array" + MakePlusRule(parameterType, ToTerm(","), compileTimeExpn + ".." + "*") + "of" + typeSpec
                               | "array" + MakePlusRule(parameterType, ToTerm(","), compileTimeExpn + ".." + "*") + "of" + "string" + "(" + "*" + ")";
            subprogramBody.Rule = declarationsAndStatements + "end" + id;

            //Statements and Input/Output
            declarationsAndStatements.Rule = MakePlusRule(declarationsAndStatements, declarationOrStatement);
            declarationOrStatement.Rule = declaration | statement;
            statement.Rule = variableReference + ":=" + expn
                            | procedureCall
                            | "assert" + booleanExpr
                            | "result" + expn
                            | ifStatement
                            | loopStatement
                            | "exit" + ("when" + booleanExpr | Empty)
                            | caseStatement
                            | forStatement
                            | putStatement
                            | getStatement
                            | openStatement
                            | closeStatement;
            procedureCall.Rule = reference;
            ifStatement.Rule = ToTerm("if") + booleanExpr + "then" + declarationsAndStatements + MakeStarRule(ifStatement, ToTerm("elsif") + booleanExpr + "then" + declarationsAndStatements) + ("else" + declarationsAndStatements | Empty) + "end" + "if";
            loopStatement.Rule = ToTerm("loop") + declarationsAndStatements + "end" + "loop";
            caseStatement.Rule = ToTerm("case") + expn + "of" + MakePlusRule(caseStatement, ToTerm("label") + MakePlusRule(caseStatement, ToTerm(","), compileTimeExpn) + ":" + declarationsAndStatements) + ("label" + ":" + declarationsAndStatements | Empty) + "end" + "case";
            forStatement.Rule = ToTerm("for") + (id | Empty) + ":" + expn + ".." + expn + ("by" + expn | Empty) + declarationsAndStatements + "end" + "for"
                                | "for" + "decreasing" + (id | Empty) + ":" + expn + ".." + expn + ("by" + expn | Empty) + declarationsAndStatements + "end" + "for";
            putStatement.Rule = ToTerm("put") + (":" + streamNumber + "," | Empty) + MakePlusRule(putStatement, ToTerm(","), putItem) + (".." | Empty);
            putItem.Rule = expn + (":" + widthExpn + (":" + fractionWidth + (":" + exponentWidth | Empty) | Empty) | Empty) | "skip";
            getStatement.Rule = ToTerm("get") + (":" + streamNumber + "," | Empty) + MakePlusRule(getStatement, ToTerm(","), getItem);
            getItem.Rule = variableReference | "skip" | variableReference + ":" + "*" | variableReference + ":" + widthExpn;
            openStatement.Rule = ToTerm("open") + ":" + fileNumberVariable + "," + fileName + "," + MakePlusRule(openStatement, ToTerm(","), capability);
            capability.Rule = ToTerm("put") | "get";
            closeStatement.Rule = ToTerm("close") + ":" + fileNumber;
            streamNumber.Rule = widthExpn.Rule = fractionWidth.Rule = exponentWidth.Rule = fileNumber.Rule = expn;
            
            //Following are guesses as to the rules, since the spec does not state the rules for them
            fileNumberVariable.Rule = variableReference;
            fileName.Rule = explicitStringConstant | variableReference;
            
            //References and Expressions
            variableReference.Rule = reference;
            reference.Rule = id | reference + componentSelector;
            componentSelector.Rule = "(" + MakePlusRule(componentSelector, ToTerm(","), expn) + ")" | "." + id;
            booleanExpr.Rule = compileTimeExpn.Rule = expn;
            expn.Rule = reference | explicitConstant | substring | expn + infixOperator + expn | prefixOperator + expn | "(" + expn + ")";
            expn.SetFlag(TermFlags.InheritPrecedence);
            explicitConstant.Rule = explicitUnsignedIntegerConstant | explicitUnsignedRealConstant | explicitStringConstant | "true" | "false";
            infixOperator.Rule = ToTerm("+") | "-" | "*" | "/" | "div" | "mod" | "**" | "<" | ">" | "=" | "<=" | ">=" | "not=" | "and" | "or";
            //prefixOperator.Precedence = 70;
            prefixOperator.Rule = ToTerm("+") | "-" | "not";
            substring.Rule = reference + "(" + substringPosition + (".." + substringPosition | Empty) + ")";
            substringPosition.Rule = expn | "*" + ("-" + expn | Empty);

            this.Root = program;

            //4. Set operator precendence and associativity
            RegisterOperators(80, Associativity.Left, "**");//this is VERY odd, but Turing simplifies associativity by saying it's all left associative
            RegisterOperators(60, "*","/","div","mod");
            RegisterOperators(50, "+", "-");
            RegisterOperators(40, "<", ">", "=", "<=", ">=", "not=");
            RegisterOperators(30, "not");
            RegisterOperators(20, "and");
            RegisterOperators(10, "or");

            //5. Register Parenthesis as punctuation symbols so they will not appear in the syntax tree
            MarkPunctuation("(", ")", ",");
            RegisterBracePair("(", ")");
            //MarkTransient(Expr, BinOp, ParExpr);

            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF;
        }
    }
}
