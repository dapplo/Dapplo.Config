//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

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