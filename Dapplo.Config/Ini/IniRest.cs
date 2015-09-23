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

using Dapplo.Config.Support;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dapplo.Config.Ini
{
	public static class IniRest
	{
		/// <summary>
		/// Process an Rest URI, this can be used to read or write values via e.g. a HttpListener
		/// format:
		/// schema://hostname:port/IniConfig/Command/Applicationname/Configname/Section/Property/NewValue(optional)?query
		/// schema is not important, this can be an application specific thing
		/// hostname is not important, this can be an application specific thing
		/// port is not important, this can be an application specific thing
		/// The command is get/set/add/remove/reset
		/// the Applicationname & Configname must be registered by new IniConfig(Applicationname,Configname)
		/// The Section is that which is used in the IniSection
		/// The property needs to be available
		/// NewValue is optional (read) can be used to set the property (write)
		/// The query can be used to add values to lists (?item1&item2&item2) or dictionaries (?key1=value1&key2=value2)
		/// Or when removing from lists (?item1&item2&item2) or dictionaries (?key1&key2)
		/// </summary>
		/// <param name="restUri"></param>
		/// <returns>Value (before set) or actual value with get/add/remove</returns>
		public static IniValue ProcessRestUri(Uri restUri)
		{
			var removeSlash = new Regex(@"\/$");
			var segments = (from segment in restUri.Segments.Skip(1)
							select removeSlash.Replace(segment, "")).ToList();

			if (segments[0] != "IniConfig")
			{
				return null;
			}
			segments.RemoveAt(0);
			var command = segments[0];
			segments.RemoveAt(0);
			var iniConfig = IniConfig.Get(segments[0], segments[1]);
			segments.RemoveAt(0);
			segments.RemoveAt(0);
			var iniSection = iniConfig[segments[0]];
			segments.RemoveAt(0);
			var iniValue = iniSection[segments[0]];
			segments.RemoveAt(0);

			var iniValueType = iniValue.Value != null ? iniValue.Value.GetType() : iniValue.ValueType;

			switch (command)
			{
				case "set":
					if (segments.Count > 0)
					{
						if (iniValueType.IsGenericDirectory() || iniValueType.IsGenericList())
						{
							throw new NotSupportedException(string.Format("Can't set type of {0}, use add/remove", iniValueType));
						}
						iniValue.Value = iniValue.Converter.ConvertFromInvariantString(segments[0]);
					}
					break;
				case "reset":
					iniValue.ResetToDefault();
					break;
				case "get":
					// Ignore, as the default logic covers a read
					break;
				case "remove":
					if (iniValueType.IsGenericDirectory() || iniValueType.IsGenericList())
					{
						Type itemType = iniValueType.GetGenericArguments()[0];
						var converter = itemType.GetTypeConverter();
						// TODO: Fix IList<T>.Remove not found!
						var removeMethodInfo = iniValueType.GetMethod("Remove");
						var removeQuestionmark = new Regex(@"^\?");
						var itemsToRemove = (from item in restUri.Query.Split('&')
											 select converter.ConvertFromInvariantString(removeQuestionmark.Replace(item, ""))).ToList();
						foreach (var item in itemsToRemove)
						{
							removeMethodInfo.Invoke(iniValue.Value, new[] { item });
						}
					}
					else
					{
						throw new NotSupportedException(string.Format("Can't remove from type {0}, use set / reset", iniValueType));
					}
					break;
				case "add":
					if (iniValueType.IsGenericDirectory())
					{
						var variables = restUri.QueryToDictionary();
						Type keyType = iniValueType.GetGenericArguments()[0];
						var keyConverter = keyType.GetTypeConverter();
						Type valueType = iniValueType.GetGenericArguments()[1];
						var valueConverter = valueType.GetTypeConverter();
						var addMethodInfo = iniValueType.GetMethod("Add");
						foreach (var key in variables.Keys)
						{
							var keyObject = keyConverter.ConvertFromInvariantString(key);
							var valueObject = valueConverter.ConvertFromInvariantString(variables[key]);
							addMethodInfo.Invoke(iniValue.Value, new[] { keyObject, valueObject });
						}
					}
					else if (iniValueType.IsGenericList())
					{
						Type itemType = iniValueType.GetGenericArguments()[0];
						var converter = itemType.GetTypeConverter();
						var itemValue = converter.ConvertFromInvariantString(segments[0]);
						var addMethodInfo = iniValueType.GetMethod("Add");
						addMethodInfo.Invoke(iniValue.Value, new[]{ itemValue });
					}
					else
					{
						throw new NotSupportedException(string.Format("Can't add to type {0}, use set / reset", iniValueType));
					}
					break;
				default:
					throw new NotSupportedException(string.Format("Don't know command {0}, there is only get/set/reset/add/remove", command));
			}
			return iniValue;
		}

	}
}
