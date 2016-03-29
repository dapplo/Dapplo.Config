//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Config
// 
//  Dapplo.Config is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Config is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System.Collections.Generic;
using Dapplo.Config.Interceptor.Implementation;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Support;

#endregion

namespace Dapplo.Config.Interceptor.Extensions
{
	/// <summary>
	///     This implements logic to add transactional support to your proxied interface.
	/// </summary>
	[Extension(typeof (ITransactionalProperties))]
	internal class TransactionExtension : AbstractInterceptorExtension
	{
		// A store for the values that are set during the transaction
		private readonly IDictionary<string, object> _transactionProperties = new Dictionary<string, object>(AbcComparer.Instance);
		// This boolean has the value true if we are currently in a transaction
		private bool _inTransaction;

		/// <summary>
		/// Register methods and getter/setter
		/// </summary>
		public override void Initialize()
		{
			Interceptor.RegisterSetter((int) CallOrder.First, TransactionalSetter);
			Interceptor.RegisterGetter((int) CallOrder.First, TransactionalGetter);

			// Use Lambdas to make refactoring possible
			Interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<ITransactionalProperties>(x => x.StartTransaction()), StartTransaction);
			Interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<ITransactionalProperties>(x => x.CommitTransaction()), CommitTransaction);
			Interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<ITransactionalProperties>(x => x.RollbackTransaction()), RollbackTransaction);
			Interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<ITransactionalProperties>(x => x.IsTransactionDirty()), IsTransactionDirty);
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
				Interceptor.Properties = _transactionProperties;
				_transactionProperties.Clear();
				_inTransaction = false;
			}
			// TODO: Throw exception if we are not inside a transaction?
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
		///     Logic to start the transaction, any setter used after this will be in the transaction
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void StartTransaction(MethodCallInfo methodCallInfo)
		{
			_inTransaction = true;
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
	}
}