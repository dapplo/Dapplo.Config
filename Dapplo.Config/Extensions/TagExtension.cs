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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapplo.Config.Support;

namespace Dapplo.Config.Extensions {
	/// <summary>
	///     Interface which your interface needs to implement to be able to see if a property is tagged
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ITagging<T> {
		/// <summary>
		///     Checks if the supplied expression resolves to a property which has the expert attribute
		/// </summary>
		/// <typeparam name="TProp">Your interfaces</typeparam>
		/// <param name="propertyExpression"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>true if the property has the expert attribute, else false</returns>
		bool IsTaggedWith<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag);
	}

	[Extension(typeof (ITagging<>))]
	public class TagExtension<T> : IPropertyProxyExtension<T> {
		// The set of found expert properties
		private readonly IDictionary<string, ISet<object>> _taggedProperties = new Dictionary<string, ISet<object>>();

		public TagExtension(IPropertyProxy<T> proxy) {
			Type proxiedType = typeof (T);
			// Loop over all the properties, and check if they have the TagAttribute.
			foreach (PropertyInfo propertyInfo in proxiedType.GetProperties()) {
				Attribute[] customAttributes = Attribute.GetCustomAttributes(propertyInfo);
				foreach (Attribute customAttribute in customAttributes) {
					TagAttribute tagAttribute = customAttribute as TagAttribute;
					if (tagAttribute != null) {
						ISet<object> tags;
						if (!_taggedProperties.TryGetValue(propertyInfo.Name, out tags)) {
							tags = new HashSet<object>();
							_taggedProperties.Add(propertyInfo.Name, tags);
						}
						tags.Add(tagAttribute.Tag);
					}
				}
			}

			if (!typeof (T).GetInterfaces().Contains(typeof (ITagging<T>))) {
				throw new NotSupportedException("Type needs to implement ITagging");
			}
			proxy.RegisterMethod("IsTaggedWith", HandleIsTaggedWith);
		}

		/// <summary>
		///     This is the invoke "handler" which gets the parameters, and returns a message.
		///     The real logic is in the IsTagged method.
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void HandleIsTaggedWith(MethodCallInfo methodCallInfo) {
			methodCallInfo.ReturnValue = IsTaggedWith((LambdaExpression)methodCallInfo.Arguments[0], methodCallInfo.Arguments[1]);
		}

		/// <summary>
		/// This is the logic which resolves the supplied expression, and checks if the property is tagged
		/// </summary>
		/// <param name="propertyExpression"></param>
		/// <param name="tag">tag to test for</param>
		/// <returns></returns>
		private bool IsTaggedWith(LambdaExpression propertyExpression, object tag) {
			string property = propertyExpression.GetMemberName();
			ISet<object> tags;
			if (_taggedProperties.TryGetValue(property, out tags)) {
				return tags.Contains(tag);				
			}
			return false;
		}
	}
}