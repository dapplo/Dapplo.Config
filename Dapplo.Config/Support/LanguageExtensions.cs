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

using Dapplo.Config.Language;
using System;
using System.Linq.Expressions;

namespace Dapplo.Config.Support
{
	/// <summary>
	/// All ILanguage Extension methods go here.
	/// </summary>
	public static class LanguageExtensions
	{
		/// <summary>
		/// This extension can help when the language file cannot be created.
		/// It can give you the default value in the interface, even if the ILanguage extending interface instance is null!
		/// Usage:
		/// ILanguageExtendingInterface someInstance = null;
		/// string defaultTranslation = someInstance.DefaultTranslation(x => x.Ok);
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="type"></param>
		/// <param name="propertyExpression"></param>
		/// <returns>string with the default translation</returns>
		public static string DefaultTranslation<T, TProp>(this T type, Expression<Func<T, TProp>> propertyExpression) where T : ILanguage
		{
			var propertyName = propertyExpression.GetMemberName();
			return (string)typeof(T).GetProperty(propertyName).GetDefaultValue();
		}

		/// <summary>
		/// This extension can help when the language file cannot be created.
		/// It gives you the translation, or the default translation if the instance is null.
		/// Usage:
		/// ILanguageExtendingInterface someInstance = .... ;
		/// string defaultTranslation = someInstance.TranslationOrDefault(x => x.Ok);
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="type"></param>
		/// <param name="propertyExpression"></param>
		/// <returns>string with the translation</returns>
		public static string TranslationOrDefault<T, TProp>(this T type, Expression<Func<T, TProp>> propertyExpression) where T : ILanguage
		{
			var propertyName = propertyExpression.GetMemberName();
			var propertyInfo = typeof(T).GetProperty(propertyName);
            if (type != null)
			{
				return (string)propertyInfo.GetValue(type);
            }
			return (string)propertyInfo.GetDefaultValue();
		}
	}
}
