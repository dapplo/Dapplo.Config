// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using Dapplo.Config.Extensions;

namespace Dapplo.Config.Language
{
	/// <summary>
	///     All ILanguage Extension methods go here.
	/// </summary>
	public static class LanguageExtensions
	{
		/// <summary>
		///     This extension can help when the language file cannot be created.
		///     It can give you the default value in the interface, even if the ILanguage extending interface instance is null!
		///     Usage:
		///     ILanguageExtendingInterface someInstance = null;
		///     string defaultTranslation = someInstance.DefaultTranslation(x => x.Ok);
		/// </summary>
		/// <typeparam name="TLanguage">Type implementing ILanguage</typeparam>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="language">ILanguage</param>
		/// <param name="propertyExpression"></param>
		/// <returns>string with the default translation</returns>
		// ReSharper disable once UnusedParameter.Global
		public static string DefaultTranslation<TLanguage, TProp>(this TLanguage language, Expression<Func<TLanguage, TProp>> propertyExpression) where TLanguage : ILanguage
		{
			var propertyName = propertyExpression.GetMemberName();
			return (string) typeof(TLanguage).GetProperty(propertyName).GetDefaultValue();
		}

		/// <summary>
		///     This extension can help when the language file cannot be created.
		///     It gives you the translation, or the default translation if the instance is null.
		///     Usage:
		///     ILanguageExtendingInterface someInstance = .... ;
		///     string defaultTranslation = someInstance.TranslationOrDefault(x => x.Ok);
		/// </summary>
		/// <typeparam name="TLanguage">Type implementing ILanguage</typeparam>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="type"></param>
		/// <param name="propertyExpression"></param>
		/// <returns>string with the translation</returns>
		public static string TranslationOrDefault<TLanguage, TProp>(this TLanguage type, Expression<Func<TLanguage, TProp>> propertyExpression) where TLanguage : ILanguage
		{
			var propertyName = propertyExpression.GetMemberName();
			var propertyInfo = typeof(TLanguage).GetProperty(propertyName);
			if (type != null)
			{
				return (string) propertyInfo?.GetValue(type);
			}
			return (string) propertyInfo?.GetDefaultValue();
		}
	}
}