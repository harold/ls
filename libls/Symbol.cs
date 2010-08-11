using System;

namespace libls
{
    public class Symbol
    {
        public string Name;

        public Symbol(string inName)
        {
            Name = inName;
        }

        public override string ToString()
        {
            return "Symbol:" + Name;
        }
    }
}
