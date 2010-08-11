﻿using System;
using System.Collections;

namespace libls
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
                ArrayList theReturn = new ArrayList();
                theReturn.AddRange((ArrayList)inForm);
                theReturn[0] = new Symbol("procedure");
                return theReturn;
            }

            if (IsTaggedList(inForm, "macro"))
            {
                ArrayList theReturn = new ArrayList();
                theReturn.AddRange((ArrayList)inForm);
                theReturn[0] = new Symbol("macro-procedure");
                return theReturn;
            }

            if (IsTaggedList(inForm, "begin"))
            {
                ArrayList theList = (ArrayList)inForm;
                object theReturn = null;
                for (int i = 1; i < theList.Count; ++i)
                    theReturn = Eval(theList[i], inEnvironment);
                return theReturn;
            }

            if (IsTaggedList(inForm, "backquote")) // TODO: unquote-splicing and simpler logic here
            {
                ArrayList theList = (ArrayList)inForm;
                object theBackquotedForm = theList[1];
                if (!( theBackquotedForm is ArrayList)) // literal
                    return theList[1];
                else
                { // process backquote list
                    if (IsTaggedList(theBackquotedForm, "backquote")) // directly nested
                        return Eval(theBackquotedForm, inEnvironment);
                    else if (IsTaggedList(theBackquotedForm, "unquote")) // directly nested
                        return Eval(((ArrayList)theBackquotedForm)[1], inEnvironment);
                    else
                    {
                        ArrayList theResultingList = new ArrayList();
                        ArrayList theBackquoteList = (ArrayList)theBackquotedForm;
                        foreach (object theObject in theBackquoteList)
                        {
                            if (IsTaggedList(theObject, "unquote"))
                                theResultingList.Add(Eval(((ArrayList)theObject)[1], inEnvironment));
                            else if (IsTaggedList(theObject, "backquote")) // recurse
                                theResultingList.Add(Eval(theObject, inEnvironment));
                            else
                                theResultingList.Add(theObject); // implicitly quoted
                        }
                        return theResultingList;
                    }
                }
            }

            if (IsTaggedList(inForm, "unquote"))
            {
                throw new Exception("Unquote must be inside a backquote");
            }

            if (inForm is ArrayList)
            { // application
                ArrayList theList = (ArrayList)inForm;
                ArrayList theArgs = theList.GetRange(1, theList.Count-1);
                if (theList.Count == 0) throw new Exception("You can't call the empty list");
                return Apply(Eval(theList[0], inEnvironment), theArgs, inEnvironment);
            }

            return inForm;
        }

        private static object Apply(object inForm, ArrayList inArgs, Environment inEnvironment)
        {
            if (IsTaggedList(inForm, "macro-procedure"))
            { // macro expansion/evaluation
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
                return Eval(Eval(theList[2], theExtendedEnvironment), inEnvironment);
            }

            // Evaluate arguments
            ArrayList theEvaluatedArguments = new ArrayList();
            for (int i = 0; i < inArgs.Count; ++i)
                theEvaluatedArguments.Add(Eval(inArgs[i], inEnvironment));

            if (inForm is Hashtable)
                return ((Hashtable)inForm)[theEvaluatedArguments[0]];

            if (inForm is MetaFunction)
                return ((MetaFunction)inForm)(theEvaluatedArguments, inEnvironment);

            if (IsTaggedList(inForm, "procedure"))
            { // normal procedure application
                ArrayList theList = (ArrayList)inForm;
                Environment theExtendedEnvironment = new Environment();
                theExtendedEnvironment.Extend(inEnvironment);

                ArrayList theNamedArguments = (ArrayList)theList[1];
                for (int i = 0; i < theNamedArguments.Count; ++i)
                {
                    // Make sure to only copy over the arguments that were supplied
                    if (i < theEvaluatedArguments.Count)
                        theExtendedEnvironment[((Symbol)theNamedArguments[i]).Name] = theEvaluatedArguments[i];
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
