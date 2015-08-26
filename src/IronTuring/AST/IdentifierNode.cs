using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    class IdentifierNode:ExpressionNode
    {
        public string Name { get; }
        public IdentifierNode(string name)
        {
            Name = name;
        }
        public override Type TypeOfExpression()
        {
            throw new NotImplementedException();
        }
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            il.Emit(OpCodes.Ldloc, st.Locals[Name].LocalIndex);
        }
    }
}
