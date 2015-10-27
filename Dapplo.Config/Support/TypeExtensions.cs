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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dapplo.Config.Support
{
	/// <summary>
	/// Extension for types
	/// </summary>
	public static class TypeExtensions
	{
		// A map for converting interfaces to types
		private static readonly IDictionary<Type, Type> TypeMap = new Dictionary<Type, Type>
			{
				{ typeof(IList<>), typeof(List<>)},
				{ typeof(IEnumerable<>), typeof(List<>)},
				{ typeof(ICollection<>), typeof(List<>)},
				{ typeof(ISet<>), typeof(HashSet<>)},
				{ typeof(IDictionary<,>), typeof(Dictionary<,>) },
				{ typeof(IReadOnlyDictionary<,>), typeof(Dictionary<,>) }
				
			};

		/// <summary>
		/// Create an instance of the supplied type
		/// </summary>
		/// <param name="valueType">Type which should be created</param>
		/// <returns>instance of the type</returns>
		public static object CreateInstance(this Type valueType)
		{
			Type typeForInstance = valueType;
			if (typeForInstance == typeof(string) || typeForInstance.IsArray)
			{
				return null;
			}

			// If it's an interface and IEnumerable is assignable from it, we try to map it to a real type
			if (typeForInstance.IsInterface && typeof(IEnumerable).IsAssignableFrom(typeForInstance))
			{
				// Get the type without the generic types, like IList<> or ICollection<> or IDictionary<,>, so we can map it
				Type genericType = typeForInstance.GetGenericTypeDefinition();

				// check if we have a mapping
                if (TypeMap.ContainsKey(genericType))
				{
					// Get the generic arguments (IList<argument1> or Dictionary<argument1, argument2>)
					Type[] genericArguments = typeForInstance.GetGenericArguments();
					// Create the List<T> or similar
					typeForInstance = TypeMap[genericType].MakeGenericType(genericArguments);
				}
			}

			// If it's not an enum, check if it's an interface or doesn't have a default constructor
			if (!typeForInstance.IsEnum && (typeForInstance.IsInterface || typeForInstance.GetConstructor(Type.EmptyTypes) == null))
			{
				// Nope, we can't create it
				return null;
			}

			// Create
			return Activator.CreateInstance(typeForInstance);
		}

		/// <summary>
		/// Generic version of the same method with Type parameter, 
		/// </summary>
		/// <typeparam name="T">target type</typeparam>
		/// <param name="value"></param>
		/// <param name="typeConverter"></param>
		/// <returns>T</returns>
		public static T ConvertOrCastValueToType<T>(object value, TypeConverter typeConverter = null)
		{
			return (T)ConvertOrCastValueToType(typeof(T), value, typeConverter);
		}

		/// <summary>
		/// Convert or Cast the value so it matches the targetType
		/// </summary>
		/// <param name="targetType">target type</param>
		/// <param name="value">value to convert</param>
		/// <param name="typeConverter">A type converter can be passed for special cases</param>
		/// <returns>object as targetType, or null if this wasn't possible</returns>
		public static object ConvertOrCastValueToType(this Type targetType, object value, TypeConverter typeConverter = null)
		{
			if (value == null)
			{
				return null;
			}
			var valueType = value.GetType();
			if (targetType == valueType || targetType.IsAssignableFrom(valueType))
			{
				return value;
			}

			if (typeConverter == null)
			{
				typeConverter = TypeDescriptor.GetConverter(targetType);
			}
			var stringValue = value as string;
			if (stringValue != null && (bool)typeConverter?.CanConvertFrom(typeof(string)))
			{
				return typeConverter.ConvertFromInvariantString(stringValue);
			}
			if ((bool)typeConverter?.CanConvertFrom(valueType))
			{
				try
				{
					return typeConverter.ConvertFrom(value);
				}
				catch
				{
					// Ignore, can't convert the value, this should actually not happen much
				}
			}
			// Collection -> string
			if (targetType == typeof(string) && typeof(IEnumerable).IsAssignableFrom(valueType))
			{
				var result = string.Join(",", ((IEnumerable)value).Cast<object>().ToArray());
                return result;
			}
			// String -> collection
			if (typeof(IEnumerable).IsAssignableFrom(targetType) && (stringValue != null || typeof(IEnumerable).IsAssignableFrom(valueType)))
			{
				// We can try to create the type
				var instance = targetType.CreateInstance();
				if (instance != null)
				{
					// Fill it
					Type instanceType = instance.GetType();
					Type[] genericArguments = instanceType.GetGenericArguments();

					// Check if it's a collection like thing, has only one generic argument
					if (genericArguments.Length == 1)
					{
						Type genericType = genericArguments[0];
						var addMethod = instanceType.GetMethod("Add");

						foreach (var item in stringValue != null ? stringValue.SplitCSV() : (IEnumerable)value)
						{
							try
							{
								addMethod.Invoke(instance, new[] { genericType.ConvertOrCastValueToType(item) });
							}
							catch
							{
								// Ignore
							}
						}
						return instance;
					}
					else if (genericArguments.Length == 2)
					{
						Type valueType1 = genericArguments[0];
						Type valueType2 = genericArguments[1];
						var addMethod = instanceType.GetMethod("Add");
						IDictionary dictionary = stringValue != null ? (IDictionary)stringValue.SplitDictionary() : (IDictionary)value;
                        foreach (var key in dictionary.Keys)
						{
							try
							{
								addMethod.Invoke(instance, new[] { valueType1.ConvertOrCastValueToType(key), valueType2.ConvertOrCastValueToType(dictionary[key]) });
							}
							catch
							{
								// Ignore
							}
                        }
						return instance;
					}
				}
            }
			try
			{
				return Convert.ChangeType(value, targetType);
			}
			catch
			{
				// Ignore, can't convert the value
			}

			return null;
		}
	}
}
