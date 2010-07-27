using System;
using System.IO;

namespace ls
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment theEnvironment = new Environment();
            theEnvironment.Init();
            Binding.PopulateEnvironment(theEnvironment);

            Evaluator.Eval(Reader.Read("(define box (fn (contents title) (. 'System.Windows.Forms.MessageBox 'Show contents title (. 'System.Windows.Forms.MessageBoxButtons 'OKCancel) (. 'System.Windows.Forms.MessageBoxIcon 'Error)))))"), theEnvironment);
            Evaluator.Eval(Reader.Read("(define file-text (fn (path) (. 'System.IO.File 'ReadAllText path)))"), theEnvironment);
            Evaluator.Eval(Reader.Read("(define read (fn (str) (. 'ls.Reader 'Read str))"), theEnvironment);
            Evaluator.Eval(Reader.Read("(define eval (fn (str) (. 'ls.Evaluator 'Eval str *env*))"), theEnvironment);

            if (System.IO.File.Exists("main.ls"))
            {
                StringReader sr = new StringReader(System.IO.File.ReadAllText("main.ls"));
                while (sr.Peek() != -1)
                    Evaluator.Eval(Reader.Read(sr), theEnvironment);
            }

            while (true)
            {
                Console.Write("> ");
                Printer.Print(Evaluator.Eval(Reader.Read(Console.ReadLine()), theEnvironment));
                Console.Write("\n\n");
            }
        }
    }
}
