// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

#if !NETSTANDARD
using System.ComponentModel.DataAnnotations;
#endif

namespace Dapplo.Config.Extensions
{
	/// <summary>
	/// Extensions for PropertyInfo
	/// </summary>
	public static class PropertyInfoExtension
	{
		/// <summary>
		///     Retrieve the Category from the CategoryAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Category</returns>
		public static string GetCategory(this PropertyInfo propertyInfo)
		{
			var categoryAttribute = propertyInfo.GetAttribute<CategoryAttribute>();
			return categoryAttribute?.Category;
		}

		/// <summary>
		///     Retrieve the Name from the DataMemberAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Name</returns>
		public static string GetDataMemberName(this PropertyInfo propertyInfo)
		{
			var dataMemberAttribute = propertyInfo.GetAttribute<DataMemberAttribute>();
			if (!string.IsNullOrEmpty(dataMemberAttribute?.Name))
			{
				return dataMemberAttribute.Name;
			}
			return null;
		}

        /// <summary>
        /// Retrieve an attribute from a property
        /// </summary>
        /// <typeparam name="TAttribute">Type of the attribute</typeparam>
        /// <param name="memberInfo">MemberInfo</param>
        /// <param name="inherit">bool default true to also check inherit class attributes</param>
        /// <param name="includeInterfaces">bool default true if the interfaces of the declaring type also need to be checked</param>
        /// <returns>TAttribute or null</returns>
        public static TAttribute GetAttribute<TAttribute>(this MemberInfo memberInfo, bool inherit = true, bool includeInterfaces = true) where TAttribute : Attribute
        {
			var attribute = memberInfo.GetCustomAttribute<TAttribute>(inherit);
			if (attribute != null)
			{
				return attribute;
			}

			// If we didn't find the DefaultValueAttribute on the property, we check for the same property on the implementing interfaces
	        if (!includeInterfaces || memberInfo.DeclaringType == null)
	        {
		        return null;
	        }

	        foreach (var interfaceType in memberInfo.DeclaringType.GetInterfaces())
	        {
		        var interfacePropertyInfo = interfaceType.GetProperty(memberInfo.Name);
		        var attributeOnInterface = interfacePropertyInfo?.GetCustomAttribute<TAttribute>(false);
		        if (attributeOnInterface != null)
		        {
			        return attributeOnInterface;
		        }
	        }

	        return null;
        }

        /// <summary>
        /// Retrieve attributes from a property
        /// </summary>
        /// <typeparam name="TAttribute">Type of the attribute</typeparam>
        /// <param name="memberInfo">MemberInfo</param>
        /// <param name="inherit">bool default true to also check inherit class attributes</param>
        /// <param name="includeInterfaces">bool default true if the interfaces of the declaring type also need to be checked</param>
        /// <returns>IEnumerable of TAttribute</returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo memberInfo, bool inherit = true, bool includeInterfaces = true) where TAttribute : Attribute
		{
			var attributes = memberInfo.GetCustomAttributes<TAttribute>(inherit);
			
			// If we didn't find the DefaultValueAttribute on the property, we check for the same property on the implementing interfaces
			if (!includeInterfaces || memberInfo.DeclaringType == null)
			{
				return attributes;
			}

			foreach (var interfaceType in memberInfo.DeclaringType.GetInterfaces())
			{
				var interfacePropertyInfo = interfaceType.GetProperty(memberInfo.Name);
				var attributesOnInterface = interfacePropertyInfo?.GetCustomAttributes<TAttribute>(false);
				if (attributesOnInterface != null)
				{
					attributes = attributes.Concat(attributesOnInterface);
				}
			}

			return attributes;
		}

        /// <summary>
        ///     Create a default for the property.
        ///     This can come from the DefaultValueFor from the DefaultValueAttribute
        ///     Or it can be something like an empty collection
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="includeInterfaces">bool default true if the interfaces of the declaring type also need to be checked</param>
        /// <returns>object with a default value</returns>
        public static object GetDefaultValue(this PropertyInfo propertyInfo, bool includeInterfaces = true)
		{
			var defaultValueAttribute = propertyInfo.GetAttribute<DefaultValueAttribute>(true, includeInterfaces);
			if (defaultValueAttribute != null)
			{
				return defaultValueAttribute.Value;
			}

