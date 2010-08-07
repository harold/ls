using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;

namespace ls
{
    // Wraps meta functions
    public delegate object MetaFunction(ArrayList inList, Environment inEnvironment);

    // Wraps anonymous methods for invoking bindings on another thread
    public delegate object MetaInvoker();

    class Binding
    {
        public static void PopulateEnvironment(Environment inEnvironment)
        {
            inEnvironment["new"] = new MetaFunction(NewIntrisic);
            inEnvironment["."] = new MetaFunction(DotIntrinsic);
            inEnvironment["window"] = new MetaFunction(WindowIntrinsic);
            inEnvironment["register"] = new MetaFunction(RegisterIntrinsic);
            inEnvironment["unregister"] = new MetaFunction(UnregisterIntrinsic);
            inEnvironment["print"] = new MetaFunction(PrintIntrinsic);

            inEnvironment["+"] = new MetaFunction(PlusIntrinsic);
            inEnvironment["-"] = new MetaFunction(DashIntrinsic);
            inEnvironment["*"] = new MetaFunction(StarIntrinsic);
            inEnvironment["/"] = new MetaFunction(SlashIntrinsic);
            inEnvironment["="] = new MetaFunction(EqualsIntrinsic);
            inEnvironment["<"] = new MetaFunction(LessThanIntrinsic);
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

        static object PrintIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            foreach (object theForm in inList)
            {
                Printer.Print(theForm);
                Console.Write("\n");
            }

            return null;
        }

        static object NewIntrisic(ArrayList inList, Environment inEnvironment)
        {
            ArrayList theArgs = new ArrayList();
            for (int i = 1; i < inList.Count; i++)
            {
                theArgs.Add(inList[i]);
            }

            Type theType = SeriouslyGetType((Symbol)inList[0]);

            if (theType.IsValueType)
                return Activator.CreateInstance(theType);

            ConstructorInfo constructor = theType.GetConstructor(Type.GetTypeArray(theArgs.ToArray()));
            return constructor.Invoke(theArgs.ToArray());
        }

        static object DotIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            object theTarget = null;
            Type theType = null;
            Control theControl = null;

            if (inList[0] is Symbol)
            {
                theType = SeriouslyGetType((Symbol)inList[0]);
            }
            else
            {
                theType = inList[0].GetType();
                theTarget = inList[0];
            }

            theControl = theTarget as Control;  // Try to cast the target into a UI control
            // If this is targeting a UI control, wait until it's been created for sure
            while (theControl != null && !theControl.IsHandleCreated)
                System.Threading.Thread.Sleep(1);

            string theMember = ((Symbol)inList[1]).Name;

            ArrayList theArgs = new ArrayList();
            for (int i = 2; i < inList.Count; i++)
            {
                theArgs.Add(inList[i]);
            }

            MethodInfo theMethodInfo = theType.GetMethod(theMember, Type.GetTypeArray(theArgs.ToArray()));

            if (theMethodInfo != null)
            {
                // Actions targeting UI controls must execute in the UI thread
                if (theControl != null)
                    return theControl.Invoke((MetaInvoker)delegate() { return theMethodInfo.Invoke(theTarget, theArgs.ToArray()); });

                return theMethodInfo.Invoke(theTarget, theArgs.ToArray()); // Execute on own thread
            }

            if (theType != null)
            {
                PropertyInfo thePropertyInfo = theType.GetProperty(theMember);
                if (thePropertyInfo != null)
                { // Didn't find a method, maybe it's a property.
                    if (theArgs.Count > 0)
                    {
                        // Actions targeting UI controls must execute in the UI thread
                        if (theControl != null)
                            theControl.Invoke((MetaInvoker)delegate() { thePropertyInfo.SetValue(theTarget, theArgs[0], null); return null; });

                        thePropertyInfo.SetValue(theTarget, theArgs[0], null);
                        return theArgs[0];
                    }
                    else
                    {
                        return thePropertyInfo.GetValue(theTarget, null);
                    }
                }

                FieldInfo theFieldInfo = theType.GetField(theMember);
                if (theFieldInfo != null)
                { // Not a property either, perhaps it's a field.
                    if (theArgs.Count > 0)
                    {
                        // Actions targeting UI controls must execute in the UI thread
                        if (theControl != null)
                            theControl.Invoke((MetaInvoker)delegate() { theFieldInfo.SetValue(theTarget, theArgs[0]); return null; });

                        theFieldInfo.SetValue(theTarget, theArgs[0]);
                        return theArgs[0];
                    }
                    else
                    {
                        return theFieldInfo.GetValue(theTarget);
                    }
                }

                EventInfo theEventInfo = theType.GetEvent(theMember);
                if (theEventInfo != null)
                {
                    return new Symbol(theEventInfo.EventHandlerType.FullName);
                }

                return Enum.Parse(theType, ((Symbol)inList[1]).Name, true); // Final chance, are you an enum?
            }

