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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// This TypeConverter should be used to convert a dictionary<T1,T2> to a dictionary<string,string>
	/// Use the TypeConverterAttribute with GenericDictionaryConverter where T1 is the first and T2 the second type of your Dictionary.
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	public class GenericDictionaryConverter<T1, T2> : TypeConverter
	{
		protected readonly TypeConverter _typeConverter1;
		protected readonly TypeConverter _typeConverter2;

		public GenericDictionaryConverter()
		{
			_typeConverter1 = TypeDescriptor.GetConverter(typeof(T1));
			if (_typeConverter1 == null)
			{
				throw new InvalidOperationException("No type converter exists for type " + typeof(T1).FullName);
			}
			_typeConverter2 = TypeDescriptor.GetConverter(typeof(T2));
			if (_typeConverter2 == null)
			{
				throw new InvalidOperationException("No type converter exists for type " + typeof(T2).FullName);
			}
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(IDictionary<string, string>))
			{
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(IDictionary<string, string>))
			{
				return true;
			}

			if (destinationType == typeof(string))
			{
				return false;
			}

			return base.CanConvertFrom(context, destinationType);
		}

		/// <summary>
		/// This type converter can convert from a string to a list "T1"
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns>Converted value</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			IDictionary<string, string> values = value as IDictionary<string, string>;
			if (values != null)
			{

				// Split, and where all element are not null or empty, convert the item to T and add the items to a list<T>
				return (from key in values.Keys
						select key).Distinct().ToDictionary(x => (T1)_typeConverter1.ConvertFromInvariantString(x), x => (T2)_typeConverter2.ConvertFromInvariantString(values[x]));
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// Normally the convert to is only called if the value needs to be converted to a specific type.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(IDictionary<string, string>))
			{
				IDictionary<T1, T2> values = value as IDictionary<T1, T2>;
				if (values != null) {
					return (from key in values.Keys
							select key).ToDictionary(x => _typeConverter1.ConvertToInvariantString(x), x => _typeConverter2.ConvertToInvariantString(values[x]));
				} else {
					return null;
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
