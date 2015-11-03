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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;
using Dapplo.Config.Converters;
using Dapplo.Config.Ini;

namespace Dapplo.Config.Test.ConfigTests.Interfaces
{

	public enum IniConfigTestEnum
	{
		Value1,
		Value2
	}
	/// <summary>
	/// This is the interface under test
	/// </summary>
	[IniSection("Test")]
	[Description("Test Configuration")]
	public interface IIniConfigTest : IIniConfigSubInterfaceTest, IIniSection
	{
		[Description("Test property for enums"), DefaultValue(IniConfigTestEnum.Value2)]
        IniConfigTestEnum TestWithEnum
		{
			get;
			set;
		}
		[Description("List of enums")]
		IList<IniConfigTestEnum> TestEnums
		{
			get;
			set;
		}

		[DefaultValue("")]
		Size MySize
		{
			get;
			set;
		}

		[Description("Name of the person")]
		string Name
		{
			get;
			set;
		}

		[DefaultValue(21), DataMember(EmitDefaultValue = true)]
		long Age
		{
			get;
			set;
		}

		[DefaultValue(185), DataMember(EmitDefaultValue = true)]
		uint Height {
			get;
			set;
		}

		[TypeConverter(typeof (StringEncryptionTypeConverter))]
		string FirstName
		{
			get;
			set;
		}

		[DefaultValue("5,3,2,1,1")]
		IList<int> WindowCornerCutShape
		{
			get;
			set;
		}

		[IniPropertyBehavior(Read = false, Write = false)]
		string NotWritten
		{
			get;
			set;
		}

		[Description("Here are some values")]
		IDictionary<string, int> SomeValues
		{
			get;
			set;
		}

		[Description("Here are some cool values")]
		IDictionary<string, IList<int>> DictionaryOfLists
		{
			get;
			set;
		}

		[DefaultValue("16,16")]
		Size PropertySize {
			get;
			set;
		}
		[DefaultValue("16,16,100,100")]
		Rectangle PropertyArea {
			get;
			set;
		}
	}
}