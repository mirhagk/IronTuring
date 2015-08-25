using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    class PutNode : StatementNode
    {
        public IList<ExpressionNode> Items { get; }
        public bool NewLine { get; }
        public PutNode(IEnumerable<ExpressionNode> items, bool newLine)
        {
            Items = items.ToList();
            NewLine = newLine;

        }
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            foreach(var item in Items)
            {
                item.GenerateIL(il, st);
                il.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(object) }));
            }
            if (NewLine)
                il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { }));
        }
    }
}
