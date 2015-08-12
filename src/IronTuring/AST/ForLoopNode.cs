using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronTuring.AST
{
    class ForLoopNode : BlockStatementNode
    {
        public abstract class LoopRange { }
        public class LiteralRange : LoopRange {
        public int Lower { get; }
        public int Upper { get; }
            public LiteralRange(int upper, int lower)
            {
                Upper = upper;
                Lower = lower;
            }
        }
        public string Index { get; }
        public LoopRange Range { get; }
        public bool Decreasing { get; }
        public ForLoopNode(IEnumerable<StatementNode> block, LoopRange range, bool decreasing) : base(block)
        {
            Range = range;
            Decreasing = decreasing;
        }
    }
}
