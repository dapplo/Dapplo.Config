/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2014 Robin Krom
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
using System.Linq;
using Dapplo.Config.Extensions;
using Dapplo.Config.Support;

namespace Dapplo.Config.Extensions {
	/// <summary>
	///     This implements logic to add transactional support to your proxied interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (ITransactionalProperties))]
	public class TransactionExtension<T> : IPropertyProxyExtension<T> {
		private readonly IPropertyProxy<T> _proxy;
		// A store for the values that are set during the transaction
		private readonly IDictionary<string, object> _transactionProperties = new Dictionary<string, object>();
		// This boolean has the value true if we are currently in a transaction
		private bool _inTransaction;

		public TransactionExtension(IPropertyProxy<T> proxy) {
			_proxy = proxy;
			if (!typeof (T).GetInterfaces().Contains(typeof (ITransactionalProperties))) {
				throw new NotSupportedException("Type needs to implement ITransactionalProperties");
			}
			proxy.RegisterSetter(int.MinValue, TransactionalSetter);
			proxy.RegisterGetter(int.MinValue, TransactionalGetter);
			proxy.RegisterMethod("StartTransaction", StartTransaction);
			proxy.RegisterMethod("CommitTransaction", CommitTransaction);
			proxy.RegisterMethod("RollbackTransaction", RollbackTransaction);
			proxy.RegisterMethod("IsTransactionDirty", IsTransactionDirty);
		}

		/// <summary>
		///     This is the implementation of the set logic
		/// </summary>
		/// <param name="setInfo">SetInfo with all the information on the set call</param>
		private void TransactionalSetter(SetInfo setInfo) {
			if (_inTransaction) {
				object oldValue;
				if (_transactionProperties.TryGetValue(setInfo.PropertyName, out oldValue)) {
					_transactionProperties[setInfo.PropertyName] = setInfo.NewValue;
					setInfo.OldValue = oldValue;
					setInfo.HasOldValue = true;
				} else {
					_transactionProperties.Add(setInfo.PropertyName, setInfo.NewValue);
					setInfo.OldValue = null;
					setInfo.HasOldValue = false;
				}
				// No more (prevents NPC)
				setInfo.CanContinue = false;
			}
		}

		/// <summary>
		///     This is the implementation of the getter logic for a transactional proxy
		/// </summary>
		/// <param name="getInfo">GetInfo with all the information on the get call</param>
		private void TransactionalGetter(GetInfo getInfo) {
			if (_inTransaction) {
				// Get the value from the dictionary
				object value;
				if (_transactionProperties.TryGetValue(getInfo.PropertyName, out value)) {
					getInfo.Value = value;
					getInfo.CanContinue = false;
				}
			}
		}

		/// <summary>
		///     This returns true if we have set (changed) values during a transaction
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void IsTransactionDirty(MethodCallInfo methodCallInfo) {
			methodCallInfo.ReturnValue = _inTransaction && _transactionProperties.Count > 0;
		}

		/// <summary>
		///     Logic to start the transaction, any setter used after this will be in the transaction
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void StartTransaction(MethodCallInfo methodCallInfo) {
			_inTransaction = true;
		}

		/// <summary>
		///     Logic to rollback the transaction, all values set (changed) after starting the transaction will be cleared
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void RollbackTransaction(MethodCallInfo methodCallInfo) {
			if (_inTransaction) {
				_transactionProperties.Clear();
				_inTransaction = false;
			}
			// TODO: Throw exception if we are not inside a transaction?
		}

		/// <summary>
		///     Logic to commit the transaction, all values set (changed) after starting the transaction are stored in the proxy
		///     property store
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void CommitTransaction(MethodCallInfo methodCallInfo) {
			if (_inTransaction) {
				_proxy.SetProperties(_transactionProperties);
				_transactionProperties.Clear();
				_inTransaction = false;
			}
			// TODO: Throw exception if we are not inside a transaction?
		}
	}
}