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
public sealed class CodeGenOld
{
    /// <summary>
    /// A function definition. Stores all of the required information to generate and call the functions
    /// </summary>
    class FunctionDefinition
    {
        public class Argument
        {
            public string argName;
            public Type argType;
        }
        public MethodBuilder methodDefinition;
        public List<Argument> arguments;
    }
    /// <summary>
    /// The point of this class is to gather together the local variables available to a scope, as well as the arguments it can access.
    /// </summary>
    class SymbolTable
    {
        public Dictionary<string, LocalBuilder> locals = new Dictionary<string, LocalBuilder>();
        public List<Tuple<string, Type>> parameters = new List<Tuple<string, Type>>();

        public void AddLocal(string ident, LocalBuilder localBuilder)
        {
            locals.Add(ident, localBuilder);
        }
        public void AddParameter(string ident, Type type)
        {
            parameters.Add(new Tuple<string, Type>(ident, type));
        }
        public void PushVar(string ident, ref ILGenerator il)
        {
            if (!locals.ContainsKey(ident))
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i].Item1 == ident)
                    {
                        il.Emit(OpCodes.Ldarg, i);
                        return;
                    }
                }
                throw new System.Exception("undeclared variable '" + ident + "'");
            }
            il.Emit(OpCodes.Ldloc, locals[ident]);
        }
        public Type TypeOfVar(string ident)
        {
            if (locals.ContainsKey(ident))
            {
                LocalBuilder locb = locals[ident];
                return locb.LocalType;
            }   
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Item1 == ident)
                {
                    return parameters[i].Item2;
                }
            }
            throw new System.Exception("undeclared variable '" + ident + "'");
        }
        public void Store(string name, System.Type type, ILGenerator il)
        {
            if (locals.ContainsKey(name))
            {
                LocalBuilder locb = locals[name];

                if (locb.LocalType == type)
                {
                    il.Emit(OpCodes.Stloc, locals[name]);
                }
                else
                {
                    throw new System.Exception("'" + name + "' is of type " + locb.LocalType.Name + " but attempted to store value of type " + type.Name);
                }
                return;
            }
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Item1 == name)
                {
                    il.Emit(OpCodes.Starg, i);
                    return;
                }
            }
            throw new System.Exception("undeclared variable '" + name + "'");
        }
    }
    Dictionary<string, FunctionDefinition> functionTable = new Dictionary<string, FunctionDefinition>();
    TypeBuilder mainProgram;

    public CodeGenOld(ParseTreeNode stmt, string moduleName)
    {
        if (Path.GetFileName(moduleName) != moduleName)
        {
            throw new System.Exception("can only output into current directory!");
        }

        AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(moduleName));
        AssemblyBuilder asmb = System.AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);
        ModuleBuilder modb = asmb.DefineDynamicModule(moduleName);
        mainProgram = modb.DefineType("Program");
        var mainArgs = new List<Tuple<string,Type>>();
        var mainProgramDef = new FunctionDefinition()
        {
            methodDefinition=mainProgram.DefineMethod("Main", MethodAttributes.Static, typeof(void), System.Type.EmptyTypes),
            arguments=new List<FunctionDefinition.Argument>()
        };
        functionTable.Add("Main",mainProgramDef);
        SymbolTable symbolTable = new SymbolTable();

        // CodeGenerator
        var il = functionTable["Main"].methodDefinition.GetILGenerator();

        // Go Compile!
        this.GenStmt(stmt, il, symbolTable);

        il.Emit(OpCodes.Ldstr, "Press any key to exit the program...");
        il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
        il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadKey", new Type[] { }));

        il.Emit(OpCodes.Ret);
        mainProgram.CreateType();
        modb.CreateGlobalFunctions();
        asmb.SetEntryPoint(functionTable["Main"].methodDefinition);
        asmb.Save(moduleName);
        foreach (var symbol in symbolTable.locals)
        {
            Console.WriteLine("{0}: {1}", symbol.Key, symbol.Value);
        }
        symbolTable = null;
        il = null;
    }

    private void GenStmt(ParseTreeNode stmt, ILGenerator il, SymbolTable symbolTable)
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
                localType = this.TypeOfTypeDeclaration(stmt.ChildNodes[2].ChildNodes[1]);
            }
            else
            {
                localType = this.TypeOfExpr(stmt.ChildNodes[2].ChildNodes[1], symbolTable);
                //symbolTable.locals[stmt.ChildNodes[1].ChildNodes[0].Token.ValueString] = il.DeclareLocal(this.TypeOfExpr(stmt.ChildNodes[2].ChildNodes[1], symbolTable));
            }
            Action<string> generateAssign = null;
            ParseTreeNode assign = stmt.ChildNodes.Where(x => x.Term.Name == "setEqual").SingleOrDefault();
            // set the initial value
            if (assign != null)
            {
                generateAssign = new Action<string>(name =>
                {
                    this.GenExpr(assign.ChildNodes[1], symbolTable.locals[name].LocalType, il, symbolTable);
                    symbolTable.Store(name, this.TypeOfExpr(assign.ChildNodes[1], symbolTable), il);
                });
            }
            var variableIden = stmt.ChildNodes[1];
            while (true)
            {
                string name = variableIden.ChildNodes[0].Token.ValueString;
                symbolTable.AddLocal(name,il.DeclareLocal(localType));
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
                ParseTreeNode argItem = stmt.ChildNodes[1];
                while (true)
                {
                    this.GenExpr(argItem.ChildNodes[0], typeof(string), il, symbolTable);
                    il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", new System.Type[] { typeof(string) }));
                    if (argItem.ChildNodes.Count > 1)
                        argItem = argItem.ChildNodes[1];
                    else 
                        break;
                }
                il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new System.Type[] { }));
            }
        }
        else if (stmt.Term.Name == "assignment")
        {
            string ident = stmt.ChildNodes[0].Token.ValueString;
            this.GenExpr(stmt.ChildNodes[1].ChildNodes[1], this.TypeOfExpr(stmt.ChildNodes[1].ChildNodes[1], symbolTable), il, symbolTable);
            symbolTable.Store(ident, this.TypeOfExpr(stmt.ChildNodes[1].ChildNodes[1], symbolTable), il);
            //this.Store(ident, this.TypeOfExpr(stmt.ChildNodes[1].ChildNodes[1], symbolTable), ref il, symbolTable);
        }
        else if (stmt.Term.Name == "functionDefinition")
        {
            string functionName = stmt.ChildNodes[1].Token.ValueString;
            if (functionTable.ContainsKey(functionName))
            {
                throw new Exception(functionName + " has already been defined");
            }
            var parameterList = new List<FunctionDefinition.Argument>();
            List<Type> types = new List<Type>();
            SymbolTable localSymbols = new SymbolTable();
            if (stmt.ChildNodes[2].ChildNodes.Count > 0)
            {
                var currParam = stmt.ChildNodes[2].ChildNodes[0];
                while (true)
                {
                    var parameterType = TypeOfExpr(currParam.ChildNodes[0].ChildNodes[1].ChildNodes[1], symbolTable);
                    var paramIdentifier = currParam.ChildNodes[0].ChildNodes[0];
                    while (true)
                    {
                        var parameterName = paramIdentifier.ChildNodes[0].Token.ValueString;
                        parameterList.Add(new FunctionDefinition.Argument() { argName = parameterName, argType = parameterType });
                        localSymbols.AddParameter(parameterName, parameterType);
                        types.Add(parameterType);
                        if (paramIdentifier.ChildNodes.Count == 1)
                            break;
                        paramIdentifier = paramIdentifier.ChildNodes[1];
                    }
                    
                    if (currParam.ChildNodes.Count == 1)
                        break;
                    currParam = currParam.ChildNodes[1];
                }
            }
            var methodDeclaration = mainProgram.DefineMethod(functionName, MethodAttributes.Static, TypeOfExpr(stmt.ChildNodes[3].ChildNodes[1], symbolTable), types.ToArray());
            var ilMeth = methodDeclaration.GetILGenerator();
            //if (stmt.ChildNodes[2].ChildNodes.Count > 0)
            //{
            //    var currParam = stmt.ChildNodes[2].ChildNodes[0];
            //    while (true)
            //    {
            //        //localSymbols.parameters.Add(new Tuple<string, Type>(currParam.ChildNodes[0].ChildNodes[0].Token.ValueString, TypeOfExpr(currParam.ChildNodes[0].ChildNodes[1].ChildNodes[1], symbolTable)));
            //        if (currParam.ChildNodes.Count == 1)
            //            break;
            //        currParam = currParam.ChildNodes[1];
            //    }
            //}
            GenStmt(stmt.ChildNodes[4], ilMeth, localSymbols);
            ilMeth.Emit(OpCodes.Ret);
            var methodDec = new FunctionDefinition()
            {
                methodDefinition = methodDeclaration,
                arguments = parameterList
            };
            functionTable.Add(functionName, methodDec);

        }
        else if (stmt.Term.Name == "result")
        {
            GenExpr(stmt.ChildNodes[1], TypeOfExpr(stmt.ChildNodes[1], symbolTable), il, symbolTable);
            var result = il.DeclareLocal(TypeOfExpr(stmt.ChildNodes[1], symbolTable));
            il.Emit(OpCodes.Stloc, result);
            il.Emit(OpCodes.Ldloc, result);
            il.Emit(OpCodes.Ret, result);
        }
        /*
        else if (stmt is ReadInt)
        {
            this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { }, null));
            this.il.Emit(Emit.OpCodes.Call, typeof(int).GetMethod("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(string) }, null));
            this.Store(((ReadInt)stmt).Ident, typeof(int));
        }
        else if (stmt is ForLoop)
        {
            // example: 
            // for x = 0 to 100 do
            //   print "hello";
            // end;

            // x = 0
            ForLoop forLoop = (ForLoop)stmt;
            Assign assign = new Assign();
            assign.Ident = forLoop.Ident;
            assign.Expr = forLoop.From;
            this.GenStmt(assign);
            // jump to the test
            Emit.Label test = this.il.DefineLabel();
            this.il.Emit(Emit.OpCodes.Br, test);

            // statements in the body of the for loop
            Emit.Label body = this.il.DefineLabel();
            this.il.MarkLabel(body);
            this.GenStmt(forLoop.Body);

            // to (increment the value of x)
            this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[forLoop.Ident]);
            this.il.Emit(Emit.OpCodes.Ldc_I4, 1);
            this.il.Emit(Emit.OpCodes.Add);
            this.Store(forLoop.Ident, typeof(int));

            // **test** does x equal 100? (do the test)
            this.il.MarkLabel(test);
            this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[forLoop.Ident]);
            this.GenExpr(forLoop.To, typeof(int));
            this.il.Emit(Emit.OpCodes.Blt, body);
        }*/
        else
        {
            throw new System.Exception("don't know how to gen a " + stmt.Term.Name);
        }



    }

    /*private void Store(string name, System.Type type, ref ILGenerator il, SymbolTable symbolTable)
    {
        if (symbolTable.locals.ContainsKey(name))
        {
            LocalBuilder locb = symbolTable.locals[name];

            if (locb.LocalType == type)
            {
                il.Emit(OpCodes.Stloc, symbolTable.locals[name]);
            }
            else
            {
                throw new System.Exception("'" + name + "' is of type " + locb.LocalType.Name + " but attempted to store value of type " + type.Name);
            }
        }
        else
        {
            throw new System.Exception("undeclared variable '" + name + "'");
        }
    }*/

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
                deliveredType = typeof(float);
                il.Emit(OpCodes.Ldc_R4, float.Parse(expr.Token.ValueString));
            }
        }
        else if (expr.Term.Name == "binExpr")
        {
            deliveredType = TypeOfExpr(expr.ChildNodes[0], symbolTable);
            GenExpr(expr.ChildNodes[0], deliveredType, il, symbolTable);
            GenExpr(expr.ChildNodes[2], deliveredType, il, symbolTable);
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
                case "mod":
                    il.Emit(OpCodes.Rem);
                    break;
                default:
                    throw new Exception("Unrecognized operator " + expr.ChildNodes[1].Term.Name);
            }

        }
        else if (expr.Term.Name == "identifier")
        {
            string ident = expr.Token.ValueString;
            symbolTable.PushVar(ident, ref il);
            deliveredType = this.TypeOfExpr(expr, symbolTable);
        }
        else if (expr.Term.Name == "functionCall")
        {
            deliveredType = TypeOfExpr(expr, symbolTable);

            string funcName = expr.ChildNodes[0].Token.ValueString;
            if (!this.functionTable.ContainsKey(funcName))
            {
                throw new System.Exception("undeclared function or procedure '" + funcName+ "'");
            }
            var parameters = this.functionTable[funcName].arguments;
            int currentArgument = 0;
            if (expr.ChildNodes[1].ChildNodes.Count > 0)
            {//push all the arguments onto the stack

                ParseTreeNode argItem = expr.ChildNodes[1];
                while (true)
                {
                    this.GenExpr(argItem.ChildNodes[0], parameters[currentArgument].argType, il, symbolTable);
                    if (argItem.ChildNodes.Count == 1)
                        break;
                    argItem = argItem.ChildNodes[1];
                    currentArgument++;
                }
            }
            il.Emit(OpCodes.Call, this.functionTable[funcName].methodDefinition);
        }
        else
        {
            throw new System.Exception("don't know how to generate " + expr.GetType().Name);
        }
        
        if (deliveredType != expectedType)
        {
            if (deliveredType == typeof(int) &&
                expectedType == typeof(string))
            {
                il.Emit(OpCodes.Box, typeof(int));
                il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
            }
            else if (deliveredType==typeof(float)&&expectedType==typeof(string))
            {
                il.Emit(OpCodes.Box, typeof(float));
                il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
            }
            else
            {
                throw new System.Exception("can't coerce a " + deliveredType.Name + " to a " + expectedType.Name);
            }
        }
    }

    private Type TypeOfExpr(ParseTreeNode expr, SymbolTable symbolTable)
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
                return typeof(float);
            }
        }
        else if (expr.Term.Name == "binExpr")
        {
            Type type1 = TypeOfExpr(expr.ChildNodes[0], symbolTable);
            Type type2 = TypeOfExpr(expr.ChildNodes[2], symbolTable);
            if (type1 == typeof(float) || type2 == typeof(float))
                return typeof(float);
            return typeof(int);
        }
        else if (expr.Term.Name == "identifier")
        {
            string ident = expr.Token.ValueString;
            return symbolTable.TypeOfVar(ident);
        }
        else if (expr.Term.Name == "functionCall")
        {
            string funcName = expr.ChildNodes[0].Token.ValueString;
            if (!this.functionTable.ContainsKey(funcName))
            {
                throw new System.Exception("undeclared function or procedure '" + funcName + "'");
            }
            return functionTable[funcName].methodDefinition.ReturnType;
        }
        else if (expr.Term.Name == "varType")
        {
            switch (expr.ChildNodes[0].Token.ValueString)
            {
                case "int":
                    return typeof(int);
                case "real":
                    return typeof(float);
                default:
                    throw new Exception("Did not recognize type: " + expr.ChildNodes[0].Token.ValueString);
            }
        }
        else
        {
            throw new System.Exception("don't know how to calculate the type of " + expr.Term.Name);
        }
    }

    private Type TypeOfTypeDeclaration(ParseTreeNode expr)
    {
        switch (expr.ChildNodes[0].Token.ValueString )
        {
            case "array":
                return TypeOfTypeDeclaration(expr.ChildNodes[3]).MakeArrayType();
                throw new NotImplementedException();
            case "int":
                return typeof(int);
            case "string":
                return typeof(string);
            case "real":
                return typeof(float);
            default:
                throw new System.Exception("don't know how to calculate the type of " + expr.ToString());
        }
    }
}
}