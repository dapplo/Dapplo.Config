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

		/// <summary>
		///     Creates an implementation as Type for a given interface, which can be intercepted
		/// </summary>
		/// <param name="assemblyNameString"></param>
		/// <param name="interfaceType"></param>
		/// <returns>Type</returns>
		internal static Type CreateType(string assemblyNameString, Type interfaceType)
		{
			string dllName = $"{assemblyNameString}.dll";
			var assemblyName = new AssemblyName(assemblyNameString);
			var appDomain = AppDomain.CurrentDomain;
			var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, AppDomain.CurrentDomain.BaseDirectory);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, dllName, false);

			string indexerName = null;

			// GetInterfaces doesn't return the type itself, so we need to add it.
			var implementingInterfaces = interfaceType.GetInterfaces().Concat(new[] {interfaceType, typeof (IIntercepted)}).Distinct().ToArray();
			// Create the type, and let it implement our interface
			var typeBuilder = moduleBuilder.DefineType(interfaceType.Name + "Impl",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
				typeof (object), implementingInterfaces);

			foreach (var implementingInterface in implementingInterfaces)
			{
				var defaultMemberAttribute = implementingInterface.GetCustomAttributes<DefaultMemberAttribute>().FirstOrDefault();
				if (defaultMemberAttribute != null)
				{
					var memberParams = new[] {typeof (string)};
					var constructor = typeof (DefaultMemberAttribute).GetConstructor(memberParams);
					if (constructor != null)
					{
						indexerName = defaultMemberAttribute.MemberName;
						var customAttributeBuilder = new CustomAttributeBuilder(constructor, new object[] {indexerName});
						typeBuilder.SetCustomAttribute(customAttributeBuilder);
					}
				}
			}

			// Add the interceptor property
			var interceptorField = IlGetSetBuilder.BuildProperty(typeBuilder, "Interceptor", typeof (IInterceptor));

			var propertyInfos =
				from iface in implementingInterfaces
				from propertyInfo in iface.GetProperties()
				select propertyInfo;

			foreach (var propertyInfo in propertyInfos)
			{
				if (propertyInfo.Name == "Interceptor")
				{
					Log.Debug().WriteLine("Skipping property {0}", propertyInfo.Name);
					continue;
				}
				if (!propertyInfo.CanRead && !propertyInfo.CanWrite)
				{
					Log.Debug().WriteLine("Skipping property {0}", propertyInfo.Name);
					continue;
				}

				// Create get and/or set
				IlGetSetBuilder.BuildGetSet(typeBuilder, propertyInfo, propertyInfo.Name == indexerName, interceptorField);
			}

			var methodInfos =
				from iface in implementingInterfaces
				from methodInfo in iface.GetMethods()
				select methodInfo;

			foreach (var methodInfo in methodInfos)
			{
				if (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))
				{
					Log.Debug().WriteLine("Skipping method {0}", methodInfo.Name);
					continue;
				}
				IlMethodBuilder.BuildMethod(typeBuilder, methodInfo, interceptorField);
				Log.Debug().WriteLine("Created method {0}", methodInfo.Name);
			}

			// Example for making a exe, for a methodBuilder which creates a static main
			//assemblyBuilder.SetEntryPoint(methodBuilder,PEFileKinds.Exe);
			var returnType = typeBuilder.CreateType();
			//Log.Debug().WriteLine("Wrote {0} to {1}", dllName, AppDomain.CurrentDomain.BaseDirectory);
			//assemblyBuilder.Save(dllName, PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);
			return returnType;
		}
	}
}