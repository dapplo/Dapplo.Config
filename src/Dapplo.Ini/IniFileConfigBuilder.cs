using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Dapplo.Ini
{
    /// <summary>
    /// This is a builder for the IniFileConfig
    /// </summary>
    public class IniFileConfigBuilder
    {
        private const string IniFileConfigAlreadyBuild = "The IniFileConfig was already build.";
        private const string Defaults = "-defaults";
        private const string Constants = "-constants";
        private const string IniExtension = "ini";
        private readonly IniFileConfig _iniFileConfig = new IniFileConfig();

        /// <summary>
        /// True if the ApplicationConfig was already build.
        /// </summary>
        public bool IsBuild { get; private set; }

        /// <summary>
        /// Private constructor, to prevent constructing this.
        /// Please use the Create factory method.
        /// </summary>
        private IniFileConfigBuilder()
        {
        }

        /// <summary>
        /// Factory
        /// </summary>
        /// <returns>IniFileConfigBuilder</returns>
        [Pure]
        public static IniFileConfigBuilder Create() => new IniFileConfigBuilder();

        /// <summary>
        /// Build or finalize the configuration, so it can be used
        /// </summary>
        /// <returns>ApplicationConfig</returns>
        [Pure]
        public IniFileConfig BuildApplicationConfig()
        {
            if (IsBuild)
            {
                throw new NotSupportedException(IniFileConfigAlreadyBuild);
            }
            IsBuild = true;

            // Create an application name, if there is none
            if (string.IsNullOrEmpty(_iniFileConfig.ApplicationName))
            {
                using (var process = Process.GetCurrentProcess())
                {
                    _iniFileConfig.ApplicationName = process.ProcessName;
                }
            }

            if (string.IsNullOrEmpty(_iniFileConfig.FileName))
            {
                _iniFileConfig.FileName = _iniFileConfig.ApplicationName;
            }

            if (string.IsNullOrEmpty(_iniFileConfig.ContantsPostfix))
            {
                _iniFileConfig.ContantsPostfix = Constants;
            }
            if (string.IsNullOrEmpty(_iniFileConfig.DefaultsPostfix))
            {
                _iniFileConfig.DefaultsPostfix = Defaults;
            }
            if (string.IsNullOrEmpty(_iniFileConfig.IniExtension))
            {
                _iniFileConfig.IniExtension = IniExtension;
            }

            // Assign all complex / builded values

            return _iniFileConfig;
        }

        /// <summary>
        /// Change the application name
        /// </summary>
        /// <param name="applicationName">string</param>
        /// <returns>ApplicationConfigBuilder</returns>
        public IniFileConfigBuilder WithApplicationName(string applicationName)
        {
            if (IsBuild)
            {
                throw new NotSupportedException(IniFileConfigAlreadyBuild);
            }
            _iniFileConfig.ApplicationName = applicationName;
            return this;
        }


        /// <summary>
        /// Disable SaveOnExit
        /// </summary>
        /// <returns>IniFileConfigBuilder</returns>
        public IniFileConfigBuilder WithoutSaveOnExit()
        {
            if (IsBuild)
            {
                throw new NotSupportedException(IniFileConfigAlreadyBuild);
            }
            _iniFileConfig.SaveOnExit = false;
            return this;
        }

        /// <summary>
        /// Disable WatchFileChanges
        /// </summary>
        /// <returns>IniFileConfigBuilder</returns>
        public IniFileConfigBuilder WithoutWatchingChanges()
        {
            if (IsBuild)
            {
                throw new NotSupportedException(IniFileConfigAlreadyBuild);
            }
            _iniFileConfig.WatchFileChanges = false;
            return this;
        }

        /// <summary>
        /// Specify the filename to use
        /// </summary>
        /// <param name="filename">string</param>
        /// <returns>IniFileConfigBuilder</returns>
        public IniFileConfigBuilder WithFilename(string filename)
        {
            if (IsBuild)
            {
                throw new NotSupportedException(IniFileConfigAlreadyBuild);
            }
            _iniFileConfig.FileName = filename;
            return this;
        }

        /// <summary>
        /// Specify a directory if you want to read and write from a specific directory, instead of using the default logic.
        /// </summary>
        /// <param name="directory">string</param>
        /// <returns>IniFileConfigBuilder</returns>
        public IniFileConfigBuilder WithFixedDirectory(string directory)
        {
            if (IsBuild)
            {
                throw new NotSupportedException(IniFileConfigAlreadyBuild);
            }
            _iniFileConfig.FixedDirectory = directory;
            return this;
        }

        /// <summary>
        /// Specify the auto save interval
        /// </summary>
        /// <param name="interval">uint interval</param>
        /// <returns>IniFileConfigBuilder</returns>
        public IniFileConfigBuilder WithAutoSaveInterval(uint interval)
        {
            if (IsBuild)
            {
                throw new NotSupportedException(IniFileConfigAlreadyBuild);
            }
            _iniFileConfig.AutoSaveInterval = interval;
            return this;
        }
    }
}