            if (propertyInfo.PropertyType.GetTypeInfo().IsValueType)
			{
				// msdn information: If this PropertyInfo object is a value type and value is null, then the property will be set to the default value for that type.
				return null;
			}

			try
			{
				return propertyInfo.PropertyType.CreateInstance();
			}
			catch (Exception)
			{
				// Ignore creating the default type, this might happen if there is no default constructor.
			}

			return null;
		}

		/// <summary>
		///     Retrieve the Description from the DescriptionAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Description</returns>
		public static string GetDescription(this PropertyInfo propertyInfo)
		{
			var descriptionAttribute = propertyInfo.GetAttribute<DescriptionAttribute>();
#if !NETSTANDARD
			if (descriptionAttribute != null)
			{
				return descriptionAttribute.Description;
			}
			var displayAttribute = propertyInfo.GetAttribute<DisplayAttribute>();
			return displayAttribute?.Description;
#else
            return descriptionAttribute?.Description;
#endif
		}

		/// <summary>
		///     Retrieve the EmitDefaultValue from the DataMemberAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>EmitDefaultValue</returns>
		public static bool GetEmitDefaultValue(this PropertyInfo propertyInfo)
		{
			var dataMemberAttribute = propertyInfo.GetAttribute<DataMemberAttribute>();
			if (dataMemberAttribute != null)
			{
				return dataMemberAttribute.EmitDefaultValue;
			}
			return false;
		}

		/// <summary>
		///     Retrieve the IsReadOnly from the ReadOnlyAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>IsReadOnly</returns>
		public static bool GetReadOnly(this PropertyInfo propertyInfo)
		{
			var readOnlyAttribute = propertyInfo.GetAttribute<ReadOnlyAttribute>();
			return readOnlyAttribute != null && readOnlyAttribute.IsReadOnly;
		}

		/// <summary>
		///     Retrieve the TypeConverter from the TypeConverterAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <param name="createIfNothingSpecified">true if this should always create a converter</param>
		/// <returns>TypeConverter</returns>
		public static TypeConverter GetTypeConverter(this PropertyInfo propertyInfo, bool createIfNothingSpecified = false)
		{
			var typeConverterAttribute = propertyInfo.GetAttribute<TypeConverterAttribute>();
			if (!string.IsNullOrEmpty(typeConverterAttribute?.ConverterTypeName))
			{
				var typeConverterType = Type.GetType(typeConverterAttribute.ConverterTypeName);
				if (typeConverterType != null)
				{
					return (TypeConverter) Activator.CreateInstance(typeConverterType);
				}
			}

			return createIfNothingSpecified ? propertyInfo.PropertyType.GetConverter() : null;
		}

		/// <summary>
		/// Gets property information for the specified <paramref name="property"/> expression.
		/// </summary>
		/// <typeparam name="TSource">Type of the parameter in the <paramref name="property"/> expression.</typeparam>
		/// <typeparam name="TValue">Type of the property's value.</typeparam>
		/// <param name="property">The expression from which to retrieve the property information.</param>
		/// <returns>Property information for the specified expression.</returns>
		/// <exception cref="ArgumentException">The expression is not understood.</exception>
		public static PropertyInfo GetPropertyInfo<TSource, TValue>(this Expression<Func<TSource, TValue>> property)
		{
			if (property == null)
			{
				throw new ArgumentNullException(nameof(property));
			}

			if (property.Body is not MemberExpression body)
			{
				throw new ArgumentException("Expression is not a property", nameof(property));
			}

			if (body.Member is not PropertyInfo propertyInfo)
			{
				throw new ArgumentException("Expression is not a property", nameof(property));
			}

			return propertyInfo;
		}
	}
}