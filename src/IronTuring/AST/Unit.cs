using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    class ProgramNode : Node
    { 
        public IList<StatementNode> Statements { get; }
        public ProgramNode(IEnumerable<StatementNode> statements)
        {
            Statements = statements.ToList();
        }
    }
    class ImportSectionNode : Node { }
    class UnitNode:Node
    {
        public BlockNode Program { get; }
        public ImportSectionNode ImportSection { get; }
        public UnitNode(BlockNode program, ImportSectionNode importSection)
        {

            Program = program;
            ImportSection = importSection;
        }
        public void CreateProgram(string moduleName)
        {
            AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(moduleName));
            var asmb = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);

            ModuleBuilder modb = asmb.DefineDynamicModule(moduleName);

            var type = modb.DefineType("$program");

            var symbolTable = new SymbolTable(type);


            symbolTable.AddFunctionHeader("$main", MethodAttributes.Static, null, new FunctionDefinition.Argument[] { },"$program");

            var il = symbolTable.functionTable["$main"].GetILGenerator();
            
            Program.GenerateIL(il, symbolTable);

            il.Emit(OpCodes.Ret);

            modb.CreateGlobalFunctions();
            asmb.SetEntryPoint(symbolTable.functionTable["$main"].methodDefinition);

            symbolTable.typeTable.types[0].typeBuilder.CreateType();

            asmb.Save(moduleName);
        }
    }
}
