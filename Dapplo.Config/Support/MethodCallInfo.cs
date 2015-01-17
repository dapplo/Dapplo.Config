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

namespace Dapplo.Config.Support {
	/// <summary>
	///     This container holds all the information that is needed for extending the proxy with a method call
	/// </summary>
	public class MethodCallInfo {
		public string MethodName {
			get;
			set;
		}

		public object[] Arguments {
			get;
			set;
		}

		public object ReturnValue {
			get;
			set;
		}

		public Exception Error {
			get;
			set;
		}

		public bool HasError {
			get {
				return Error != null;
			}
		}
	}
}