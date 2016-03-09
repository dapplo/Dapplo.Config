//  Dapplo - building blocks for desktop applications
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Dapplo.LogFacade;

#endregion

namespace Dapplo.Config.Support
{
	/// <summary>
	///     Extension for types
	/// </summary>
	public static class TypeExtensions
	{
		private static readonly LogSource Log = new LogSource();
		private static readonly IDictionary<Type, Type> _converters = new Dictionary<Type, Type>();

		// A map for converting interfaces to types
		private static readonly IDictionary<Type, Type> TypeMap = new Dictionary<Type, Type>
		{
			{typeof (IList<>), typeof (List<>)},
			{typeof (IEnumerable<>), typeof (List<>)},
			{typeof (ICollection<>), typeof (List<>)},
			{typeof (ISet<>), typeof (HashSet<>)},
			{typeof (IDictionary<,>), typeof (Dictionary<,>)},
			{typeof (IReadOnlyDictionary<,>), typeof (Dictionary<,>)}
		};


		/// <summary>
		///     Add the default converter for the specified type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="typeConverter"></param>
		public static void AddDefaultConverter(Type type, Type typeConverter)
		{
			_converters.SafelyAddOrOverwrite(type, typeConverter);
		}

		/// <summary>
		///     Generic version of the same method with Type parameter,
		/// </summary>
		/// <typeparam name="T">target type</typeparam>
		/// <param name="value"></param>
		/// <param name="typeConverter">A TypeConverter can be passed for special cases</param>
		/// <param name="typeDescriptorContext">A TypeDescriptorContext can be passed for special cases</param>
		/// <returns>T</returns>
		public static T ConvertOrCastValueToType<T>(object value, TypeConverter typeConverter = null, ITypeDescriptorContext typeDescriptorContext = null,
			bool convertFrom = true)
		{
			return (T) ConvertOrCastValueToType(typeof (T), value, typeConverter, typeDescriptorContext, convertFrom);
		}

		/// <summary>
		///     Convert or Cast the value TO targetType
		/// </summary>
		/// <param name="targetType">target type</param>
		/// <param name="value">value to convert</param>
		/// <param name="typeConverter">A TypeConverter can be passed for special cases</param>
		/// <param name="typeDescriptorContext">A TypeDescriptorContext can be passed for special cases</param>
		/// <param name="convertFrom">
		///     True: the TypeConverter is called with convertFrom, false the TypeConverter is called with
		///     convertTo
		/// </param>
		/// <returns>object as targetType, or null if this wasn't possible</returns>
		public static object ConvertOrCastValueToType(this Type targetType, object value, TypeConverter typeConverter = null,
			ITypeDescriptorContext typeDescriptorContext = null, bool convertFrom = true)
		{
			if (value == null)
			{
				return targetType.CreateInstance();
			}

			if (typeConverter != null)
			{
				object returnValue;
				if (TryConvert(value, targetType, typeConverter, typeDescriptorContext, convertFrom, out returnValue))
				{
					return returnValue;
				}
			}

			var valueType = value.GetType();
			// Only return unconverted when types are assignable but no converter is specified
			if (targetType == valueType || targetType.IsAssignableFrom(valueType))
			{
				return value;
			}

			var stringValue = value as string;
			if (stringValue != null && stringValue.Length == 0)
			{
				return targetType.CreateInstance();
			}
			// Collection -> string
			if (targetType == typeof (string) && valueType != typeof (string) && typeof (IEnumerable).IsAssignableFrom(valueType))
			{
				var result = string.Join(",", ((IEnumerable) value).Cast<object>().ToArray());
				return result;
			}
			// String -> collection
			if (typeof (IEnumerable).IsAssignableFrom(targetType) && (stringValue != null || typeof (IEnumerable).IsAssignableFrom(valueType)))
			{
				// We can try to create the type
				var instance = targetType.CreateInstance();
				if (instance != null)
				{
					// Fill it
					var instanceType = instance.GetType();
					Type[] genericArguments = instanceType.GetGenericArguments();

					// Check if it's a collection like thing, has only one generic argument
					if (genericArguments.Length == 1)
					{
						var genericType = genericArguments[0];
						var addMethod = instanceType.GetMethod("Add");

						foreach (var item in stringValue != null ? stringValue.SplitCSV() : (IEnumerable) value)
						{
							try
							{
								addMethod.Invoke(instance, new[] {genericType.ConvertOrCastValueToType(item)});
							}
							catch (Exception ex)
							{
								// Ignore
								Log.Warn().WriteLine(ex.Message);
							}
						}
						return instance;
					}
					if (genericArguments.Length == 2)
					{
						var valueType1 = genericArguments[0];
						var valueType2 = genericArguments[1];
						var addMethod = instanceType.GetMethod("Add");
						var dictionary = stringValue != null ? (IDictionary) stringValue.SplitDictionary() : (IDictionary) value;
						foreach (var key in dictionary.Keys)
						{
							try
							{
								addMethod.Invoke(instance,
									new[]
									{valueType1.ConvertOrCastValueToType(key, convertFrom: convertFrom), valueType2.ConvertOrCastValueToType(dictionary[key], convertFrom: convertFrom)});
							}
							catch (Exception ex)
							{
								// Ignore
								Log.Warn().WriteLine(ex.Message);
							}
						}
						return instance;
					}
				}
			}
			if (typeConverter == null)
			{
				typeConverter = GetConverter(convertFrom ? targetType : valueType);
				if (typeConverter != null)
				{
					object returnValue;
					if (TryConvert(value, targetType, typeConverter, typeDescriptorContext, convertFrom, out returnValue))
					{
						return returnValue;
					}
				}
			}

			if (value is IConvertible)
			{
				try
				{
					return Convert.ChangeType(value, targetType);
				}
				catch (Exception ex)
				{
					// Ignore, as there is not much we can do
					Log.Warn().WriteLine(ex.Message);
				}
			}

			return targetType.CreateInstance();
		}

