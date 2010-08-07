using System;
using System.IO;

namespace ls
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.Global = new Environment();
            Environment.Global.Init();
            Binding.PopulateEnvironment(Environment.Global);

            string[] theDefaultFiles = { "core.ls", "main.ls" };
            foreach(string theFile in theDefaultFiles )
            {
                if (System.IO.File.Exists(theFile))
                {
                    StringReader sr = new StringReader(System.IO.File.ReadAllText(theFile));
                    while (sr.Peek() != -1)
                        Evaluator.Eval(Reader.Read(sr), Environment.Global);
                }
            }

            while (true)
            {
                Printer.Out.Write("> ");
                try
                {
                    Printer.Print(Evaluator.Eval(Reader.Read(Console.ReadLine()), Environment.Global));
                }
                catch(Exception e)
                {
                    Printer.Out.WriteLine("Oops: " + e.ToString());
                }
                Printer.Out.Write("\n\n");
            }
        }
    }
}
