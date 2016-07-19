#region Dapplo 2016 - GNU Lesser General Public License

// Dapplo - building blocks for .NET applications
// Copyright (C) 2016 Dapplo
// 
// For more information see: http://dapplo.net/
// Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
// This file is part of Dapplo.Config
// 
// Dapplo.Config is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Dapplo.Config is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have a copy of the GNU Lesser General Public License
// along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#endregion

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