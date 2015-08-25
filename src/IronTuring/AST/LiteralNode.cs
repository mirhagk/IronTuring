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
        public override Type TypeOfExpression() => typeof(T);
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
    abstract class NumberLiteralNode<T>:LiteralNode<T>
    {
    }
    static class NumberLiteralNode
    {
        public static ExpressionNode GetNumber(string number)
        {
            if (number.Contains("."))
            {

            }
            return new IntegerLiteralNode(int.Parse(number));
        }
    }
    class IntegerLiteralNode : NumberLiteralNode<int>
    {
        public override int Value { get; }
        public IntegerLiteralNode(int value)
        {
            Value = value;
        }
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            il.Emit(OpCodes.Ldc_I4, Value);
        }
    }
}
