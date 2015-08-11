using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

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
        public MethodInfo methodDefinition => methodBuilder ?? methodInfo;
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
        public void AddHeader(string functionName, FunctionDefinition functionDefinition) => functionTable.Add(functionName, functionDefinition);
        public FunctionDefinition this[string functionName]
        {
            get
            {
                if (functionTable.ContainsKey(functionName))
                    return functionTable[functionName];
                if (functionName.Contains("."))//then it is a member call, and we should look in importedModules
                {
                    int lastPeriod = functionName.LastIndexOf('.');
                    string className = functionName.Substring(0, lastPeriod);
                    string funcName = functionName.Substring(lastPeriod + 1);
                    FunctionDefinition def = new FunctionDefinition(importedModules[className].GetMethod(funcName));
                    return def;
                }
                throw new Exception($"Function {functionName} has not been declared");
            }
        }
        public bool ContainsKey(string functionName)
        {
            if (functionTable.ContainsKey(functionName))
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
            var assembly = Assembly.LoadFrom(location);
            var type = assembly.GetType(name);
            TypeBuilder typeBuild;
            importedModules.Add(name, type);
        }
    }
}
