//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

using System;
using System.Linq.Expressions;

namespace Dapplo.Config.Extensions
{
	/// <summary>
	///     Lambda expressions Utils
	/// </summary>
	public static class ExpressionExtensions
	{
		/// <summary>
		///     Non extension helper method to get a refactorable name of a member.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression"></param>
		/// <returns>Name of member</returns>
		public static string GetMemberName<T>(Expression<Action<T>> expression)
		{
			return expression.GetMemberName();
		}

		/// <summary>
		///     Non extension helper method to get a refactorable name of a member.
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
		///     Get the name of the member in a Lambda expression
		/// </summary>
		/// <param name="memberSelector">LambdaExpression</param>
		/// <returns>string with the member name</returns>
		public static string GetMemberName(this LambdaExpression memberSelector)
		{
			static string NameSelector(Expression e)
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
						return NameSelector(((UnaryExpression) e).Operand);
					case ExpressionType.Invoke:
						return NameSelector(((InvocationExpression) e).Expression);
					case ExpressionType.ArrayLength:
						return "Length";
					default:
						throw new Exception("not a proper member selector");
				}
			}

			return NameSelector(memberSelector.Body);
		}
	}
}