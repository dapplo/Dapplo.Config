// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Interfaces
{
    /// <summary>
    /// The interface for the ShallowClone method.
    /// </summary>
    public interface IShallowCloneable
    {
        /// <summary>
        /// Make a memberwise clone of the object, this is "shallow".
        /// </summary>
        /// <returns>"Shallow" Cloned instance</returns>
        object ShallowClone();
    }

    /// <summary>
    /// The interface for the generic ShallowClone method.
    /// </summary>
    /// <typeparam name="T">Type of the copy which is returned</typeparam>
    public interface IShallowCloneable<out T> where T : class
    {
        /// <summary>
        /// Make a memberwise clone of the object, this is "shallow".
        /// </summary>
        /// <returns>"Shallow" Cloned instance of type T</returns>
        T ShallowClone();
    }
}