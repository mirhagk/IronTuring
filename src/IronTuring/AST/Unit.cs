using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronTuring.AST
{
    class ProgramNode : Node
    { 
        public IList<StatementNode> Statements { get; }
        public ProgramNode(IEnumerable<StatementNode> statements)
        {
            Statements = statements.ToList();
        }
    }
    class ImportSectionNode : Node { }
    class UnitNode:Node
    {
        public ProgramNode Program { get; }
        public ImportSectionNode ImportSection { get; }
        public UnitNode(ProgramNode program, ImportSectionNode importSection)
        {
            Program = program;
            ImportSection = importSection;
        }
    }
}
