// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dapplo.Config.Language.Implementation
{
    /// <summary>
    ///     Some utils for managing the location of files
    /// </summary>
    internal static class FileLocations
    {
        /// <summary>
        ///     Get the startup location, which is either the location of the entry assemby, or the executing assembly
        /// </summary>
        /// <returns>string with the directory of where the running code/applicationName was started</returns>
        public static string StartupDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Get the roaming AppData directory
        /// </summary>
        /// <returns>string with the directory the appdata roaming directory</returns>
        public static string RoamingAppDataDirectory(string applicationName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);
        }

        /// <summary>
        ///     Scan the supplied directories for files which match the passed file pattern
        /// </summary>
        /// <param name="directories"></param>
        /// <param name="filePattern">Regular expression for the filename</param>
        /// <param name="searchOption">
        ///     Makes it possible to specify if the search is recursive, SearchOption.AllDirectories is
        ///     default, use SearchOption.TopDirectoryOnly for non recursive
        /// </param>
        /// <returns>IEnumerable with paths</returns>
        public static IEnumerable<Tuple<string, Match>> Scan(IEnumerable<string> directories, Regex filePattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return from directory in directories
                from path in DirectoriesFor(directory)
                where Directory.Exists(path)
                from file in Directory.EnumerateFiles(path, "*", searchOption)
                let match = filePattern.Match(file)
                where match.Success
                select Tuple.Create(file, match);
        }

        /// <summary>
        ///     Scan the supplied directories for files which match the passed file pattern
        /// </summary>
        /// <param name="directories"></param>
        /// <param name="simplePattern"></param>
        /// <param name="searchOption">
        ///     Makes it possible to specify if the search is recursive, SearchOption.AllDirectories is
        ///     default, use SearchOption.TopDirectoryOnly for non recursive
        /// </param>
        /// <returns>IEnumerable with paths</returns>
        public static IEnumerable<string> Scan(IEnumerable<string> directories, string simplePattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return from directory in directories
                from path in DirectoriesFor(directory)
                where Directory.Exists(path)
                from file in Directory.EnumerateFiles(path, simplePattern, searchOption)
                select file;
        }

        /// <summary>
        ///     For the given directory this will return possible location.
        ///     It might be that multiple are returned, also normalization is made
        /// </summary>
        /// <param name="directory">A absolute or relative directory</param>
        /// <param name="allowCurrentDirectory">true to allow relative to current working directory</param>
        /// <returns>IEnumerable with possible directories</returns>
        public static IEnumerable<string> DirectoriesFor(string directory, bool allowCurrentDirectory = true)
        {
            var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // If the path is rooted, it's absolute
            if (Path.IsPathRooted(directory))
            {
                try
                {
                    var normalizedDirectory = Path.GetFullPath(new Uri(directory, UriKind.Absolute).LocalPath)
                        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (Directory.Exists(normalizedDirectory))
                    {
                        directories.Add(normalizedDirectory);
                    }
                }
                catch
                {
                    // Do nothing
                }

                if (Directory.Exists(directory))
                {
                    directories.Add(directory);
                }
            }
            else
            {
                // Relative to the assembly location
                try
                {
                    var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(assemblyLocation) && File.Exists(assemblyLocation))
                    {
                        var exeDirectory = Path.GetDirectoryName(assemblyLocation);
                        if (!string.IsNullOrEmpty(exeDirectory) && exeDirectory != Environment.CurrentDirectory)
                        {
                            var relativeToExe = Path.GetFullPath(Path.Combine(exeDirectory, directory))
                                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            if (Directory.Exists(relativeToExe))
                            {
                                directories.Add(relativeToExe);
                            }
                        }
                    }
                }
                catch
                {
                    // Do nothing
                }

                // Relative to the current working directory
                try
                {
                    if (allowCurrentDirectory)
                    {
                        var relativetoCurrent = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, directory))
                            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                        if (Directory.Exists(relativetoCurrent))
                        {
                            directories.Add(relativetoCurrent);
                        }
                    }
                }
                catch
                {
                    // Do nothing
                }
            }
            return directories.OrderBy(x => x);
        }
    }
}
