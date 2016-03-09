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

namespace Dapplo.Config.Proxy
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