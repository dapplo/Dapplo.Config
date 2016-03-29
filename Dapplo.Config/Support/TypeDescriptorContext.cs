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

using System;
using System.ComponentModel;

#endregion

namespace Dapplo.Config.Support
{
	[ImmutableObject(true)]
	public sealed class TypeDescriptorContext : ITypeDescriptorContext
	{
		private readonly object _component;
		private readonly PropertyDescriptor _property;

		public TypeDescriptorContext(object component, PropertyDescriptor property)
		{
			_component = component;
			_property = property;
		}

		IContainer ITypeDescriptorContext.Container => null;

		object ITypeDescriptorContext.Instance => _component;

		void ITypeDescriptorContext.OnComponentChanged()
		{
		}

		bool ITypeDescriptorContext.OnComponentChanging()
		{
			return true;
		}

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => _property;

		object IServiceProvider.GetService(Type serviceType)
		{
			return null;
		}
	}
}