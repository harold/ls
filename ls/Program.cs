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
                Console.Write("> ");
                try
                {
                    Printer.Print(Evaluator.Eval(Reader.Read(Console.ReadLine()), Environment.Global));
                }
                catch(Exception e)
                {
                    Console.WriteLine("Oops: " + e.ToString());
                }
                Console.Write("\n\n");
            }
        }
    }
}
