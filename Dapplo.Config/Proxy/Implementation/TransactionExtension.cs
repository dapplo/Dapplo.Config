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

using System.Collections.Generic;
using Dapplo.Config.Support;

namespace Dapplo.Config.Proxy.Implementation
{
	/// <summary>
	///     This implements logic to add transactional support to your proxied interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (ITransactionalProperties))]
	internal class TransactionExtension<T> : AbstractPropertyProxyExtension<T>
	{
		// A store for the values that are set during the transaction
		private readonly IDictionary<string, object> _transactionProperties = new NonStrictLookup<object>();
		// This boolean has the value true if we are currently in a transaction
		private bool _inTransaction;

		public TransactionExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof (ITransactionalProperties));
			proxy.RegisterSetter((int) CallOrder.First, TransactionalSetter);
			proxy.RegisterGetter((int) CallOrder.First, TransactionalGetter);

			// Use Lambdas to make refactoring possible
			proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ITransactionalProperties>(x => x.StartTransaction()), StartTransaction);
			proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ITransactionalProperties>(x => x.CommitTransaction()), CommitTransaction);
			proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ITransactionalProperties>(x => x.RollbackTransaction()), RollbackTransaction);
			proxy.RegisterMethod(ExpressionExtensions.GetMemberName<ITransactionalProperties>(x => x.IsTransactionDirty()), IsTransactionDirty);
		}

		/// <summary>
		///     This is the implementation of the set logic
		/// </summary>
		/// <param name="setInfo">SetInfo with all the information on the set call</param>
		private void TransactionalSetter(SetInfo setInfo)
		{
			if (_inTransaction)
			{
				object oldValue;
				if (_transactionProperties.TryGetValue(setInfo.PropertyName, out oldValue))
				{
					_transactionProperties[setInfo.PropertyName] = setInfo.NewValue;
					setInfo.OldValue = oldValue;
					setInfo.HasOldValue = true;
				}
				else
				{
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
		private void TransactionalGetter(GetInfo getInfo)
		{
			if (_inTransaction)
			{
				// Get the value from the dictionary
				object value;
				if (_transactionProperties.TryGetValue(getInfo.PropertyName, out value))
				{
					getInfo.Value = value;
					getInfo.CanContinue = false;
				}
			}
		}

		/// <summary>
		///     This returns true if we have set (changed) values during a transaction
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void IsTransactionDirty(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = _inTransaction && _transactionProperties.Count > 0;
		}

		/// <summary>
		///     Logic to start the transaction, any setter used after this will be in the transaction
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void StartTransaction(MethodCallInfo methodCallInfo)
		{
			_inTransaction = true;
		}

		/// <summary>
		///     Logic to rollback the transaction, all values set (changed) after starting the transaction will be cleared
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void RollbackTransaction(MethodCallInfo methodCallInfo)
		{
			if (_inTransaction)
			{
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
		private void CommitTransaction(MethodCallInfo methodCallInfo)
		{
			if (_inTransaction)
			{
				Proxy.Properties = _transactionProperties;
				_transactionProperties.Clear();
				_inTransaction = false;
			}
			// TODO: Throw exception if we are not inside a transaction?
		}
	}
}