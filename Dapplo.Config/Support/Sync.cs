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
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.Config.Support
{
	/// <summary>
	/// A simple helper to synchronize async code
	/// Usage:
	/// private readonly SemaphoreSlim _sync = new SemaphoreSlim(1);
	/// using (await Sync.WaitAsync(_sync).ConfigureAwait(false)) {
	///		// Do your stuff
	///	}
	/// </summary>
	public class Sync : IDisposable
	{
		private readonly SemaphoreSlim _syncSemaphore;

		public static async Task<Sync> WaitAsync(SemaphoreSlim syncSemaphore, CancellationToken token = default(CancellationToken))
		{
			await syncSemaphore.WaitAsync(token).ConfigureAwait(false);
			return new Sync(syncSemaphore);
		}

		private Sync(SemaphoreSlim syncSemaphore)
		{
			_syncSemaphore = syncSemaphore;
		}

		public void Dispose()
		{
			_syncSemaphore.Release();
		}
	}
}