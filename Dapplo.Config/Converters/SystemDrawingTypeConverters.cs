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
using System.Drawing;

namespace Dapplo.Config.Converters {
	/// <summary>
	/// This TypeConverter should be used to convert a string to Size and back.
	/// </summary>
	public class SizeTypeConverter : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, destinationType);
		}

		/// <summary>
		/// This type converter can convert from a string
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns>Converted value</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			string sizeString = value as string;
			if (sizeString != null) {
				var splitSizeString = sizeString.Split(',');
				if (splitSizeString.Length == 2) {
					int width, height;
					if (int.TryParse(splitSizeString[0], out width) && int.TryParse(splitSizeString[1], out height)) {
						return new Size(width, height);
					}
				}
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
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				Size size = (Size)value;
				return string.Format("{0},{1}", size.Width, size.Height);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	/// <summary>
	/// This TypeConverter should be used to convert a string to Point and back.
	/// </summary>
	public class PointTypeConverter : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, destinationType);
		}

		/// <summary>
		/// This type converter can convert from a string
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns>Converted value</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			string pointString = value as string;
			if (pointString != null) {
				var splitPointString = pointString.Split(',');
				if (splitPointString.Length == 2) {
					int x, y;
					if (int.TryParse(splitPointString[0], out x) && int.TryParse(splitPointString[1], out y)) {
						return new Point(x, y);
					}
				}
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
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				Point point = (Point)value;
				return string.Format("{0},{1}", point.X, point.Y);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	/// <summary>
	/// This TypeConverter should be used to convert a string to Rectangle and back.
	/// </summary>
	public class RectangleTypeConverter : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, destinationType);
		}

		/// <summary>
		/// This type converter can convert from a string
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns>Converted value</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			string rectangleString = value as string;
			if (rectangleString != null) {
				var splitRectangleString = rectangleString.Split(',');
				if (splitRectangleString.Length == 4) {
					int x, y, width, height;
					if (int.TryParse(splitRectangleString[0], out x) && int.TryParse(splitRectangleString[1], out y) && int.TryParse(splitRectangleString[2], out width) && int.TryParse(splitRectangleString[3], out height)) {
						return new Rectangle(x, y, width, height);
					}
				}
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
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				Rectangle rectangle = (Rectangle)value;
				return string.Format("{0},{1},{2},{3}", rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	/// <summary>
	/// This TypeConverter should be used to convert a string to Color and back.
	/// </summary>
	public class ColorTypeConverter : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, destinationType);
		}

		/// <summary>
		/// This type converter can convert from a string
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns>Converted value</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			string colorString = value as string;
			if (colorString != null) {
				var splitColorString = colorString.Split(',');
				if (splitColorString.Length == 3 || splitColorString.Length == 4) {
					byte red, green, blue, alpha = 0;
					if (byte.TryParse(splitColorString[1], out red) && byte.TryParse(splitColorString[2], out green) && byte.TryParse(splitColorString[3], out blue)) {
						byte.TryParse(splitColorString[0], out alpha);
						return Color.FromArgb(alpha, red, green, blue);
					}
				} else {
					return Color.FromName(colorString);
				}
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
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				Color color = (Color)value;
				if (color.IsNamedColor) {
					return color.Name;
				} else {
					return string.Format("{0},{1},{2},{3}", color.A, color.R, color.G, color.B);
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
