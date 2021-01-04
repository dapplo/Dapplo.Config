// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TinyCsvParser.Tokenizer.RFC4180;

namespace Dapplo.Config.Extensions
{
	/// <summary>
	///     This class contains extensions for strings
	/// </summary>
	public static class StringExtensions
	{
		private static readonly Regex CleanupRegex = new Regex(@"[^a-z0-9]+", RegexOptions.Compiled);

		private static readonly Regex PropertyRegex = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		/// <summary>
		///     Helper method for converting a string to a non strict value.
		///     This means, ToLowerInvariant and remove all non alpha/digits
		/// </summary>
		/// <param name="value">string</param>
		/// <returns>string which is tolower an only has alpha and digits</returns>
		public static string RemoveNonAlphaDigitsToLower(this string value)
		{
			return CleanupRegex.Replace(value.ToLowerInvariant(), "");
		}

		/// <summary>
		///     Format the string "format" with the source
		/// </summary>
		/// <param name="format">String with formatting, like {name}</param>
		/// <param name="sources">
		///     object [] with properties, if a property has the type IDictionary string,string it can used these
		///     parameters too
		/// </param>
		/// <returns>Formatted string</returns>
		public static string FormatWith(this string format, params object[] sources)
		{
			return FormatWith(format, null, sources);
		}

		/// <summary>
		///     Format the string "format" with the source
		/// </summary>
		/// <param name="format">String with formatting, like {name}</param>
		/// <param name="provider">IFormatProvider</param>
		/// <param name="sources">
		///     object with properties, if a property has the type IDictionary string,string it can used these
		///     parameters too
		/// </param>
		/// <returns>Formatted string</returns>
		public static string FormatWith(this string format, IFormatProvider provider, params object[] sources)
		{
			if (format == null)
			{
				throw new ArgumentNullException(nameof(format));
			}
			if (sources == null)
			{
				return format;
			}
			var properties = new Dictionary<string, object>();

			for (var index = 0; index < sources.Length; index++)
			{
				var source = sources[index];
				MapToProperties(properties, index, source);
			}

			var values = new List<object>();
			var rewrittenFormat = PropertyRegex.Replace(format, delegate(Match m)
			{
				var startGroup = m.Groups["start"];
				var propertyGroup = m.Groups["property"];
				var formatGroup = m.Groups["format"];
				var endGroup = m.Groups["end"];

				if (properties.TryGetValue(propertyGroup.Value, out var value))
				{
					values.Add(value is Enum enumValue ? enumValue.EnumValueOf() : value);
				}
				else
				{
					values.Add(propertyGroup.Value);
				}
				return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value + new string('}', endGroup.Captures.Count);
			});

			return string.Format(provider, rewrittenFormat, values.ToArray());
		}


		/// <summary>
		///     Helper method to fill the properties with the values from the source
		/// </summary>
		/// <param name="properties">IDictionary with the possible properties</param>
		/// <param name="index">int with index in the current sources</param>
		/// <param name="source">object</param>
		private static void MapToProperties(IDictionary<string, object> properties, int index, object source)
		{
			if (source == null)
			{
				return;
			}
			var sourceType = source.GetType();
			if (sourceType.GetTypeInfo().IsPrimitive || sourceType == typeof(string))
			{
				properties.AddWhenNew(index.ToString(), source);
				return;
			}

			if (properties.DictionaryToGenericDictionary(source as IDictionary))
			{
				return;
			}

			foreach (var propertyInfo in source.GetType().GetRuntimeProperties())
			{
				if (!propertyInfo.CanRead)
				{
					continue;
				}

				var value = propertyInfo.GetValue(source, null);
				if (value == null)
				{
					properties.AddWhenNew(propertyInfo.Name, "");
					continue;
				}

				if (properties.DictionaryToGenericDictionary(value as IDictionary))
				{
					continue;
				}

				if (propertyInfo.PropertyType.GetTypeInfo().IsEnum)
				{
					var enumValue = value as Enum;
					properties.AddWhenNew(propertyInfo.Name, enumValue.EnumValueOf());
					continue;
				}
				properties.AddWhenNew(propertyInfo.Name, value);
			}
		}

		/// <summary>
		///     Check if 2 strings are equal if both are made ToLower and all non alpha and digits are removed.
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <returns>true if they are 'equal'</returns>
		public static bool NonStrictEquals(this string value1, string value2)
		{
			var abcComparer = new AbcComparer();
			return abcComparer.Equals(value1, value2);
		}

		/// <summary>
		///     Extension method to remove start and end quotes.
		/// </summary>
		/// <param name="input"></param>
		/// <returns>string</returns>
		public static string RemoveStartEndQuotes(this string input)
		{
			if (input == null)
			{
				return null;
			}
			if (input.StartsWith("\"") && input.EndsWith("\""))
			{
				return input.Substring(1, input.Length - 2);
			}
			return input;
		}

		/// <summary>
		///     Parse input for comma separated values
		/// </summary>
		/// <param name="input">string with comma separated values</param>
		/// <param name="delimiter">string with delimiters, default is ,</param>
		/// <param name="trimWhiteSpace"></param>
		/// <returns>IEnumerable with value</returns>
		public static IEnumerable<string> SplitCsv(this string input, char delimiter = ',', bool trimWhiteSpace = true)
		{
            var d = new RFC4180Tokenizer(new Options('"', '#', delimiter));
            return d.Tokenize(input);
		}

		/// <summary>
		///     Parse input for comma separated name=value pairs
		/// </summary>
		/// <param name="input">string with comma separated value pairs</param>
		/// <returns>IDictionary with values</returns>
		public static IDictionary<string, string> SplitDictionary(this string input)
		{
			return (from valuePair in
					(from pair in input.SplitCsv()
						select pair.Split('='))
					where valuePair.Length == 2
					select valuePair
				).ToLookup(e => e[0], StringComparer.OrdinalIgnoreCase)
				.ToDictionary(x => x.Key, x => x.First()[1]);
		}
	}
}