using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ls
{
    public delegate object MetaFunction(ArrayList inList);

    public static class Evaluator
    {
        static Hashtable GlobalEnvironment = new Hashtable();

        static object NewIntrisic(ArrayList inList)
        {
            string theClass = ((Symbol)inList[0]).Name;
            return Activator.CreateInstance(Type.GetType(theClass));
        }

        static object DotIntrinsic(ArrayList inList)
        {
            string theClass = ((Symbol)inList[0]).Name;
            string theMethod = ((Symbol)inList[1]).Name;

            ArrayList theArgs = new ArrayList();

            for (int i = 2; i < inList.Count; i++)
            {
                theArgs.Add(inList[i]);
            }

            List<Type> theTypes = new List<Type>();

            foreach (object arg in theArgs)
            {
                theTypes.Add(arg.GetType());
            }

            object[] args = theArgs.ToArray();
            Type[] argTypes = theTypes.ToArray();

            Type type = Type.GetType(theClass);
            MethodInfo method = type.GetMethod(theMethod, argTypes);
            return method.Invoke(null, args);
        }

        static object PlusIntrinsic(ArrayList inList)
        {
            return (int)Eval(inList[0]) + (int)Eval(inList[1]);
        }

        static object StarIntrinsic(ArrayList inList)
        {
            return (int)Eval(inList[0]) * (int)Eval(inList[1]);
        }

        static Evaluator()
        {
            GlobalEnvironment.Add(new Symbol("new").Name, new MetaFunction(NewIntrisic));
            GlobalEnvironment.Add(new Symbol(".").Name, new MetaFunction(DotIntrinsic));
            GlobalEnvironment.Add(new Symbol("+").Name, new MetaFunction(PlusIntrinsic));
            GlobalEnvironment.Add(new Symbol("*").Name, new MetaFunction(StarIntrinsic));

            GlobalEnvironment.Add(new Symbol("true").Name, true);
            GlobalEnvironment.Add(new Symbol("false").Name, false);
        }

        public static bool IsTaggedList(object inForm, string inTag)
        {
            return inForm is ArrayList && ((ArrayList)inForm)[0] is Symbol && (((Symbol)((ArrayList)inForm)[0])).Name == inTag;
        }

        public static object Eval(object inForm)
        {
            if (inForm is int || inForm is string)
                return inForm;

            if (inForm is Symbol)
                return GlobalEnvironment[((Symbol)inForm).Name];

            if (IsTaggedList(inForm, "quote"))
                return ((ArrayList)inForm)[1];

            if (IsTaggedList(inForm, "define"))
            {
                GlobalEnvironment[((Symbol)((ArrayList)inForm)[1]).Name] = Eval(((ArrayList)inForm)[2]);
                return new Symbol("ok");
            }

            if (IsTaggedList(inForm, "if"))
            {
                ArrayList theList = (ArrayList)inForm;
                object thePredicate = Eval(theList[1]);

                if (thePredicate is bool)
                { // A bool.
                    if ((bool)thePredicate)
                        return Eval(theList[2]);
                    else
                        return Eval(theList[3]);
                }
                else
                { // Not a bool.
                    if (thePredicate != null)
                        return Eval(theList[2]);
                    else
                        return Eval(theList[3]);
                }
            }

            if (IsTaggedList(inForm, "fn"))
            {
                ArrayList theList = (ArrayList)inForm;
                theList[0] = new Symbol("procedure");
                return theList;
            }

            if (inForm is ArrayList)
            {
                ArrayList theList = (ArrayList)inForm;
                ArrayList theArgs = new ArrayList();
                for (int i = 1; i < theList.Count; ++i)
                {
                    theArgs.Add(Eval(theList[i]));
                }
                return Apply(Eval(theList[0]),theArgs);
            }

            return inForm;
        }

        private static object Apply(object inForm, ArrayList inArgs)
        {
            if (inForm is MetaFunction)
                return ((MetaFunction)inForm)(inArgs);

            ArrayList theList = (ArrayList)inForm;
            if (IsTaggedList(theList, "procedure"))
            {
                ArrayList theNamedArguments = (ArrayList)theList[1];
                for (int i = 0; i < theNamedArguments.Count; ++i)
                {
                    GlobalEnvironment[((Symbol)theNamedArguments[i]).Name] = inArgs[i];
                }
                return Eval(theList[2]);
            }

            // else?
            return "wtf?";
        }
    }
}
