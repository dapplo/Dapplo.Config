//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Config
// 
//  Dapplo.Config is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Config is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dapplo.LogFacade;

namespace Dapplo.Config.Interceptor.IlGeneration
{
	/// <summary>
	/// Internally used to generate a method via IL
	/// </summary>
	internal static class IlMethodBuilder
	{
		private static readonly MethodAttributes MethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final;
		private static readonly MethodInfo InterceptorInvoke = typeof(IExtensibleInterceptor).GetMethod("Invoke");

		/// <summary>
		///     Create the method invoke
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="methodInfo"></param>
		internal static void BuildMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
		{
			var parameterTypes = (
				from parameterInfo in methodInfo.GetParameters()
				select parameterInfo.ParameterType).ToList();

			var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes);
			methodBuilder.SetParameters(parameterTypes.ToArray());
			methodBuilder.SetReturnType(methodInfo.ReturnType);

			if (methodInfo.IsGenericMethod)
			{
				var genericArguments = methodInfo.GetGenericArguments();
				methodBuilder.DefineGenericParameters(GetArgumentNames(genericArguments));
				methodBuilder.MakeGenericMethod(genericArguments);
			}

			GenerateIlMethod(methodBuilder, methodInfo);
		}

		/// <summary>
		/// Gets the argument names from an array of generic argument types.
		/// </summary>
		/// <param name="genericArguments">The generic arguments.</param>
		private static string[] GetArgumentNames(Type[] genericArguments)
		{
			var genericArgumentNames = new string[genericArguments.Length];
			for (var i = 0; i < genericArguments.Length; i++)
			{
				genericArgumentNames[i] = genericArguments[i].Name;
			}
			return genericArgumentNames;
		}

		private static void GenerateIlMethod(MethodBuilder methodBuilder, MethodInfo methodInfo)
		{

			var ilMethod = methodBuilder.GetILGenerator();

			var local = ilMethod.DeclareLocal(typeof(object[]));

			var arraySize = methodInfo.GetParameters().Length;
			ilMethod.Emit(OpCodes.Ldc_I4, arraySize);
			ilMethod.Emit(OpCodes.Newarr, typeof(object));
			ilMethod.Emit(OpCodes.Stloc, local);
			for (var i = 0; i < arraySize; i++)
			{
				ilMethod.Emit(OpCodes.Ldloc, local);
				ilMethod.Emit(OpCodes.Ldc_I4, i);
				ilMethod.Emit(OpCodes.Ldarg, i + 1);
				ilMethod.Emit(OpCodes.Stelem_Ref);
			}

			// Load the instance of the class (this) on the stack
			ilMethod.Emit(OpCodes.Ldarg_0);
			ilMethod.Emit(OpCodes.Ldstr, methodInfo.Name);
			ilMethod.Emit(OpCodes.Ldloc, local);

			ilMethod.Emit(OpCodes.Callvirt, InterceptorInvoke);
			if (methodInfo.ReturnType == typeof(void))
			{
				ilMethod.Emit(OpCodes.Pop);
			}
			else if (methodInfo.ReturnType.IsValueType)
			{
				ilMethod.Emit(OpCodes.Unbox_Any, methodInfo.ReturnType);
			}
			else
			{
				// Cast the return value
				ilMethod.Emit(OpCodes.Castclass, methodInfo.ReturnType);
			}
			ilMethod.Emit(OpCodes.Ret);

		}
	}
}
