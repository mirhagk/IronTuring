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
        BlockNode Block(ParseTreeNode node) => new BlockNode(StatementList(node));
        StatementNode Statement(ParseTreeNode node)
        {
            //unwrap the statement
            node = node.ChildNodes[0];
            if (node.Term.Name == "loop")
            {
                return new LoopNode(Block(node.ChildNodes[0]));
            }
            if (node.Term.Name == "forLoop")
            {

                return new ForLoopNode(
                    Block(node.ChildNodes[4]), 
                    GetRange(node.ChildNodes[2]), 
                    node.ChildNodes[0].ChildNodes.Count > 0,
                    Get<string>(node.ChildNodes[1]));
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
            if (node.Term.Name == "variableDeclaration")
            {
                var identifierList = GetPiece(node, "identifierList").ChildNodes.Select(c=>Get<string>(c)).ToList();
                var setEqualVal = GetPiece(node, "setEqual").ChildNodes[1];
                return new VariableNode(identifierList, Expression(setEqualVal));
            }
            throw new NotImplementedException();
        }
        ParseTreeNode GetPiece(ParseTreeNode node, string termName)
        {
            return node.ChildNodes.SingleOrDefault(n => n.Term.Name == termName);
        }
        T Get<T>(ParseTreeNode node)
        {
            return (T)node.Token.Value;
        }
        ExpressionNode Expression(ParseTreeNode node)
        {
            if (node.Term.Name == "putItem")
                return Expression(node.ChildNodes[0]);
            if (node.Term.Name == "stringLiteral")
                return new StringLiteralNode(Get<string>(node));
            if (node.Term.Name == "number")
                return NumberLiteralNode.GetNumber(node.Token.ValueString);

            throw new NotImplementedException();
        }
        ForLoopNode.LoopRange GetRange(ParseTreeNode node)
        {
            return new ForLoopNode.LiteralRange(Get<int>(node.ChildNodes[1]), Get<int>(node.ChildNodes[0]));
        }
        ImportSectionNode ImportSection(ParseTreeNode node)
        {
            return new ImportSectionNode();
        }
        public Node BuildAST(ParseTreeNode stmt)
        {
            if (stmt.Term.Name == "unit")
            {
                return new UnitNode(Block(stmt.ChildNodes[1]), ImportSection(stmt.ChildNodes[0]));
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
