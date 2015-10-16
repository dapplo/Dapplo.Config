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
using System.Reflection;
using System.Linq;

namespace Dapplo.Config.Extension.Implementation
{
	/// <summary>
	/// Base class for extensions, this should take away some default handling
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class AbstractPropertyProxyExtension<T> : IPropertyProxyExtension
	{
		protected readonly IPropertyProxy<T> Proxy;

		protected AbstractPropertyProxyExtension(IPropertyProxy<T> proxy)
		{
			Proxy = proxy;
		}

		/// <summary>
		/// This returns 0, which means somewhere in the middle
		/// If an extension needs to be called last, it should override this and for example return int.MaxValue
		/// If an extension needs to be called first, it should override this and for example return int.MinValue
		/// </summary>
		public virtual int InitOrder
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// Force that the type extends the type we build an extension for
		/// </summary>
		/// <param name="extensionType"></param>
		protected void CheckType(Type extensionType)
		{
			if (!typeof (T).GetInterfaces().Contains(extensionType))
			{
				throw new NotSupportedException(string.Format("Type needs to implement {0}", extensionType.Name));
			}
		}

		/// <summary>
		/// Handle every property
		/// </summary>
		/// <param name="propertyInfo"></param>
		public virtual void InitProperty(PropertyInfo propertyInfo)
		{
		}

		/// <summary>
		/// After property initialization
		/// </summary>
		public virtual void AfterInitialization()
		{
		}
	}
}