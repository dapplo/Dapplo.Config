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
using System.Text;

namespace Dapplo.Config.Language
{
    /// <summary>
    /// This specifies the configuration for the language container
    /// </summary>
    public class LanguageConfig
    {
        /// <summary>
        /// The name of the application, used for the directory
        /// </summary>
        public string ApplicationName { get; internal set; }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileNamePattern { get; internal set; } = @"language(_(?<module>[a-zA-Z0-9]*))?-(?<IETF>[a-zA-Z]{2}(-[a-zA-Z]+)?-[a-zA-Z]+)\.(ini|xml)";

        /// <summary>
        /// Specify a path if you don't want to use the default loading
        /// </summary>
        public ICollection<string> SpecifiedDirectories { get; } = new List<string>();

        /// <summary>
        /// The default language to take
        /// </summary>
        public string DefaultLanguage { get; internal set; } = "en-US";

        /// <summary>
        /// Check in the startup directory for language files?
        /// </summary>
        public bool CheckStartupDirectory { get; internal set; } = true;

        /// <summary>
        /// Check in the AppData directory for language files?
        /// </summary>
        public bool CheckAppDataDirectory { get; internal set; } = true;

        /// <summary>
        /// Specify what file encoding needs to be used, default is Encoding.UTF8
        /// </summary>
        public Encoding FileEncoding { get; internal set; } = Encoding.UTF8;
    }
}
