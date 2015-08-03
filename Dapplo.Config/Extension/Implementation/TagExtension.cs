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

using Dapplo.Config.Support;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dapplo.Config.Extension.Implementation
{
	[Extension(typeof (ITagging))]
	internal class TagExtension<T> : AbstractPropertyProxyExtension<T>
	{
		// The set of found expert properties
		private readonly IDictionary<string, IDictionary<object, object>> _taggedProperties = new Dictionary<string, IDictionary<object, object>>();

		public TagExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof (ITagging));

			// Use Lambda to make refactoring possible, this registers one method and the overloading is handled in the IsTaggedWith
			proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ITagging>(x => x.IsTaggedWith("", null)), IsTaggedWith);
			proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ITagging>(x => x.GetTagValue("", null)), GetTagValue);
		}

		/// <summary>
		/// Process the property, in our case get the tags
		/// </summary>
		/// <param name="propertyInfo"></param>
		public override void InitProperty(PropertyInfo propertyInfo)
		{
			Attribute[] customAttributes = Attribute.GetCustomAttributes(propertyInfo);
			foreach (Attribute customAttribute in customAttributes)
			{
				TagAttribute tagAttribute = customAttribute as TagAttribute;
				if (tagAttribute != null)
				{
					IDictionary<object, object> tags;
					if (!_taggedProperties.TryGetValue(propertyInfo.Name, out tags))
					{
						tags = new Dictionary<object, object>();
						_taggedProperties.Add(propertyInfo.Name, tags);
					}
					tags.SafelyAddOrOverwrite(tagAttribute.Tag, tagAttribute.TagValue);
				}
			}
		}

		/// <summary>
		/// Check if a property is tagged with a certain tag
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void IsTaggedWith(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = false;
			IDictionary<object, object> tags;
			if (_taggedProperties.TryGetValue(methodCallInfo.PropertyNameOf(0), out tags))
			{
				methodCallInfo.ReturnValue = tags.ContainsKey(methodCallInfo.Arguments[1]);
			}
		}

		/// <summary>
		/// Check if a property is tagged with a certain tag
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void GetTagValue(MethodCallInfo methodCallInfo) {
			methodCallInfo.ReturnValue = false;
			IDictionary<object, object> tags;
			if (_taggedProperties.TryGetValue(methodCallInfo.PropertyNameOf(0), out tags)) {
				bool hasTag = tags.ContainsKey(methodCallInfo.Arguments[1]);
				object returnValue = null;
				if (hasTag) {
					returnValue = tags[methodCallInfo.Arguments[1]];
				}
				methodCallInfo.ReturnValue = returnValue;
			}
		}
	}
}