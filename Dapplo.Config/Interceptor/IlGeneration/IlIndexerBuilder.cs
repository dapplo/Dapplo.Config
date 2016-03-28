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
	/// Internally used to generate a method as indexer via IL
	/// </summary>
	internal static class IlIndexerBuilder
	{
		private static readonly LogSource Log = new LogSource();

		private static readonly MethodInfo InterceptorInvoke = typeof(IInterceptor).GetMethod("Invoke");
		private static readonly MethodAttributes IndexerMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;

		/// <summary>
		/// Build with IL a get for the indexer
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="interceptorField"></param>
		/// <returns>MethodBuilder</returns>
		internal static MethodBuilder GenerateIlGetIndexerMethod(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldInfo interceptorField)
		{
			if (interceptorField == null)
			{
				throw new ArgumentNullException(nameof(interceptorField));
			}
			var methodName = "get_" + propertyInfo.Name;
			var parameterTypes = new[] { propertyInfo.GetIndexParameters().First().ParameterType };
			var getterBuilder = typeBuilder.DefineMethod(methodName, IndexerMethodAttributes, propertyInfo.PropertyType, parameterTypes);
			var ilGetter = getterBuilder.GetILGenerator();
			var local = ilGetter.DeclareLocal(typeof(object[]));

			var arraySize = 1;
			ilGetter.Emit(OpCodes.Ldc_I4, arraySize);
			ilGetter.Emit(OpCodes.Newarr, typeof(object));
			ilGetter.Emit(OpCodes.Stloc, local);
			for (var i = 0; i < arraySize; i++)
			{
				ilGetter.Emit(OpCodes.Ldloc, local);
				ilGetter.Emit(OpCodes.Ldc_I4, i);
				ilGetter.Emit(OpCodes.Ldarg, i + 1);
				ilGetter.Emit(OpCodes.Stelem_Ref);
			}

			// Load the instance of the class (this) on the stack
			ilGetter.Emit(OpCodes.Ldarg_0);
			// Get the interceptor value of this._interceptor
			ilGetter.Emit(OpCodes.Ldfld, interceptorField);
			ilGetter.Emit(OpCodes.Ldstr, methodName);
			ilGetter.Emit(OpCodes.Ldloc, local);

			ilGetter.Emit(OpCodes.Callvirt, InterceptorInvoke);
			if (propertyInfo.PropertyType == typeof(void))
			{
				ilGetter.Emit(OpCodes.Pop);
			}
			else if (propertyInfo.PropertyType.IsValueType)
			{
				ilGetter.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
			}
			else
			{
				// Cast the return value
				ilGetter.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
			}
			ilGetter.Emit(OpCodes.Ret);
			return getterBuilder;
		}

		/// <summary>
		/// Build with IL a set for the indexer
		/// </summary>
		/// <param name="typeBuilder"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="interceptorField"></param>
		/// <returns>MethodBuilder</returns>
		internal static MethodBuilder GenerateIlSetIndexerMethod(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldInfo interceptorField)
		{
			if (interceptorField == null)
			{
				throw new ArgumentNullException(nameof(interceptorField));
			}

			// TODO: unfinished method!!!

			var methodName = "set_" + propertyInfo.Name;
			var parameterTypes = new[] { propertyInfo.GetIndexParameters().First().ParameterType };
			var getterBuilder = typeBuilder.DefineMethod(methodName, IndexerMethodAttributes, propertyInfo.PropertyType, parameterTypes);
			var ilGetter = getterBuilder.GetILGenerator();
			var local = ilGetter.DeclareLocal(typeof(object[]));

			var arraySize = 1;
			ilGetter.Emit(OpCodes.Ldc_I4, arraySize);
			ilGetter.Emit(OpCodes.Newarr, typeof(object));
			ilGetter.Emit(OpCodes.Stloc, local);
			for (var i = 0; i < arraySize; i++)
			{
				ilGetter.Emit(OpCodes.Ldloc, local);
				ilGetter.Emit(OpCodes.Ldc_I4, i);
				ilGetter.Emit(OpCodes.Ldarg, i + 1);
				ilGetter.Emit(OpCodes.Stelem_Ref);
			}

			// Load the instance of the class (this) on the stack
			ilGetter.Emit(OpCodes.Ldarg_0);
			// Get the interceptor value of this._interceptor
			ilGetter.Emit(OpCodes.Ldfld, interceptorField);
			ilGetter.Emit(OpCodes.Ldstr, methodName);
			ilGetter.Emit(OpCodes.Ldloc, local);

			ilGetter.Emit(OpCodes.Callvirt, InterceptorInvoke);
			if (propertyInfo.PropertyType == typeof(void))
			{
				ilGetter.Emit(OpCodes.Pop);
			}
			else if (propertyInfo.PropertyType.IsValueType)
			{
				ilGetter.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
			}
			else
			{
				// Cast the return value
				ilGetter.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
			}
			ilGetter.Emit(OpCodes.Ret);
			return getterBuilder;
		}
	}
}
