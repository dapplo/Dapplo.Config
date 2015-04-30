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

namespace Dapplo.Config.Extension {
	/// <summary>
	///     Extending the to be property interface with this, adds transactional support
	/// </summary>
	public interface ITransactionalProperties {
		/// <summary>
		/// This method will start the transaction, all changes will be stored in a separate cache.
		/// </summary>
		void StartTransaction();

		/// <summary>
		/// Apply the stored changes from the cache to the property object
		/// </summary>
		void CommitTransaction();

		/// <summary>
		/// Cancel the transaction, this will clear the stored changes
		/// </summary>
		void RollbackTransaction();

		/// <summary>
		/// Check if there are changes pending
		/// </summary>
		/// <returns>true wenn es stored changes gibt</returns>
		bool IsTransactionDirty();
	}
}