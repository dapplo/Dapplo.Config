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
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dapplo.InterfaceImpl;
using Dapplo.InterfaceImpl.Extensions;
using Dapplo.InterfaceImpl.Implementation;
using Dapplo.LogFacade;
using Dapplo.Utils.Extensions;
using Microsoft.Win32;

#endregion

namespace Dapplo.Config.WindowsRegistry.Implementation
{
	/// <summary>
	///     Extend the PropertyProxy with Registry functionality
	/// </summary>
	[Extension(typeof (IRegistry))]
	public class RegistryExtension<T> : AbstractInterceptorExtension
	{
		private static readonly LogSource Log = new LogSource();
		private RegistryAttribute _registryAttribute;

		/// <summary>
		///     Make sure this extension is initialized last
		/// </summary>
		public override int InitOrder => int.MaxValue;

		/// <summary>
		///     Initialize the extension
		/// </summary>
		public override void Initialize()
		{
			_registryAttribute = typeof (T).GetCustomAttribute<RegistryAttribute>() ?? new RegistryAttribute();
			Interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IRegistry, object>(x => x.PathFor("")), PathFor);
		}

		/// <summary>
		///     Process the property, in our case read the registry
		/// </summary>
		/// <param name="propertyInfo"></param>
		public override void InitProperty(PropertyInfo propertyInfo)
		{
			var registryPropertyAttribute = propertyInfo.GetCustomAttribute<RegistryPropertyAttribute>();

			var path = registryPropertyAttribute.Path;
			if (_registryAttribute.Path != null)
			{
				path = Path.Combine(_registryAttribute.Path, path);
			}
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException($"{propertyInfo.Name} doesn't have a path mapping");
			}
			if (path.StartsWith(@"\"))
			{
				path = path.Remove(0, 1);
			}
			var hive = _registryAttribute.Hive;
			if (registryPropertyAttribute.HasHive)
			{
				hive = registryPropertyAttribute.Hive;
			}

			var view = _registryAttribute.View;
			if (registryPropertyAttribute.HasView)
			{
				view = registryPropertyAttribute.View;
			}

			using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
			{
				using (var key = baseKey.OpenSubKey(path))
				{
					try
					{
						if (key == null)
						{
							throw new ArgumentException($"No registry entry in {hive}/{path} for {view}");
						}
						if (registryPropertyAttribute.Value == null)
						{
							// Read all values, assume IDictionary<string, object>
							IDictionary<string, object> values;
							var getInfo = Interceptor.Get(propertyInfo.Name);
							if (!getInfo.HasValue)
							{
								// No value yet, create a new default
								values = new SortedDictionary<string, object>();
								Interceptor.Set(propertyInfo.Name, values);
							}
							else
							{
								values = (IDictionary<string, object>) getInfo.Value;
							}
							foreach (var valueName in key.GetValueNames())
							{
								var value = key.GetValue(valueName);
								if (!values.ContainsKey(valueName))
								{
									values.Add(valueName, value);
								}
								else
								{
									values[valueName] = value;
								}
							}
						}
						else
						{
							// Read a specific value
							Interceptor.Set(propertyInfo.Name, key.GetValue(registryPropertyAttribute.Value));
						}
					}
					catch (Exception ex)
					{
						Log.Warn().WriteLine(ex.Message);
						if (!registryPropertyAttribute.IgnoreErrors)
						{
							throw;
						}
					}
				}
			}
		}

		/// <summary>
		///     Supply the path to the property
		/// </summary>
		private void PathFor(MethodCallInfo methodCallInfo)
		{
			var propertyInfo = typeof (T).GetProperty(methodCallInfo.PropertyNameOf(0));
			methodCallInfo.ReturnValue = PathFor(propertyInfo);
		}

		/// <summary>
		///     Hrlp
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		private string PathFor(PropertyInfo propertyInfo)
		{
			var registryPropertyAttribute = propertyInfo.GetCustomAttribute<RegistryPropertyAttribute>();

			var path = registryPropertyAttribute.Path;
			if (_registryAttribute.Path != null)
			{
				path = Path.Combine(_registryAttribute.Path, path);
			}
			return path;
		}
	}
}