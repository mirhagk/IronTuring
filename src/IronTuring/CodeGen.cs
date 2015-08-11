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
    public sealed class CodeGen
    {
        TypeBuilder mainProgram;
        AssemblyBuilder asmb;

        public CodeGen(ParseTreeNode stmt, string moduleName)
        {
            if (Path.GetFileName(moduleName) != moduleName)
            {
                throw new System.Exception("can only output into current directory!");
            }

            AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(moduleName));
            
            asmb = System.AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);
            
            ModuleBuilder modb = asmb.DefineDynamicModule(moduleName);
            
            //mainProgram = modb.DefineType("Program");
            
            
            //var mainArgs = new List<Tuple<string, Type>>();
            //var mainProgramDef = new FunctionDefinition
            //(
            //    mainProgram.DefineMethod("Main", MethodAttributes.Static, typeof(void), System.Type.EmptyTypes),
            //    new List<FunctionDefinition.Argument>()
            //);

            SymbolTable symbolTable = new SymbolTable(modb.DefineType("__Program"));

            symbolTable.AddFunctionHeader("__Main", MethodAttributes.Static, null, new FunctionDefinition.Argument[]{});

            //symbolTable.functionTable.AddHeader("Main", mainProgramDef);

            stmt = PreGenerate(stmt, symbolTable);
            stmt = ImportList(stmt, symbolTable);
            
            // CodeGenerator
            var il = symbolTable.functionTable["__Main"].GetILGenerator();

            // Go Compile!
            this.GenStmt(stmt, il, symbolTable);

            //il.Emit(OpCodes.Ldstr, "Press any key to exit the program...");
            //il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            //il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadKey", new Type[] { }));

            il.Emit(OpCodes.Ret);
            //mainProgram.CreateType();
            modb.CreateGlobalFunctions();
            asmb.SetEntryPoint(symbolTable.functionTable["__Main"].methodDefinition);

            symbolTable.typeTable.types[0].typeBuilder.CreateType();
            
            asmb.Save(moduleName);
            foreach (var symbol in symbolTable.locals)
            {
                Console.WriteLine("{0}: {1}", symbol.Key, symbol.Value);
            }
            symbolTable = null;
            il = null;

        }

        static ParseTreeNode ImportList(ParseTreeNode stmt, SymbolTable symbolTable)
        {
            ParseTreeNode result = stmt.ChildNodes[1];
            foreach (ParseTreeNode item in stmt.ChildNodes[0].ChildNodes)
            {
                if (item.Term.Name == "import")
                {
                    symbolTable.functionTable.AddLibrary(GetIdentifier(item.ChildNodes[0]), item.ChildNodes.Count > 1 ? item.ChildNodes[1].Token.ValueString : null);
                }
                else
                    result.ChildNodes.Add(item);
            }
            return result;
        }

        static string GetIdentifier(ParseTreeNode stmt)
        {
            if (stmt.Term.Name == "identifier")
                return stmt.Token.ValueString;
            else if (stmt.Term.Name == "functionCall")
            {
                return stmt.ChildNodes[0].Token.ValueString;
            }
            else if (stmt.Term.Name == "memberCall")
            {
                return stmt.ChildNodes[0].Token.ValueString + "." + GetIdentifier(stmt.ChildNodes[1]);
            }
            throw new Exception("Don't know how to generate indentifier for " + stmt.Term.Name);
        }

        static ParseTreeNodeList GetArgs(ParseTreeNode stmt)
        {
            if (stmt.Term.Name == "functionCall")
            {
                return stmt.ChildNodes[1].ChildNodes;
            }
            else if (stmt.Term.Name == "memberCall")
            {
                return GetArgs(stmt.ChildNodes[1]);
            }
            throw new Exception("Can't get arguments for " + stmt.Term.Name);
        }

        /// <summary>
        /// The pregeneration step. During this stage the compiler replaces all include statements with the appropriate code. It also builds the function table (without definitions)
        /// so that functions can be referenced before they are declared in the code
        /// </summary>
        /// <param name="stmt"></param>
        /// <param name="il"></param>
        /// <param name="symbolTable"></param>
        /// <returns></returns>
        static ParseTreeNode PreGenerate(ParseTreeNode stmt, SymbolTable symbolTable)
        {
            if (stmt.Term.Name == "program")
            {
                if (stmt.ChildNodes.Count > 0)
                {
                    stmt.ChildNodes[0].ChildNodes[0] = PreGenerate(stmt.ChildNodes[0].ChildNodes[0], symbolTable);
                    stmt.ChildNodes[1] = PreGenerate(stmt.ChildNodes[1], symbolTable);
                    return stmt;
                }
            }
            else if (stmt.Term.Name == "unit")
            {
                stmt.ChildNodes[0] = PreGenerate(stmt.ChildNodes[0], symbolTable);
                stmt.ChildNodes[1] = PreGenerate(stmt.ChildNodes[1], symbolTable);
            }
            else if (stmt.Term.Name == "importSection")
            {
                for (int i = 0; i < stmt.ChildNodes.Count; i++)
                {
                    stmt.ChildNodes[i] = PreGenerate(stmt.ChildNodes[i], symbolTable);
                }
            }
            else if (stmt.Term.Name == "include")
            {
                string filename = stmt.Token.ValueString;
                if (!File.Exists(filename))
                {
                    Console.WriteLine("File does not exist");
                    Console.ReadKey();
                    return null;
                }
                var rootNode = Program.getRoot(System.IO.File.ReadAllText(filename), new TSharpCompiler.TuringGrammarBroken());
                if (rootNode == null)
                {
                    Console.WriteLine("Parsing failed");
                    Console.ReadKey();
                    return null;
                }
                return PreGenerate(rootNode, symbolTable);
            }
            else if (stmt.Term.Name == "functionDefinition")
            {
                string functionName = stmt.ChildNodes[0].ChildNodes[1].Token.ValueString;
                if (symbolTable.functionTable.ContainsKey(functionName))
                {
                    throw new Exception(functionName + " has already been defined");
                }
                var parameterList = new List<FunctionDefinition.Argument>();
                if (stmt.ChildNodes[0].ChildNodes[2].ChildNodes.Count > 0)
                {
                    var currParam = stmt.ChildNodes[0].ChildNodes[2].ChildNodes[0];
                    while (true)
                    {
                        var parameterType = TypeOfExpr(currParam.ChildNodes[0].ChildNodes[1].ChildNodes[0], symbolTable);
                        var paramIdentifier = currParam.ChildNodes[0].ChildNodes[0];
                        while (true)
                        {
                            var parameterName = paramIdentifier.ChildNodes[0].Token.ValueString;
                            parameterList.Add(new FunctionDefinition.Argument() { argName = parameterName, argType = parameterType });
                            if (paramIdentifier.ChildNodes.Count == 1)
                                break;
                            paramIdentifier = paramIdentifier.ChildNodes[1];
                        }
                        if (currParam.ChildNodes.Count == 1)
                            break;
                        currParam = currParam.ChildNodes[1];
                    }
                }
                Type type = null;
                if (stmt.ChildNodes[0].ChildNodes.Count > 3)//if there's a 4th childnode then it's the type specifier
                    type = TypeOfExpr(stmt.ChildNodes[0].ChildNodes[3].ChildNodes[0], symbolTable);

                symbolTable.AddFunctionHeader(functionName, MethodAttributes.Static, type, parameterList.ToArray());
                //MethodBuilder methodDeclaration;
                //if (stmt.ChildNodes[0].ChildNodes.Count > 3)//if there's a 4th childnode then it's the type specifier
                //    methodDeclaration = mainProgram.DefineMethod(functionName, MethodAttributes.Static, TypeOfExpr(stmt.ChildNodes[0].ChildNodes[3].ChildNodes[0], symbolTable), parameterList.Select(x => x.argType).ToArray());
                //else
                //    methodDeclaration = mainProgram.DefineMethod(functionName, MethodAttributes.Static, null, parameterList.Select(x => x.argType).ToArray());

                //var methodDec = new FunctionDefinition
                //(
                //    methodDeclaration,
                //    parameterList
                //);
                //symbolTable.functionTable.AddHeader(functionName, methodDec);
            }
            else if (stmt.Term.Name == "type")
            {
                
                if (stmt.ChildNodes[1].ChildNodes[0].ChildNodes[0].Term.Name == "recordList")
                {
                    symbolTable.CreateNewType(stmt.ChildNodes[1].ChildNodes[0].ChildNodes[0], stmt.ChildNodes[0].Token.ValueString);
                    //var parameterList = new List<FunctionDefinition.Argument>();
                    //var newType = mainProgram.DefineNestedType(stmt.ChildNodes[0].Token.ValueString);

                    //var currParam = stmt.ChildNodes[0].ChildNodes[2].ChildNodes[0];
                    //while (true)
                    //{
                    //    var parameterType = TypeOfExpr(currParam.ChildNodes[0].ChildNodes[1].ChildNodes[0], symbolTable);
                    //    var paramIdentifier = currParam.ChildNodes[0].ChildNodes[0];
                    //    while (true)
                    //    {
                    //        var parameterName = paramIdentifier.ChildNodes[0].Token.ValueString;
                    //        parameterList.Add(new FunctionDefinition.Argument() { argName = parameterName, argType = parameterType });
                    //        if (paramIdentifier.ChildNodes.Count == 1)
                    //            break;
                    //        paramIdentifier = paramIdentifier.ChildNodes[1];
                    //    }
                    //    if (currParam.ChildNodes.Count == 1)
                    //        break;
                    //    currParam = currParam.ChildNodes[1];
                    //}

                    /*ParseTreeNode field = stmt.ChildNodes[1].ChildNodes[0].ChildNodes[0];

                    while (true)
                    {
                        if (field.ChildNodes.Count > 1)
                        {
                            field = field.ChildNodes[1];
                        }
                        else
                            break;
                    }*/
                }
            }
            return stmt;
        }

        private void GenStmt(ParseTreeNode stmt, ILGenerator il, SymbolTable symbolTable, Label? exitScope = null)
        {
            if (stmt.Term.Name == "program")
            {
                if (stmt.ChildNodes.Count > 0)
                {
                    this.GenStmt(stmt.ChildNodes[0].ChildNodes[0], il, symbolTable);
                    this.GenStmt(stmt.ChildNodes[1], il, symbolTable);
                }
            }
            else if (stmt.Term.Name == "variableDeclaration")
            {
                Type localType;
                // declare a local
                if (stmt.ChildNodes[2].Term.Name == "typeSpecifier")
                {
                    localType = this.TypeOfTypeDeclaration(stmt.ChildNodes[2].ChildNodes[0]);
                }
                else
                {
                    localType = TypeOfExpr(stmt.ChildNodes[2].ChildNodes[1], symbolTable);
                }
                Action<string> generateAssign = null;
                ParseTreeNode assign = stmt.ChildNodes.Where(x => x.Term.Name == "setEqual").SingleOrDefault();
                // set the initial value
                if (assign != null)
                {
                    generateAssign = new Action<string>(name =>
                    {
                        this.GenExpr(assign.ChildNodes[1], symbolTable.locals[name].LocalType, il, symbolTable);
                        symbolTable.Store(name, TypeOfExpr(assign.ChildNodes[1], symbolTable), il);
                    });
                }
                var variableIden = stmt.ChildNodes[1];
                while (true)
                {
                    string name = variableIden.ChildNodes[0].Token.ValueString;
                    symbolTable.AddLocal(name, il.DeclareLocal(localType));
                    if (generateAssign != null)
                        generateAssign(name);

                    if (variableIden.ChildNodes.Count < 2)
                        break;
                    variableIden = variableIden.ChildNodes[1];
                }
            }
            else if (stmt.Term.Name == "io")
            {
                if (stmt.ChildNodes[0].Token.ValueString == "put")
                {
                    //the first argument is always there, until we can build a proper AST this'll have to do
                    ParseTreeNode argItem = stmt.ChildNodes[1];
                    this.GenExpr(argItem.ChildNodes[0], typeof(string), il, symbolTable);
                    il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", new System.Type[] { typeof(string) }));
                    argItem = stmt.ChildNodes[2];
                    while (true)
                    {
                        if (argItem.ChildNodes.Count == 0)
                            break;
                        this.GenExpr(argItem.ChildNodes[0].ChildNodes[0], typeof(string), il, symbolTable);
                        il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", new System.Type[] { typeof(string) }));
                        argItem = argItem.ChildNodes[1];
                    }
                    if (stmt.ChildNodes[3].ChildNodes.Count == 0)//put a newline character if there is no ...
                        il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new System.Type[] { }));
                }
                else if (stmt.ChildNodes[0].Token.ValueString == "get")
                {
                    foreach (var argument in stmt.ChildNodes[1].ChildNodes)
                    {
                        //switch(symbolTable.TypeOfVar(
                        il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", new System.Type[] { }));
                        symbolTable.Store(argument.Token.ValueString, typeof(string), il);
                    }
                }
            }
            else if (stmt.Term.Name == "assignment")
            {

                if (stmt.ChildNodes[0].Term.Name == "functionCall")//if we see this as a function call, we know that's not true, and it's actually an array (which is kinda the same thing in turing)
                {
                    string arrayName = stmt.ChildNodes[0].ChildNodes[0].Token.ValueString;
                    if (symbolTable.TypeOfVar(arrayName).IsArray)
                    {
                        symbolTable.PushVar(arrayName, il);
                        if (stmt.ChildNodes[0].ChildNodes[1].ChildNodes.Count > 1)
                            throw new NotImplementedException("Multi-Dimensional arrays are not yet supported");

                        this.GenExpr(stmt.ChildNodes[0].ChildNodes[1].ChildNodes[0], typeof(int), il, symbolTable);
                        this.GenExpr(stmt.ChildNodes[1].ChildNodes[1], TypeOfExpr(stmt.ChildNodes[1].ChildNodes[1], symbolTable), il, symbolTable);

                        il.Emit(OpCodes.Stelem, symbolTable.TypeOfVar(arrayName).GetElementType());
                    }
                    else
                        throw new NotSupportedException(String.Format("Non-array identifier used like an array: {0}", arrayName));
                }
                else
                {
                    this.GenExpr(stmt.ChildNodes[1].ChildNodes[1], TypeOfExpr(stmt.ChildNodes[1].ChildNodes[1], symbolTable), il, symbolTable);
                    string ident = stmt.ChildNodes[0].Token.ValueString;
                    symbolTable.Store(ident, TypeOfExpr(stmt.ChildNodes[1].ChildNodes[1], symbolTable), il);
                }
            }
            else if (stmt.Term.Name == "functionDefinition")
            {
                string functionName = stmt.ChildNodes[0].ChildNodes[1].Token.ValueString;
                
                SymbolTable localSymbols = new SymbolTable(symbolTable);
                foreach (var parameter in symbolTable.functionTable[functionName].arguments)
                {
                    localSymbols.AddParameter(parameter.argName, parameter.argType);
                }

                var ilMeth = symbolTable.functionTable[functionName].GetILGenerator();
                GenStmt(stmt.ChildNodes[1], ilMeth, localSymbols);
                ilMeth.Emit(OpCodes.Ret);
            }
            else if (stmt.Term.Name == "result")
            {
                GenExpr(stmt.ChildNodes[1], TypeOfExpr(stmt.ChildNodes[1], symbolTable), il, symbolTable);
                var result = il.DeclareLocal(TypeOfExpr(stmt.ChildNodes[1], symbolTable));
                il.Emit(OpCodes.Stloc, result);
                il.Emit(OpCodes.Ldloc, result);
                il.Emit(OpCodes.Ret, result);
            }
            else if (stmt.Term.Name == "functionCall" | stmt.Term.Name == "memberCall")
            {
                GenExpr(stmt, null, il, symbolTable);
            }
            else if (stmt.Term.Name == "ifBlock")
            {
                Label ifTrue = il.DefineLabel();
                Label ifFalse = il.DefineLabel();
                Label endLabel = il.DefineLabel();

                GenExpr(stmt.ChildNodes[0], typeof(bool), il, symbolTable);//expression to check if true
                il.Emit(OpCodes.Brtrue, ifTrue);//if true then jump to true block
                il.Emit(OpCodes.Br, ifFalse);//otherwise jump to false block

                il.MarkLabel(ifTrue);//true block
                GenStmt(stmt.ChildNodes[1], il, symbolTable);
                il.Emit(OpCodes.Br, endLabel);//jump to after false block

                il.MarkLabel(ifFalse);//false block
                if (stmt.ChildNodes[2].ChildNodes.Count > 0)//then there's an else-if, this takes place in the else section
                {
                    ParseTreeNode elseBlockStmt = stmt.ChildNodes[2];//Turn the elsif to an inner if statement
                    elseBlockStmt.ChildNodes.Add(stmt.ChildNodes[3]);//Move the optional else statement to the inner if statement
                    elseBlockStmt.Term.Name = "ifBlock";

                    GenStmt(elseBlockStmt, il, symbolTable);
                }
                else if (stmt.ChildNodes[3].ChildNodes.Count > 0)
                    GenStmt(stmt.ChildNodes[3].ChildNodes[0], il, symbolTable);//generate expresson for false section, otherwise the label will be at the same spot as the end

                il.MarkLabel(endLabel);//the end of the if statement
            }
            else if (stmt.Term.Name == "loop")
            {
                Label beginLoop = il.DefineLabel();
                Label endLoop = il.DefineLabel();
                il.MarkLabel(beginLoop);
                GenStmt(stmt.ChildNodes[0], il, symbolTable, endLoop);
                il.Emit(OpCodes.Br, beginLoop);
                il.MarkLabel(endLoop);
            }
            else if (stmt.Term.Name == "forLoop")
            {
                il.BeginScope();

                Label beginLoop = il.DefineLabel();
                Label endLoop = il.DefineLabel();
                LocalBuilder i = il.DeclareLocal(typeof(int));
                string identName = stmt.ChildNodes[1].Token.ValueString;
                symbolTable.AddLocal(identName, i);
                symbolTable.AddLocal("___endLoop", il.DeclareLocal(typeof(int)));
                if (stmt.ChildNodes[2].ChildNodes.Count == 1)//then an identifier is used as a range, or char. We just fail for now
                    throw new NotImplementedException();
                else
                {
                    GenExpr(stmt.ChildNodes[2].ChildNodes[0], typeof(int), il, symbolTable);
                    symbolTable.Store(identName, typeof(int), il);
                    GenExpr(stmt.ChildNodes[2].ChildNodes[1], typeof(int), il, symbolTable);
                    symbolTable.Store("___endLoop", typeof(int), il);
                }
                il.MarkLabel(beginLoop);
                GenStmt(stmt.ChildNodes[4], il, symbolTable, endLoop);
                symbolTable.PushVar(identName, il);
                il.Emit(OpCodes.Ldc_I4_1);
                if (stmt.ChildNodes[3].ChildNodes.Count > 0)//then there is a decreasing statement, so do decreasing
                    il.Emit(OpCodes.Sub);
                else
                    il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Dup);
                symbolTable.Store(identName, typeof(int), il);
                symbolTable.PushVar("___endLoop", il);

                if (stmt.ChildNodes[3].ChildNodes.Count > 0)//then there is a decreasing statement, so do decreasing
                    il.Emit(OpCodes.Bge, beginLoop);
                else
                    il.Emit(OpCodes.Ble, beginLoop);
                il.MarkLabel(endLoop);
                symbolTable.RemoveLocal(identName);
                symbolTable.RemoveLocal("___endLoop");

                il.EndScope();
            }
            else
            {
                throw new System.Exception("don't know how to gen a " + stmt.Term.Name);
            }
        }

        private void GenExpr(ParseTreeNode expr, System.Type expectedType, ILGenerator il, SymbolTable symbolTable)
        {
            Type deliveredType;
            if (expr.Term.Name == "stringLiteral")
            {
                deliveredType = typeof(string);
                il.Emit(OpCodes.Ldstr, expr.Token.ValueString);
            }
            else if (expr.Term.Name == "number")
            {
                if (expr.Token.Value is int)
                {
                    deliveredType = typeof(int);
                    il.Emit(OpCodes.Ldc_I4, (int)expr.Token.Value);
                }
                else
                {
                    deliveredType = typeof(double);
                    il.Emit(OpCodes.Ldc_R8, float.Parse(expr.Token.ValueString));
                }
            }
            else if (expr.Term.Name == "binExpr")
            {
                Type innerExpectedType = TypeOfAny(symbolTable, expr.ChildNodes[0], expr.ChildNodes[2]);
                if (new string[] { "=", ">", "<", ">=", "<=", "!=", "~=", "not=", "and", "or", "xor" }.Contains(expr.ChildNodes[1].Term.Name))
                {
                    deliveredType = typeof(bool);
                }
                else
                {
                    deliveredType = innerExpectedType;
                }
                GenExpr(expr.ChildNodes[0], innerExpectedType, il, symbolTable);
                GenExpr(expr.ChildNodes[2], innerExpectedType, il, symbolTable);
                if (deliveredType == typeof(bool))
                {
                    switch (expr.ChildNodes[1].Term.Name)
                    {
                        case "=":
                            il.Emit(OpCodes.Ceq);
                            break;
                        case "<":
                            il.Emit(OpCodes.Clt);
                            break;
                        case ">":
                            il.Emit(OpCodes.Cgt);
                            break;
                        case "<=":
                            il.Emit(OpCodes.Cgt);
                            il.Emit(OpCodes.Not);
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.And);
                            break;
                        case ">=":
                            il.Emit(OpCodes.Clt);
                            il.Emit(OpCodes.Not);
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.And);
                            break;
                        default:
                            throw new Exception("Unrecognized operator " + expr.ChildNodes[1].Term.Name);
                    }
                }
                else if (deliveredType == typeof(string))
                {
                    switch (expr.ChildNodes[1].Term.Name)
                    {
                        case "+":
                            il.Emit(OpCodes.Call, typeof(System.String).GetMethod("Concat", new System.Type[] { typeof(string), typeof(string) }));
                            break;
                        default:
                            throw new Exception("Unrecognized operator " + expr.ChildNodes[1].Term.Name);
                    }
                }
                else
                {
                    switch (expr.ChildNodes[1].Term.Name)
                    {
                        case "+":
                            il.Emit(OpCodes.Add);
                            break;
                        case "*":
                            il.Emit(OpCodes.Mul);
                            break;
                        case "-":
                            il.Emit(OpCodes.Sub);
                            break;
                        case "/":
                            il.Emit(OpCodes.Div);
                            break;
                        case "div":
                            il.Emit(OpCodes.Div);
                            expectedType = typeof(int);
                            break;
                        case "mod":
                            il.Emit(OpCodes.Rem);
                            break;
                        default:
                            throw new Exception("Unrecognized operator " + expr.ChildNodes[1].Term.Name);
                    }
                }
            }
            else if (expr.Term.Name == "identifier")
            {
                string ident = expr.Token.ValueString;
                symbolTable.PushVar(ident, il);
                deliveredType = TypeOfExpr(expr, symbolTable);
                if (deliveredType == typeof(float))
                {
                    throw new NotImplementedException();
                }
            }
            else if (expr.Term.Name == "functionCall"|expr.Term.Name=="memberCall")
            {
                deliveredType = TypeOfExpr(expr, symbolTable);
                if (deliveredType == typeof(float))
                {
                    throw new NotImplementedException();
                }

                string funcName = GetIdentifier(expr);
                if (!symbolTable.functionTable.ContainsKey(funcName))
                {
                    if (symbolTable.HasVar(funcName) && symbolTable.TypeOfVar(funcName).IsArray)
                    {
                        //this is an array, return the appropriate value
                        symbolTable.PushVar(funcName, il);
                        if (expr.ChildNodes[1].ChildNodes.Count > 1)
                            throw new NotImplementedException("Multi-Dimensional arrays are not yet supported");

                        this.GenExpr(expr.ChildNodes[1].ChildNodes[0], typeof(int), il, symbolTable);

                        il.Emit(OpCodes.Stelem, symbolTable.TypeOfVar(funcName).GetElementType());
                    }
                    else
                        throw new System.Exception("undeclared function or procedure '" + funcName + "'");
                }
                else
                {
                    var parameters = symbolTable.functionTable[funcName].arguments;
                    int currentArgument = 0;
                    foreach (var arg in GetArgs(expr))//expr.ChildNodes[1].ChildNodes)
                    {
                        this.GenExpr(arg, parameters[currentArgument].argType, il, symbolTable);
                        currentArgument++;
                    }
                    il.Emit(OpCodes.Call, symbolTable.functionTable[funcName].methodDefinition);
                }
            }
            else if (expr.Term.Name == "initExpr")
            {
                deliveredType = TypeOfAny(symbolTable, expr.ChildNodes[0].ChildNodes.ToArray());

                int arraySize = expr.ChildNodes[0].ChildNodes.Count;
                //LocalBuilder paramValues = il.DeclareLocal(deliveredType.MakeArrayType());
                //paramValues.SetLocalSymInfo("parameters");
                il.Emit(OpCodes.Ldc_I4_S, arraySize);
                il.Emit(OpCodes.Newarr, deliveredType);
                //il.Emit(OpCodes.Stloc, paramValues);
                for (int i = 0; i < expr.ChildNodes[0].ChildNodes.Count; i++)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    GenExpr(expr.ChildNodes[0].ChildNodes[i], deliveredType, il, symbolTable);
                    il.Emit(OpCodes.Stelem, deliveredType);
                }
                deliveredType = deliveredType.MakeArrayType();
            }
            else if (expr.Term.Name == "skip")
            {
                deliveredType = typeof(string);
                il.Emit(OpCodes.Ldstr, Environment.NewLine);
            }
            else
            {
                throw new System.Exception("don't know how to generate " + expr.Term.Name);
            }
            if (deliveredType != expectedType)
            {
                if (deliveredType == typeof(int) &&
                    expectedType == typeof(string))
                {
                    il.Emit(OpCodes.Box, typeof(int));
                    il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
                }
                else if (deliveredType == typeof(double) && expectedType == typeof(string))
                {
                    il.Emit(OpCodes.Box, typeof(double));
                    il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
                }
                else if (expectedType == null)//if the expected type is null then it doesn't matter what you give it
                {
                    
                }
                else
                {
                    throw new System.Exception("can't coerce a " + deliveredType.Name + " to a " + expectedType.Name);
                }
            }
        }

        static public Type TypeOfExpr(ParseTreeNode expr, SymbolTable symbolTable)
        {
            if (expr.Term.Name == "stringLiteral")
            {
                return typeof(string);
            }
            if (expr.Term.Name == "number")
            {
                if (expr.Token.Value is int)
                {
                    return typeof(int);
                }
                else
                {
                    return typeof(double);
                }
            }
            else if (expr.Term.Name == "binExpr")
            {
                //Type type1 = TypeOfExpr(expr.ChildNodes[0], symbolTable);
                //Type type2 = TypeOfExpr(expr.ChildNodes[2], symbolTable);
                return TypeOfAny(symbolTable, expr.ChildNodes[0], expr.ChildNodes[2]);
                //if (type1 == typeof(float) || type2 == typeof(float))
                //    return typeof(float);
                //return typeof(int);
            }
            else if (expr.Term.Name == "identifier")
            {
                string ident = expr.Token.ValueString;
                return symbolTable.TypeOfVar(ident);
            }
            else if (expr.Term.Name == "functionCall")
            {
                string funcName = expr.ChildNodes[0].Token.ValueString;
                if (!symbolTable.functionTable.ContainsKey(funcName))
                {
                    //it might be an array, so we should check for that
                    if (symbolTable.HasVar(funcName) && symbolTable.TypeOfVar(funcName).IsArray)
                    {
                        //it is an array, so just return the appropriate type for the array
                        return symbolTable.TypeOfVar(funcName).GetElementType();
                    }
                    else//nope, throw an error
                        throw new System.Exception("undeclared function or procedure '" + funcName + "'");
                }
                return symbolTable.functionTable[funcName].methodDefinition.ReturnType;
            }
            else if (expr.Term.Name == "memberCall")
            {
                return symbolTable.functionTable[GetIdentifier(expr)].methodDefinition.ReturnType;
            }
            else if (expr.Term.Name == "varType")
            {
                switch (expr.ChildNodes[0].Token.ValueString)
                {
                    case "int":
                        return typeof(int);
                    case "real":
                        return typeof(double);
                    default:
                        throw new Exception("Did not recognize type: " + expr.ChildNodes[0].Token.ValueString);
                }
            }
            else if (expr.Term.Name == "initExpr")
            {
                return TypeOfAny(symbolTable, expr.ChildNodes[0].ChildNodes.ToArray()).MakeArrayType();
            }
            else
            {
                throw new System.Exception("don't know how to calculate the type of " + expr.Term.Name);
            }
        }
        /// <summary>
        /// This method takes in types and selects the best type to represent the resulting type (for instance int and real would return real)
        /// </summary>
        /// <param name="inputTypes">The types to select from</param>
        /// <returns>The best type to represent values from all the input types</returns>
        static Type TypeOfAny(SymbolTable symbolTable, params ParseTreeNode[] inputExprs)
        {
            Type bestType=null;
            foreach (var inputExpr in inputExprs)
            {
                Type inputType = TypeOfExpr(inputExpr, symbolTable);
                if (bestType == null)
                    bestType = inputType;
                else if (bestType == inputType)
                {
                }
                else if (bestType == typeof(int) && inputType == typeof(float))
                {
                    bestType = typeof(float);
                }
                else
                {
                    throw new InvalidCastException(String.Format("Cannot convert type {0} to type {1}", inputType, bestType));
                }
            }
            return bestType;
        }
        private Type TypeOfTypeDeclaration(ParseTreeNode expr)
        {
            switch (expr.ChildNodes[0].Token.ValueString)
            {
                case "array":
                    return TypeOfTypeDeclaration(expr.ChildNodes[3]).MakeArrayType();
                    throw new NotImplementedException();
                case "int":
                    return typeof(int);
                case "string":
                    return typeof(string);
                case "real":
                    return typeof(double);
                default:
                    throw new System.Exception("don't know how to calculate the type of " + expr.ToString());
            }
        }
    }
}