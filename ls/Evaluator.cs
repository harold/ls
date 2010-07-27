﻿using System;
using System.Collections;

using ls;

namespace ls
{
    public static class Evaluator
    {
        public static bool IsTaggedList(object inForm, string inTag)
        {
            return inForm is ArrayList && ((ArrayList)inForm)[0] is Symbol && (((Symbol)((ArrayList)inForm)[0])).Name == inTag;
        }

        public static object Eval(object inForm, Environment inEnvironment)
        {
            if (inForm is int || inForm is string)
                return inForm;

            if (inForm is Symbol)
                return inEnvironment[((Symbol)inForm).Name];

            if (IsTaggedList(inForm, "quote"))
                return ((ArrayList)inForm)[1];

            if (IsTaggedList(inForm, "define"))
            {
                inEnvironment[((Symbol)((ArrayList)inForm)[1]).Name] = Eval(((ArrayList)inForm)[2], inEnvironment);
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

            if (inForm is ArrayList)
            {
                ArrayList theList = (ArrayList)inForm;
                ArrayList theArgs = new ArrayList();
                for (int i = 1; i < theList.Count; ++i)
                {
                    theArgs.Add(Eval(theList[i], inEnvironment));
                }
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

            ArrayList theList = (ArrayList)inForm;
            if (IsTaggedList(theList, "procedure"))
            {
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
