/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.Config

	Dapplo.Config is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.Config is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have Config a copy of the GNU Lesser General Public License
	along with Dapplo.HttpExtensions. If not, see <http://www.gnu.org/licenses/lgpl.txt>.
 */

namespace Dapplo.Config.Support
{
	/// <summary>
	///     This class contains all the information for the setter actions
	/// </summary>
	public class SetInfo : GetSetInfo
	{
		/// <summary>
		///     The new value for the property
		/// </summary>
		public object NewValue
		{
			get;
			set;
		}

		/// <summary>
		///     The old value of the property, if any (see HasOldValue)
		/// </summary>
		public object OldValue
		{
			get;
			set;
		}

		/// <summary>
		///     Does property have an old value?
		/// </summary>
		public bool HasOldValue
		{
			get;
			set;
		}
	}
}