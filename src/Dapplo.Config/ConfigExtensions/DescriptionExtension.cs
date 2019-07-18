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

using System;
using Dapplo.Config.Extensions;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.ConfigExtensions
{
    /// <summary>
    /// Implements the IDescription functionality
    /// </summary>
    /// <typeparam name="TProperty">Type for the property</typeparam>
    internal class DescriptionExtension<TProperty, TInterface> : IDescription, IConfigExtension<TProperty>
    {
        private readonly ConfigurationBase<TProperty> _parent;

        public DescriptionExtension(ConfigurationBase<TProperty> parent)
        {
            _parent = parent;
        }

        /// <inheritdoc />
        public int GetOrder { get; } = -1;

        /// <inheritdoc />
        public int SetOrder { get; } = -1;

        /// <inheritdoc />
        public void Getter(GetInfo<TProperty> getInfo)
        {
            // This should never be called due to the get order being < 0
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Setter(SetInfo<TProperty> setInfo)
        {
            // This should never be called due to the set order being < 0
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Return the description for a property
        /// </summary>
        public string DescriptionFor(string propertyName)
        {
            return _parent.PropertyInfoFor(propertyName).GetDescription();
        }
    }
}
