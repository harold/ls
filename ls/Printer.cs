using System;
using System.Collections;
using System.Text;

namespace ls
{
    class Printer
    {
        public static void Print(object inForm)
        {
            if (inForm is int)
                Console.Write(inForm);

            else if (inForm is string)
                Console.Write("\"{0}\"", inForm);

            else if (inForm is Symbol)
            {
                Console.Write(((Symbol)inForm).Name);
            }

            else if (inForm is bool)
            {
                if ((bool)inForm)
                    Console.Write("true");
                else
                    Console.Write("false");
            }

            else if (inForm is ArrayList)
            {
                ArrayList theList = (ArrayList)inForm;
                Console.Write("(");
                for (int i = 0; i < theList.Count; ++i)
                {
                    Print(theList[i]);
                    if (i != theList.Count - 1)
                        Console.Write(" ");
                }
                Console.Write(")");
            }

            else
                Console.Write(inForm);
        }
    }
}
