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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Dapplo.Config.Language
{
    /// <summary>
    /// This is a builder for the IniFileConfig
    /// </summary>
    public class LanguageConfigBuilder
    {
        private const string LanguageConfigAlreadyBuild = "The LanguageConfig was already build.";
        private readonly LanguageConfig _languageConfig = new LanguageConfig();

        /// <summary>
        /// True if the LanguageConfig was already build.
        /// </summary>
        public bool IsBuild { get; private set; }

        /// <summary>
        /// Private constructor, to prevent constructing this.
        /// Please use the Create factory method.
        /// </summary>
        private LanguageConfigBuilder()
        {
        }

        /// <summary>
        /// Factory
        /// </summary>
        /// <returns>LanguageConfigBuilder</returns>
        [Pure]
        public static LanguageConfigBuilder Create() => new LanguageConfigBuilder();

        /// <summary>
        /// Build or finalize the configuration, so it can be used
        /// </summary>
        /// <returns>LanguageConfig</returns>
        [Pure]
        public LanguageConfig BuildLanguageConfig()
        {
            if (IsBuild)
            {
                throw new NotSupportedException(LanguageConfigAlreadyBuild);
            }
            IsBuild = true;

            // Create an application name, if there is none
            if (string.IsNullOrEmpty(_languageConfig.ApplicationName))
            {
                using (var process = Process.GetCurrentProcess())
                {
                    _languageConfig.ApplicationName = process.ProcessName;
                }
            }

            // Assign all complex / builded values

            return _languageConfig;
        }

        /// <summary>
        /// Change the application name
        /// </summary>
        /// <param name="applicationName">string</param>
        /// <returns>LanguageConfigBuilder</returns>
        public LanguageConfigBuilder WithApplicationName(string applicationName)
        {
            if (IsBuild)
            {
                throw new NotSupportedException(LanguageConfigAlreadyBuild);
            }
            _languageConfig.ApplicationName = applicationName;
            return this;
        }


        /// <summary>
        /// Disable CheckAppDataDirectory
        /// </summary>
        /// <returns>LanguageConfigBuilder</returns>
        public LanguageConfigBuilder WithoutCheckAppDataDirectory()
        {
            if (IsBuild)
            {
                throw new NotSupportedException(LanguageConfigAlreadyBuild);
            }
            _languageConfig.CheckAppDataDirectory = false;
            return this;
        }

        /// <summary>
        /// Disable CheckStartupDirectory
        /// </summary>
        /// <returns>LanguageConfigBuilder</returns>
        public LanguageConfigBuilder WithoutCheckStartupDirectory()
        {
            if (IsBuild)
            {
                throw new NotSupportedException(LanguageConfigAlreadyBuild);
            }
            _languageConfig.CheckStartupDirectory = false;
            return this;
        }

        /// <summary>
        /// Specify the filename pattern to use
        /// </summary>
        /// <param name="filenamePattern">string</param>
        /// <returns>LanguageConfigBuilder</returns>
        public LanguageConfigBuilder WithFilenamePattern(string filenamePattern)
        {
            if (IsBuild)
            {
                throw new NotSupportedException(LanguageConfigAlreadyBuild);
            }
            _languageConfig.FileNamePattern = filenamePattern;
            return this;
        }


        /// <summary>
        /// Specify a directory if you want to read and write from a specific directory, instead of using the default logic.
        /// </summary>
        /// <param name="directories">strings</param>
        /// <returns>LanguageConfigBuilder</returns>
        public LanguageConfigBuilder WithSpecificDirectories(params string [] directories)
        {
            return WithSpecificDirectories(directories.ToList());
        }

        /// <summary>
        /// Specify a directory if you want to read and write from a specific directory, instead of using the default logic.
        /// </summary>
        /// <param name="directories">IEnumerable of string</param>
        /// <returns>LanguageConfigBuilder</returns>
        public LanguageConfigBuilder WithSpecificDirectories(IEnumerable<string> directories)
        {
            if (IsBuild)
            {
                throw new NotSupportedException(LanguageConfigAlreadyBuild);
            }

            foreach (var directory in directories)
            {
                _languageConfig.SpecifiedDirectories.Add(directory);
            }
            
            return this;
        }

        /// <summary>
        /// Specify the default language
        /// </summary>
        /// <param name="defaultLanguage">string</param>
        /// <returns>LanguageConfigBuilder</returns>
        public LanguageConfigBuilder WithDefaultLanguage(string defaultLanguage)
        {
            if (IsBuild)
            {
                throw new NotSupportedException(LanguageConfigAlreadyBuild);
            }
            _languageConfig.DefaultLanguage = defaultLanguage;
            return this;
        }
    }
}
