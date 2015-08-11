using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Ast;

namespace TSharpCompiler
{
    [Language("Turing", "0.4 Based off of spec located on compsci.ca", "A .NET implementation of Turing that compiles to CIL")]
    public class CompsciSpecParser : Irony.Parsing.Grammar
    {
        public CompsciSpecParser()
        {
            //Expr -> n | v | Expr BinOp Expr | UnOP Expr | ( Expr )
            //BinOp -> + | - | * | / | **
            //UnOp -> -
            //ExprLine -> Expr EOF

            //1. Terminals
            //Terminal num = new NumberLiteral("number");
            Terminal id = new IdentifierTerminal("id");
            Terminal explicitStringConstant = TerminalFactory.CreateCSharpString("explicitStringConstant");
            Terminal explicitIntegerConstant = new NumberLiteral("explicitIntegerConstant", NumberOptions.IntOnly);
            Terminal explicitRealConstant = new NumberLiteral("explicitRealConstant", NumberOptions.None);
            Terminal explicitCharConstant = new StringLiteral("Terminal explicitCharConstant", "'", StringOptions.IsChar);
            
            //Terminal stringLiteral = TerminalFactory.CreateCSharpString("stringLiteral");
            CommentTerminal SingleLineComment = new CommentTerminal("SingleLineComment", "%", "\r", "\n", "\u2085", "\u2028", "\u2029");
            CommentTerminal DelimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(DelimitedComment);

            //2. Non-Terminals
            var variableId = new NonTerminal("variableId");
            var variableReference = new NonTerminal("variableReference");
            var constantId = new NonTerminal("constantId");
            var constantReference = new NonTerminal("constantReference");
            var explicitTrueFalseConstant = new NonTerminal("explicitTrueFalseConstant");
            var explicitConstant = new NonTerminal("explicitConstant");
            var infixOperator = new NonTerminal("infixOperator");
            var prefixOperator = new NonTerminal("prefixOperator");
            var leftPosition = new NonTerminal("leftPosition");
            var rightPosition = new NonTerminal("rightPosition");
            var charPosition = new NonTerminal("charPosition");
            var stringReference = new NonTerminal("stringReference");
            var substring = new NonTerminal("substring");
            var functionId = new NonTerminal("functionId");
            var moduleId = new NonTerminal("moduleId");
            var classId = new NonTerminal("classId");
            var pointerId = new NonTerminal("pointerId");
            var functionRefence = new NonTerminal("functionReference");
            var functionCall = new NonTerminal("functionCall");
            var setTypeId = new NonTerminal("setTypeId");
            var membersOfSet = new NonTerminal("membersOfSet");
            var setConstructor = new NonTerminal("setConstructor");
            var enumeratedTypeId = new NonTerminal("enumeratedTypeId");
            var enumeratedId = new NonTerminal("enumeratedId");
            var enumeratedValue = new NonTerminal("enumeratedValue");
            var expn = new NonTerminal("expn");
            var expnList = new NonTerminal("expnList");
            var fieldId = new NonTerminal("fieldId");
            var componentSelector = new NonTerminal("componentSelector");
            var maximumLength = new NonTerminal("maximumLength");
            var numberOfCharacters = new NonTerminal("numberOfCharacters");
            var standardType = new NonTerminal("standardType");
            var subrangeType = new NonTerminal("subrangeType");
            var typeDeclaration = new NonTerminal("typeDeclaration");
            var stringType = new NonTerminal("stringType");
            var idList = new NonTerminal("idList");
            var enumeratedType = new NonTerminal("enumeratedType");
            var indexType = new NonTerminal("indexType");
            var indexTypeList = new NonTerminal("indexTypeList");
            var arrayFlexible = new NonTerminal("arrayFlexible");
            var arrayType = new NonTerminal("arrayType");
            var setType = new NonTerminal("setType");
            var recordField = new NonTerminal("recordField");
            var recordFieldList = new NonTerminal("recordFieldList");
            var recordType = new NonTerminal("recordType");
            var labelExpn = new NonTerminal("labelExpn");
            var labelExpnList = new NonTerminal("labelExpnList");
            var unionLabelList = new NonTerminal("unionLabelList");
            var unionEndList = new NonTerminal("unionEndList");
            var unionEndLabel = new NonTerminal("unionEndLabel");
            var unionType = new NonTerminal("unionType");
            var collectionId = new NonTerminal("collectionId");
            var pointerType = new NonTerminal("pointerType");
            var namedType = new NonTerminal("namedType");
            var typeSpec = new NonTerminal("typeSpec");
            var subprogramType = new NonTerminal("subprogramType");
            var paramDeclaration = new NonTerminal("paramDeclaration");
            var paramList = new NonTerminal("paramList");
            var subParams = new NonTerminal("subParams");
            var subPervasive = new NonTerminal("subPervasive");
            var subDevice = new NonTerminal("subDevice");
            var subprogramHeader = new NonTerminal("subprogramHeader");
            var subBody = new NonTerminal("subBody");
            var trueFalseExpn = new NonTerminal("trueFalseExpn");
            var subPre = new NonTerminal("subPre");
            var initList = new NonTerminal("initList");
            var subInit = new NonTerminal("subInit");
            var subPost = new NonTerminal("subPost");
            var subExcept = new NonTerminal("subExcept");
            var subprogramDeclaration = new NonTerminal("subprogramDeclaration");
            var assignmentOp = new NonTerminal("assignmentOp");
            var assignmentStatement = new NonTerminal("assignmentStatement");
            var beginStatement = new NonTerminal("beginStatement");
            var bindList = new NonTerminal("bindList");
            var bindDeclaration = new NonTerminal("bindDeclaration");
            var compileTimeExpn = new NonTerminal("compileTimeExpn");
            var compileTimeExpnList = new NonTerminal("compileTimeExpnList");
            var caseLabel = new NonTerminal("caseLabel");
            var caseLabelList = new NonTerminal("caseLabelList");
            var caseLabels = new NonTerminal("caseLabels");
            var caseDefaultLabel = new NonTerminal("caseDefaultLabel");
            var caseStatement = new NonTerminal("caseStatement");
            var typeId = new NonTerminal("typeId");
            var targetType = new NonTerminal("targetType");
            var compileTimeIntExpn = new NonTerminal("compileTimeIntExpn");
            var sizeSpec = new NonTerminal("sizeSpec");
            var typeCheat = new NonTerminal("typeCheat");
            var classMonitor = new NonTerminal("classMonitor");
            var inheritItem = new NonTerminal("inheritItem");
            var classInherit = new NonTerminal("classInherit");
            var implementItem = new NonTerminal("implementItem");
            var classImplement = new NonTerminal("classImplement");
            var implementByItem = new NonTerminal("implementByItem");
            var classBy = new NonTerminal("classBy");
            var howImport = new NonTerminal("howImport");
            var importItem = new NonTerminal("importItem");
            var importList = new NonTerminal("importList");
            var classImport = new NonTerminal("classImport");
            var exportMethod = new NonTerminal("exportMethod");
            var howExport = new NonTerminal("howExport");
            var exportItem = new NonTerminal("exportItem");
            var exportList = new NonTerminal("exportList");
            var classExport = new NonTerminal("classExport");
            var classDeclaration = new NonTerminal("classDeclaration");
            var fileNumber = new NonTerminal("fileNumber");
            var closeStatement = new NonTerminal("closeStatement");
            var collectionCheck = new NonTerminal("collectionCheck");
            var collectionDeclaration = new NonTerminal("collectionDeclaration");
            var comparisonOperator = new NonTerminal("comparisonOperator");
            var conditionArray = new NonTerminal("conditionArray");
            var conditionOption = new NonTerminal("conditionOption");
            var conditionDeclaration = new NonTerminal("conditionDeclaration");
            var constTypeSpec = new NonTerminal("constTypeSpec");
            var initializingValue = new NonTerminal("initializingValue");
            var constPervasive = new NonTerminal("constPervasive");
            var constRegister = new NonTerminal("constRegister");
            var constantDeclaration = new NonTerminal("constantDeclaration");
            var declaration = new NonTerminal("declaration");
            var deferredDeclaration = new NonTerminal("deferredDeclaration");
            var exitStatement = new NonTerminal("exitStatement");
            var externalOverrideName = new NonTerminal("externalOverrideName");
            var externalAddressSpec = new NonTerminal("externalAddressSpec");
            var externalTypeSpec = new NonTerminal("externalTypeSpec");
            var externalExpn = new NonTerminal("externalExpn");
            var externalDeclaration = new NonTerminal("externalDeclaration");
            var increment = new NonTerminal("increment");
            var forIncrement = new NonTerminal("forIncrement");
            var integerExpn = new NonTerminal("integerExpn");
            var rangeId = new NonTerminal("rangeId");
            var forRange = new NonTerminal("forRange");
            var forId = new NonTerminal("forId");
            var forDecreasing = new NonTerminal("forDecreasing");
            var forStatement = new NonTerminal("forStatement");
            var forExpnLIst = new NonTerminal("forExpnList");
            var forkParams = new NonTerminal("forParams");
            var forkRefExpnRef = new NonTerminal("forkRefExpnRef");
            var forkRefExpn = new NonTerminal("forkRefExpn");
            var forkReference = new NonTerminal("forReference");
            var addressReference = new NonTerminal("addressReference");
            var booleanVariableReference = new NonTerminal("booleanVariableReference");
            var processId = new NonTerminal("processId");
            var forkStatement = new NonTerminal("forkStatement");
            //new rules
            var deviceSpecification = new NonTerminal("deviceSpecification");
            var exceptionHandler = new NonTerminal("exceptionHandler");
            var statementsAndDeclarations = new NonTerminal("statementsAndDeclarations");
            var fileName = new NonTerminal("fileName");
            var procedureDeclaration = new NonTerminal("procedureDeclaration");
            var functionDeclaration = new NonTerminal("functionDeclaration");
            //var moduleDeclaration = new NonTerminal("moduleDeclaration");
            //var monitorDeclaration = new NonTerminal("monitorDeclaration");
            //var processDeclaration = new NonTerminal("processDeclaration");
            var variableDeclaration = new NonTerminal("variableDeclaration");
            var overrideName = new NonTerminal("overrideName");
            var addressSpec = new NonTerminal("addressSpec");
            var forkExpnList = new NonTerminal("forkExpnList");
            var statements = new NonTerminal("statements");
            var putStatement = new NonTerminal("putStatement");
            var putItem = new NonTerminal("putItem");
            var getStatement = new NonTerminal("getStatement");
            var getItem = new NonTerminal("getItem");
            var openStatement = new NonTerminal("openStatement");
            var capability = new NonTerminal("capability");
            var streamNumber = new NonTerminal("streamNumber");
            var widthExpn = new NonTerminal("widthExpn");
            var fractionWidth = new NonTerminal("fractionWidth");
            var exponentWidth = new NonTerminal("exponentWidth");
            var fileNumberVariable = new NonTerminal("fileNumberVariable");
            var loopStatement = new NonTerminal("loopStatement");
            var functionHeader = new NonTerminal("functionHeader");
            var procedureHeader = new NonTerminal("procedureHeader");

            //3. BNF Rules
            variableId.Rule = id;
            variableReference.Rule = variableId | variableId + PreferShiftHere() + componentSelector;
            constantId.Rule = id;
            constantReference.Rule = constantId | constantId + PreferShiftHere() + componentSelector;
            explicitTrueFalseConstant.Rule = ToTerm("true") | "false";
            explicitConstant.Rule = explicitStringConstant | explicitIntegerConstant | explicitRealConstant | explicitTrueFalseConstant | explicitCharConstant;
            infixOperator.Rule = ToTerm("+") | "-" | "*" | "/" | "div" | "mod" | "rem" | "**" | "<" | ">" | "=" | "<=" | ">=" | "not=" | "and" | "or" | "=>" | "in" | "not" + "in" | "shr" | "shl" | "xor";
            prefixOperator.Rule = ToTerm("+") | "-" | "not" | "#" | "^";
            leftPosition.Rule = expn | "*" | "*" + "-" + expn;
            rightPosition.Rule = expn | "*" | "*" + "-" + expn;
            charPosition.Rule = expn | "*" | "*" + "-" + expn;
            stringReference.Rule = variableReference;
            substring.Rule = stringReference + "(" + leftPosition + ".." + rightPosition + ")"
                            | stringReference + "(" + charPosition + ")";
            functionId.Rule = variableReference;
            moduleId.Rule = variableReference;
            classId.Rule = variableReference;
            pointerId.Rule = variableReference;
            functionRefence.Rule = moduleId + "." + functionId | functionId
                                | classId + "(" + pointerId + ")" + "." + functionId
                                | pointerId + "->" + functionId;
            functionCall.Rule = functionRefence | functionRefence + "(" + ")" | functionRefence + "(" + expnList + ")";
            setTypeId.Rule = variableReference;
            membersOfSet.Rule = expnList | "all";
            setConstructor.Rule = setTypeId + "(" + membersOfSet + ")" | setTypeId + "(" + ")";
            enumeratedTypeId.Rule = variableReference;
            enumeratedId.Rule = id;
            enumeratedValue.Rule = enumeratedTypeId + "." + enumeratedId;
            expn.Rule = explicitConstant | variableReference | constantReference
                    | expn + infixOperator + expn
                    | prefixOperator + expn
                    | "(" + expn + ")"
                    | substring | functionCall | setConstructor | enumeratedValue;
            expnList.Rule = expn | expn + "," + expn;
            fieldId.Rule = id;
            componentSelector.Rule = "(" + expnList + ")" | "." + fieldId;
            maximumLength.Rule = expn;
            numberOfCharacters.Rule = expn;
            standardType.Rule = ToTerm("int") | "real" | stringType | "boolean" | "nat"
                            | "int1" | "int2" | "int4" | "nat1" | "nat2" | "nat4"
                            | "real4" | "real8" | "char" | "char" + "(" + numberOfCharacters + ")";
            subrangeType.Rule = expn + ".." + expn;
            typeDeclaration.Rule = "type" + id + ":" + typeSpec | "type" + id + ":" + "forward";
            stringType.Rule = "string" | "string" + PreferShiftHere() + "(" + maximumLength + ")";
            idList.Rule = id | id + PreferShiftHere() + "," + idList;
            enumeratedType.Rule = "enum" + "(" + idList + ")";
            indexType.Rule = subrangeType | enumeratedType | namedType | "char" | "boolean";
            indexTypeList.Rule = indexType | indexType + "," + indexTypeList;
            arrayFlexible.Rule = "flexible" | Empty;
            arrayType.Rule = arrayFlexible + "array" + indexTypeList + "of" + typeSpec;
            setType.Rule = "set" + "of" + typeSpec;
            recordField.Rule = idList + ":" + typeSpec;
            recordFieldList.Rule = recordField | recordField + recordFieldList;
            recordType.Rule = "record" + recordFieldList + "end" + "record";
            labelExpn.Rule = expn;
            labelExpnList.Rule = labelExpn | labelExpn + "," + labelExpnList;
            unionLabelList.Rule = "label" + labelExpnList + ":" + idList + ":" + typeSpec
                                | "label" + labelExpnList + ":";
            unionEndLabel.Rule = "label" + ":" + idList + ":" + typeSpec | "label" + ":";
            unionType.Rule = "union" + ":" + indexType + "of" + unionLabelList + "end" + "union"
                        | "union" + id + ":" + indexType + "of" + unionLabelList + "end" + "union"
                        | "union" + ":" + indexType + "of" + unionLabelList + unionEndLabel + "end" + "union"
                        | "union" + id + ":" + indexType + "of" + unionLabelList + unionEndLabel + "end" + "union";
            collectionId.Rule = variableReference;
            pointerType.Rule = "pointer" + "to" + collectionId
                            | "unchecked" + "pointer" + "to" + collectionId
                            | "^" + collectionId
                            | "pointer" + "to" + classId
                            | "unchecked" + "pointer" + "to" + classId
                            | "^" + classId
                            | "pointer" + "to" + typeSpec
                            | "unchecked" + "pointer" + "to" + typeSpec
                            | "^" + typeSpec;
            namedType.Rule = variableReference;
            typeSpec.Rule = ToTerm("int") | "real" | "boolean" | stringType | subrangeType
                        | enumeratedType | arrayType | setType | recordType | unionType
                        | pointerType | namedType | "nat" | "int1" | "int2" | "int4"
                        | "nat1" | "nat2" | "nat4" | "real4" | "real8" | "char"
                        | "char" + "(" + numberOfCharacters + ")" | subprogramType;
            subprogramType.Rule = subprogramHeader;
            paramDeclaration.Rule = idList + ":" + typeSpec
                                | "var" + idList + ":" + typeSpec
                                | subprogramHeader;
            paramList.Rule = paramDeclaration | paramDeclaration + "," + paramList;
            subParams.Rule = "(" + paramList + ")" | "(" + ")" | Empty;
            subPervasive.Rule = "pervasive" | Empty;
            subDevice.Rule = ":" + deviceSpecification | Empty;
            //subprogramHeader.Rule = "procedure" + subPervasive + id + subParams + subDevice
            //                    | "function" + subPervasive + id + subParams + ":" + typeSpec
            //                    | "function" + subPervasive + id + subParams + id + ":" + typeSpec;
            subBody.Rule = "body" | Empty;
            trueFalseExpn.Rule = expn;
            subPre.Rule = "pre" + trueFalseExpn | Empty;
            initList.Rule = id + ":=" + expn | id + ":=" + expn + "," + initList;
            subInit.Rule = "init" + initList | Empty;
            subPost.Rule = "post" + trueFalseExpn | Empty;
            subExcept.Rule = exceptionHandler | Empty;
            //subprogramDeclaration.Rule = subBody + subprogramHeader + subPre + subInit + subPost + subExcept + statementsAndDeclarations + "end" + id
            //                        | "body" + id + statementsAndDeclarations + "end" + id;
            assignmentOp.Rule = ToTerm(":=") | "+=" | "-=" | "*=" | "/=" | "div="
                            | "mod=" | "rem=" | "shl=" | "shr=" | "xor=";
            assignmentStatement.Rule = variableReference + assignmentOp + expn;
            beginStatement.Rule = "begin" + statementsAndDeclarations + "end";
            bindList.Rule = id + "to" + variableReference
                        | "var" + id + "to" + variableReference
                        | id + "to" + variableReference + "," + bindList
                        | "var" + id + "to" + variableReference + "," + bindList
                        | "register" + id + "to" + variableReference
                        | "var" + "register" + id + "+to" + variableReference
                        | "register" + id + "to" + variableReference + "," + bindList
                        | "var" + "register" + id + "to" + variableReference + "," + bindList;
            bindDeclaration.Rule = "bind" + bindList;
            compileTimeExpn.Rule = expn;
            compileTimeExpnList.Rule = compileTimeExpn | compileTimeExpn + "," + compileTimeExpnList;
            caseLabel.Rule = "label" + compileTimeExpnList + ":" + statementsAndDeclarations;
            caseLabelList.Rule = caseLabel | caseLabel + "," + caseLabelList;
            caseLabels.Rule = caseLabelList | Empty;
            caseDefaultLabel.Rule = "label" + ":" + statementsAndDeclarations | Empty;
            caseStatement.Rule = "case" + expn + "of" + caseLabels + caseDefaultLabel + "end" + "case";
            typeId.Rule = variableId | id + ":" + variableReference;
            targetType.Rule = typeId | "int" | "int1" | "int2" | "int4" | "nat" | "nat1"
                            | "nat2" | "nat4" | "boolean" | "char" | "string"
                            | "char" + "(" + numberOfCharacters + ")"
                            | "string" + "(" + maximumLength + ")"
                            | "addressint";
            compileTimeIntExpn.Rule = expn;
            sizeSpec.Rule = ":" + compileTimeIntExpn | Empty;
            typeCheat.Rule = "cheat" + "(" + targetType + "," + expn + sizeSpec + ")"
                        | "#" + expn | id + ":" + "cheat" + typeSpec;
            classMonitor.Rule = "monitor" | Empty;
            inheritItem.Rule = id;
            classInherit.Rule = "inherit" + inheritItem | Empty;
            implementItem.Rule = id;
            classImplement.Rule = "implement" + implementItem | Empty;
            implementByItem.Rule = id;
            classBy.Rule = "implement" + "by" + implementByItem | Empty;
            howImport.Rule = ToTerm("var") | "const" | "forward" | Empty;
            importItem.Rule = howImport + id | howImport + id + "in" + fileName
                            | "(" + howImport + id + ")"
                            | "(" + howImport + id + "in" + fileNumber + ")";
            importList.Rule = importItem | importItem + "," + importList;
            classImport.Rule = "import" + importList | Empty;
            exportMethod.Rule = ToTerm("var") | "unqualified" | "pervasive" | "opaque";
            howExport.Rule = exportMethod + howExport | Empty;
            exportItem.Rule = howExport + id;
            exportList.Rule = exportItem | exportItem + "," + exportList;
            classExport.Rule = "export" + exportList | "export" + "all" | Empty;
            classDeclaration.Rule = classMonitor + "class" + id + classInherit + classImplement + classBy + classImport + classExport + statementsAndDeclarations + "end" + id;
            fileNumber.Rule = expn;
            closeStatement.Rule = "close" + ":" + fileNumber
                            | "close" + "(" + fileNumber + ":" + "int" + ")";
            collectionCheck.Rule = "unchecked" | Empty;
            collectionDeclaration.Rule = "var" + idList + ":" + collectionCheck + "collection" + "of" + typeSpec
                                    | "var" + idList + ":" + collectionCheck + "collection" + "of" + "forward" + typeId;
            comparisonOperator.Rule = ToTerm("<") | ">" | "=" | "<=" | ">=" | "not=";
            conditionArray.Rule = "array" + indexTypeList + "of" | Empty;
            conditionOption.Rule = ToTerm("priority") | "deferred" | "timeout" | Empty;
            conditionDeclaration.Rule = "var" + idList + ":" + conditionArray + conditionOption + "condition";
            constTypeSpec.Rule = ":" + typeSpec | Empty;
            initializingValue.Rule = expn | "init" + "(" + initializingValue + "," + initializingValue + ")";
            constPervasive.Rule = "pervasive" | Empty;
            constRegister.Rule = "register" | Empty;
            constantDeclaration.Rule = "const" + constPervasive + constRegister + id + constTypeSpec + ":=" + initializingValue;
            declaration.Rule = constantDeclaration | typeDeclaration | bindDeclaration
                            | procedureDeclaration | functionDeclaration
                            | variableDeclaration | conditionDeclaration
                //| monitorDeclaration | moduleDeclaration| processDeclaration
                            | classDeclaration;
            deferredDeclaration.Rule = "deferred" + subprogramHeader;
            exitStatement.Rule = "exit" + "when" + trueFalseExpn;// | "exit";
            externalOverrideName.Rule = overrideName | Empty;
            externalAddressSpec.Rule = addressSpec | Empty;
            externalTypeSpec.Rule = ":" + typeSpec | Empty;
            externalExpn.Rule = ":=" + expn | Empty;
            externalDeclaration.Rule = "external" + externalOverrideName + subprogramHeader
                                    | "external" + externalAddressSpec + "var" + id + externalTypeSpec + externalExpn;
            increment.Rule = expn;
            forIncrement.Rule = "by" + increment | Empty;
            integerExpn.Rule = expn;
            rangeId.Rule = variableDeclaration;
            forRange.Rule = integerExpn + ".." + integerExpn | rangeId;
            forId.Rule = id | Empty;
            forDecreasing.Rule = "decreasing" | Empty;
            forStatement.Rule = "for" + forDecreasing + forId + ":" + forRange + forIncrement + statementsAndDeclarations + "end" + "for";
            forkExpnList.Rule = expnList | Empty;
            forkParams.Rule = "(" + forkExpnList + ")" | Empty;
            forkRefExpnRef.Rule = "," + addressReference | Empty;
            forkRefExpn.Rule = "," + expn + forkRefExpnRef | Empty;
            forkReference.Rule = ":" + booleanVariableReference + forkRefExpn | Empty;
            addressReference.Rule = variableReference;
            booleanVariableReference.Rule = variableReference;
            processId.Rule = variableReference;
            forkStatement.Rule = "fork" + processId + forkParams + forkReference;

            //new rules
            deviceSpecification.Rule = explicitIntegerConstant;
            exceptionHandler.Rule = "handler" + "(" + id + ")" + statementsAndDeclarations + "end" + "handler";
            fileName.Rule = explicitStringConstant;
            overrideName.Rule = explicitStringConstant;
            addressSpec.Rule = compileTimeExpn;
            procedureDeclaration.Rule = subprogramDeclaration;//seems a little redudant
            functionDeclaration.Rule = subprogramDeclaration;
            //processDeclaration.Rule = forkStatement;
            statementsAndDeclarations.Rule = declaration + statementsAndDeclarations | statements + statementsAndDeclarations | Empty;
            statements.Rule = forStatement | forkStatement | exitStatement | closeStatement | assignmentStatement | caseStatement | beginStatement | putStatement | getStatement | loopStatement;

            variableDeclaration.Rule = ToTerm("var") + MakePlusRule(variableDeclaration, ToTerm(","), id) + ":=" + expn
                                       | "var" + MakePlusRule(variableDeclaration, ToTerm(","), id) + ":" + typeSpec + (":=" + initializingValue | Empty);
            putStatement.Rule = ToTerm("put") + (":" + streamNumber + "," | Empty) + MakePlusRule(putStatement, ToTerm(","), putItem) + (".." | Empty);
            putItem.Rule = expn + (":" + widthExpn + (":" + fractionWidth + (":" + exponentWidth | Empty) | Empty) | Empty) | "skip";
            getStatement.Rule = ToTerm("get") + (":" + streamNumber + "," | Empty) + MakePlusRule(getStatement, ToTerm(","), getItem);
            getItem.Rule = variableReference | "skip" | variableReference + ":" + "*" | variableReference + ":" + widthExpn;
            openStatement.Rule = ToTerm("open") + ":" + fileNumberVariable + "," + fileName + "," + MakePlusRule(openStatement, ToTerm(","), capability);
            capability.Rule = ToTerm("put") | "get";
            closeStatement.Rule = ToTerm("close") + ":" + fileNumber;
            streamNumber.Rule = widthExpn.Rule = fractionWidth.Rule = exponentWidth.Rule = fileNumber.Rule = expn;
            loopStatement.Rule = ToTerm("loop") + statementsAndDeclarations + "end" + "loop";

            functionDeclaration.Rule = subBody + functionHeader + subPre + subInit + subPost + subExcept + statementsAndDeclarations + "end" + id
                                    | "body" + id + statementsAndDeclarations + "end" + id;
            procedureDeclaration.Rule = subBody + procedureHeader + subPre + subInit + subPost + subExcept + statementsAndDeclarations + "end" + id
                                    | "body" + id + statementsAndDeclarations + "end" + id;
            subprogramDeclaration.Rule = functionDeclaration | procedureDeclaration;
            functionHeader.Rule = "function" + subPervasive + id + subParams + ":" + typeSpec
                                | "function" + subPervasive + id + subParams + id + ":" + typeSpec;
            procedureHeader.Rule = "procedure" + subPervasive + id + subParams + subDevice;
            subprogramHeader.Rule = functionHeader | procedureHeader;
                                
            
            //Following are guesses as to the rules, since the spec does not state the rules for them
            fileNumberVariable.Rule = variableReference;
            fileName.Rule = explicitStringConstant | variableReference;
            
            //variableDeclaration.Rule = collectionDeclaration;
            //moduleDeclaration.Rule = variableDeclaration;
            //monitorDeclaration.Rule = variableDeclaration;

            this.Root = statementsAndDeclarations;
            //procedureDeclaration = new NonTerminal("procedureDeclaration");
            //functionDeclaration = new NonTerminal("functionDeclaration");
            //moduleDeclaration = new NonTerminal("moduleDeclaration");
            //monitorDeclaration = new NonTerminal("monitorDeclaration");
            //processDeclaration = new NonTerminal("processDeclaration");
            //variableDeclaration = new NonTerminal("variableDeclaration");

            //this.Root = program;
            
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
