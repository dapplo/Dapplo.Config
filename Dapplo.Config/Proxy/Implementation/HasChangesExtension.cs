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

using Dapplo.Config.Support;

namespace Dapplo.Config.Proxy.Implementation
{
	/// <summary>
	///     This implements logic to add change detection to your proxied interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (IHasChanges))]
	internal class HasChangesExtension<T> : AbstractPropertyProxyExtension<T>
	{
		// This boolean has the value true if we have changes sind the last "reset"
		private bool _hasChanges;

		public HasChangesExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof (IHasChanges));
			proxy.RegisterSetter((int) CallOrder.Last, HasChangesSetter);

			// Use Lambdas to make refactoring possible
			proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IHasChanges>(x => x.ResetHasChanges()), ResetHasChanges);
			proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IHasChanges>(x => x.HasChanges()), HasChanges);
		}

		/// <summary>
		///     This is the implementation of the set logic
		/// </summary>
		/// <param name="setInfo">SetInfo with all the information on the set call</param>
		private void HasChangesSetter(SetInfo setInfo)
		{
			_hasChanges = !setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue);
		}

		/// <summary>
		///     This returns true if we have set (changed) values
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void HasChanges(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = _hasChanges;
		}

		/// <summary>
		///     Reset the has changes flag
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void ResetHasChanges(MethodCallInfo methodCallInfo)
		{
			_hasChanges = false;
		}
	}
}