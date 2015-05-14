/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Config.Extension.Implementation;
using Dapplo.Config.Support;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dapplo.Config.WindowsRegistry.Implementation
{
	/// <summary>
	/// Extend the PropertyProxy with Registry functionality
	/// </summary>
	[Extension(typeof(IRegistry))]
	internal class RegistryExtension<T> : AbstractPropertyProxyExtension<T>
	{
		private readonly RegistryAttribute _registryAttribute;
		public RegistryExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof(IRegistry));
			_registryAttribute = typeof(T).GetCustomAttribute<RegistryAttribute>();
			if (_registryAttribute == null)
			{
				_registryAttribute = new RegistryAttribute();
			}
			Proxy.RegisterMethod(ConfigUtils.GetMemberName<IRegistry, object>(x => x.PathFor("")), PathFor);
		}

		/// <summary>
		/// Make sure this extension is initialized last
		/// </summary>
		public override int InitOrder
		{
			get
			{
				return int.MaxValue;
			}
		}

		/// <summary>
		/// Process the property, in our case read the registry
		/// </summary>
		/// <param name="propertyInfo"></param>
		public override void InitProperty(PropertyInfo propertyInfo)
		{
			var registryPropertyAttribute = propertyInfo.GetCustomAttribute<RegistryPropertyAttribute>();

			string path = registryPropertyAttribute.Path;
			if (_registryAttribute.Path != null)
			{
				path = Path.Combine(_registryAttribute.Path, path);
			}
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException(string.Format("{0} doesn't have a path mapping", propertyInfo.Name));
			}
			if (path.StartsWith(@"\"))
			{
				path = path.Remove(0,1);
			}
			RegistryHive hive = _registryAttribute.Hive;
			if (registryPropertyAttribute.HasHive)
			{
				hive = registryPropertyAttribute.Hive;
			}

			RegistryView view = _registryAttribute.View;
			if (registryPropertyAttribute.HasView)
			{
				view = registryPropertyAttribute.View;
			}

			using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view))
			using (RegistryKey key = baseKey.OpenSubKey(path))
			{
				try
				{
					if (key == null)
					{
						throw new ArgumentException(string.Format("No registry entry in {0}/{1} for {2}", hive, path, view));
					}
					if (registryPropertyAttribute.Value == null)
					{
						// Read all values, assume IDictionary<string, object>
						IDictionary<string, object> values;
						var getInfo = Proxy.Get(propertyInfo.Name);
						if (!getInfo.HasValue)
						{
							// No value yet, create a new default
							values = new SortedDictionary<string, object>();
							Proxy.Set(propertyInfo.Name, values);
						}
						else
						{
							values = (IDictionary<string, object>)getInfo.Value;
						}
						foreach (string valueName in key.GetValueNames())
						{
							object value = key.GetValue(valueName);
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
						Proxy.Set(propertyInfo.Name, key.GetValue(registryPropertyAttribute.Value));
					}
				}
				catch
				{
					if (!registryPropertyAttribute.IgnoreErrors)
					{
						throw;
					}
				}
			}
		}

		/// <summary>
		/// Supply the path to the property
		/// </summary>
		private void PathFor(MethodCallInfo methodCallInfo)
		{
			PropertyInfo propertyInfo = typeof(T).GetProperty(methodCallInfo.PropertyNameOf(0));
			methodCallInfo.ReturnValue = PathFor(propertyInfo);
		}

		/// <summary>
		/// Hrlp
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		private string PathFor(PropertyInfo propertyInfo)
		{
			var registryPropertyAttribute = propertyInfo.GetCustomAttribute<RegistryPropertyAttribute>();

			string path = registryPropertyAttribute.Path;
			if (_registryAttribute.Path != null)
			{
				path = Path.Combine(_registryAttribute.Path, path);
			}
			return path;
		}
	}
}