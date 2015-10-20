/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Config.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// This ExportProvider takes care of resolving MEF imports for the IniConfig
	/// It will register & create the IniSection derrived class, and return the export so it can be injected
	/// </summary>
	public class IniConfigExportProvider : ExportProvider
	{
		private readonly IniConfig _iniConfig;
		private readonly string _application;
		private readonly IList<Assembly> _assemblies;
		private readonly IDictionary<string, Export> _loopup = new Dictionary<string, Export>();
		private readonly IServiceLocator _serviceLocator;

		/// <summary>
		/// Create a IniConfigExportProvider which is for the specified applicatio, iniconfig and works with the supplied assemblies
		/// </summary>
		/// <param name="application">Application name, used for the meta-data</param>
		/// <param name="iniConfig">IniConfig needed for the registering</param>
		/// <param name="assemblies">List of assemblies used for finding the type</param>
		public IniConfigExportProvider(string application, IniConfig iniConfig, IList<Assembly> assemblies, IServiceLocator serviceLocator)
		{
			_application = application;
            _iniConfig = iniConfig;
			_assemblies = assemblies;
			_serviceLocator = serviceLocator;
        }

		/// <summary>
		/// Try to find the IniSection type that wants to be imported, and get/register it.
		/// </summary>
		/// <param name="definition">ImportDefinition</param>
		/// <param name="atomicComposition">AtomicComposition</param>
		/// <returns>Export</returns>
		protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
		{
			Export export;
			// See if we already cached the value
			if (_loopup.TryGetValue(definition.ContractName, out export))
			{
				if (export != null)
				{
					yield return export;
				}
			}
			else
			{
				// Loop over all the supplied assemblies, these should come from the bootstrapper
				foreach (var assembly in _assemblies)
				{
					// Make an AssemblyQualifiedName from the contract name
					var assemblyQualifiedName = $"{definition.ContractName}, {assembly.FullName}";
					// Try to get it, don't throw an exception if not found
					Type contractType = null;
					try
					{
						contractType = Type.GetType(assemblyQualifiedName, false, true);
					}
					catch
					{
						// Ignore & break the loop at it is most likely a problem with the contract name
						break;
					}
					// Go to next assembly if it wasn't found
					if (contractType == null)
					{
						continue;
					}
					// Check if it is derrived from IIniSection
					if (typeof(IIniSection).IsAssignableFrom(contractType))
					{
						// Generate the export & meta-data
						var metadata = new Dictionary<string, object>(){
							{CompositionConstants.ExportTypeIdentityMetadataName, _application}
						};

						var exportDefinition = new ExportDefinition(_application, metadata);

						var instance = _iniConfig.RegisterAndGet(contractType);
						// Make sure it's exported
						if (_serviceLocator != null)
						{
							_serviceLocator.Export(instance);
						}
						export = new Export(exportDefinition, () => instance);

						// store the export for fast retrieval
						_loopup.Add(definition.ContractName, export);
						yield return export;
						// Nothing more to do, break
						yield break;
					}
				}
				// Add null value, so we don't try it again
				_loopup.Add(definition.ContractName, null);
			}
			yield break;
		}
	}
}
