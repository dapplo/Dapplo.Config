using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapplo.Ini
{
    /// <summary>
    /// This specifies the configuration for the IniFile
    /// </summary>
    public class IniFileConfig
    {
        /// <summary>
        /// The name of the application, used for the directory
        /// </summary>
        public string ApplicationName { get; internal set; }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// Specify a path if you don't want to use the default loading
        /// </summary>
        public string FixedDirectory { get; internal set; }

        /// <summary>
        /// 0 to disable or the amount of milliseconds that pending changes are written
        /// </summary>
        public uint AutoSaveInterval { get; internal set; } = 1000;

        /// <summary>
        /// True to enable file system watching
        /// </summary>
        public bool WatchFileChanges { get; internal set; } = true;

        /// <summary>
        /// True to save the changes on exit
        /// </summary>
        public bool SaveOnExit { get; internal set; } = true;

        /// <summary>
        /// The postfix for the defaults file
        /// </summary>
        public string DefaultsPostfix { get; internal set; }

        /// <summary>
        /// The postfix for the constants file
        /// </summary>
        public string ContantsPostfix { get; internal set; }

        /// <summary>
        /// The extension for the ini file
        /// </summary>
        public string IniExtension { get; internal set; }

        /// <summary>
        /// Specify what file encoding needs to be used, default is Encoding.UTF8
        /// </summary>
        public Encoding FileEncoding { get; internal set; } = Encoding.UTF8;
    }
}
