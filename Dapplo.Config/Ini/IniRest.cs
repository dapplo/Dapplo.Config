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

using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
		/// The query can be used to add values to lists (?item1&amp;item2&amp;item2) or dictionaries (?key1=value1&amp;key2=value2)
		/// Or when removing from lists (?item1&amp;item2&amp;item2) or dictionaries (?key1&amp;key2)
		/// 
		/// P.S.
		/// You can use the ProtocolHandler to register a custom URL protocol.
		/// </summary>
		/// <param name="restUri"></param>
		/// <returns>IniRestCommand with all details and the result</returns>
		public static IniRestCommand ProcessRestUri(Uri restUri)
		{
			var restCommand = new IniRestCommand();

			var removeSlash = new Regex(@"\/$");
			var segments = (from segment in restUri.Segments.Skip(1)
							select removeSlash.Replace(segment, "")).ToList();

			if (segments[0] != "IniConfig")
			{
				return null;
			}
			segments.RemoveAt(0);
			IniRestCommands command;
			if (!Enum.TryParse(segments[0], true, out command))
			{
				throw new ArgumentException($"{segments[0]} is not a valid command: get/set/reset/add/remove");
			}
			restCommand.Command = command;

			segments.RemoveAt(0);
			restCommand.Application = segments[0];

			segments.RemoveAt(0);
			restCommand.File = segments[0];

			segments.RemoveAt(0);
			restCommand.Section = segments[0];

			segments.RemoveAt(0);
			if (segments.Count == 1)
			{
				restCommand.Target = WebUtility.UrlDecode(segments[0]);
            }
			else 
			{
				while(segments.Count > 1)
				{
					var key = WebUtility.UrlDecode(segments[0]);
					segments.RemoveAt(0);
					var value = segments.Count >= 1 ? WebUtility.UrlDecode(segments[0]) : null;
					if (value != null)
					{
						segments.RemoveAt(0);
					}
					restCommand.Values.Add(key, value);
				}
			}

			if (!string.IsNullOrEmpty(restUri.Query))
			{
				foreach (var item in restUri.Query.Substring(1).Split('&'))
				{
					var splitItem = item.Split('=');
					restCommand.Values.Add(WebUtility.UrlDecode(splitItem[0]), splitItem.Length > 1 ? WebUtility.UrlDecode(splitItem[1]) : null);
				}

			}
			ProcessRestCommand(restCommand);
			return restCommand;
		}

		/// <summary>
		/// Process the supplied IniRestCommand
		/// </summary>
		/// <param name="restCommand">IniRestCommand to process</param>
		public static void ProcessRestCommand(IniRestCommand restCommand) {

			var iniConfig = IniConfig.Get(restCommand.Application, restCommand.File);
			var iniSection = iniConfig[restCommand.Section];

			if (restCommand.Command == IniRestCommands.Add || restCommand.Command == IniRestCommands.Remove)
			{
				if (restCommand.Target == null && restCommand.Values.Count == 0)
				{
					throw new ArgumentException("add/remove needs a target");
				}
				var iniValue = iniSection[restCommand.Target];
				restCommand.Results.Add(iniValue);
				var iniValueType = iniValue.Value?.GetType() ?? iniValue.ValueType;
				var removeMethodInfo = iniValueType.GetMethod("Remove");
				switch (restCommand.Command)
				{
					case IniRestCommands.Add:
						var genericArguments = iniValueType.GetGenericArguments();
						var keyConverter = TypeDescriptor.GetConverter(genericArguments[0]);

						// Only for IDictionary
						TypeConverter valueConverter = null;
                        if (genericArguments.Length == 2)
						{
							valueConverter = TypeDescriptor.GetConverter(genericArguments[1]);
						}
						var addMethodInfo = iniValueType.GetMethod("Add");

						foreach (var valueKey in restCommand.Values.Keys)
						{
							var key = keyConverter.ConvertFromInvariantString(valueKey);
							if (valueConverter != null)
							{
								var value = valueConverter.ConvertFromInvariantString(restCommand.Values[valueKey]);

								// IDictionary, remove the value for the key first, so we don't need to check if it's there
								removeMethodInfo.Invoke(iniValue.Value, new[] { key });
								// Now add it
								addMethodInfo.Invoke(iniValue.Value, new[] { key, value });
							}
							else
							{
								// ICollection
								addMethodInfo.Invoke(iniValue.Value, new[] { key });
							}
						}
						return;
					case IniRestCommands.Remove:
						Type itemType = iniValueType.GetGenericArguments()[0];
						var converter = TypeDescriptor.GetConverter(itemType);
						// TODO: Fix IList<T>.Remove not found!
						foreach (var valueKey in restCommand.Values.Keys)
						{
							removeMethodInfo.Invoke(iniValue.Value, new[] { converter.ConvertFromInvariantString(valueKey) });
						}
						return;
				}
			}


			foreach (var key in restCommand.Target != null ? new [] { restCommand.Target } : restCommand.Values.Keys)
			{
				var iniValue = iniSection[key];
				if (iniValue == null)
				{
					continue;
				}
				switch (restCommand.Command)
				{

					case IniRestCommands.Set:
						iniValue.Value = restCommand.Values[key];
						restCommand.Results.Add(iniValue);
						break;
					case IniRestCommands.Get:
						restCommand.Results.Add(iniValue);
						break;
					case IniRestCommands.Reset:
						iniValue.ResetToDefault();
						break;
				}
			}
		}
	}
}
