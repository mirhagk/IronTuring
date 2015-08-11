using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring
{    /// <summary>
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
            var meth = typeBuilder.DefineMethod(functionName, methodAttributes, CallingConventions.Standard, returnType, parameters.Select(x => x.argType).ToArray());
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
}
