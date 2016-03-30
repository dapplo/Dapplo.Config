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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dapplo.LogFacade;

#endregion

namespace Dapplo.Config.Interceptor.IlGeneration
{
	/// <summary>
	///     Internally used to generate a method via IL
	/// </summary>
	internal static class IlTypeBuilder
	{
		private static readonly LogSource Log = new LogSource();
		private static string _assemblyNameString = "Dapplo.Config.Interceptor.Impl";
		private static AssemblyBuilder _assemblyBuilder;
		private static ModuleBuilder _moduleBuilder;
		private static bool _isInitialized;

		/// <summary>
		/// Make it possible to save the assembly after generating types.
		/// </summary>
		public static bool AllowSave
		{
			get;
			set;
		}

		/// <summary>
		///     Creates an implementation as Type for a given interface, which can be intercepted
		/// </summary>
		/// <param name="assemblyNameString">Name of the assembly to add the type to</param>
		/// <param name="typeName">Name of the type to generate</param>
		/// <param name="implementingInterfaces">Interfaces to implement</param>
		/// <param name="baseType">Type as base</param>
		/// <returns>Type</returns>
		internal static Type CreateType(string typeName, Type[] implementingInterfaces, Type baseType)
		{
			Log.Verbose().WriteLine("Creating type {0}", typeName);
			// Cache the assembly/module builder
			if (!_isInitialized)
			{
				string dllName = $"{_assemblyNameString}.dll";
				var assemblyName = new AssemblyName(_assemblyNameString);
				var appDomain = AppDomain.CurrentDomain;
				_assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AllowSave ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.RunAndCollect, appDomain.BaseDirectory);
				_moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName.Name, dllName, false);
				_isInitialized = true;
			}

			// Create the type, and let it implement our interface
			var typeBuilder = _moduleBuilder.DefineType(typeName,
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
				baseType, implementingInterfaces);

			// Make a collection of already implemented properties
			var baseProperties = baseType.GetRuntimeProperties().Select(x=> x.Name);

			var propertyInfos =
				from iface in implementingInterfaces
				from propertyInfo in iface.GetProperties()
				where !baseProperties.Contains(propertyInfo.Name)
				select propertyInfo;

			foreach (var propertyInfo in propertyInfos)
			{
				if (propertyInfo.Name == "Interceptor")
				{
					Log.Verbose().WriteLine("Skipping property {0}", propertyInfo.Name);
					continue;
				}
				if (!propertyInfo.CanRead && !propertyInfo.CanWrite)
				{
					Log.Verbose().WriteLine("Skipping property {0}", propertyInfo.Name);
					continue;
				}

				// Create get and/or set
				IlGetSetBuilder.BuildGetSet(typeBuilder, propertyInfo);
			}

			// Make a collection of already implemented method
			var baseMethods = baseType.GetRuntimeMethods().Select(x => x.Name);

			var methodInfos =
				from iface in implementingInterfaces
				from methodInfo in iface.GetMethods()
				where !baseMethods.Contains(methodInfo.Name)
				select methodInfo;

			foreach (var methodInfo in methodInfos)
			{
				if (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))
				{
					Log.Verbose().WriteLine("Skipping method {0}", methodInfo.Name);
					continue;
				}
				IlMethodBuilder.BuildMethod(typeBuilder, methodInfo);
				Log.Verbose().WriteLine("Created method {0}", methodInfo.Name);
			}
			Log.Verbose().WriteLine("Created type {0}", typeName);
			return typeBuilder.CreateType();
		}

		/// <summary>
		/// Save the "up to now" generated assembly
		/// </summary>
		/// <param name="dllName">Full path for the DLL</param>
		public static void SaveAssemblyDll(string dllName)
		{
			if (!AllowSave)
			{
				throw new InvalidOperationException("Only allowed when before generation types the AllowSave was set to true.");
			}
			_assemblyBuilder.Save(dllName, PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);
			Log.Debug().WriteLine("Wrote {0} to {1}", dllName, AppDomain.CurrentDomain.BaseDirectory);
		}
	}
}