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
using System.Reflection;
using Dapplo.Config.Extensions;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Interfaces;
using Dapplo.Log;

namespace Dapplo.Config.ConfigExtensions
{
    /// <summary>
    /// Implements the IDefaultValue functionality
    /// </summary>
    /// <typeparam name="TProperty">Type for the property</typeparam>
    internal class DefaultValueExtension<TProperty> : IDefaultValue, IConfigExtension<TProperty>
    {
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly LogSource Log = new LogSource();
        private readonly ConfigurationBase<TProperty> _parent;

        public DefaultValueExtension(ConfigurationBase<TProperty> parent)
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

        /// <inheritdoc />
        public object DefaultValueFor(string propertyName)
        {
            return GetConvertedDefaultValue(_parent.PropertyInfoFor(propertyName));
        }

        /// <inheritdoc />
        public void RestoreToDefault(string propertyName)
        {
            var propertyInfo = _parent.PropertyInfoFor(propertyName);
            TProperty defaultValue;
            try
            {
                defaultValue = (TProperty)GetConvertedDefaultValue(propertyInfo);
            }
            catch (Exception ex)
            {
                Log.Warn().WriteLine(ex.Message);
                throw;
            }

            if (defaultValue != null)
            {
                _parent.SetValue(propertyInfo, defaultValue);
                return;
            }
            try
            {
                defaultValue = (TProperty)propertyInfo.PropertyType.CreateInstance();
                _parent.SetValue(propertyInfo, defaultValue);
            }
            catch (Exception ex)
            {
                // Ignore creating the default type, this might happen if there is no default constructor.
                Log.Warn().WriteLine(ex.Message);
            }
        }

        /// <summary>
        ///     Retrieve the default value, using the TypeConverter
        /// </summary>
        /// <param name="propertyInfo">Property to get the default value for</param>
        /// <returns>object with the type converted default value</returns>
        public static object GetConvertedDefaultValue(PropertyInfo propertyInfo)
        {
            var defaultValue = propertyInfo.GetDefaultValue();
            if (defaultValue == null)
            {
                return null;
            }

            var typeConverter = propertyInfo.GetTypeConverter();
            var targetType = propertyInfo.PropertyType;
            defaultValue = targetType.ConvertOrCastValueToType(defaultValue, typeConverter);
            return defaultValue;
        }
    }
}
