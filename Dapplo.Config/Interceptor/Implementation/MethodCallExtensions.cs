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

using System.Linq.Expressions;
using Dapplo.Config.Support;

#endregion

namespace Dapplo.Config.Interceptor.Implementation
{
	public static class MethodCallExtensions
	{
		/// <summary>
		///     Get the property name from the argument "index" of the MethodCallInfo
		///     If the argument is a string, it will be returned.
		///     If the arugment is a LambdaExpression, the member name will be retrieved
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		/// <param name="index">Index of the argument</param>
		/// <returns>Property name</returns>
		public static string PropertyNameOf(this MethodCallInfo methodCallInfo, int index)
		{
			var propertyName = methodCallInfo.Arguments[index] as string;
			if (propertyName == null)
			{
				var propertyExpression = (LambdaExpression)methodCallInfo.Arguments[index];
				propertyName = propertyExpression.GetMemberName();
			}
			return propertyName;
		}
	}
}