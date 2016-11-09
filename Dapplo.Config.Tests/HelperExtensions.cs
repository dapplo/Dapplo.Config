using System;
using System.Reactive.Linq;
using Dapplo.Config.Ini;
using Dapplo.Config.Language;

namespace Dapplo.Config.Tests
{
	public static class HelperExtensions
	{
		/// <summary>
		///     Automatically call the update action when the LanguageChanged fires
		///     If the is called on a DI object, make sure it's available.
		///     When using MEF, it would be best to call this from IPartImportsSatisfiedNotification.OnImportsSatisfied
		/// </summary>
		/// <param name="language">ILanguage</param>
		/// <param name="updateAction">Action to call on active and update, the argument is the property name</param>
		/// <param name="run">default the action is run when defining, specify false if this is not wanted</param>
		/// <returns>an IDisposable, calling Dispose on this will stop everything</returns>
		public static IDisposable OnLanguageChanged(this ILanguage language, Action<ILanguage> updateAction, bool run = true)
		{
			if (language == null)
			{
				throw new ArgumentNullException(nameof(language));
			}
			if (updateAction == null)
			{
				throw new ArgumentNullException(nameof(updateAction));
			}
			if (run)
			{
				updateAction(language);
			}
			var observable = Observable.FromEventPattern<EventHandler<EventArgs>, EventArgs>(
							   h => language.LanguageChanged += h,
							   h => language.LanguageChanged -= h);

			return observable.Subscribe(pce => updateAction(pce.Sender as ILanguage));
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
			var observable = Observable.FromEventPattern<EventHandler<IniSectionEventArgs>, IniSectionEventArgs>(
							   h => iniSection.Saved += h,
							   h => iniSection.Saved -= h);
			return observable.Subscribe(pce => eventAction(pce.EventArgs));
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
			var observable = Observable.FromEventPattern<EventHandler<IniSectionEventArgs>, IniSectionEventArgs>(
				   h => iniSection.Saving += h,
				   h => iniSection.Saving -= h);
			return observable.Subscribe(pce => eventAction(pce.EventArgs));
		}
	}
}
