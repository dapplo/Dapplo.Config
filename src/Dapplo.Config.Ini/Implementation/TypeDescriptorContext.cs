// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace Dapplo.Config.Ini.Implementation
{
	/// <summary>
	///     Used internally for type conversion
	/// </summary>
	internal sealed class TypeDescriptorContext : ITypeDescriptorContext
	{
		private readonly object _component;
		private readonly PropertyDescriptor _property;

		/// <inheritdoc />
		public TypeDescriptorContext(object component, PropertyDescriptor property)
		{
			_component = component;
			_property = property;
		}

		/// <inheritdoc />
		IContainer ITypeDescriptorContext.Container => null;

		/// <inheritdoc />
		object ITypeDescriptorContext.Instance => _component;

		/// <inheritdoc />
		void ITypeDescriptorContext.OnComponentChanged()
		{
		}

		/// <inheritdoc />
		bool ITypeDescriptorContext.OnComponentChanging()
		{
			return true;
		}

		/// <inheritdoc />
		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => _property;

		/// <inheritdoc />
		object IServiceProvider.GetService(Type serviceType)
		{
			return null;
		}
	}
}