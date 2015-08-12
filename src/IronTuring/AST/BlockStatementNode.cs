using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronTuring.AST
{
    class BlockStatementNode:StatementNode
    {
        public IList<StatementNode> Block { get; }
        public BlockStatementNode(IEnumerable<StatementNode> block)
        {
            Block = block.ToList();
        }
    }
    class BlockNode : StatementNode
    {
        public IList<StatementNode> Block { get; }
        public BlockNode(IEnumerable<StatementNode> block)
        {
            Block = (block as IList<StatementNode>) ?? block.ToList();
        }
    }
}
