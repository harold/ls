using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ls
{
    public delegate object MetaFunction(ArrayList inList, Environment inEnvironment);

    class Binding
    {
        public static void PopulateEnvironment(Environment inEnvironment)
        {
            inEnvironment["new"] = new MetaFunction(NewIntrisic);
            inEnvironment["."] = new MetaFunction(DotIntrinsic);
            inEnvironment["window"] = new MetaFunction(WindowIntrinsic);

            inEnvironment["+"] = new MetaFunction(PlusIntrinsic);
            inEnvironment["*"] = new MetaFunction(StarIntrinsic);
            inEnvironment["="] = new MetaFunction(EqualsIntrinsic);
        }

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

        static object NewIntrisic(ArrayList inList, Environment inEnvironment)
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

        static object DotIntrinsic(ArrayList inList, Environment inEnvironment)
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

        static object WindowIntrinsic(ArrayList inList, Environment inEnvironment)
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

        static object PlusIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            return (int)inList[0] + (int)inList[1];
        }

        static object StarIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            return (int)inList[0] * (int)inList[1];
        }

        static object EqualsIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            return inList[0].Equals(inList[1]);
        }
    }
}
