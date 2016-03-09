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

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dapplo.LogFacade;

#endregion

namespace Dapplo.Config.Interceptor
{
	/// <summary>
	/// This class will build a type, at runtime, which implements the given interface (and whatever it extens)
	/// The class will only pass all calls on properties and method through to a class implementing IInterceptor
	/// </summary>
	public class InterceptorFactory
	{
		private static readonly LogSource Log = new LogSource();

		private static readonly MethodAttributes SetGetMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
																		MethodAttributes.Virtual | MethodAttributes.Final;

		private static readonly MethodAttributes MethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final;

		private static readonly ConstructorInfo SetInfoConstructor = typeof (SetInfo).GetConstructor(Type.EmptyTypes);
		private static readonly MethodInfo GetSetInfoPropertyName = typeof (GetSetInfo).GetProperty("PropertyName").GetSetMethod();
		private static readonly MethodInfo GetSetInfoPropertyType = typeof (GetSetInfo).GetProperty("PropertyType").GetSetMethod();
		private static readonly MethodInfo SetInfoNewValue = typeof (SetInfo).GetProperty("NewValue").GetSetMethod();
		private static readonly MethodInfo GetInfoValue = typeof (GetInfo).GetProperty("Value").GetGetMethod();
		private static readonly MethodInfo InterceptorGet = typeof (IInterceptor).GetMethod("Get");
		private static readonly MethodInfo InterceptorSet = typeof (IInterceptor).GetMethod("Set");
		private static readonly MethodInfo InterceptorInvoke = typeof (IInterceptor).GetMethod("Invoke");

		private static readonly ConstructorInfo GetInfoConstructor = typeof (GetInfo).GetConstructor(Type.EmptyTypes);

		private static readonly IDictionary<Type, Type> TypeMap = new Dictionary<Type, Type>();

		/// <summary>
		/// Create intercepted implementation of the specified result
		/// </summary>
		/// <typeparam name="TResult">Type to implement and intercept</typeparam>
		/// <param name="interceptor">Class to act as interceptor</param>
		/// <returns>TResult</returns>
		public static TResult Create<TResult>(IInterceptor interceptor)
		{
			Type interfaceType = typeof (TResult);

			if (!interfaceType.IsInterface)
			{
				throw new ArgumentException("Only interfaces are allowed.", nameof(interfaceType));
			}
			Type implementingType;
			if (!TypeMap.TryGetValue(interfaceType, out implementingType))
			{
				implementingType = CreateType("Dapplo.Config.Interceptor", typeof(TResult).Name + "Impl", typeof(TResult));
			}
			var newInstance = (TResult)Activator.CreateInstance(implementingType);
			var intercepted = newInstance as IIntercepted;
			if (intercepted != null)
			{
				intercepted.Interceptor = interceptor;
			}
			return newInstance;
		}

		/// <summary>
		/// Creates an implementation as Type for a given interface, which can be intercepted
		/// </summary>
		/// <param name="assemblyNameString"></param>
		/// <param name="typeName"></param>
		/// <param name="interfaceType"></param>
		/// <returns>Type</returns>
		private static Type CreateType(string assemblyNameString, string typeName, Type interfaceType)
		{
			string dllName = $"{assemblyNameString}.dll";
			var assemblyName = new AssemblyName(assemblyNameString);
			var appDomain = AppDomain.CurrentDomain;
			var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, AppDomain.CurrentDomain.BaseDirectory);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, dllName, false);

			// Create the type, and let it implement our interface
			var typeBuilder = moduleBuilder.DefineType(typeName,
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
				typeof(object), new[] { interfaceType, typeof(IIntercepted) });

			var interceptorField = BuildProperty(typeBuilder, "Interceptor", typeof(IInterceptor));

			foreach (var propertyInfo in interfaceType.GetProperties())
			{
				if (!propertyInfo.CanRead && !propertyInfo.CanWrite)
				{
					continue;
				}
				var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.HasDefault, propertyInfo.PropertyType, null);

				if (propertyInfo.CanWrite)
				{
					var setterBuilder = BuildGetter(typeBuilder, propertyInfo, interceptorField);
					propertyBuilder.SetSetMethod(setterBuilder);
				}

