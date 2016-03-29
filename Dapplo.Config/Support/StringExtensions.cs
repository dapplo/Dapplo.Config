﻿//  Dapplo - building blocks for desktop applications
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

#endregion

namespace Dapplo.Config.Support
{
	public static class StringExtensions
	{
		private static readonly Regex CleanupRegex = new Regex(@"[^a-z0-9]+", RegexOptions.Compiled);

		/// <summary>
		///     Helper method for converting a string to a non strict value.
		///     This means, ToLowerInvariant and replace all non alpha/digits to ""
		/// </summary>
		/// <param name="value"></param>
		/// <returns>clean string</returns>
		public static string Cleanup(this string value)
		{
			return CleanupRegex.Replace(value.ToLowerInvariant(), "");
		}

		/// <summary>
		///     Check if 2 strings are equal if both are made ToLower and all non alpha & digits are removed.
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <returns>true if they are 'equal'</returns>
		public static bool NonStrictEquals(this string value1, string value2)
		{
			return value1.Cleanup().Equals(value2.Cleanup());
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
		public static IEnumerable<string> SplitCsv(this string input, string delimiter = ",", bool trimWhiteSpace = true)
		{
			using (var parser = new TextFieldParser(new StringReader(input))
			{
				HasFieldsEnclosedInQuotes = true,
				TextFieldType = FieldType.Delimited,
				CommentTokens = new[] {";"},
				Delimiters = new[] {delimiter},
				TrimWhiteSpace = trimWhiteSpace
			})
			{
				while (!parser.EndOfData)
				{
					var readFields = parser.ReadFields();
					if (readFields != null)
					{
						foreach (var field in readFields)
						{
							yield return field;
						}
					}
				}
			}
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