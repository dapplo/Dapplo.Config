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
using System.Collections.Generic;
using Dapplo.Config.Attributes;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.ConfigExtensions
{
    /// <summary>
    /// This implements the ITransactionalProperties logic
    /// </summary>
    internal class WriteProtectProperties<TProperty> : IWriteProtectProperties, IConfigExtension<TProperty>
    {
        private readonly ConfigurationBase<TProperty> _parent;
        // A store for the values that are write protected
        private readonly ISet<string> _writeProtectedProperties = new HashSet<string>(AbcComparer.Instance);
        private bool _isProtecting;

        public WriteProtectProperties(ConfigurationBase<TProperty> parent)
        {
            _parent = parent;
        }

        /// <inheritdoc />
        public int GetOrder { get; } = -1;

        /// <inheritdoc />
        public int SetOrder { get; } = (int) SetterOrders.WriteProtect;

        /// <inheritdoc />
        public void Getter(GetInfo<TProperty> getInfo)
        {
            // There is no getter, this should not be called when the getter order is negative
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This is the implementation of the set logic
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information on the set call</param>
        public void Setter(SetInfo<TProperty> setInfo)
        {
            if (_writeProtectedProperties.Contains(setInfo.PropertyInfo.Name))
            {
                setInfo.CanContinue = false;
                throw new AccessViolationException($"Property {setInfo.PropertyInfo.Name} is write protected");
            }
            if (_isProtecting)
            {
                _writeProtectedProperties.Add(setInfo.PropertyInfo.Name);
            }
        }

        /// <inheritdoc />
        public void DisableWriteProtect(string propertyName)
        {
            _writeProtectedProperties.Remove(propertyName);
        }

        /// <inheritdoc />
        public bool IsWriteProtected(string propertyName)
        {
            return _writeProtectedProperties.Contains(propertyName);
        }

        /// <inheritdoc />
        public void RemoveWriteProtection()
        {
            _isProtecting = false;
            _writeProtectedProperties.Clear();
        }

        /// <inheritdoc />
        public void StartWriteProtecting()
        {
            _isProtecting = true;
        }

        /// <inheritdoc />
        public void StopWriteProtecting()
        {
            _isProtecting = false;
        }

        /// <inheritdoc />
        public void WriteProtect(string propertyName)
        {
            _writeProtectedProperties.Add(propertyName);
        }
    }
}
