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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Dapplo.Config;
using Dapplo.Ini.Extensions;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

namespace Dapplo.Ini.NewImpl
{
    /// <summary>
    /// This is the base class for an IniSection, it bases on the ConfigurationBase and should be used as the base for an ini-section.
    /// </summary>
    /// <typeparam name="T">The interface which this configuration implements</typeparam>
    public class IniSectionBase<T> : DictionaryConfigurationBase<T>, IIniSection
    {
        private readonly IDictionary<string, IniValue> _iniValues = new Dictionary<string, IniValue>(AbcComparer.Instance);
        private readonly IniSectionAttribute _iniSectionAttribute;
        private readonly DescriptionAttribute _descriptionAttribute;

        /// <summary>
        /// Constructor for the IniSection based objects
        /// </summary>
        public IniSectionBase()
        {
            var thisType = GetType();
            _iniSectionAttribute = thisType.GetAttribute<IniSectionAttribute>();
            _descriptionAttribute = thisType.GetAttribute<DescriptionAttribute>();
        }

        #region Overrides of ConfigurationBase<T>

        /// <inheritdoc />
        protected override void PropertyInitializer(PropertyInfo propertyInfo)
        {
            base.PropertyInitializer(propertyInfo);

            var iniValue = new IniValue(this)
            {
                PropertyName = propertyInfo.Name,
                ValueType = propertyInfo.PropertyType,
                IniPropertyName = propertyInfo.GetDataMemberName() ?? propertyInfo.Name,
                EmitDefaultValue = propertyInfo.GetEmitDefaultValue(),
                Description = propertyInfo.GetDescription(),
                Converter = propertyInfo.GetTypeConverter(),
                Category = propertyInfo.GetCategory(),
                Behavior = propertyInfo.GetIniPropertyBehavior(),
                DefaultValue = propertyInfo.GetDefaultValue()
            };
            if (!iniValue.Behavior.IsIgnoreErrorsSet && _iniSectionAttribute != null)
            {
                iniValue.Behavior.IgnoreErrors = _iniSectionAttribute.IgnoreErrors;
            }
            _iniValues[iniValue.PropertyName] = iniValue;
        }

        #endregion

        /// <summary>
        ///     This is called after the loading of the IniSection is finished and can be used to modify certain values before they are being used.
        /// </summary>
        public virtual void AfterLoad() { }

        /// <summary>
        ///     This is called after the saving of the IniSection is finished and can be used to modify certain values
        /// </summary>
        public virtual void AfterSave() { }

        /// <summary>
        ///     This is called before the saving of the IniSection is started and can be used to modify certain values
        /// </summary>
        public virtual void BeforeSave() { }

        /// <inheritdoc />
        public IniValue GetIniValue(string propertyName)
        {
            if (_iniValues.TryGetValue(propertyName, out var value))
            {
                return value;
            }
            return null;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IniValue> GetIniValues()
        {
            return new ReadOnlyDictionary<string, IniValue>(_iniValues);
        }

        /// <inheritdoc />
        public string GetSectionDescription() => _descriptionAttribute?.Description;

        /// <inheritdoc />
        public string GetSectionName() => _iniSectionAttribute?.Name;

        /// <inheritdoc />
        public bool TryGetIniValue(string propertyName, out IniValue value)
        {
            return _iniValues.TryGetValue(propertyName, out value);
        }
    }
}
