using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapplo.Config.Language.ConfigExtensions
{
    public class LanguageExtension<TProperty> : ILanguage
    {
        private readonly ConfigurationBase<TProperty> _parent;
        private readonly Dictionary<string, string> _translationsWithoutProperty = new Dictionary<string, string>(AbcComparer.Instance);
        private readonly LanguageAttribute _languageAttribute;

        public LanguageExtension(ConfigurationBase<TProperty> parent)
        {
            _parent = parent;
            _languageAttribute = parent.ProxyFor.GetCustomAttribute<LanguageAttribute>();
        }

        /// <summary>
        ///     Get the value for a property.
        /// Note: This needs to be virtual otherwise the interface isn't implemented
        /// </summary>
        /// <param name="propertyName">string with propertyName for the property to get</param>
        /// <returns>object or null if not available</returns>
        public string this[string propertyName]
        {
            get => (string)_parent.Getter(propertyName);
            set => _parent.Setter(propertyName, value);
        }

        /// <summary>
        ///     This event is triggered after the language has been changed
        /// </summary>
        public event EventHandler<EventArgs> LanguageChanged;

        /// <inheritdoc />
        public bool TryGetTranslation(string languageKey, out string translation)
        {
            if (_parent.TryGetPropertyInfoFor(languageKey, out var propertyInfo))
            {
                translation = _parent.GetValue(propertyInfo).Value as string;
                return true;
            }

            // This property is not backed by a Property, use the local dictionary
            if (_translationsWithoutProperty.TryGetValue(languageKey, out translation))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="languageKey"></param>
        /// <param name="translation"></param>
        public void SetPropertyFreeTranslation(string languageKey, string translation)
        {
            // Store values which do not have a property
            _translationsWithoutProperty[languageKey] = translation;
        }

        /// <inheritdoc />
        public IEnumerable<string> Keys()
        {
            return _parent.PropertyNames().Concat(_translationsWithoutProperty.Keys);
        }

        /// <inheritdoc />
        public IEnumerable<string> PropertyFreeKeys()
        {
            return _translationsWithoutProperty.Keys;
        }

        /// <inheritdoc />
        public string PrefixName()
        {
            return _languageAttribute?.Prefix ?? _parent.ProxyFor.Name;
        }

        /// <summary>
        ///     Generate a LanguageChanged event, this is from ILanguageInternal
        ///     Added for Dapplo.Config/issues/10
        /// </summary>
        public void OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, new EventArgs());
        }

    }
}
