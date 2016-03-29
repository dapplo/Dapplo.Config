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

using Dapplo.Config.Interceptor.Implementation;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Support;

#endregion

namespace Dapplo.Config.Interceptor.Extensions
{
	/// <summary>
	///     This implements logic to set the default values on your property interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (IDescription))]
	internal class DescriptionExtension<T> : AbstractInterceptorExtension
	{
		/// <summary>
		/// Register methods
		/// </summary>
		public override void Initialize()
		{
			// this registers one method and the overloading is handled in the GetDescription
			Interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IDescription>(x => x.DescriptionFor("")), GetDescription);
		}

		/// <summary>
		///     Return the description for a property
		/// </summary>
		private void GetDescription(MethodCallInfo methodCallInfo)
		{
			var propertyInfo = typeof (T).GetProperty(methodCallInfo.PropertyNameOf(0));
			methodCallInfo.ReturnValue = propertyInfo.GetDescription();
		}
	}
}