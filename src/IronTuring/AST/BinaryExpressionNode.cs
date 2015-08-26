using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    class BinaryExpressionNode:ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Left { get; }
        public ExpressionNode Right { get; }
        public BinaryExpressionNode(string operatorSymbol, ExpressionNode left, ExpressionNode right)
        {
            Operator = operatorSymbol;
            Left = left;
            Right = right;
        }
        public override Type TypeOfExpression()
        {
            return Left.TypeOfExpression();
        }
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            Left.GenerateIL(il, st);
            Right.GenerateIL(il, st);
            switch (Operator)
            {
                case "+":
                    il.Emit(OpCodes.Add);
                    break;
                default: throw new NotSupportedException($"Operator {Operator} is not supported");
            }
        }
    }
}
