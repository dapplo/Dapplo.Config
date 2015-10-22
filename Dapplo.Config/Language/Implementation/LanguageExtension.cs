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
using System.Reflection;
using Dapplo.Config.Support;
using Dapplo.Config.Proxy.Implementation;

namespace Dapplo.Config.Language.Implementation
{
	/// <summary>
	/// Extend the PropertyProxy with Ini functionality
	/// </summary>
	[Extension(typeof(ILanguage))]
	internal class LanguageExtension<T> : AbstractPropertyProxyExtension<T>
	{
		public LanguageExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof(ILanguage));
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ILanguage, object>(x => x[null]), GetTranslation);
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ILanguage, object>(x => x.Keys()), GetKeys);
		}

		public override void InitProperty(PropertyInfo propertyInfo)
		{
			base.InitProperty(propertyInfo);
			if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic)
			{
				throw new NotSupportedException($"Property {propertyInfo.DeclaringType}.{propertyInfo.Name} has defined a set, this is not allowed for {typeof (ILanguage).Name} derrived interfaces. Fix by removing the set for the property, leave the get.");
			}
		}

		/// <summary>
		/// Get a single translation
		/// </summary>
		private void GetTranslation(MethodCallInfo methodCallInfo)
		{
            var key = methodCallInfo.PropertyNameOf(0);
			if (Proxy.Properties.ContainsKey(key))
			{
				methodCallInfo.ReturnValue = Proxy.Properties[key] as string;
			}
		}

		/// <summary>
		/// Get a all the keys
		/// </summary>
		private void GetKeys(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = Proxy.Properties.Keys;
		}
	}
}