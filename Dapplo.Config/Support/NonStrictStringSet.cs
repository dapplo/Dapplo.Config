/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015-2016 Dapplo
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dapplo.Config.Support
{
	/// <summary>
	/// This from HashSet inherited class has non strict items, meaning that every string is converted before added or checked:
	/// This is done by turning it to lowercase and removing everything that is not a character or digit.
	/// </summary>
	public class NonStrictStringSet : ISet<string>
	{
		readonly ISet<string> _base = new HashSet<string>();

		public int Count
		{
			get
			{
				return _base.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return _base.IsReadOnly;
			}
		}

		public bool Contains(string item)
		{
			return _base.Contains(item.Cleanup());
		}

		public bool Add(string item)
		{
			return _base.Add(item.Cleanup());
		}

		public void UnionWith(IEnumerable<string> other)
		{
			_base.UnionWith(other.Select(x => x.Cleanup()).ToArray());
		}

		public void IntersectWith(IEnumerable<string> other)
		{
			_base.IntersectWith(other.Select(x => x.Cleanup()).ToArray());
		}

		public void ExceptWith(IEnumerable<string> other)
		{
			_base.ExceptWith(other.Select(x => x.Cleanup()).ToArray());
		}

		public void SymmetricExceptWith(IEnumerable<string> other)
		{
			_base.SymmetricExceptWith(other.Select(x => x.Cleanup()).ToArray());
		}

		public bool IsSubsetOf(IEnumerable<string> other)
		{
			return _base.IsSubsetOf(other.Select(x => x.Cleanup()).ToArray());
		}

		public bool IsSupersetOf(IEnumerable<string> other)
		{
			return _base.IsSupersetOf(other.Select(x => x.Cleanup()).ToArray());
		}

		public bool IsProperSupersetOf(IEnumerable<string> other)
		{
			return _base.IsProperSupersetOf(other.Select(x => x.Cleanup()).ToArray());
		}

		public bool IsProperSubsetOf(IEnumerable<string> other)
		{
			return _base.IsProperSubsetOf(other.Select(x => x.Cleanup()).ToArray());
		}

		public bool Overlaps(IEnumerable<string> other)
		{
			return _base.Overlaps(other.Select(x => x.Cleanup()).ToArray());
		}

		public bool SetEquals(IEnumerable<string> other)
		{
			return _base.SetEquals(other.Select(x => x.Cleanup()).ToArray());
		}

		void ICollection<string>.Add(string item)
		{
			_base.Add(item.Cleanup());
		}

		public void Clear()
		{
			_base.Clear();
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			_base.CopyTo(array.Select( x => x.Cleanup()).ToArray(), arrayIndex);
		}

		public bool Remove(string item)
		{
			return _base.Remove(item.Cleanup());
		}

		public IEnumerator<string> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _base.GetEnumerator();
		}
	}
}
