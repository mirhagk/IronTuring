using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    class LoopNode : StatementNode
    {
        BlockNode Block { get; }
        public LoopNode(BlockNode block) { Block = block; }
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            Label beginLoop = il.DefineLabel();
            Label endLoop = il.DefineLabel();
            il.MarkLabel(beginLoop);
            var newTable = new SymbolTable(st, endLoop);
            Block.GenerateIL(il, newTable);

            il.Emit(OpCodes.Br, beginLoop);
            il.MarkLabel(endLoop);
        }
    }
}
