//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Dapplo.Log;

#endregion

namespace Dapplo.Config.Ini.Rest
{
	/// <summary>
	///     This class implements a "REST" API for the Ini configuration
	/// </summary>
	public static class IniRest
	{
		private static readonly LogSource Log = new LogSource();

        /// <summary>
        ///     Process the supplied IniRestCommand
        /// </summary>
        /// <param name="restCommand">IniRestCommand to process</param>
        /// <param name="iniFileContainer">IniFileContainer</param>
        public static void ProcessRestCommand(IniRestCommand restCommand, IniFileContainer iniFileContainer)
		{
			var iniSection = iniFileContainer[restCommand.Section];

			if (restCommand.Command == IniRestCommands.Add || restCommand.Command == IniRestCommands.Remove)
			{
				if (restCommand.Target is null && restCommand.Values.Count == 0)
				{
					const string message = "add/remove needs a target";
					Log.Error().WriteLine(message);
					throw new ArgumentException(message);
				}
				var iniValue = iniSection.GetIniValue(restCommand.Target);
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
						if (addMethodInfo is null)
						{
							Log.Error().WriteLine("The ini-value doesn't have an add method");
							return;
						}
						foreach (var valueKey in restCommand.Values.Keys)
						{
							var key = keyConverter.ConvertFromInvariantString(valueKey);
							if (valueConverter != null)
							{
								var value = valueConverter.ConvertFromInvariantString(restCommand.Values[valueKey]);
								if (removeMethodInfo is null)
								{
									Log.Error().WriteLine("The ini-value doesn't have a remove method");
									return;
								}
								// IDictionary, remove the value for the key first, so we don't need to check if it's there
								removeMethodInfo.Invoke(iniValue.Value, new[] {key});
								// Now add it
								addMethodInfo.Invoke(iniValue.Value, new[] {key, value});
							}
							else
							{
								// ICollection
								addMethodInfo.Invoke(iniValue.Value, new[] {key});
							}
						}
						return;
					case IniRestCommands.Remove:
						if (removeMethodInfo is null)
						{
							Log.Error().WriteLine("The ini-value doesn't have a remove method");
							return;
						}
						var itemType = iniValueType.GetGenericArguments()[0];
						var converter = TypeDescriptor.GetConverter(itemType);
						// TODO: Fix IList<T>.Remove not found!
						foreach (var valueKey in restCommand.Values.Keys)
						{
							removeMethodInfo.Invoke(iniValue.Value, new[] {converter.ConvertFromInvariantString(valueKey)});
						}
						return;
				}
			}


			foreach (var key in restCommand.Target != null ? new[] {restCommand.Target} : restCommand.Values.Keys)
			{
				var iniValue = iniSection.GetIniValue(key);
				if (iniValue is null)
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

        /// <summary>
        ///     Process an Rest URI, this can be used to read or write values via e.g. a HttpListener
        ///     format:
        ///     schema://hostname:port/IniConfig/Command/Applicationname/Configname/Section/Property/NewValue(optional)?query
        ///     schema is not important, this can be an application specific thing
        ///     hostname is not important, this can be an application specific thing
        ///     port is not important, this can be an application specific thing
        ///     The command is get/set/add/remove/reset
        ///     the Applicationname and Configname must be registered by new IniConfig(Applicationname,Configname)
        ///     The Section is that which is used in the IniSection
        ///     The property needs to be available
        ///     NewValue is optional (read) can be used to set the property (write)
        ///     The query can be used to add values to lists (?item1&amp;item2&amp;item2) or dictionaries (?key1=value1&amp;
        ///     key2=value2)
        ///     Or when removing from lists (?item1&amp;item2&amp;item2) or dictionaries (?key1&amp;key2)
        ///     P.S.
        ///     You can use the ProtocolHandler to register a custom URL protocol.
        /// </summary>
        /// <param name="restUri">Uri</param>
        /// <param name="iniFileContainer">IniFileContainer</param>
        /// <returns>IniRestCommand with all details and the result</returns>
        public static IniRestCommand ProcessRestUri(Uri restUri, IniFileContainer iniFileContainer)
		{
			Log.Debug().WriteLine("Processing REST uri: {0}", restUri);

			var restCommand = new IniRestCommand();

			var removeSlash = new Regex(@"\/$");
			var segments = (from segment in restUri.Segments.Skip(1)
				select removeSlash.Replace(segment, string.Empty)).ToList();

			if ("ini".Equals(segments[0], StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("Doesn't contain a ini link", nameof(restUri));
			}

			segments.RemoveAt(0);
			if (!Enum.TryParse(segments[0], true, out IniRestCommands command))
			{
				var message = $"{segments[0]} is not a valid command: get/set/reset/add/remove";
				Log.Error().WriteLine(message);
				throw new ArgumentException(message);
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
				restCommand.Target = Uri.UnescapeDataString(segments[0]);
			}
			else
			{
				while (segments.Count > 1)
				{
					var key = Uri.UnescapeDataString(segments[0]);
					segments.RemoveAt(0);
					var value = segments.Count >= 1 ? Uri.UnescapeDataString(segments[0]) : null;
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
					restCommand.Values.Add(Uri.UnescapeDataString(splitItem[0]), splitItem.Length > 1 ? Uri.UnescapeDataString(splitItem[1]) : null);
				}
			}
			ProcessRestCommand(restCommand, iniFileContainer);
			return restCommand;
		}
	}
}