using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace IronTuring.AST
{
    abstract class Node
    {
        public bool CanReduce => Reduce() == null;
        public virtual IEnumerable<Node> Reduce() => null;
    }
    abstract class Statement : Node
    {
        public virtual void GenerateIL(ILGenerator il, SymbolTable st)
        {
            if (!CanReduce)//if this is as reduced as it goes then this method must be overriden
                throw new NotSupportedException("This is a reduced node, it must implement GenerateIL");

            foreach(Statement node in Reduce())
                node.GenerateIL(il, st);
        }
    }
    abstract class Expression : Statement { }
}
