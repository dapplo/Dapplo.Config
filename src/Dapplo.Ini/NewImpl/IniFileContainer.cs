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
    /// This contains all the sections in one ini file
    /// </summary>
    public class IniFileContainer
    {
        /// <summary>
        /// All the ini sections for this file
        /// </summary>
        protected readonly IDictionary<string, IIniSection> _iniSections = new Dictionary<string, IIniSection>(AbcComparer.Instance);

        /// <summary>
        /// Register IIniSection
        /// </summary>
        /// <param name="iniSection">IIniSection</param>
        public void Register(IIniSection iniSection)
        {
            _iniSections[iniSection.GetSectionName()] = iniSection;
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

        }
    }
}
