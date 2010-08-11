using System;
using System.Collections;

namespace libls
{
    public class Environment
    {
        public static Environment Global { get; set; }

        static Environment()
        {
            Global = new Environment();
            Global.Init();
        }

        protected Hashtable mFrame = new Hashtable();
        protected Environment mParent;

        public object this[object inKey]
        {
            get
            {
                object theReturn = mFrame[inKey];
                if (theReturn == null && mParent != null)
                    return mParent[inKey];

                return theReturn; // Bottoming out
            }

            set
            {
                mFrame[inKey] = value;
            }
        }

        public ICollection Keys { get{ return mFrame.Keys; } }

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

        public void Extend(Environment inEnvironment)
        {
            mParent = inEnvironment;
        }
    }
}