            return "DotIntrinsic: null type?";
        }

        static object RegisterIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            object theTarget = inList[0];
            Type theType = inList[0].GetType();
            string theMember = ((Symbol)inList[1]).Name;

            ArrayList theArgs = new ArrayList();
            for (int i = 2; i < inList.Count; i++)
            {
                theArgs.Add(inList[i]);
            }

            EventInfo theEventInfo = theType.GetEvent(theMember);
            if (theEventInfo != null)
            {
                // Get the parameters the event handler takes
                MethodInfo theInvokeInfo = theEventInfo.EventHandlerType.GetMethod("Invoke");
                ParameterInfo[] theParams = theInvokeInfo.GetParameters();

                string[] theParamNames = new string[theParams.Length];
                Type[] theParamTypes = new Type[theParams.Length];

                for (int i = 0; i < theParams.Length; i++)
                {
                    theParamNames[i] = theParams[i].Name;
                    theParamTypes[i] = theParams[i].ParameterType;
                }

                // Create a dynamic method with the matching signature
                DynamicMethod theHandler = new DynamicMethod(theEventInfo.Name, theInvokeInfo.ReturnType, theParamTypes, typeof(Binder).Module);
                Type theEnvType = typeof(Environment);
                ConstructorInfo theEnvConstuctorInfo = theEnvType.GetConstructor(new Type[0]);
                ConstructorInfo theArrayListConstInfo = typeof(ArrayList).GetConstructor(new Type[0]);
                ConstructorInfo theSymbolConstInfo = typeof(Symbol).GetConstructor(new Type[] { typeof(string) });

                MethodInfo theEnvGlobalInfo = theEnvType.GetMethod("get_Global");
                MethodInfo theEnvExtendInfo = theEnvType.GetMethod("Extend");
                MethodInfo theEnvSetItemInfo = theEnvType.GetMethod("set_Item");
                MethodInfo theEvalInfo = typeof(Evaluator).GetMethod("Eval");
                MethodInfo theAddInfo = typeof(ArrayList).GetMethod("Add");

                // Emit the content of the dynamic method
                ILGenerator theIL = theHandler.GetILGenerator(256);

                // HACK: assume all events take two arguments: sender and event arguments
                theIL.DeclareLocal(typeof(Environment));
                theIL.DeclareLocal(typeof(ArrayList));

                theIL.Emit(OpCodes.Nop);
                theIL.Emit(OpCodes.Newobj, theEnvConstuctorInfo);
                theIL.Emit(OpCodes.Stloc_0);
                theIL.Emit(OpCodes.Newobj, theArrayListConstInfo);
                theIL.Emit(OpCodes.Stloc_1);
                theIL.Emit(OpCodes.Ldloc_1);
                theIL.Emit(OpCodes.Ldstr, GetSymbolFromValue(inList[2], inEnvironment).Name);
                theIL.Emit(OpCodes.Newobj, theSymbolConstInfo);
                theIL.EmitCall(OpCodes.Callvirt, theAddInfo, null);
                theIL.Emit(OpCodes.Pop);
                theIL.Emit(OpCodes.Ldloc_1);
                theIL.Emit(OpCodes.Ldstr, "sender");
                theIL.Emit(OpCodes.Newobj, theSymbolConstInfo);
                theIL.EmitCall(OpCodes.Callvirt, theAddInfo, null);
                theIL.Emit(OpCodes.Pop);
                theIL.Emit(OpCodes.Ldloc_1);
                theIL.Emit(OpCodes.Ldstr, "event");
                theIL.Emit(OpCodes.Newobj, theSymbolConstInfo);
                theIL.EmitCall(OpCodes.Callvirt, theAddInfo, null);
                theIL.Emit(OpCodes.Pop);
                theIL.Emit(OpCodes.Ldloc_0);
                theIL.EmitCall(OpCodes.Call, theEnvGlobalInfo, null);
                theIL.EmitCall(OpCodes.Callvirt, theEnvExtendInfo, null);
                theIL.Emit(OpCodes.Nop);
                theIL.Emit(OpCodes.Ldloc_0);
                theIL.Emit(OpCodes.Ldstr, "sender");
                theIL.Emit(OpCodes.Ldarg_0);
                theIL.EmitCall(OpCodes.Callvirt, theEnvSetItemInfo, null);
                theIL.Emit(OpCodes.Nop);
                theIL.Emit(OpCodes.Ldloc_0);
                theIL.Emit(OpCodes.Ldstr, "event");
                theIL.Emit(OpCodes.Ldarg_1);
                theIL.EmitCall(OpCodes.Callvirt, theEnvSetItemInfo, null);
                theIL.Emit(OpCodes.Nop);
                theIL.Emit(OpCodes.Ldloc_1);
                theIL.Emit(OpCodes.Ldloc_0);
                theIL.EmitCall(OpCodes.Call, theEvalInfo, null);
                theIL.Emit(OpCodes.Pop);
                theIL.Emit(OpCodes.Ret);

                theHandler.DefineParameter(1, ParameterAttributes.In, "sender");
                theHandler.DefineParameter(2, ParameterAttributes.In, "event");

                // Create a delegate with the handler and register it
                Delegate theDelegate = theHandler.CreateDelegate(theEventInfo.EventHandlerType);
                theEventInfo.AddEventHandler(theTarget, theDelegate);

                return new Symbol("registered to event: " + theEventInfo.Name);
            }

