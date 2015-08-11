using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronTuring.AST
{
    class BlockStatement:Statement
    {
        public IEnumerable<Statement> Block { get; }
    }
}
