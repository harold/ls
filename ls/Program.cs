using System;

namespace ls
{
    class Program
    {
        static void Main(string[] args)
        {
            Evaluator.Eval(Reader.Read("(define file-text (fn (path) (. 'System.IO.File 'ReadAllText path)))"));
            Evaluator.Eval(Reader.Read("(define read (fn (read-str) (. 'ls.Reader 'Read read-str))"));
            Evaluator.Eval(Reader.Read("(define eval (fn (eval-str) (. 'ls.Evaluator 'Eval eval-str))"));
            while (true)
            {
                Console.Write("> ");
                Printer.Print(Evaluator.Eval(Reader.Read(Console.ReadLine())));
                Console.Write("\n\n");
            }
        }
    }
}
