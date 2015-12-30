/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015-2016 Dapplo
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

using System.Linq.Expressions;

namespace Dapplo.Config.Support {
	public static class MethodCallExtensions
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
	}
}
