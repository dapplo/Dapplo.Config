// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Interfaces
{
	/// <summary>
	///     Extending the to be property interface with this, adds transactional support
	/// </summary>
	public interface ITransactionalProperties
	{
		/// <summary>
		///     Apply the stored changes from the cache to the property object
		/// </summary>
		void CommitTransaction();

		/// <summary>
		///     Check if there are changes pending
		/// </summary>
		/// <returns>true when there are changes</returns>
		bool IsTransactionDirty();

		/// <summary>
		///     Cancel the transaction, this will clear the stored changes
		/// </summary>
		void RollbackTransaction();

		/// <summary>
		///     This method will start the transaction, all changes will be stored in a separate cache.
		/// </summary>
		void StartTransaction();
	}
}