using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ls
{
    public delegate object MetaFunction(ArrayList inList);

    public static class Evaluator
    {
        static Hashtable GlobalEnvironment = new Hashtable();

        static Type SeriouslyGetType(Symbol inTypeName)
        {
            Type theType = Type.GetType(inTypeName.Name);
            string[] theSuffixes = {
                                       ", System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                                       ", System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
                                   };

            int theCursor = 0;
            while (theType == null && theCursor < theSuffixes.Length)
            {
                theType = Type.GetType(string.Format("{0}{1}", inTypeName.Name, theSuffixes[theCursor]));
                theCursor++;
            }

            return theType;
        }

        static object NewIntrisic(ArrayList inList)
        {
            ArrayList theArgs = new ArrayList();
            for (int i = 1; i < inList.Count; i++)
            {
                theArgs.Add(inList[i]);
            }

            Type theType = SeriouslyGetType((Symbol)inList[0]);
            ConstructorInfo constructor = theType.GetConstructor(Type.GetTypeArray(theArgs.ToArray()));

            return constructor.Invoke(theArgs.ToArray());
        }

        static object DotIntrinsic(ArrayList inList)
        {
            object theTarget = null;
            Type theType = null;

            if (inList[0] is Symbol)
            {
                theType = SeriouslyGetType((Symbol)inList[0]);
            }
            else
            {
                theType = inList[0].GetType();
                theTarget = inList[0];
            }

            string theMember = ((Symbol)inList[1]).Name;

            ArrayList theArgs = new ArrayList();
            for (int i = 2; i < inList.Count; i++)
            {
                theArgs.Add(inList[i]);
            }

            MethodInfo theMethodInfo = theType.GetMethod(theMember, Type.GetTypeArray(theArgs.ToArray()));
            if (theMethodInfo != null)
                return theMethodInfo.Invoke(theTarget, theArgs.ToArray());

            if (theType != null)
            {
                PropertyInfo property = theType.GetProperty(theMember);
                if (property != null)
                { // Didn't find a method, maybe it's a property.
                    if (theArgs.Count > 0)
                    {
                        property.SetValue(theTarget, theArgs[0], null);
                        return theArgs[0];
                    }
                    else
                    {
                        return property.GetValue(theTarget, null);
                    }
                }

                FieldInfo field = theType.GetField(theMember);
                if (field != null)
                { // Not a property either, perhaps it's a field.
                    if (theArgs.Count > 0)
                    {
                        field.SetValue(theTarget, theArgs[0]);
                        return theArgs[0];
                    }
                    else
                    {
                        return field.GetValue(theTarget);
                    }
                }

                return Enum.Parse(theType, ((Symbol)inList[1]).Name, true); // Final chance, are you an enum?
            }

            return "DotIntrinsic: null type?";
        }

        static object WindowIntrinsic(ArrayList inList)
        {
            Form theForm = new Form();

            if (inList.Count > 0 && inList[0] != null)
                theForm.Text = (string)inList[0];

            if (inList.Count > 1)
                theForm.Width = (int)inList[1];

            if (inList.Count > 2)
                theForm.Height = (int)inList[2];

            (new Thread(new ParameterizedThreadStart(LaunchWindow))).Start(theForm);
            return theForm;
        }

        static void LaunchWindow(object inForm)
        {
            Application.EnableVisualStyles();
            Application.Run(inForm as Form);
        }

        static object PlusIntrinsic(ArrayList inList)
        {
            return (int)Eval(inList[0]) + (int)Eval(inList[1]);
        }

        static object StarIntrinsic(ArrayList inList)
        {
            return (int)Eval(inList[0]) * (int)Eval(inList[1]);
        }

        static object EqualsIntrinsic(ArrayList inList)
        {
            return inList[0].Equals(inList[1]);
        }

        static Evaluator()
        {
            GlobalEnvironment.Add(new Symbol("new").Name, new MetaFunction(NewIntrisic));
            GlobalEnvironment.Add(new Symbol(".").Name, new MetaFunction(DotIntrinsic));
            GlobalEnvironment.Add(new Symbol("window").Name, new MetaFunction(WindowIntrinsic));

            GlobalEnvironment.Add(new Symbol("+").Name, new MetaFunction(PlusIntrinsic));
            GlobalEnvironment.Add(new Symbol("*").Name, new MetaFunction(StarIntrinsic));
            GlobalEnvironment.Add(new Symbol("=").Name, new MetaFunction(EqualsIntrinsic));

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
