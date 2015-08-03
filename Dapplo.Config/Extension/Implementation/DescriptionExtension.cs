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
using System.Reflection;

namespace Dapplo.Config.Extension.Implementation
{
	/// <summary>
	///  This implements logic to set the default values on your property interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (IDescription))]
	internal class DescriptionExtension<T> : AbstractPropertyProxyExtension<T>
	{
		public DescriptionExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof (IDescription));

			// this registers one method and the overloading is handled in the GetDescription
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IDescription>(x => x.DescriptionFor("")), GetDescription);
		}

		/// <summary>
		/// Return the description for a property
		/// </summary>
		private void GetDescription(MethodCallInfo methodCallInfo)
		{
			PropertyInfo propertyInfo = typeof (T).GetProperty(methodCallInfo.PropertyNameOf(0));
			methodCallInfo.ReturnValue = propertyInfo.GetDescription();
		}
	}
}