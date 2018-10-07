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

#region using

using Dapplo.Config.Ini.Attributes;
using System;
using System.Reflection;
using System.Runtime.Serialization;

#endregion

namespace Dapplo.Config.Ini.Extensions
{
	/// <summary>
	///     Extensions for PropertyInfo
	/// </summary>
	internal static class PropertyInfoExtension
	{
		/// <summary>
		///     Get the IniPropertyBehaviorAttribute
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>IniPropertyBehaviorAttribute</returns>
		public static IniPropertyBehaviorAttribute GetIniPropertyBehavior(this PropertyInfo propertyInfo)
		{
			var iniPropertyBehaviorAttribute = propertyInfo.GetCustomAttribute<IniPropertyBehaviorAttribute>(true) ?? new IniPropertyBehaviorAttribute();
			// Check if there is a IgnoreDataMember annotation, if this is the case don't read and write
			if (propertyInfo.GetCustomAttribute<IgnoreDataMemberAttribute>(true) != null)
			{
				iniPropertyBehaviorAttribute.Read = false;
				iniPropertyBehaviorAttribute.Write = false;
			}
			// Check if there is a <NonSerialized annotation, if this is the case don't read and write
			if (propertyInfo.GetCustomAttribute<NonSerializedAttribute>(true) != null)
			{
				iniPropertyBehaviorAttribute.Read = false;
				iniPropertyBehaviorAttribute.Write = false;
			}
			return iniPropertyBehaviorAttribute;
		}
	}
}