using IronTuring.AST;
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronTuring
{
    class ASTBuilder
    {
        ProgramNode Program(ParseTreeNode node)
        {
            return new ProgramNode(StatementList(node));
        }
        IEnumerable<StatementNode> StatementList(ParseTreeNode node)
        {
            if (node.ChildNodes.Count == 0)
                yield break;
            yield return Statement(node.ChildNodes[0]);
            foreach(var statement in StatementList(node.ChildNodes[1]))
            {
                yield return statement;
            }
        }
        StatementNode Statement(ParseTreeNode node)
        {
            //unwrap the statement
            node = node.ChildNodes[0];
            if (node.Term.Name == "loop")
            {
                return new LoopNode(StatementList(node.ChildNodes[0]));
                //return new LoopNode
            }
            else if (node.Term.Name == "io")
            {

            }
            throw new NotImplementedException();
        }
        ImportSectionNode ImportSection(ParseTreeNode node)
        {
            return new ImportSectionNode();
        }
        public Node BuildAST(ParseTreeNode stmt)
        {
            if (stmt.Term.Name == "unit")
            {
                return new UnitNode(Program(stmt.ChildNodes[1]), ImportSection(stmt.ChildNodes[0]));
            }
            else if (stmt.Term.Name == "program")
            {
                throw new NotImplementedException();
            }
            else
                throw new NotSupportedException($"Can't handle {stmt.Term.Name}");
        }
    }
}
