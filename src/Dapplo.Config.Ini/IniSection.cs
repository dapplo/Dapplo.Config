// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    public class IniSection<TInterface> : DictionaryConfigurationBase<TInterface, object>, IIniSection, IIniSectionInternal where TInterface : IIniSection
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

        /// <inheritdoc />
        protected override void PropertyInitializer(PropertyInfo propertyInfo)
        {
            base.PropertyInitializer(propertyInfo);

            var iniValue = new IniValue(this, propertyInfo)
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

        public Action<IIniSection> OnAfterLoad { get; private set; }
        public Action<IIniSection> OnAfterSave { get; private set; }
        public Action<IIniSection> OnBeforeSave { get; private set; }

        /// <inheritdoc />
        public void RegisterAfterLoad(Action<IIniSection> onAfterLoad)
        {
            OnAfterLoad += onAfterLoad;
        }

        /// <inheritdoc />
        public void RegisterAfterSave(Action<IIniSection> onAfterSave)
        {
            OnAfterSave += onAfterSave;
        }

        /// <inheritdoc />
        public void RegisterBeforeSave(Action<IIniSection> onBeforeSave)
        {
            OnBeforeSave += onBeforeSave;
        }

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
