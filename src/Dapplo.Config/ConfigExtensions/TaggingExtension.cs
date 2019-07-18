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
using System.Reflection;
using Dapplo.Config.Attributes;
using Dapplo.Config.Extensions;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.ConfigExtensions
{
    /// <summary>
    /// Implements the ITagging functionality
    /// </summary>
    /// <typeparam name="TProperty">Type for the property</typeparam>
    internal class TaggingExtension<TProperty> : ITagging, IConfigExtension<TProperty>
    {
        private readonly ConfigurationBase<TProperty> _parent;
        // The set of tagged properties
        private readonly Dictionary<string, Dictionary<object, object>> _taggedProperties = new Dictionary<string, Dictionary<object, object>>(new AbcComparer());

        public TaggingExtension(ConfigurationBase<TProperty> parent)
        {
            _parent = parent;
            // Give extended classes a way to initialize
            foreach (var propertyInfo in parent.PropertiesInformation.PropertyInfos.Values)
            {
                // In theory we could check if the typeToInitializeFor extends ITagging
                InitProperty(propertyInfo);
            }
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
        ///     Process the property, in our case get the tags
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        private void InitProperty(PropertyInfo propertyInfo)
        {
            foreach (var tagAttribute in propertyInfo.GetAttributes<TagAttribute>())
            {
                if (!_taggedProperties.TryGetValue(propertyInfo.Name, out var tags))
                {
                    tags = new Dictionary<object, object>();
                    _taggedProperties.Add(propertyInfo.Name, tags);
                }
                tags[tagAttribute.Tag] = tagAttribute.TagValue;
            }
        }

        /// <inheritdoc />
        public object GetTagValue(string propertyName, object tag)
        {
            if (!_taggedProperties.TryGetValue(propertyName, out var tags))
            {
                return null;
            }
            var hasTag = tags.ContainsKey(tag);
            object returnValue = null;
            if (hasTag)
            {
                returnValue = tags[tag];
            }
            return returnValue;
        }

        /// <inheritdoc />
        public bool IsTaggedWith(string propertyName, object tag)
        {
            if (_taggedProperties.TryGetValue(propertyName, out var tags))
            {
                return tags.ContainsKey(tag);
            }

            return false;
        }
    }
}
