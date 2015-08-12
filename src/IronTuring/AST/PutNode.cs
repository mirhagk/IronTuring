using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
