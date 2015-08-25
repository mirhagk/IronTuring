using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    abstract class LiteralNode<T> : ExpressionNode
    {
        public virtual T Value { get; }
    }
    class StringLiteralNode : LiteralNode<string>
    {
        public override string Value { get; }
        public StringLiteralNode(string value)
        {
            Value = value;
        }
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            il.Emit(OpCodes.Ldstr, Value);
        }
    }
}
