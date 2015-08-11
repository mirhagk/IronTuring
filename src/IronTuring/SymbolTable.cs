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
    /// <summary>
    /// A function definition. Stores all of the required information to generate and call the functions
    /// </summary>
    public class FunctionDefinition
    {
        public class Argument
        {
            public string argName;
            public Type argType;
        }
        public MethodInfo methodDefinition
        {
            get
            {
                if (methodBuilder != null)
                    return methodBuilder;
                return methodInfo;
            }
        }
        MethodBuilder methodBuilder;
        MethodInfo methodInfo;
        public List<Argument> arguments;
        public FunctionDefinition(MethodBuilder methodBuilder, List<Argument> arguments)
        {
            this.methodBuilder = methodBuilder;
            this.arguments = arguments;
        }
        public FunctionDefinition(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }
        public ILGenerator GetILGenerator()
        {
            if (methodBuilder == null)
                throw new Exception("Can't get generator for imported function");
            return methodBuilder.GetILGenerator();
        }
    }
    public class FunctionTable
    {
        Dictionary<string, Type> importedModules = new Dictionary<string, Type>();
        public Dictionary<string, FunctionDefinition> functionTable = new Dictionary<string, FunctionDefinition>();
        public void AddHeader(string functionName, FunctionDefinition functionDefinition)
        {
            functionTable.Add(functionName, functionDefinition);
        }
        public FunctionDefinition this[string functionName]
        {
            get
            {
                if (this.functionTable.ContainsKey(functionName))
                    return this.functionTable[functionName];
                if (functionName.Contains("."))//then it is a member call, and we should look in importedModules
                {
                    int lastPeriod = functionName.LastIndexOf('.');
                    string className = functionName.Substring(0, lastPeriod);
                    string funcName = functionName.Substring(lastPeriod + 1);
                    FunctionDefinition def = new FunctionDefinition(importedModules[className].GetMethod(funcName));
                    return def;
                }
                throw new Exception(String.Format("Function {0} has not been declared", functionName));
            }
        }
        public bool ContainsKey(string functionName)
        {
            if (this.functionTable.ContainsKey(functionName))
                return true;
            if (functionName.Contains("."))//then it is a member call, and we should look in importedModules
            {
                int lastPeriod = functionName.LastIndexOf('.');
                string className = functionName.Substring(0, lastPeriod);
                string funcName = functionName.Substring(lastPeriod + 1);
                if (importedModules.ContainsKey(className))
                {
                    return importedModules[className].GetMethod(funcName) != null;
                }
            }
            return false;
        }


        public void AddLibrary(string name, string location = null)
        {
            if (location == null)
                location = name + ".dll";
            var assembly = System.Reflection.Assembly.LoadFrom(location);
            var type = assembly.GetType(name);
            TypeBuilder typeBuild;
            importedModules.Add(name, type);
        }
    }
    /// <summary>
    /// A type definition. Encapsulates types that might be either imported or defined by the program
    /// </summary>
    public class TypeDefintion
    {
        public TypeBuilder typeBuilder;
        Type internalType;
        public FunctionTable functionTable = new FunctionTable();
        public Type type
        {
            get
            {
                if (typeBuilder != null)
                    //throw new NotSupportedException();
                    return typeBuilder;
                return internalType;
            }
        }

        public TypeDefintion(Type type)
        {
            this.internalType = type;
        }
        public TypeDefintion(TypeBuilder typeBuilder)
        {
            this.typeBuilder = typeBuilder;
        }

        internal void AddFunctionHeader(string functionName, MethodAttributes methodAttributes, Type returnType, FunctionDefinition.Argument[] parameters)
        {
            var meth=typeBuilder.DefineMethod(functionName, methodAttributes, CallingConventions.Standard, returnType, parameters.Select(x=>x.argType).ToArray());
            for (int i = 0; i < parameters.Length; i++)
            {
                meth.DefineParameter(i + 1, ParameterAttributes.In, parameters[i].argName);
            }
            var function = new FunctionDefinition(meth, parameters.ToList());
            functionTable.AddHeader(functionName, function);           
        }
    }
    public class TypeTable
    {
        public List<TypeDefintion> types = new List<TypeDefintion>();
    }
    /// <summary>
    /// The point of this class is to gather together the local variables available to a scope, as well as the arguments it can access.
    /// </summary>
    public class SymbolTable
    {
        public TypeTable typeTable = new TypeTable();
        public Dictionary<string, LocalBuilder> locals = new Dictionary<string, LocalBuilder>();
        public List<Tuple<string, Type>> parameters = new List<Tuple<string, Type>>();
        SymbolTable parentTable = null;
        public FunctionTable functionTable
        {
            get
            {
                var programType = typeTable.types.SingleOrDefault(t => t.type.Name == "__Program");
                if (programType != null)
                    return programType.functionTable;
                return parentTable.functionTable;
            }
        }
        public SymbolTable(TypeBuilder mainClass)
        {
            typeTable.types.Add(new TypeDefintion(mainClass));
        }
        public SymbolTable(SymbolTable parentTable)
        {
            this.parentTable = parentTable;
        }
        public List<TypeBuilder> types = new List<TypeBuilder>();
        public void AddLocal(string ident, LocalBuilder localBuilder)
        {
            locals.Add(ident, localBuilder);
        }
        public void RemoveLocal(string identName)
        {
            locals.Remove(identName);
        }
        public bool HasVar(string ident)
        {
            if (locals.ContainsKey(ident))
                return true;
            if (parameters.Exists((x) => x.Item1 == ident))
                return true;
            return false;
        }
        public void AddParameter(string ident, Type type)
        {
            parameters.Add(new Tuple<string, Type>(ident, type));
        }
        public void PushVar(string ident, ILGenerator il)
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
        int lastAnonNum = 0;
        public Type CreateNewType(ParseTreeNode stmt, string name = null, string typeName = "__Program")
        {
            if (name == null)
            {
                lastAnonNum++;
                name = String.Format("__Anonymous{0}", lastAnonNum);
            }
            var t = typeTable.types.Where(x => x.type.Name == typeName).First().typeBuilder.DefineNestedType(name);
            //this should be passed the vartype and it'll create a type from it
            List<Tuple<string, Type>> fields = new List<Tuple<string, Type>>();
            ParseTreeNode fieldGroup = stmt;
            while (true)
            {
                if (fieldGroup.ChildNodes.Count == 0)
                    break;
                Type fieldType = CodeGen.TypeOfExpr(fieldGroup.ChildNodes[1].ChildNodes[0], this);
                foreach (var iden in fieldGroup.ChildNodes[0].ChildNodes)
                {
                    t.DefineField(iden.Token.ValueString, fieldType, FieldAttributes.Public);
                }

                fieldGroup = fieldGroup.ChildNodes[2];
            }
            throw new NotImplementedException();
            //var newType = mainProgram.DefineNestedType(name);
            
        }

        internal void AddFunctionHeader(string functionName, MethodAttributes methodAttributes, Type returnType, FunctionDefinition.Argument[] parameters, string typeName="__Program")
        {
            var type = typeTable.types.Single(x => x.type.Name == typeName);
            type.AddFunctionHeader(functionName, methodAttributes, returnType, parameters);
            //throw new NotImplementedException();
        }
    }
}
