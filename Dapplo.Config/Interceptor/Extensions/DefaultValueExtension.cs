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
using System.Reflection;
using Dapplo.Config.Interceptor.Implementation;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Support;
using Dapplo.LogFacade;

#endregion

namespace Dapplo.Config.Interceptor.Extensions
{
	/// <summary>
	///     This implements logic to set the default values on your property interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (IDefaultValue))]
	internal class DefaultValueExtension<T> : AbstractInterceptorExtension
	{
		private static readonly LogSource Log = new LogSource();

		/// <summary>
		/// Register the methods
		/// </summary>
		public override void Initialize()
		{
			// this registers one method and the overloading is handled in the GetDefaultValue
			Interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IDefaultValue>(x => x.DefaultValueFor("")), GetDefaultValue);
			Interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IDefaultValue>(x => x.RestoreToDefault("")), RestoreToDefault);
		}

		/// <summary>
		///     Make sure this extension is initialized first
		/// </summary>
		public override int InitOrder => int.MinValue;

		/// <summary>
		///     Retrieve the default value, using the TypeConverter
		/// </summary>
		/// <param name="propertyInfo">Property to get the default value for</param>
		/// <returns>object with the type converted default value</returns>
		private object GetConvertedDefaultValue(PropertyInfo propertyInfo)
		{
			var defaultValue = propertyInfo.GetDefaultValue();
			if (defaultValue != null)
			{
				var typeConverter = propertyInfo.GetTypeConverter();
				var targetType = propertyInfo.PropertyType;
				defaultValue = targetType.ConvertOrCastValueToType(defaultValue, typeConverter);
			}
			return defaultValue;
		}

		/// <summary>
		///     Return the default value for a property
		/// </summary>
		private void GetDefaultValue(MethodCallInfo methodCallInfo)
		{
			var propertyInfo = typeof (T).GetProperty(methodCallInfo.PropertyNameOf(0));
			// Prevent ArgumentNullExceptions
			if (propertyInfo != null)
			{
				methodCallInfo.ReturnValue = GetConvertedDefaultValue(propertyInfo);
			}
		}

		/// <summary>
		///     Process the property, in our case set the default
		/// </summary>
		/// <param name="propertyInfo"></param>
		public override void InitProperty(PropertyInfo propertyInfo)
		{
			Exception ex;
			RestoreToDefault(propertyInfo, out ex);
			if (ex != null)
			{
				throw ex;
			}
		}

		/// <summary>
		///     Return the default value for a property
		/// </summary>
		private void RestoreToDefault(MethodCallInfo methodCallInfo)
		{
			var propertyInfo = typeof (T).GetProperty(methodCallInfo.PropertyNameOf(0));
			// Prevent ArgumentNullExceptions
			if (propertyInfo != null)
			{
				Exception ex;
				RestoreToDefault(propertyInfo, out ex);
			}
		}

		/// <summary>
		///     Method to restore a property to its default
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <param name="exception">out value to get an exception</param>
		private void RestoreToDefault(PropertyInfo propertyInfo, out Exception exception)
		{
			object defaultValue = null;
			exception = null;
			try
			{
				defaultValue = GetConvertedDefaultValue(propertyInfo);
			}
			catch (Exception ex)
			{
				Log.Warn().WriteLine(ex.Message);
				// Store the exception so it can be used
				exception = ex;
			}

			if (defaultValue != null)
			{
				Interceptor.Set(propertyInfo.Name, defaultValue);
				return;
			}
			if (!propertyInfo.PropertyType.IsInterface && !propertyInfo.PropertyType.IsByRef && propertyInfo.PropertyType != typeof (string))
			{
				try
				{
					defaultValue = propertyInfo.PropertyType.CreateInstance();
					Interceptor.Set(propertyInfo.Name, defaultValue);
					return;
				}
				catch (Exception ex)
				{
					// Ignore creating the default type, this might happen if there is no default constructor.
					Log.Warn().WriteLine(ex.Message);
				}
			}
			if (Interceptor.Properties.ContainsKey(propertyInfo.Name))
			{
				// TODO: This doesn't create a NotifyPropertyChanged/ing event as set isn't called.
				Interceptor.Properties.Remove(propertyInfo.Name);
			}
		}
	}
}