		/// <summary>
		///     Create an instance of the supplied type
		/// </summary>
		/// <param name="valueType">Type which should be created</param>
		/// <returns>instance of the type</returns>
		public static object CreateInstance(this Type valueType)
		{
			var typeForInstance = valueType;
			if (typeForInstance == typeof (string) || typeForInstance.IsArray)
			{
				return null;
			}

			// If it's an interface and IEnumerable is assignable from it, we try to map it to a real type
			if (typeForInstance.IsInterface && typeof (IEnumerable).IsAssignableFrom(typeForInstance))
			{
				// Get the type without the generic types, like IList<> or ICollection<> or IDictionary<,>, so we can map it
				var genericType = typeForInstance.GetGenericTypeDefinition();

				// check if we have a mapping
				if (TypeMap.ContainsKey(genericType))
				{
					// Get the generic arguments (IList<argument1> or Dictionary<argument1, argument2>)
					var genericArguments = typeForInstance.GetGenericArguments();
					// Create the List<T> or similar
					typeForInstance = TypeMap[genericType].MakeGenericType(genericArguments);
				}
			}

			// If it's not an enum, check if it's an interface or doesn't have a default constructor
			if (!typeForInstance.IsValueType && !typeForInstance.IsEnum && (typeForInstance.IsInterface || typeForInstance.GetConstructor(Type.EmptyTypes) == null))
			{
				// Nope, we can't create it
				return null;
			}

			// Create
			return Activator.CreateInstance(typeForInstance);
		}

		/// <summary>
		///     Get the TypeConverter for the Type
		/// </summary>
		/// <param name="valueType">Type</param>
		/// <returns>TypeConverter</returns>
		public static TypeConverter GetConverter(this Type valueType)
		{
			TypeConverter converter = null;
			Type converterType;
			if (_converters.TryGetValue(valueType, out converterType))
			{
				converter = (TypeConverter) Activator.CreateInstance(converterType);
			}
			else
			{
				converter = TypeDescriptor.GetConverter(valueType);
			}
			return converter;
		}

		/// <summary>
		///     A helper method, this will try to convert the value to the target type with the supplied converter
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="typeConverter"></param>
		/// <param name="typeDescriptorContext"></param>
		/// <param name="convertFrom"></param>
		/// <param name="outValue">converted value</param>
		/// <returns>true if success</returns>
		private static bool TryConvert(object value, Type targetType, TypeConverter typeConverter, ITypeDescriptorContext typeDescriptorContext, bool convertFrom,
			out object outValue)
		{
			var valueType = value.GetType();
			var stringValue = value as string;
			outValue = null;
			if (convertFrom)
			{
				if (stringValue != null && (bool) typeConverter?.CanConvertFrom(typeof (string)))
				{
					try
					{
						outValue = typeConverter.ConvertFromInvariantString(typeDescriptorContext, stringValue);
						return true;
					}
					catch (Exception ex)
					{
						// Ignore, can't convert the value, this should actually not happen much
						Log.Warn().WriteLine(ex.Message);
					}
				}
				else if ((bool) typeConverter?.CanConvertFrom(valueType))
				{
					try
					{
						outValue = typeConverter.ConvertFrom(typeDescriptorContext, CultureInfo.CurrentCulture, value);
						return true;
					}
					catch (Exception ex)
					{
						// Ignore, can't convert the value, this should actually not happen much
						Log.Warn().WriteLine(ex.Message);
					}
				}
			}
			else
			{
				if ((bool) typeConverter?.CanConvertTo(typeof (string)))
				{
					try
					{
						outValue = typeConverter.ConvertToInvariantString(typeDescriptorContext, value);
						return true;
					}
					catch (Exception ex)
					{
						// Ignore, can't convert the value, this should actually not happen much
						Log.Warn().WriteLine(ex.Message);
					}
				}
				else if ((bool) typeConverter?.CanConvertTo(targetType))
				{
					try
					{
						outValue = typeConverter.ConvertTo(typeDescriptorContext, CultureInfo.CurrentCulture, value, targetType);
						return true;
					}
					catch (Exception ex)
					{
						// Ignore, can't convert the value, this should actually not happen much
						Log.Warn().WriteLine(ex.Message);
					}
				}
			}
			return false;
		}
	}
}