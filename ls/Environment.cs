using System;
using System.Collections;

namespace ls
{
    public class Environment : Hashtable
    {
        public Environment()
        {
            this["*env*"] = this; // Oh snap!
        }

        public void Init()
        {
            this["true"] = true;
            this["false"] = false;
        }
    }
}
