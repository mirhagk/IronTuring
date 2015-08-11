using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    class Loop : BlockStatement
    {
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            Label beginLoop = il.DefineLabel();
            Label endLoop = il.DefineLabel();
            il.MarkLabel(beginLoop);
            var newTable = new SymbolTable(st, endLoop);
            foreach (var statment in Block)
            {
                statment.GenerateIL(il, st);
            }
            il.Emit(OpCodes.Br, beginLoop);
            il.MarkLabel(endLoop);
        }
    }
}
