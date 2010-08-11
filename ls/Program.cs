using System.IO;
using libls;

namespace ls
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment theEnvironment = Environment.Global;
            Binding.PopulateEnvironment(theEnvironment);

            string[] theDefaultFiles = { "core.ls", "main.ls" };
            foreach(string theFile in theDefaultFiles )
            {
                if (File.Exists(theFile))
                {
                    StringReader sr = new StringReader(File.ReadAllText(theFile));
                    while (sr.Peek() != -1)
                        Evaluator.Eval(Reader.Read(sr), theEnvironment);
                }
            }

            while (true)
            {
                Printer.Out.Write("> ");
                try
                {
                    Printer.Print(Evaluator.Eval(Reader.Read(System.Console.ReadLine()), theEnvironment));
                }
                catch(System.Exception e)
                {
                    Printer.Out.WriteLine("Oops: " + e.ToString());
                }
                Printer.Out.Write("\n\n");
            }
        }
    }
}
