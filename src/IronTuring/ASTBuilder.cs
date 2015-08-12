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
        IEnumerable<T> GetList<T>(ParseTreeNode node, Func<ParseTreeNode,T> function)
        {
            if (node.ChildNodes.Count == 0)
                yield break;
            yield return function(node.ChildNodes[0]);
            foreach (var n in GetList(node.ChildNodes[1], function))
                yield return n;
        }
        IEnumerable<StatementNode> StatementList(ParseTreeNode node) => GetList(node, Statement);
        IEnumerable<ExpressionNode> ExpressionList(ParseTreeNode node) => GetList(node, Expression);
        ProgramNode Program(ParseTreeNode node) => new ProgramNode(StatementList(node));
        StatementNode Statement(ParseTreeNode node)
        {
            //unwrap the statement
            node = node.ChildNodes[0];
            if (node.Term.Name == "loop")
            {
                return new LoopNode(StatementList(node.ChildNodes[0]));
            }
            if (node.Term.Name == "io")
            {
                if (node.ChildNodes[0].Term.Name == "put")
                {

                    return new PutNode(
                        ExpressionList(node.ChildNodes[2]).Append(Expression(node.ChildNodes[1])),
                        node.ChildNodes.Count > 3
                        );
                }
            }
            throw new NotImplementedException();
        }
        ExpressionNode Expression(ParseTreeNode node)
        {
            if (node.Term.Name == "putItem")
                return Expression(node.ChildNodes[0]);
            if (node.Term.Name == "stringLiteral")
                return new StringLiteralNode(node.Token.ValueString);

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
