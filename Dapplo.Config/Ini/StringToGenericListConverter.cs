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
	/// This TypeConverter should be used to convert a comma separated string to a List of type T.
	/// Use the TypeConverterAttribute with StringToGenericListConverter where T is your type (not list of type)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class StringToGenericListConverter<T> : TypeConverter
	{
		protected readonly TypeConverter _typeConverter;

		public StringToGenericListConverter()
		{
			_typeConverter = TypeDescriptor.GetConverter(typeof(T));
			if (_typeConverter == null)
			{
				throw new InvalidOperationException("No type converter exists for type " + typeof(T).FullName);
			}
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(List<T>))
			{
				return true;
			}
			if (destinationType == typeof(IList<T>))
			{
				return true;
			}
			if (destinationType == typeof(string))
			{
				return true;
			}

			return base.CanConvertFrom(context, destinationType);
		}

		/// <summary>
		/// This type converter can convert from a string to a list "T"
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns>Converted value</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string)
			{
				// Split, and where all element are not null or empty, convert the item to T and add the items to a list<T>
				return (from item in ((string)value).Split(',')
						where !string.IsNullOrWhiteSpace(item)
						select (T)_typeConverter.ConvertFromInvariantString(item.Trim())).ToList<T>();
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
			if (destinationType == typeof(string))
			{
				return string.Join(",", ((IList<T>)value));
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
