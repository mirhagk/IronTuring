using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronTuringTest
{
    [TestClass]
    public class ParseTests
    {
        [TestMethod]
        public void ParseAllPrograms()
        {
            var grammar = new TSharpCompiler.TuringGrammarBroken();
            foreach(var program in TestProgramLoader.Instance.AvailablePrograms())
            {
                IronTuring.Program.getRoot(program, grammar);
            }
        }
    }
}
