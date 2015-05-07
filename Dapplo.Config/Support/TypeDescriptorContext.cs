using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapplo.Config.Support {
	[ImmutableObject(true)]
	public sealed class TypeDescriptorContext : ITypeDescriptorContext {
		private readonly object component;
		private readonly PropertyDescriptor property;
		public TypeDescriptorContext(object component, PropertyDescriptor property) {
			this.component = component;
			this.property = property;
		}
		IContainer ITypeDescriptorContext.Container {
			get { return null; }
		}

		object ITypeDescriptorContext.Instance {
			get { return component; }
		}

		void ITypeDescriptorContext.OnComponentChanged() {
		}

		bool ITypeDescriptorContext.OnComponentChanging() {
			return true;
		}

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor {
			get { return property; }
		}

		object IServiceProvider.GetService(Type serviceType) {
			return null;
		}
	}
}