				if (propertyInfo.CanRead)
				{
					var getterBuilder = BuildSetter(typeBuilder, propertyInfo, interceptorField);
					propertyBuilder.SetGetMethod(getterBuilder);
				}
			}

			foreach (var methodInfo in interfaceType.GetMethods())
			{
				if (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))
				{
					continue;
				}
				BuildMethod(typeBuilder, methodInfo, interceptorField);
			}

			// Example for making a exe, for a methodBuilder which creates a static main
			//assemblyBuilder.SetEntryPoint(methodBuilder,PEFileKinds.Exe);
			var returnType = typeBuilder.CreateType();
			Log.Debug().WriteLine("Wrote {0} to {1}", dllName, AppDomain.CurrentDomain.BaseDirectory);
			assemblyBuilder.Save(dllName, PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);
			return returnType;
		}

		#region Helpers
		/// <summary>
		///     Build the getter for the property
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="interceptorField"></param>
		/// <returns>MethodBuilder with the getter</returns>
		private static MethodBuilder BuildGetter(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldInfo interceptorField)
		{
			if (interceptorField == null) throw new ArgumentNullException(nameof(interceptorField));
			var getterBuilder = typeBuilder.DefineMethod("get_" + propertyInfo.Name, SetGetMethodAttributes, propertyInfo.PropertyType, Type.EmptyTypes);
			var ilGetter = getterBuilder.GetILGenerator();

			// Local GetInfo variable
			var getInfo = ilGetter.DeclareLocal(typeof (GetInfo));

			// Create new GetInfo class for the argument which are passed to the IInterceptor
			ilGetter.Emit(OpCodes.Newobj, GetInfoConstructor);
			// Store it in the local setInfo variable
			ilGetter.Emit(OpCodes.Stloc, getInfo);

			// Get the getInfo local variable value
			ilGetter.Emit(OpCodes.Ldloc, getInfo);
			// Load the name of the property on the stack
			ilGetter.Emit(OpCodes.Ldstr, propertyInfo.Name);
			// Set the value to the PropertyName property of the GetSetInfo (call set_PropertyName)
			ilGetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyName);

			// Get the getInfo local variable value
			ilGetter.Emit(OpCodes.Ldloc, getInfo);
			// Load the type of the property as RuntimeTypeHandle on the stack
			ilGetter.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
			// Convert the RuntimeTypeHandle to a type
			ilGetter.Emit(OpCodes.Call, typeof (Type).GetMethod("GetTypeFromHandle", new[] {typeof (RuntimeTypeHandle)}));
			// Set the value to the PropertyType property of the GetSetInfo (call set_PropertyType)
			ilGetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyType);

			// Load the instance of the class (this) on the stack
			ilGetter.Emit(OpCodes.Ldarg_0);
			// Get the interceptor value from this._interceptor
			ilGetter.Emit(OpCodes.Ldfld, interceptorField);
			// Get the getInfo local variable value
			ilGetter.Emit(OpCodes.Ldloc, getInfo);
			// Call the Get() method
			ilGetter.Emit(OpCodes.Callvirt, InterceptorGet);

			// Get the getInfo local variable value
			ilGetter.Emit(OpCodes.Ldloc, getInfo);
			// Call the get_Value method of the GetInfo
			ilGetter.Emit(OpCodes.Callvirt, GetInfoValue);

			// Cast the return value to the type of the property
			ilGetter.Emit(OpCodes.Castclass, propertyInfo.PropertyType);

			// Return the object on the stack, left by the InterceptorGet call
			ilGetter.Emit(OpCodes.Ret);

			return getterBuilder;
		}

		/// <summary>
		///     Create the method invoke
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="methodInfo"></param>
		/// <param name="interceptorField"></param>
		private static void BuildMethod(TypeBuilder typeBuilder, MethodInfo methodInfo, FieldInfo interceptorField)
		{
			var parameterTypes = (
				from parameterInfo in methodInfo.GetParameters()
				select parameterInfo.ParameterType).ToArray();

			var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes, methodInfo.ReturnType, parameterTypes);
			var ilMethod = methodBuilder.GetILGenerator();

			var local = ilMethod.DeclareLocal(typeof (object[]));

			var arraySize = methodInfo.GetParameters().Count();
			ilMethod.Emit(OpCodes.Ldc_I4, arraySize);
			ilMethod.Emit(OpCodes.Newarr, typeof (object));
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
			// Get the interceptor value of this._interceptor
			ilMethod.Emit(OpCodes.Ldfld, interceptorField);
			ilMethod.Emit(OpCodes.Ldstr, methodInfo.Name);
			ilMethod.Emit(OpCodes.Ldloc, local);

			ilMethod.Emit(OpCodes.Callvirt, InterceptorInvoke);
			if (methodInfo.ReturnType == typeof (void))
			{
				ilMethod.Emit(OpCodes.Pop);
			}
			else
			{
				// Cast the return value
				ilMethod.Emit(OpCodes.Castclass, methodInfo.ReturnType);
			}
			ilMethod.Emit(OpCodes.Ret);
		}

		private static FieldInfo BuildProperty(TypeBuilder typeBuilder, string name, Type type)
		{
			Log.Debug().WriteLine("Generating property {0} with type {1}", name, type.FullName);

			var backingField = typeBuilder.DefineField("_" + name.ToLowerInvariant(), type, FieldAttributes.Private | FieldAttributes.HasDefault);
			var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, type, null);

			var getter = typeBuilder.DefineMethod("get_" + name, SetGetMethodAttributes, type, Type.EmptyTypes);
			var getIl = getter.GetILGenerator();
			getIl.Emit(OpCodes.Ldarg_0);
			getIl.Emit(OpCodes.Ldfld, backingField);
			getIl.Emit(OpCodes.Ret);
			propertyBuilder.SetGetMethod(getter);

			var setter = typeBuilder.DefineMethod("set_" + name, SetGetMethodAttributes, null, new[] {type});
			var setIl = setter.GetILGenerator();
			setIl.Emit(OpCodes.Ldarg_0);
			setIl.Emit(OpCodes.Ldarg_1);
			setIl.Emit(OpCodes.Stfld, backingField);
			setIl.Emit(OpCodes.Ret);
			propertyBuilder.SetSetMethod(setter);
			return backingField;
		}

		/// <summary>
		///     Build the getter for the property
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="interceptorField"></param>
		/// <returns>MethodBuilder with the getter</returns>
		private static MethodBuilder BuildSetter(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldInfo interceptorField)
		{
			var setterBuilder = typeBuilder.DefineMethod("set_" + propertyInfo.Name, SetGetMethodAttributes, typeof (void), new[] {propertyInfo.PropertyType});
			var ilSetter = setterBuilder.GetILGenerator();

			// Local SetInfo variable
			var setInfo = ilSetter.DeclareLocal(typeof (SetInfo));

			// Create new SetInfo class for the argument which are passed to the IInterceptor
			ilSetter.Emit(OpCodes.Newobj, SetInfoConstructor);
			// Store it in the local setInfo variable
			ilSetter.Emit(OpCodes.Stloc, setInfo);

			// Get the setInfo local variable value
			ilSetter.Emit(OpCodes.Ldloc, setInfo);
			// Load the argument with the value on the stack
			ilSetter.Emit(OpCodes.Ldarg_1);
			// Set the value to the NewValue property of the SetInfo (call set_NewValue)
			ilSetter.Emit(OpCodes.Callvirt, SetInfoNewValue);

			// Get the setInfo local variable value
			ilSetter.Emit(OpCodes.Ldloc, setInfo);
			// Load the name of the property on the stack
			ilSetter.Emit(OpCodes.Ldstr, propertyInfo.Name);
			// Set the value to the PropertyName property of the SetInfo (call set_PropertyName)
			ilSetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyName);

			// Get the setInfo local variable value
			ilSetter.Emit(OpCodes.Ldloc, setInfo);
			// Load the type of the property on the stack
			ilSetter.Emit(OpCodes.Ldtoken, propertyInfo.PropertyType);
			// Convert the RuntimeTypeHandle to a type
			ilSetter.Emit(OpCodes.Call, typeof (Type).GetMethod("GetTypeFromHandle", new[] {typeof (RuntimeTypeHandle)}));
			// Set the value to the PropertyType property of the SetInfo (call set_PropertyType)
			ilSetter.Emit(OpCodes.Callvirt, GetSetInfoPropertyType);

			// Load the instance of the class (this) on the stack
			ilSetter.Emit(OpCodes.Ldarg_0);
			// Get the interceptor value of this._interceptor
			ilSetter.Emit(OpCodes.Ldfld, interceptorField);
			// Get the setInfo local variable value
			ilSetter.Emit(OpCodes.Ldloc, setInfo);
			// Call the "SetMethod" method
			ilSetter.Emit(OpCodes.Callvirt, InterceptorSet);

			// return
			ilSetter.Emit(OpCodes.Ret);

			return setterBuilder;
		}
#endregion

	}
}