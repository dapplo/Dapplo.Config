// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dapplo.Config.Attributes
{
    /// <summary>
    ///     This attribute should be used to mark a method as a getter, which in fact needs to be protected (or public)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
	public class GetSetInterceptorAttribute : Attribute
	{
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="order">The order of the getter method</param>
        /// <param name="isSetter">bool</param>
        public GetSetInterceptorAttribute(object order, bool isSetter = false)
		{
			Order = Convert.ToInt32(order);
			IsSetter = isSetter;
		}

        /// <summary>
        ///     Order for the getter
        /// </summary>
        public int Order { get; private set; }

		/// <summary>
		///     Is this interceptor a setter or getter
		/// </summary>
		public bool IsSetter { get; private set; }
	}
}