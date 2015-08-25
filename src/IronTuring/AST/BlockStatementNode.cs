using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
        public SymbolTable LocalSymbolTable { get; private set; }
        public IList<StatementNode> Block { get; }
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            LocalSymbolTable = new SymbolTable(st);
            foreach(var statement in Block)
            {
                statement.GenerateIL(il, LocalSymbolTable);
            }
        }
        public BlockNode(IEnumerable<StatementNode> block)
        {
            Block = (block as IList<StatementNode>) ?? block.ToList();
        }
    }
}
