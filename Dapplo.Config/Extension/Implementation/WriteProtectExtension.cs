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

using System;
using System.Collections.Generic;
using Dapplo.Config.Support;

namespace Dapplo.Config.Extension.Implementation
{
	/// <summary>
	///     This implements logic to add write protect support to your proxied interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof(IWriteProtectProperties))]
	internal class WriteProtectExtension<T> : AbstractPropertyProxyExtension<T>
	{
		// A store for the values that are write protected
		private readonly ISet<string> _writeProtectedProperties = new HashSet<string>();
		private bool _isProtecting;

		public WriteProtectExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof(IWriteProtectProperties));
			proxy.RegisterSetter((int)CallOrder.First, WriteProtectSetter);

			// Use Lambdas to make refactoring possible
			proxy.RegisterMethod(ConfigUtils.GetMemberName<IWriteProtectProperties>(x => x.StartWriteProtecting()), StartWriteProtecting);
			proxy.RegisterMethod(ConfigUtils.GetMemberName<IWriteProtectProperties>(x => x.RemoveWriteProtection()), RemoveWriteProtection);
			proxy.RegisterMethod(ConfigUtils.GetMemberName<IWriteProtectProperties>(x => x.StopWriteProtecting()), StopWriteProtecting);
			proxy.RegisterMethod(ConfigUtils.GetMemberName<IWriteProtectProperties>(x => x.WriteProtect("")), WriteProtect);
			proxy.RegisterMethod(ConfigUtils.GetMemberName<IWriteProtectProperties>(x => x.DisableWriteProtect("")), DisableWriteProtect);
			proxy.RegisterMethod(ConfigUtils.GetMemberName<IWriteProtectProperties>(x => x.IsWriteProtected("")), IsWriteProtected);
		}

		/// <summary>
		///     This is the implementation of the set logic
		/// </summary>
		/// <param name="setInfo">SetInfo with all the information on the set call</param>
		private void WriteProtectSetter(SetInfo setInfo)
		{
			if (_writeProtectedProperties.Contains(setInfo.PropertyName))
			{
				setInfo.CanContinue = false;
				setInfo.Error = new AccessViolationException(string.Format("Property {0} is write protected", setInfo.PropertyName));
			}
			else if (_isProtecting)
			{
				_writeProtectedProperties.Add(setInfo.PropertyName);
			}
		}

		/// <summary>
		///     After calling this, every set will be write protected
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void StartWriteProtecting(MethodCallInfo methodCallInfo)
		{
			_isProtecting = true;
		}

		/// <summary>
		///     After calling this, nothing is write protected
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void RemoveWriteProtection(MethodCallInfo methodCallInfo)
		{
			_isProtecting = false;
			_writeProtectedProperties.Clear();
		}

		/// <summary>
		///     Stop write protecting every property
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void StopWriteProtecting(MethodCallInfo methodCallInfo)
		{
			_isProtecting = false;
		}

		/// <summary>
		///     IsWriteProtected logic checks if the supplied property Lambda expression is write protected.
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void IsWriteProtected(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = _writeProtectedProperties.Contains(methodCallInfo.PropertyNameOf(0));
		}

		/// <summary>
		///     WriteProtect sets the write protection of the supplied property in the LambdaExpression
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void WriteProtect(MethodCallInfo methodCallInfo)
		{
			_writeProtectedProperties.Add(methodCallInfo.PropertyNameOf(0));
		}

		/// <summary>
		///     DisableWriteProtect removes the write protection of the supplied property in the LambdaExpression
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void DisableWriteProtect(MethodCallInfo methodCallInfo)
		{
			_writeProtectedProperties.Remove(methodCallInfo.PropertyNameOf(0));
		}
	}
}