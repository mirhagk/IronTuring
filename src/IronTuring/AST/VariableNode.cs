using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    class VariableNode : StatementNode
    {
        public ExpressionNode InitialValue { get; }
        public IList<string> VariableNames { get; }
        public VariableNode(IList<string> variableNames, ExpressionNode initialValue)
        {
            VariableNames = variableNames;
            InitialValue = initialValue;
        }
        public override bool CanReduce => VariableNames.Count > 1;
        public override IEnumerable<Node> Reduce()
        {
            if (VariableNames.Count == 1)
                return null;
            var splitList = new List<Node>();
            foreach (var variable in VariableNames)
            {
                splitList.Add(new VariableNode(new List<string> { variable }, InitialValue));
            }
            return splitList;
        }
        public override void GenerateIL(ILGenerator il, SymbolTable st)
        {
            if (CanReduce)
                base.GenerateIL(il, st);
            var name = VariableNames[0];
            st.AddLocal(name, il.DeclareLocal(InitialValue.TypeOfExpression()));
            InitialValue.GenerateIL(il, st);
            il.Emit(OpCodes.Stloc, st.locals[name].LocalIndex);
        }
    }
}
