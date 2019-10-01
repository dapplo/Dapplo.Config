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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Dapplo.Config.Ini.Extensions;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Extensions;

namespace Dapplo.Config.Ini
{
    /// <summary>
    /// This is the base class for an IniSection, it bases on the ConfigurationBase and should be used as the base for an ini-section.
    /// </summary>
    /// <typeparam name="TInterface">The interface which this configuration implements</typeparam>
    public class IniSection<TInterface> : DictionaryConfiguration<TInterface, object>, IIniSectionInternal where TInterface : IIniSection
    {
        private readonly IDictionary<string, IniValue> _iniValues = new Dictionary<string, IniValue>(AbcComparer.Instance);
        private readonly IniSectionAttribute _iniSectionAttribute = typeof(TInterface).GetAttribute<IniSectionAttribute>();
        private readonly DescriptionAttribute _descriptionAttribute = typeof(TInterface).GetAttribute<DescriptionAttribute>();

        /// <summary>
        /// Factory for IniSection implementations
        /// </summary>
        /// <returns>TInterface</returns>
        public static TInterface Create()
        {
            return ConfigProxy.Create<TInterface>(new IniSection<TInterface>());
        }

        /// <summary>
        /// Constructor for the IniSection based objects
        /// </summary>
        protected IniSection()
        {
        }

        #region Overrides of ConfigurationBase<T>

        /// <inheritdoc />
        protected override void PropertyInitializer(PropertyInfo propertyInfo)
        {
            base.PropertyInitializer(propertyInfo);

            // ReSharper disable once SuspiciousTypeConversion.Global
            var iniValue = new IniValue((IIniSection)Proxy, propertyInfo)
            {
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

        public Action<IIniSection> OnAfterLoad { get; private set; }
        public Action<IIniSection> OnAfterSave { get; private set; }
        public Action<IIniSection> OnBeforeSave { get; private set; }

        /// <inheritdoc cref="IIniSection"/>
        public void RegisterAfterLoad(Action<IIniSection> onAfterLoad)
        {
            OnAfterLoad += onAfterLoad;
        }

        /// <inheritdoc cref="IIniSection"/>
        public void RegisterAfterSave(Action<IIniSection> onAfterSave)
        {
            OnAfterSave += onAfterSave;
        }

        /// <inheritdoc cref="IIniSection"/>
        public void RegisterBeforeSave(Action<IIniSection> onBeforeSave)
        {
            OnBeforeSave += onBeforeSave;
        }

        /// <inheritdoc cref="IIniSection"/>
        public IniValue GetIniValue(string propertyName)
        {
            if (_iniValues.TryGetValue(propertyName, out var value))
            {
                return value;
            }
            return null;
        }

        /// <inheritdoc cref="IIniSection"/>
        public IReadOnlyDictionary<string, IniValue> GetIniValues()
        {
            return new ReadOnlyDictionary<string, IniValue>(_iniValues);
        }

        /// <inheritdoc cref="IIniSection"/>
        public string GetSectionDescription() => _descriptionAttribute?.Description;

        /// <inheritdoc cref="IIniSection"/>
        public string GetSectionName() => _iniSectionAttribute?.Name;

        /// <inheritdoc cref="IIniSection"/>
        public bool TryGetIniValue(string propertyName, out IniValue value)
        {
            return _iniValues.TryGetValue(propertyName, out value);
        }
    }
}
