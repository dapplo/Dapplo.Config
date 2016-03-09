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
using Dapplo.Config.Interceptor;
using Dapplo.Config.Proxy.Implementation;
using Dapplo.Config.Support;

#endregion

namespace Dapplo.Config.Language.Implementation
{
	/// <summary>
	///     Extend the PropertyProxy with Ini functionality
	/// </summary>
	[Extension(typeof (ILanguage))]
	internal class LanguageExtension<T> : AbstractPropertyProxyExtension<T>
	{
		public LanguageExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof (ILanguage));
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ILanguage, object>(x => x[null]), GetTranslation);
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ILanguage, object>(x => x.Keys()), GetKeys);
		}

		/// <summary>
		///     Get a all the keys
		/// </summary>
		private void GetKeys(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = Proxy.Properties.Keys;
		}

		/// <summary>
		///     Get a single translation
		/// </summary>
		private void GetTranslation(MethodCallInfo methodCallInfo)
		{
			var key = methodCallInfo.PropertyNameOf(0);
			if (Proxy.Properties.ContainsKey(key))
			{
				methodCallInfo.ReturnValue = Proxy.Properties[key] as string;
			}
		}

		public override void InitProperty(PropertyInfo propertyInfo)
		{
			base.InitProperty(propertyInfo);
			if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic)
			{
				throw new NotSupportedException(
					$"Property {propertyInfo.DeclaringType}.{propertyInfo.Name} has defined a set, this is not allowed for {typeof (ILanguage).Name} derrived interfaces. Fix by removing the set for the property, leave the get.");
			}
		}
	}
}