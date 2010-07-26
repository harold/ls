using System;
using System.IO;

namespace ls
{
    class Program
    {
        static void Main(string[] args)
        {
            Evaluator.Eval(Reader.Read("(define box (fn (contents title) (. 'System.Windows.Forms.MessageBox 'Show contents title (. 'System.Windows.Forms.MessageBoxButtons 'OKCancel) (. 'System.Windows.Forms.MessageBoxIcon 'Error)))))"));
            Evaluator.Eval(Reader.Read("(define file-text (fn (path) (. 'System.IO.File 'ReadAllText path)))"));
            Evaluator.Eval(Reader.Read("(define read (fn (read-str) (. 'ls.Reader 'Read read-str))"));
            Evaluator.Eval(Reader.Read("(define eval (fn (eval-str) (. 'ls.Evaluator 'Eval eval-str))"));

            if (System.IO.File.Exists("main.ls"))
            {
                StringReader sr = new StringReader(System.IO.File.ReadAllText("main.ls"));
                while (sr.Peek() != -1)
                    Evaluator.Eval(Reader.Read(sr));
            }

            while (true)
            {
                Console.Write("> ");
                Printer.Print(Evaluator.Eval(Reader.Read(Console.ReadLine())));
                Console.Write("\n\n");
            }
        }
    }
}
