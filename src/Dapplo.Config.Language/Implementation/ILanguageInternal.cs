// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Language.Implementation
{
	/// <summary>
	///     Interface which is used by the language loader
	/// </summary>
	internal interface ILanguageInternal
	{
		/// <summary>
		///     Generate a LanguageChanged event
		///     Added for Dapplo.Config/issues/10
		/// </summary>
		void OnLanguageChanged();
	}
}