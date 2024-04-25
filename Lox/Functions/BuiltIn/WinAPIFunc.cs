using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace UcciLox.Functions.BuiltIn;
internal sealed class WinAPIFunc : ICallable
{
    int ICallable.Arity => 0;

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var delegateArgs = new List<object>();

        var passedArguments = arguments.Skip(1).ToArray();
        List<Type> paramTypes = new List<Type>();

        for (int i = 0; i < passedArguments.Length; i++)
        {
            var argType = passedArguments[i];
            var actualArg = passedArguments[i + 1];

            if (argType.Equals("IntPtr"))
            {
                if (actualArg.Equals((double)0))
                {
                    delegateArgs.Add((object)IntPtr.Zero);
                    paramTypes.Add(typeof(IntPtr));
                }
            }

            if (argType.Equals("string"))
            {
                delegateArgs.Add((string)actualArg);
                paramTypes.Add(typeof(string));
            }

            if (argType.Equals("uint"))
            {
                delegateArgs.Add(Convert.ToUInt32(actualArg));
                paramTypes.Add(typeof(uint));
            }

            i += 1;
        }

        IntPtr functionPtr = (nint)arguments[0];
        Type returnType = typeof(int);

        // Create the dynamic delegate type
        Type delegateType = CreateDynamicDelegateType(paramTypes.ToArray(), returnType);

        // Create the delegate from the function pointer
        Delegate dynamicDelegate = Marshal.GetDelegateForFunctionPointer(functionPtr, delegateType);

        if (dynamicDelegate is null)
        {
            throw new Exception("Unable to create delegate type.");
        }

        // Invoke the delegate dynamically
        object? result = dynamicDelegate.DynamicInvoke(delegateArgs.ToArray());
        return result;
    }

    private static Type CreateDynamicDelegateType(Type[] parameterTypes, Type returnType)
    {
        AssemblyName assemblyName = new AssemblyName("DynamicDelegateAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        TypeBuilder typeBuilder = moduleBuilder.DefineType("DynamicDelegate", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class, typeof(MulticastDelegate));

        ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(IntPtr) });
        constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

        MethodBuilder methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, returnType, parameterTypes);
        methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

        return typeBuilder.CreateType();
    }
}