            return "event not found: " + theMember;
        }

        static object UnregisterIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            object theTarget = inList[0];
            Type theType = inList[0].GetType();
            string theMember = ((Symbol)inList[1]).Name;

            ArrayList theArgs = new ArrayList();
            for (int i = 2; i < inList.Count; i++)
            {
                theArgs.Add(inList[i]);
            }

            EventInfo theEventInfo = theType.GetEvent(theMember);
            if (theEventInfo != null)
            {
                // TODO: figure out a way to unregister handlers
                return new Symbol("unregistered from event: " + theEventInfo.Name);
            }

            return "event not found: " + theMember;
        }

        static Symbol GetSymbolFromValue(object inValue, Environment inEnvironment)
        {
            foreach (string theKey in inEnvironment.Keys)
            {
                if (inEnvironment[theKey] == inValue)
                    return new Symbol(theKey);
            }

            return null;
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
            object theLeft = inList[0];
            object theRight = inList[1];
            if (theLeft == null && theRight == null) return "null add fail";
            if (theLeft is int && theRight is int) return (int)theLeft + (int)theRight;
            if (theLeft is int && theRight is double) return Convert.ToDouble(theLeft) + (double)theRight;
            if (theLeft is double && theRight is int) return (double)theLeft + Convert.ToDouble(theRight);
            if (theLeft is double && theRight is double) return (double)theLeft + (double)theRight;
            return String.Format("Unspported math operation: {0}+{1}", theLeft.GetType(), theRight.GetType());
        }

        static object DashIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            object theLeft = inList[0];
            object theRight = inList[1];
            if (theLeft == null && theRight == null) return "null subtraction fail";
            if (theLeft is int && theRight is int) return (int)theLeft - (int)theRight;
            if (theLeft is int && theRight is double) return Convert.ToDouble(theLeft) - (double)theRight;
            if (theLeft is double && theRight is int) return (double)theLeft - Convert.ToDouble(theRight);
            if (theLeft is double && theRight is double) return (double)theLeft - (double)theRight;
            return String.Format("Unspported math operation: {0}-{1}", theLeft.GetType(), theRight.GetType());
        }

        static object StarIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            object theLeft = inList[0];
            object theRight = inList[1];
            if (theLeft == null && theRight == null) return "null mult fail";
            if (theLeft is int && theRight is int) return (int)theLeft * (int)theRight;
            if (theLeft is int && theRight is double) return Convert.ToDouble(theLeft) * (double)theRight;
            if (theLeft is double && theRight is int) return (double)theLeft * Convert.ToDouble(theRight);
            if (theLeft is double && theRight is double) return (double)theLeft * (double)theRight;
            return String.Format("Unspported math operation: {0}*{1}", theLeft.GetType(), theRight.GetType());
        }

        static object SlashIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            object theLeft = inList[0];
            object theRight = inList[1];
            if (theLeft == null && theRight == null) return "null divide fail";
            if (theLeft is int && theRight is int) return (int)theLeft / (int)theRight;
            if (theLeft is int && theRight is double) return Convert.ToDouble(theLeft) / (double)theRight;
            if (theLeft is double && theRight is int) return (double)theLeft / Convert.ToDouble(theRight);
            if (theLeft is double && theRight is double) return (double)theLeft / (double)theRight;
            return String.Format("Unspported math operation: {0}/{1}", theLeft.GetType(), theRight.GetType());
        }

        static object EqualsIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            return inList[0].Equals(inList[1]);
        }

        static object LessThanIntrinsic(ArrayList inList, Environment inEnvironment)
        {
            object theLeft = inList[0];
            object theRight = inList[1];
            if (theLeft == null && theRight == null) return "null less-than fail";
            if (theLeft is int && theRight is int) return (int)theLeft < (int)theRight;
            if (theLeft is int && theRight is double) return Convert.ToDouble(theLeft) < (double)theRight;
            if (theLeft is double && theRight is int) return (double)theLeft < Convert.ToDouble(theRight);
            if (theLeft is double && theRight is double) return (double)theLeft < (double)theRight;
            return String.Format("Unspported math operation: {0}<{1}", theLeft.GetType(), theRight.GetType());
        }
    }
}
