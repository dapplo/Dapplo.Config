// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
