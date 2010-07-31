using System;
using System.Collections;

using ls;

namespace ls
{
    public static class Evaluator
    {
        public static bool IsTaggedList(object inForm, string inTag)
        {
            return inForm is ArrayList && ((ArrayList)inForm).Count > 0 && ((ArrayList)inForm)[0] is Symbol && (((Symbol)((ArrayList)inForm)[0])).Name == inTag;
        }

        public static object Eval(object inForm, Environment inEnvironment)
        {
            if (inForm is int || inForm is string || inForm is double)
                return inForm;

            if (inForm is Symbol)
                return inEnvironment[((Symbol)inForm).Name];

            if (IsTaggedList(inForm, "quote"))
                return ((ArrayList)inForm)[1];

            if (IsTaggedList(inForm, "define"))
            {
                Environment.Global[((Symbol)((ArrayList)inForm)[1]).Name] = Eval(((ArrayList)inForm)[2], inEnvironment);
                return new Symbol("ok");
            }

            if (IsTaggedList(inForm, "if"))
            {
                ArrayList theList = (ArrayList)inForm;
                object thePredicate = Eval(theList[1], inEnvironment);

                if (thePredicate is bool)
                { // A bool.
                    if ((bool)thePredicate)
                        return Eval(theList[2], inEnvironment);
                    else
                        return Eval(theList[3], inEnvironment);
                }
                else
                { // Not a bool.
                    if (thePredicate != null)
                        return Eval(theList[2], inEnvironment);
                    else
                        return Eval(theList[3], inEnvironment);
                }
            }

            if (IsTaggedList(inForm, "fn"))
            {
                ArrayList theList = (ArrayList)inForm;
                theList[0] = new Symbol("procedure");
                return theList;
            }

            if (IsTaggedList(inForm, "begin"))
            {
                ArrayList theList = (ArrayList)inForm;
                object theReturn = null;
                for (int i = 1; i < theList.Count; ++i)
                    theReturn = Eval(theList[i], inEnvironment);
                return theReturn;
            }

            if (IsTaggedList(inForm, "backquote")) // TODO: nested backquotes+unquotes, and unquote-splicing
            {
                ArrayList theList = (ArrayList)inForm;
                if (!(theList[1] is ArrayList))
                { // literal
                    return theList[1];
                }
                else
                { // process backquote list
                    ArrayList theBackquoteList = (ArrayList)theList[1];
                    ArrayList theResultingList = new ArrayList();
                    foreach (object theObject in theBackquoteList)
                    {
                        if (IsTaggedList(theObject, "unquote"))
                        {
                            ArrayList theUnquoteList = (ArrayList)theObject;
                            theResultingList.Add(Eval(theUnquoteList[1], inEnvironment));
                        }
                        else
                        {
                            theResultingList.Add(theObject);
                        }
                    }
                    return theResultingList;
                }
            }

            if (IsTaggedList(inForm, "unquote"))
            {
                throw new Exception("Unquote must be inside a backquote");
            }

            if (inForm is ArrayList)
            { // Normal procedure application
                ArrayList theList = (ArrayList)inForm;
                ArrayList theArgs = new ArrayList();
                for (int i = 1; i < theList.Count; ++i)
                {
                    theArgs.Add(Eval(theList[i], inEnvironment));
                }
                if (theList.Count == 0) throw new Exception("You can't call the empty list");
                return Apply(Eval(theList[0], inEnvironment), theArgs, inEnvironment);
            }

            return inForm;
        }

        private static object Apply(object inForm, ArrayList inArgs, Environment inEnvironment)
        {
            if (inForm is Hashtable)
                return ((Hashtable)inForm)[inArgs[0]];

            if (inForm is MetaFunction)
                return ((MetaFunction)inForm)(inArgs, inEnvironment);

            if (IsTaggedList(inForm, "procedure"))
            {
                ArrayList theList = (ArrayList)inForm;
                Environment theExtendedEnvironment = new Environment();
                theExtendedEnvironment.Extend(inEnvironment);

                ArrayList theNamedArguments = (ArrayList)theList[1];
                for (int i = 0; i < theNamedArguments.Count; ++i)
                {
                    // Make sure to only copy over the arguments that were supplied
                    if (i < inArgs.Count)
                        theExtendedEnvironment[((Symbol)theNamedArguments[i]).Name] = inArgs[i];
                    else
                        theExtendedEnvironment[((Symbol)theNamedArguments[i]).Name] = null;
                }
                return Eval(theList[2], theExtendedEnvironment);
            }

            // else?
            return "wtf?";
        }
    }
}
