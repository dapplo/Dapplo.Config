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

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// The different IniSection events
	/// </summary>
	public enum IniEventTypes
	{
		/// <summary>
		/// This is used to tell the subscriber that a reset was triggered on the IIniConfig
		/// </summary>
		Reset,
		/// <summary>
		///     This is used to tell the subscriber that the IIniConfig was (re)loaded
		/// </summary>
		Loaded,
		/// <summary>
		/// This is used to tell the subscriber that the IIniConfig is going to be saved
		/// </summary>
		Saving,
		/// <summary>
		/// This is used to tell the subscriber that the IIniConfig was saved
		/// </summary>
		Saved
	}
}