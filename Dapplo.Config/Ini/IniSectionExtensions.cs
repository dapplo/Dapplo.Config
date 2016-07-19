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

#region Usings

using System;
using Dapplo.Utils.Events;

#endregion

namespace Dapplo.Config.Ini
{
	/// <summary>
	///     Supply an extension to simplify the usage of IIniSection.OnLoaded / IIniSection.OnSaved / IIniSection.OnSaving / IIniSection.Reset
	/// </summary>
	public static class IniSectionExtensions
	{
		/// <summary>
		///     Automatically call the update action when the Reset fires
		///     If the is called on a DI object, make sure it's available.
		///     When using MEF, it would be best to call this from IPartImportsSatisfiedNotification.OnImportsSatisfied
		/// </summary>
		/// <param name="iniSection">IIniSection</param>
		/// <param name="eventAction">Action to call on events, argument is the IniSectionEventArgs</param>
		/// <returns>an IDisposable, calling Dispose on this will stop everything</returns>
		public static IDisposable OnReset(this IIniSection iniSection, Action<IniSectionEventArgs> eventAction)
		{
			if (iniSection == null)
			{
				throw new ArgumentNullException(nameof(iniSection));
			}
			if (eventAction == null)
			{
				throw new ArgumentNullException(nameof(eventAction));
			}
			return EventObservable.From<IniSectionEventArgs>(iniSection, nameof(IIniSection.Reset)).OnEach(pce => eventAction(pce.Args));
		}

		/// <summary>
		///     Automatically call the update action when the Loaded fires
		///     If the is called on a DI object, make sure it's available.
		///     When using MEF, it would be best to call this from IPartImportsSatisfiedNotification.OnImportsSatisfied
		/// </summary>
		/// <param name="iniSection">IIniSection</param>
		/// <param name="eventAction">Action to call on events, argument is the IniSectionEventArgs</param>
		/// <returns>an IDisposable, calling Dispose on this will stop everything</returns>
		public static IDisposable OnLoaded(this IIniSection iniSection, Action<IniSectionEventArgs> eventAction)
		{
			if (iniSection == null)
			{
				throw new ArgumentNullException(nameof(iniSection));
			}
			if (eventAction == null)
			{
				throw new ArgumentNullException(nameof(eventAction));
			}
			return EventObservable.From<IniSectionEventArgs>(iniSection, nameof(IIniSection.Loaded)).OnEach(pce => eventAction(pce.Args));
		}

		/// <summary>
		///     Automatically call the update action when the Saved fires
		///     If the is called on a DI object, make sure it's available.
		///     When using MEF, it would be best to call this from IPartImportsSatisfiedNotification.OnImportsSatisfied
		/// </summary>
		/// <param name="iniSection">IIniSection</param>
		/// <param name="eventAction">Action to call on events, argument is the IniSectionEventArgs</param>
		/// <returns>an IDisposable, calling Dispose on this will stop everything</returns>
		public static IDisposable OnSaved(this IIniSection iniSection, Action<IniSectionEventArgs> eventAction)
		{
			if (iniSection == null)
			{
				throw new ArgumentNullException(nameof(iniSection));
			}
			if (eventAction == null)
			{
				throw new ArgumentNullException(nameof(eventAction));
			}
			return EventObservable.From<IniSectionEventArgs>(iniSection, nameof(IIniSection.Saved)).OnEach(pce => eventAction(pce.Args));
		}

		/// <summary>
		///     Automatically call the update action when the Saving fires
		///     If the is called on a DI object, make sure it's available.
		///     When using MEF, it would be best to call this from IPartImportsSatisfiedNotification.OnImportsSatisfied
		/// </summary>
		/// <param name="iniSection">IIniSection</param>
		/// <param name="eventAction">Action to call on events, argument is the IniSectionEventArgs</param>
		/// <returns>an IDisposable, calling Dispose on this will stop everything</returns>
		public static IDisposable OnSaving(this IIniSection iniSection, Action<IniSectionEventArgs> eventAction)
		{
			if (iniSection == null)
			{
				throw new ArgumentNullException(nameof(iniSection));
			}
			if (eventAction == null)
			{
				throw new ArgumentNullException(nameof(eventAction));
			}
			return EventObservable.From<IniSectionEventArgs>(iniSection, nameof(IIniSection.Saving)).OnEach(pce => eventAction(pce.Args));
		}
	}
}