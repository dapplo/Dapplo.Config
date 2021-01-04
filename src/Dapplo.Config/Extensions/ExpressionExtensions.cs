// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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