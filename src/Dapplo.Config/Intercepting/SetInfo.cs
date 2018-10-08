//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2018 Dapplo
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

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This provides the value for a set interceptor
    /// </summary>
    public class SetInfo<TProperty> : GetSetInfo
    {
        /// <summary>
        ///     Does property have an old value?
        /// </summary>
        public bool HasOldValue { get; set; }

        /// <summary>
        ///     The new value for the property
        /// </summary>
        public TProperty NewValue { get; set; }

        /// <summary>
        ///     The old value of the property, if any (see HasOldValue)
        /// </summary>
        public TProperty OldValue { get; set; }
    }
}
