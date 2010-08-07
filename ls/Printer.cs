using System;
using System.Collections;
using System.Text;
using System.IO;

namespace ls
{
    class Printer
    {
        public static TextWriter Out { get; set; }

        static Printer()
        {
            Out = Console.Out;
        }

        public static void Print(object inForm)
        {
            if (inForm is int)
                Out.Write(inForm);

            else if (inForm is string)
                Out.Write("\"{0}\"", inForm);

            else if (inForm is Symbol)
            {
                Out.Write(((Symbol)inForm).Name);
            }

            else if (inForm is bool)
            {
                if ((bool)inForm)
                    Out.Write("true");
                else
                    Out.Write("false");
            }

            else if (inForm is ArrayList)
            {
                ArrayList theList = (ArrayList)inForm;
                Out.Write("(");
                for (int i = 0; i < theList.Count; ++i)
                {
                    Print(theList[i]);
                    if (i != theList.Count - 1)
                        Out.Write(" ");
                }
                Out.Write(")");
            }

            else if (inForm is Hashtable)
            {
                Hashtable theMap = (Hashtable)inForm;
                Out.Write("{");
                int i = 0;
                foreach (DictionaryEntry theEntry in theMap)
                {
                    Print(theEntry.Key);
                    Out.Write(" ");
                    Print(theEntry.Value);
                    if (i != theMap.Count - 1)
                        Out.Write(" ");

                    ++i;
                }
                Out.Write("}");
            }

            else if (inForm == null)
                Out.Write("null");

            else
                Out.Write(inForm);
        }
    }
}
