using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace TuringGrammar
{
    [Language("Turing", "0.6", "A .NET implementation of Turing that compiles to CIL")]
    class OOPGrammar : Grammar
    {
        public OOPGrammar()
        {
            var identifier = new IdentifierTerminal("identifier");
            var numberLiteral = new NumberLiteral("numberLiteral");
            var stringLiteral = TerminalFactory.CreateCSharpString("stringLiteral");
            var charLiteral = new StringLiteral("charLiteral", "'", StringOptions.IsChar);
            var SingleLineComment = new CommentTerminal("SingleLineComment", "%", "\r", "\n", "\u2085", "\u2028", "\u2029");
            var DelimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(DelimitedComment);

            var fullIdentifier = new NonTerminal("fullIdentifier");
            var boolean = new NonTerminal("boolean");
            var range = new NonTerminal("range");
            var varType = new NonTerminal("varType");
            var variableDeclaration = new NonTerminal("variableDeclaration");
            var varOrConst = new NonTerminal("varOrConst");
            var idenList = new NonTerminal("identifierList");
            var typeSpecifier = new NonTerminal("typeSpecifier");
            var result = new NonTerminal("result");
            var recordList = new NonTerminal("recordList");
            var type = new NonTerminal("type");

            var assignment = new NonTerminal("assignment");
            var setEqual = new NonTerminal("setEqual");
            var assignmentOp = new NonTerminal("assignmentOp");
            
            var expr = new NonTerminal("expr");
            var binOp = new NonTerminal("binOp", "operator");
            var binExpr = new NonTerminal("binExpr");
            var unOp = new NonTerminal("unOp");
            var unExpr = new NonTerminal("unExpr");
            var parExpr = new NonTerminal("parExpr"); 
            var and = new NonTerminal("and");
            var or = new NonTerminal("or");
            var not = new NonTerminal("not");

            var functionCall = new NonTerminal("functionCall");
            var args = new NonTerminal("args");

            var loop = new NonTerminal("loop");
            var forLoop = new NonTerminal("forLoop");
            var optDecreasing = new NonTerminal("optDecreasing");
            var optIncrementBy = new NonTerminal("optIncrementBy");
            var exitLoop = new NonTerminal("exitLoop");

            var ifBlock = new NonTerminal("ifBlock");
            var elseIfBlock = new NonTerminal("elseIfBlock");
            var optElseBlock = new NonTerminal("optElseBlock");
            var caseBlock = new NonTerminal("caseBlock");
            var labelBlock = new NonTerminal("labelBlock");

            var functionDefinition = new NonTerminal("functionDefinition");
            var optParams = new NonTerminal("optParams");
            var parameters = new NonTerminal("parameters");
            var parameter = new NonTerminal("parameter");
            var functionHeader = new NonTerminal("functionHeader");
            var overrideName = new NonTerminal("overrideName");
            var externalOverrideName = new NonTerminal("externalOverrideName");
            var externalDeclaration = new NonTerminal("externalDeclaration");

            var statement = new NonTerminal("statement");
            var program = new NonTerminal("program");

            var importOrInclude = new NonTerminal("importOrInclude");
            var importSection = new NonTerminal("importSection");
            var import = new NonTerminal("import");
            var include = new NonTerminal("include");
            var unit = new NonTerminal("unit");
            var uncheckedStatement = new NonTerminal("unchecked");

            fullIdentifier.Rule = identifier | identifier + "." + fullIdentifier| functionCall + "." + fullIdentifier;
            typeSpecifier.Rule = ToTerm(":") + varType; 
            boolean.Rule = ToTerm("true") | "false";
            varType.Rule = ToTerm("int") | "nat" | "string" | "real" | "boolean" | "array" + range + "of" + varType | ToTerm("flexible") + "array" + range + "of" + varType | "record" + recordList + "end" + "record" |
                "int1" | "int2" | "int4" | "real1" | "real2" | "real4" | "nat1" | "nat2" | "nat4" | ToTerm("char") + "(" + numberLiteral + ")" | ToTerm("string") + "(" + numberLiteral + ")" | fullIdentifier;
            range.Rule = expr + ".." + expr | "char" | fullIdentifier;
            idenList.Rule =  identifier + "," + idenList |  identifier;
            varOrConst.Rule = ToTerm("var") | "const";
            variableDeclaration.Rule = varOrConst + idenList | varOrConst + idenList + setEqual | varOrConst + idenList + typeSpecifier | varOrConst + idenList + typeSpecifier + setEqual;

            result.Rule = "result" + expr | "return" + expr;
            recordList.Rule = idenList + typeSpecifier + recordList | Empty;
            type.Rule = "type" + identifier + typeSpecifier;

            assignment.Rule = fullIdentifier + setEqual | functionCall + setEqual;
            setEqual.Rule = assignmentOp + expr;
            assignmentOp.Rule = ToTerm(":=") | "+=" | "-=" | "*=" | "/=" | "div="
                 | "mod=" | "rem=" | "shl=" | "shr=" | "xor=";

            binOp.Rule = ToTerm("-") | "*" | "/" | "**" | "+" | "div" | "mod" | and | or | "=" | ">" | "<" | ">=" | "<=" | "~=" | "not=";
            binOp.Precedence = 1;
            unOp.Rule = not | "-";
            unOp.Precedence = 2;

            binExpr.Rule = expr + PreferShiftHere() + binOp + expr;
            unExpr.Rule = unOp + expr;
            parExpr.Rule = "(" + expr + ")";
            and.Rule = ToTerm("and") | "&";
            or.Rule = ToTerm("or") | "|";
            not.Rule = ToTerm("not") | "~" | "!";

            expr.Rule = numberLiteral | identifier | stringLiteral | charLiteral | binExpr | unExpr | parExpr | functionCall;
            //expr.Rule = numberLiteral | identifier | BinExpr | ParExpr | stringLiteral | charLiteral | unExpr | functionCall | memberCall | boolean | initExpr;

            args.Rule = MakeStarRule(args, ToTerm(","), (expr | identifier + ":=" + expr));
            functionCall.Rule = fullIdentifier + "(" + args + ")";

            loop.Rule = "loop" + program + "end" + "loop";
            optDecreasing.Rule = "decreasing" | Empty;
            optIncrementBy.Rule = "by" + expr | Empty;
            forLoop.Rule = "for" + optDecreasing + identifier + ":" + range + optIncrementBy + program + "end" + "for";
            exitLoop.Rule = ToTerm("exit") | ToTerm("exit") + "when" + expr;

            ifBlock.Rule = ToTerm("if") + expr + ToTerm("then") + program + elseIfBlock + optElseBlock + ToTerm("end") + "if";
            elseIfBlock.Rule = ToTerm("elsif") + expr + ToTerm("then") + program + elseIfBlock | Empty;
            optElseBlock.Rule = ToTerm("else") + program | Empty;
            caseBlock.Rule = ToTerm("case") + fullIdentifier + "of" + labelBlock + "end case";
            labelBlock.Rule = ToTerm("label") + expr + ":" + program + labelBlock | ToTerm("label") + ":" + labelBlock | Empty;

            overrideName.Rule = stringLiteral;
            externalOverrideName.Rule = overrideName | Empty;
            externalDeclaration.Rule = "external" + externalOverrideName + functionHeader;
            optParams.Rule = ToTerm("(") + parameters + ")" | ToTerm("(") + ")" | Empty;
            parameters.Rule = parameter + "," + parameters | parameter;
            parameter.Rule = idenList + typeSpecifier | "var" + idenList + typeSpecifier;
            functionHeader.Rule = "function" + identifier + optParams + typeSpecifier
                                | "fcn" + identifier + optParams + typeSpecifier
                                | "procedure" + identifier + optParams
                                | "proc" + identifier + optParams;
            functionDefinition.Rule = functionHeader + program + "end" + identifier;

            importOrInclude.Rule = import | include;
            import.Rule = "import" + fullIdentifier | "import" + fullIdentifier + "in" + stringLiteral;
            include.Rule = "include" + stringLiteral;
            importSection.Rule = MakeStarRule(importSection, importOrInclude);
            unit.Rule = "unit" + importSection + program | importSection + program;

            uncheckedStatement.Rule = ToTerm("unchecked");

            statement.Rule = assignment | functionDefinition | variableDeclaration | functionCall | loop | forLoop | exitLoop | ifBlock | caseBlock | type | result | uncheckedStatement;
            //Statement.Rule = functionCall | memberCall | iden | variableDeclaration | ifBlock | caseBlock | functionCall | functionDefinition | io | assignment | result | loop | forLoop | type | newer | exitLoop | externalDeclaration;
            program.Rule = statement + program | Empty;
            this.Root = unit;

        }
    }
}
