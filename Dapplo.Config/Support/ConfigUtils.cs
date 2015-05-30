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

using Dapplo.Config.Ini;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Dapplo.Config.Support
{
	/// <summary>
	/// Lambda expressions Utils
	/// </summary>
	public static class ConfigUtils
	{
		/// <summary>
		/// Get the property name from the argument "index" of the MethodCallInfo
		/// If the argument is a string, it will be returned.
		/// If the arugment is a LambdaExpression, the member name will be retrieved
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		/// <param name="index">Index of the argument</param>
		/// <returns>Property name</returns>
		public static string PropertyNameOf(this MethodCallInfo methodCallInfo, int index)
		{
			string propertyName = methodCallInfo.Arguments[index] as string;
			if (propertyName == null)
			{
				LambdaExpression propertyExpression = (LambdaExpression)methodCallInfo.Arguments[index];
				propertyName = propertyExpression.GetMemberName();
			}
			return propertyName;
		}

		/// <summary>
		/// Non extension helper method to get a refactorable name of a member.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression"></param>
		/// <returns>Name of member</returns>
		public static string GetMemberName<T>(Expression<Action<T>> expression)
		{
			return expression.GetMemberName();
		}

		/// <summary>
		/// Non extension helper method to get a refactorable name of a member.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="expression"></param>
		/// <returns>Name of member</returns>
		public static string GetMemberName<T, TProp>(Expression<Func<T, TProp>> expression)
		{
			return expression.GetMemberName();
		}

		/// <summary>
		/// Get the name of the member in a Lambda expression
		/// </summary>
		/// <param name="memberSelector">LambdaExpression</param>
		/// <returns>string with the member name</returns>
		public static string GetMemberName(this LambdaExpression memberSelector)
		{
			Func<Expression, string> nameSelector = null; //recursive func
			nameSelector = e => {
				//or move the entire thing to a separate recursive method
				switch (e.NodeType)
				{
					case ExpressionType.Parameter:
						return ((ParameterExpression)e).Name;
					case ExpressionType.MemberAccess:
						return ((MemberExpression)e).Member.Name;
					case ExpressionType.Call:
						return ((MethodCallExpression)e).Method.Name;
					case ExpressionType.Convert:
					case ExpressionType.ConvertChecked:
						return nameSelector(((UnaryExpression)e).Operand);
					case ExpressionType.Invoke:
						return nameSelector(((InvocationExpression)e).Expression);
					case ExpressionType.ArrayLength:
						return "Length";
					default:
						throw new Exception("not a proper member selector");
				}
			};

			return nameSelector(memberSelector.Body);
		}

		/// <summary>
		/// Retrieve the DefaultValueFor from the DefaultValueAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>DefaultValueFor</returns>
		public static object GetDefaultValue(this PropertyInfo propertyInfo)
		{
			var defaultValueAttribute = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
			if (defaultValueAttribute != null)
			{
				return defaultValueAttribute.Value;
			}
			return null;
		}

		/// <summary>
		/// Retrieve the TypeConverter from the TypeConverterAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>TypeConverter</returns>
		public static TypeConverter GetTypeConverter(this PropertyInfo propertyInfo)
		{
			var typeConverterAttribute = propertyInfo.GetCustomAttribute<TypeConverterAttribute>();
			if (typeConverterAttribute != null && !string.IsNullOrEmpty(typeConverterAttribute.ConverterTypeName))
			{
				Type typeConverterType = Type.GetType(typeConverterAttribute.ConverterTypeName);
				if (typeConverterType != null)
				{
					return (TypeConverter)Activator.CreateInstance(typeConverterType);
				}
			}
			return null;
		}

		/// <summary>
		/// Retrieve the Description from the DescriptionAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Description</returns>
		public static string GetDescription(this PropertyInfo propertyInfo)
		{
			var descriptionAttribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
			if (descriptionAttribute != null)
			{
				return descriptionAttribute.Description;
			}
			return null;
		}

		/// <summary>
		/// Retrieve the Name from the DataMemberAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Name</returns>
		public static string GetDataMemberName(this PropertyInfo propertyInfo)
		{
			var dataMemberAttribute = propertyInfo.GetCustomAttribute<DataMemberAttribute>();
			if (dataMemberAttribute != null)
			{
				if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
				{
					return dataMemberAttribute.Name;
				}
			}
			return null;
		}

		/// <summary>
		/// Retrieve the EmitDefaultValue from the DataMemberAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>EmitDefaultValue</returns>
		public static bool GetEmitDefaultValue(this PropertyInfo propertyInfo)
		{
			var dataMemberAttribute = propertyInfo.GetCustomAttribute<DataMemberAttribute>();
			if (dataMemberAttribute != null)
			{
				return dataMemberAttribute.EmitDefaultValue;
			}
			return false;
		}

		/// <summary>
		/// Check if the property is non serialized (annotated with the NonSerializedAttribute)
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>true if the NonSerialized attribute is set on the property</returns>
		public static IniPropertyBehaviorAttribute GetIniPropertyBehavior(this PropertyInfo propertyInfo)
		{
			var iniPropertyBehaviorAttribute = propertyInfo.GetCustomAttribute<IniPropertyBehaviorAttribute>();
			if (iniPropertyBehaviorAttribute == null)
			{
				iniPropertyBehaviorAttribute = new IniPropertyBehaviorAttribute();
			}
			return iniPropertyBehaviorAttribute;
		}

		/// <summary>
		/// Retrieve the IsReadOnly from the ReadOnlyAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>IsReadOnly</returns>
		public static bool GetReadOnly(this PropertyInfo propertyInfo)
		{
			var readOnlyAttribute = propertyInfo.GetCustomAttribute<ReadOnlyAttribute>();
			if (readOnlyAttribute != null)
			{
				return readOnlyAttribute.IsReadOnly;
			}
			return false;
		}

		/// <summary>
		/// Retrieve the Category from the CategoryAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Category</returns>
		public static string GetCategory(this PropertyInfo propertyInfo)
		{
			var categoryAttribute = propertyInfo.GetCustomAttribute<CategoryAttribute>();
			if (categoryAttribute != null)
			{
				return categoryAttribute.Category;
			}
			return null;
		}

		/// <summary>
		/// Safely retrieve a value from the dictionary, by using a key
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <returns>object</returns>
		public static object SafeGet(this IDictionary<string, object> dictionary, string key)
		{
			object value;
			if (dictionary.TryGetValue(key, out value))
			{
				return value;
			}
			return null;
		}

		/// <summary>
		/// Safely add or overwrite a value in the dictionary, supply the key & value
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key">string key</param>
		/// <param name="newValue">object</param>
		/// <returns>dictionary for fluent API calls</returns>
		public static IDictionary<T1, T2> SafelyAddOrOverwrite<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, T2 newValue)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = newValue;
			}
			else
			{
				dictionary.Add(key, newValue);
			}
			return dictionary;
		}
	}
}
