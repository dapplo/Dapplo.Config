/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2014 Robin Krom
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapplo.Config.Support;

namespace Dapplo.Config.Extensions {
	/// <summary>
	///     Interface which your interface needs to implement to be able to see if a property is expert relevant
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IExpert<T> {
		/// <summary>
		///     Checks if the supplied expression resolves to a property which has the expert attribute
		/// </summary>
		/// <typeparam name="TProp">Your interfaces</typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>true if the property has the expert attribute, else false</returns>
		bool IsExpert<TProp>(Expression<Func<T, TProp>> propertyExpression);
	}

	[Extension(typeof (IExpert<>))]
	public class ExpertPropertyExtension<T> : IPropertyProxyExtension<T> {
		// The set of found expert properties
		private readonly ISet<string> _expertProperties = new HashSet<string>();

		public ExpertPropertyExtension(IPropertyProxy<T> proxy) {
			Type proxiedType = typeof (T);
			// Loop over all the properties, and check if they have an expert attribute.
			foreach (PropertyInfo propertyInfo in proxiedType.GetProperties()) {
				Attribute[] customAttributes = Attribute.GetCustomAttributes(propertyInfo);
				foreach (Attribute customAttribute in customAttributes) {
					if (customAttribute.GetType() == typeof (ExpertAttribute)) {
						_expertProperties.Add(propertyInfo.Name);
					}
				}
			}

			if (!typeof (T).GetInterfaces().Contains(typeof (IExpert<T>))) {
				throw new NotSupportedException("Type needs to implement IExpert");
			}
			proxy.RegisterMethod("IsExpert", HandleIsExpert);
		}

		/// <summary>
		///     This is the invoke "handler" which gets the parameters, and returns a message.
		///     The real logic is in the IsExpert method.
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void HandleIsExpert(MethodCallInfo methodCallInfo) {
			methodCallInfo.ReturnValue = IsExpert((LambdaExpression) methodCallInfo.Arguments[0]);
		}

		/// <summary>
		///     This is the logic which resolves the supplied expression, and checks if the property has an expert attribute
		/// </summary>
		/// <param name="propertyExpression"></param>
		/// <returns></returns>
		private bool IsExpert(LambdaExpression propertyExpression) {
			string property = propertyExpression.GetMemberName();
			return _expertProperties.Contains(property);
		}
	}
}