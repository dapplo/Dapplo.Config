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

namespace Dapplo.Config.Ini {
	/// <summary>
	/// This TypeConverter should be used to convert a comma separated string to a List of type T.
	/// Use the TypeConverterAttribute with GenericListTypeConverter where T is your type (not list of type)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GenericListTypeConverter<T> : TypeConverter {
		protected readonly TypeConverter _typeConverter;

		public GenericListTypeConverter() {
			_typeConverter = TypeDescriptor.GetConverter(typeof(T));
			if (_typeConverter == null) {
				throw new InvalidOperationException("No type converter exists for type " + typeof(T).FullName);
			}
		}

		protected string[] GetStringArray(string input) {
			string[] result = input.Split(',');
			Array.ForEach(result, s => s.Trim());
			return result;
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				string[] items = GetStringArray(sourceType.ToString());
				return (items.Count() > 0);
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			if (value is string) {
				string[] items = GetStringArray((string)value);

				List<T> result = new List<T>();
				Array.ForEach(items, s => {
					object item = _typeConverter.ConvertFromInvariantString(s);
					if (item != null) {
						result.Add((T)item);
					}
				});

				return result;
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				return string.Join(",", ((IList<T>)value));
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
