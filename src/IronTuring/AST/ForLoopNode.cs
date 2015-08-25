using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronTuring.AST
{
    class ForLoopNode : StatementNode
    {
        public abstract class LoopRange { }
        public class LiteralRange : LoopRange
        {
            public int Lower { get; }
            public int Upper { get; }
            public LiteralRange(int upper, int lower)
            {
                Upper = upper;
                Lower = lower;
            }
        }
        public BlockNode Block { get; }
        public string Index { get; }
        public LoopRange Range { get; }
        public bool Decreasing { get; }
        public int ByAmount { get; }
        public ForLoopNode(BlockNode block, LoopRange range, bool decreasing, string index, int byAmount = 1)
        {
            Block = block;
            Range = range;
            Decreasing = decreasing;
            Index = index;
            ByAmount = byAmount;
        }
        public override IEnumerable<Node> Reduce()
        {
            var statements = new List<StatementNode>();
            //statements.Add(new );
            return base.Reduce();
        }
    }
}
