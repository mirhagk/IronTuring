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
    /// The point of this class is to gather together the local variables available to a scope, as well as the arguments it can access.
    /// </summary>
    public class SymbolTable
    {
        public Label? ExitScope { get; }
        public TypeTable typeTable = new TypeTable();
        public Dictionary<string, LocalBuilder> locals = new Dictionary<string, LocalBuilder>();
        List<Tuple<string, Type>> parameters = new List<Tuple<string, Type>>();
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
        public SymbolTable(SymbolTable parentTable, Label? exitScope = null)
        {
            this.parentTable = parentTable;
            ExitScope = exitScope;
        }
        public List<TypeBuilder> types = new List<TypeBuilder>();
        public void AddLocal(string ident, LocalBuilder localBuilder) => locals.Add(ident, localBuilder);
        public void RemoveLocal(string identName) => locals.Remove(identName);
        public bool HasVar(string ident)
        {
            if (locals.ContainsKey(ident))
                return true;
            if (parameters.Exists((x) => x.Item1 == ident))
                return true;
            return false;
        }
        public void AddParameter(string ident, Type type) => parameters.Add(Tuple.Create(ident, type));
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
                throw new Exception("undeclared variable '" + ident + "'");
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
            throw new Exception("undeclared variable '" + ident + "'");
        }
        public void Store(string name, Type type, ILGenerator il)
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
                    throw new Exception("'" + name + "' is of type " + locb.LocalType.Name + " but attempted to store value of type " + type.Name);
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
            throw new Exception("undeclared variable '" + name + "'");
        }
        int lastAnonNum = 0;
        public Type CreateNewType(ParseTreeNode stmt, string name = null, string typeName = "__Program")
        {
            if (name == null)
            {
                lastAnonNum++;
                name = string.Format("__Anonymous{0}", lastAnonNum);
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
        }
    }
}
