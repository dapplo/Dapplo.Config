//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2017 Dapplo
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

using Dapplo.Ini.Implementation;
using Dapplo.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.Ini.NewImpl
{
    /// <summary>
    /// This contains all the ini sections in one ini file
    /// </summary>
    public class IniFileContainer
    {
        /// <summary>
        /// All the ini sections for this file
        /// </summary>
        protected readonly IDictionary<string, IIniSection> IniSections = new Dictionary<string, IIniSection>(AbcComparer.Instance);

        /// <summary>
        /// Register IIniSection
        /// </summary>
        /// <param name="iniSection">IIniSection</param>
        public void Register(IIniSection iniSection)
        {
            IniSections[iniSection.GetSectionName()] = iniSection;
        }

        /// <summary>
        /// Register multiple IIniSection
        /// </summary>
        /// <param name="iniSections">IEnumerable of IIniSection</param>
        public void Register(IEnumerable<IIniSection> iniSections)
        {
            foreach(var iniSection in iniSections)
            {
                Register(iniSection);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task Read(Stream stream, CancellationToken cancellationToken = default)
        {
            var iniValues = await IniFile.ReadAsync(stream, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

            foreach (var iniSection in IniSections.Values)
            {
                iniSection.AfterLoad();
            }
        }
    }
}
