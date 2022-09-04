// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapplo.Log;

namespace Dapplo.Config.Extensions
{
    /// <summary>
    ///     Extension for types
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly LogSource Log = new LogSource();
        private static readonly IDictionary<Type, Type> Converters = new ConcurrentDictionary<Type, Type>();

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
        /// Used for the generation of friendly names
        /// </summary>
        private static readonly Dictionary<Type, string> TypeToFriendlyName = new Dictionary<Type, string>
        {
            {typeof(string), "string"},
            {typeof(object), "object"},
            {typeof(bool), "bool"},
            {typeof(byte), "byte"},
            {typeof(char), "char"},
            {typeof(decimal), "decimal"},
            {typeof(double), "double"},
            {typeof(short), "short"},
            {typeof(int), "int"},
            {typeof(long), "long"},
            {typeof(sbyte), "sbyte"},
            {typeof(float), "float"},
            {typeof(ushort), "ushort"},
            {typeof(uint), "uint"},
            {typeof(ulong), "ulong"},
            {typeof(void), "void"}
        };

        /// <summary>
        /// Retrieve an attribute from a type
        /// </summary>
        /// <typeparam name="TAttribute">Type of the attribute</typeparam>
        /// <param name="type">Type</param>
        /// <param name="inherit">bool default true to also check inherit class attributes</param>
        /// <param name="includeInterfaces">bool default true if the interfaces of the declaring type also need to be checked</param>
        /// <returns>TAttribute or null</returns>
        public static TAttribute GetAttribute<TAttribute>(this Type type, bool inherit = true, bool includeInterfaces = true) where TAttribute : Attribute
        {
            var attribute = type.GetTypeInfo().GetCustomAttribute<TAttribute>(inherit);
            if (attribute != null)
            {
                return attribute;
            }

            // If we didn't find the DefaultValueAttribute on the property, we check for the same property on the implementing interfaces
            if (!includeInterfaces)
            {
                return null;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                var attributeOnInterface = interfaceType.GetTypeInfo().GetCustomAttribute<TAttribute>(false);
                if (attributeOnInterface != null)
                {
                    return attributeOnInterface;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieve attributes from a Type
        /// </summary>
        /// <typeparam name="TAttribute">Type of the attribute</typeparam>
        /// <param name="type">Type</param>
        /// <param name="inherit">bool default true to also check inherit class attributes</param>
        /// <param name="includeInterfaces">bool default true if the interfaces of the declaring type also need to be checked</param>
        /// <returns>IEnumerable of TAttribute</returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type, bool inherit = true, bool includeInterfaces = true) where TAttribute : Attribute
        {
            var attributes = type.GetTypeInfo().GetCustomAttributes<TAttribute>(inherit);

            // If we didn't find the DefaultValueAttribute on the property, we check for the same property on the implementing interfaces
            if (!includeInterfaces)
            {
                return attributes;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                var attributesOnInterface = interfaceType.GetTypeInfo().GetCustomAttributes<TAttribute>(false);
                attributes = attributes.Concat(attributesOnInterface);
            }

            return attributes;
        }

        /// <summary>
        ///     Add the default converter for the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeConverter"></param>
        public static void AddDefaultConverter(Type type, Type typeConverter)
        {
            Converters[type] = typeConverter;
        }

        /// <summary>
        /// Cast the supplied object to a certain type
        /// </summary>
        /// <param name="type">Type to cast to</param>
        /// <param name="data">object to cast</param>
        /// <returns>object</returns>
        public static object Cast(this Type type, object data)
        {
            var dataParam = Expression.Parameter(typeof(object), "data");
            var body = Expression.Block(Expression.Convert(Expression.Convert(dataParam, data.GetType()), type));

            var run = Expression.Lambda(body, dataParam).Compile();
            var ret = run.DynamicInvoke(data);
            return ret;
        }

        /// <summary>
        ///     Generic version of the same method with Type parameter,
        /// </summary>
        /// <typeparam name="T">target type</typeparam>
        /// <param name="value"></param>
        /// <param name="typeConverter">A TypeConverter can be passed for special cases</param>
        /// <param name="typeDescriptorContext">A TypeDescriptorContext can be passed for special cases</param>
        /// <param name="convertFrom"></param>
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

            if (typeConverter != null && TryConvert(value, targetType, typeConverter, typeDescriptorContext, convertFrom, out var convertedValueViaSuppliedConverter))
            {
                return convertedValueViaSuppliedConverter;
            }

            var valueType = value.GetType();
            // Only return unconverted when types are assignable but no converter is specified
            if (targetType == valueType || targetType.GetTypeInfo().IsAssignableFrom(valueType.GetTypeInfo()))
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
                    var genericArguments = instanceType.GetGenericArguments();

                    // Check if it's a collection like thing, has only one generic argument
                    if (genericArguments.Length == 1)
                    {
                        var genericType = genericArguments[0];
                        var addMethod = instanceType.GetMethod("Add");
                        if (addMethod == null)
                        {
                            return instance;
                        }
                        foreach (var item in stringValue != null ? stringValue.SplitCsv() : (IEnumerable) value)
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
                if (typeConverter != null && TryConvert(value, targetType, typeConverter, typeDescriptorContext, convertFrom, out var convertedValue))
                {
                    return convertedValue;
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
            var typeInfoForInstance = typeForInstance.GetTypeInfo();
            if (typeForInstance == typeof (string) || typeForInstance.IsArray)
            {
                return null;
            }

            // If it's an interface and IEnumerable is assignable from it, we try to map it to a real type
            if (typeInfoForInstance.IsInterface && typeof (IEnumerable).GetTypeInfo().IsAssignableFrom(typeInfoForInstance))
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
                    // Update the TypeInfo
                    typeInfoForInstance = typeForInstance.GetTypeInfo();
                }
            }

            // If it's not an enum, check if it's an interface or doesn't have a default constructor
            if (!typeInfoForInstance.IsValueType && !typeInfoForInstance.IsEnum && (typeInfoForInstance.IsInterface || typeForInstance.GetConstructor(Type.EmptyTypes) == null))
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
            TypeConverter converter;
            if (Converters.TryGetValue(valueType, out var converterType))
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
                var canConvertFrom = typeConverter?.CanConvertFrom(typeof (string));
                if (canConvertFrom != null && stringValue != null && (bool) canConvertFrom)
                {
                    try
                    {
#if !NET471
                        outValue = typeConverter.ConvertFromInvariantString(stringValue);
#else
                        outValue = typeConverter.ConvertFromInvariantString(typeDescriptorContext, stringValue);
#endif
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Ignore, can't convert the value, this should actually not happen much
                        Log.Warn().WriteLine(ex.Message);
                    }
                }
                else
                {
                    var canConverFrom = typeConverter?.CanConvertFrom(valueType);
                    if (canConverFrom == null || !(bool) canConverFrom)
                    {
                        return false;
                    }

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
                var canConvertTo = typeConverter?.CanConvertTo(typeof (string));
                if (canConvertTo != null && (bool) canConvertTo)
                {
                    try
                    {
#if !NET471
                        outValue = typeConverter.ConvertToInvariantString(value);
#else
                        outValue = typeConverter.ConvertToInvariantString(typeDescriptorContext, value);
#endif
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Ignore, can't convert the value, this should actually not happen much
                        Log.Warn().WriteLine(ex.Message);
                    }
                }
                else
                {
                    var convertTo = typeConverter?.CanConvertTo(targetType);
                    if (convertTo == null || !(bool) convertTo)
                    {
                        return false;
                    }

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

        /// <summary>
        /// Get the name of a type which is readable, even if generics are used.
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>string</returns>
        public static string FriendlyName(this Type type)
        {
            if (TypeToFriendlyName.TryGetValue(type, out var friendlyName))
            {
                return friendlyName;
            }

            if (type.IsArray)
            {
                return type.GetElementType().FriendlyName() + "[]";
            }

            friendlyName = type.Name;

            if (!type.GetTypeInfo().IsGenericType)
            {
                return friendlyName;
            }
            int backtick = friendlyName.IndexOf('`');
            if (backtick > 0)
            {
                friendlyName = friendlyName.Remove(backtick);
            }
            var friendlyNameBuilder = new StringBuilder(friendlyName);
            friendlyNameBuilder.Append('<');
            var typeParameters = type.GetGenericArguments();
            for (int i = 0; i < typeParameters.Length; i++)
            {
                string typeParamName = typeParameters[i].FriendlyName();
                friendlyNameBuilder.Append(i == 0 ? typeParamName : ", " + typeParamName);
            }
            friendlyNameBuilder.Append('>');
            return friendlyNameBuilder.ToString();
        }

        /// <summary>
        /// Create a default value for a type, this usually is "null" for reference type, but for other, e.g. bool it's false or for int it's 0
        /// This extension method takes care of this.
        /// 
        /// Note: this differs a LOT from CreateInstance, as there we get an instance (e.g. for IList of string an empty List of string).
        /// </summary>
        /// <param name="type">Type to create a default for</param>
        /// <returns>Default for type</returns>
        public static object Default(this Type type)
        {
            return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}