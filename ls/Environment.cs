﻿using System;
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
            this["null"] = null;
            this["true"] = true;
            this["false"] = false;
        }

        internal void Extend(Environment inEnvironment)
        { // An optimization here would be not to do a full copy.
            foreach (DictionaryEntry theEntry in inEnvironment)
                this[theEntry.Key] = theEntry.Value;
        }
    }
}
