// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Runtime.Serialization;

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