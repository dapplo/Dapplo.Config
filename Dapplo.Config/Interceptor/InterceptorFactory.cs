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
using Dapplo.Config.Interceptor.IlGeneration;
using Dapplo.Config.Interceptor.Implementation;

#endregion

namespace Dapplo.Config.Interceptor
{
	/// <summary>
	/// This class is a factory which can create an implementation for an interface.
	/// It can new the implementation, and add intercepting code.
	/// </summary>
	public class InterceptorFactory
	{
		private static readonly List<Type> ExtensionTypes = new List<Type>();
		private static readonly IDictionary<Type, Type> TypeMap = new Dictionary<Type, Type>();
		private static readonly IDictionary<Type, Type> BaseTypeMap = new Dictionary<Type, Type>();
		private static readonly IDictionary<Type, Type[]> DefaultInterfacesMap = new Dictionary<Type, Type[]>();

		static InterceptorFactory()
		{
			var types =
				from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where
					!assembly.FullName.StartsWith("System") && !assembly.FullName.StartsWith("mscorlib") && !assembly.FullName.StartsWith("Microsoft") &&
					!assembly.FullName.StartsWith("xunit")
				from someType in assembly.GetTypes()
				where someType.GetCustomAttributes(typeof(ExtensionAttribute), true).Length > 0
				select someType;
			ExtensionTypes.AddRange(types);
		}

		public static void DefineBaseTypeForInterface(Type interfaceType, Type baseType)
		{
			BaseTypeMap.Add(interfaceType, baseType);
		}

		public static void DefineDefaultInterfaces(Type interfaceType, Type[] defaultInterfaces)
		{
			DefaultInterfacesMap.Add(interfaceType, defaultInterfaces);
		}

		/// <summary>
		/// Create an implementation, or reuse an existing, for an interface.
		/// Create an instance, add intercepting code, which implements a range of interfaces
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <returns>implementation</returns>
		public static TResult New<TResult>()
		{
			// create the intercepted object
			var interfaceType = typeof(TResult);
			if (!interfaceType.IsVisible)
			{
				throw new ArgumentException("Internal types are not allowed.", interfaceType.Name);
			}
			if (!interfaceType.IsInterface)
			{
				throw new ArgumentException("Only interfaces are allowed.", nameof(interfaceType));
			}
			// GetInterfaces doesn't return the type itself, so we need to add it.
			var implementingInterfaces = interfaceType.GetInterfaces().Concat(new[] { interfaceType }).ToList();

			var implementingAndDefaultInterfaces = new List<Type>();
			foreach (var implementingInterface in implementingInterfaces.ToList())
			{
				implementingAndDefaultInterfaces.Add(implementingInterface);
				Type[] defaultInterfaces;
				if (DefaultInterfacesMap.TryGetValue(implementingInterface, out defaultInterfaces))
				{
					implementingAndDefaultInterfaces.AddRange(defaultInterfaces);
				}
			}
			implementingInterfaces = implementingAndDefaultInterfaces;

			// Create an implementation, or lookup
			Type implementingType;
			if (!TypeMap.TryGetValue(interfaceType, out implementingType))
			{
				// Use this baseType if nothing is specified
				Type baseType = typeof(ExtensibleInterceptorImpl<>);
				foreach(var implementingInterface in implementingInterfaces.Distinct())
				{
					if (BaseTypeMap.ContainsKey(implementingInterface))
					{
						baseType = BaseTypeMap[implementingInterface];
						break;
					}
				}
				// Make sure we have a non generic type, by filling in the "blanks"
				if (baseType.IsGenericType)
				{
					baseType = baseType.MakeGenericType(interfaceType);
				}
				implementingType = IlTypeBuilder.CreateType("Dapplo.Config.Interceptor", interfaceType.Name + "Impl", implementingInterfaces.Distinct().ToArray(), baseType);
				TypeMap.Add(interfaceType, implementingType);
			}

			// Create an instance for the implementation
			var result = (TResult)Activator.CreateInstance(implementingType);

			// cast to IIntercepted, so we can set the interceptor and use it in the extensions
			var interceptor = result as IExtensibleInterceptor;
			if (interceptor == null)
			{
				throw new ArgumentNullException(nameof(interceptor), "The created type didn't implement IExtensibleInterceptor.");
			}

			// Add the extensions
			foreach (var extensionType in ExtensionTypes)
			{
				var extensionAttributes = (ExtensionAttribute[]) extensionType.GetCustomAttributes(typeof (ExtensionAttribute), false);
				foreach (var extensionAttribute in extensionAttributes)
				{
					var implementing = extensionAttribute.Implementing;
					if (implementingInterfaces.Contains(implementing))
					{
						interceptor.AddExtension(extensionType);
					}
				}
			}
			interceptor.Init();
			return result;
		}

	}
}