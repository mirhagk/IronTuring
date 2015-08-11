using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IronTuringTest
{
    class TestProgramLoader
    {
        const string DirectoryName = "TestPrograms";
        public string LoadProgram(string programName)
        {
            return File.ReadAllText($"{DirectoryName}\\{programName}.t");
        }
        public IEnumerable<string> AvailableProgramNames()
        {
            return Directory.EnumerateFiles(DirectoryName).Where(f=>Path.GetExtension(f)==".t").Select(f => Path.GetFileNameWithoutExtension(f));
        }
        public IEnumerable<string> AvailablePrograms()
        {
            foreach(var programName in AvailableProgramNames())
            {
                yield return LoadProgram(programName);
            }
        }
        public static TestProgramLoader Instance { get; } = new TestProgramLoader();
    }
}
