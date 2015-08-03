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
using System.Linq.Expressions;

namespace Dapplo.Config.Support
{
	/// <summary>
	/// Lambda expressions Utils
	/// </summary>
	public static class ExpressionExtensions
	{
		/// <summary>
		/// Non extension helper method to get a refactorable name of a member.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression"></param>
		/// <returns>Name of member</returns>
		public static string GetMemberName<T>(Expression<Action<T>> expression)
		{
			return expression.GetMemberName();
		}

		/// <summary>
		/// Non extension helper method to get a refactorable name of a member.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="expression"></param>
		/// <returns>Name of member</returns>
		public static string GetMemberName<T, TProp>(Expression<Func<T, TProp>> expression)
		{
			return expression.GetMemberName();
		}

		/// <summary>
		/// Get the name of the member in a Lambda expression
		/// </summary>
		/// <param name="memberSelector">LambdaExpression</param>
		/// <returns>string with the member name</returns>
		public static string GetMemberName(this LambdaExpression memberSelector)
		{
			Func<Expression, string> nameSelector = null; //recursive func
			nameSelector = e =>
			{
				//or move the entire thing to a separate recursive method
				switch (e.NodeType)
				{
					case ExpressionType.Parameter:
						return ((ParameterExpression) e).Name;
					case ExpressionType.MemberAccess:
						return ((MemberExpression) e).Member.Name;
					case ExpressionType.Call:
						return ((MethodCallExpression) e).Method.Name;
					case ExpressionType.Convert:
					case ExpressionType.ConvertChecked:
						return nameSelector(((UnaryExpression) e).Operand);
					case ExpressionType.Invoke:
						return nameSelector(((InvocationExpression) e).Expression);
					case ExpressionType.ArrayLength:
						return "Length";
					default:
						throw new Exception("not a proper member selector");
				}
			};

			return nameSelector(memberSelector.Body);
		}
	}
}