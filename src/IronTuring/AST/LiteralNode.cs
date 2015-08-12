using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronTuring.AST
{
    class LiteralNode<T> : ExpressionNode
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
    }
}
