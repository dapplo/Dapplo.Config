//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Dapplo.Config.Converters;
using Dapplo.Config.Ini;
using Dapplo.Config.Tests.ConfigTests.Interfaces;
using Dapplo.LogFacade;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigTests
{
	public class IniConfigTest : IDisposable
	{
		private const string Name = "Dapplo";
		private const string FirstName = "Robin";
		private const string TestValueForNonSerialized = "Hello!";

		public IniConfigTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevels.Verbose);
			StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
			StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
		}

		public void Dispose()
		{
			// Make sure we cleanup any created ini file, as it will influence other tests
			var location = IniConfig.Current.IniLocation;
			if (location != null && File.Exists(location))
			{
				File.Delete(location);
			}

			// Remove the IniConfig drom the IniConfig-store
			IniConfig.Delete("Dapplo", "dapplo");
		}

		private async Task ConfigureMemoryStreamAsync()
		{
			using (var testMemoryStream = new MemoryStream())
			{
				await IniConfig.Current.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
			}
		}

		private IniConfig Create()
		{
			// Important to disable the auto-save, otherwise we get test issues
			return new IniConfig("Dapplo", "dapplo", autoSaveInterval: 0, saveOnExit: false);
		}

		private async Task<IniConfig> InitializeAsync()
		{
			var iniConfig = Create();
			await ConfigureMemoryStreamAsync();
			return iniConfig;
		}

		/// <summary>
		///     This method tests that the initialization of the ini works.
		///     Including the after load
		/// </summary>
		[Fact]
		public async Task TestIniAfterLoad()
		{
			var iniConfig = Create();
			iniConfig.AfterLoad<IIniConfigTest>(x =>
			{
				if (!x.SomeValues.ContainsKey("dapplo"))
				{
					x.SomeValues.Add("dapplo", 2015);
				}
			});
			await ConfigureMemoryStreamAsync();

			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			Assert.True(iniTest.SomeValues.ContainsKey("dapplo"));
			Assert.True(iniTest.SomeValues["dapplo"] == 2015);
		}

		[Fact]
		public async Task TestIniConfigIndex()
		{
			var iniConfig = await InitializeAsync();
			await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			// Test indexers
			Assert.True(iniConfig.SectionNames.Contains("Test"));
			var iniTest = iniConfig["Test"];
			Assert.Equal("It works!", iniTest["SubValuewithDefault"].Value);
		}

		[Fact]
		public async Task TestIniConfigIndexConvertion()
		{
			var iniConfig = await InitializeAsync();
			await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			// Test indexers
			Assert.True(iniConfig.SectionNames.Contains("Test"));
			var iniTest = (IIniConfigTest) iniConfig["Test"];

			var iniSection = iniConfig["Test"];

			// Set value with wrong type (but valid value)
			iniSection["Height"].Value = "100";

			Assert.Equal((uint) 100, iniTest.Height);
		}

		[Fact]
		public async Task TestIniGeneral()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);

			Assert.Equal(iniTest.Height, 185u);
			iniTest.Height++;
			Assert.Equal(iniTest.Height, 186u);
			iniTest.Height = 185;
			Assert.Equal(iniTest.Height, 185u);
			Assert.Equal(iniTest.PropertySize.Width, 16);
			Assert.Equal(iniTest.PropertyArea.Width, 100);
			Assert.True(iniTest.WindowCornerCutShape.Count > 0);
			Assert.Equal("It works!", iniTest.SubValuewithDefault);
			Assert.Equal(IniConfigTestEnum.Value2, iniTest.TestWithEnum);
			iniTest.RestoreToDefault("TestWithEnum");
			Assert.Equal(IniConfigTestEnum.Value2, iniTest.TestWithEnum);
			Assert.Equal(IniConfigTestEnum.Value2, iniTest.TestWithEnumSubValue);
		}

		[Fact]
		public async Task TestIniIndexAccess()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			// Test ini value retrieval, by checking the Type and return value
			var iniValue = iniTest["WindowCornerCutShape"];
			Assert.True(iniValue.ValueType == typeof (IList<int>));
			Assert.True(((IList<int>) iniValue.Value).Count > 0);
		}

		[Fact]
		public async Task TestIniSectionTryGet()
		{
			var iniConfig = await InitializeAsync();
			await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			// Test try get
			IIniSection section;
			Assert.True(iniConfig.TryGet("Test", out section));
			IniValue tryGetValue;
			// TODO: The generated code doesn't support out parameters
			Assert.True(section.TryGetIniValue("WindowCornerCutShape", out tryGetValue));
			Assert.True(((IList<int>) tryGetValue.Value).Count > 0);
			Assert.False(section.TryGetIniValue("DoesNotExist", out tryGetValue));
		}

		[Fact]
		public async Task TestIniWriteRead()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);

			iniTest.WindowCornerCutShape.Add(100);
			iniTest.DictionaryOfLists.Add("firstValue", new List<int> {10, 20});
			// Change some values
			iniTest.Name = Name;
			iniTest.FirstName = FirstName;

			Assert.Equal(Size.Empty, iniTest.MySize);
			// This value should not be written to the file
			iniTest.NotWritten = "Whatever";

			// Dictionary test
			iniTest.SomeValues.Add("One", 1);
			iniTest.TestEnums.Add(IniConfigTestEnum.Value2);
			// Enum test
			iniTest.TestWithEnum = IniConfigTestEnum.Value1;
			iniTest.TestWithEnumSubValue = IniConfigTestEnum.Value1;

			// Some "random" value that needs to be there again after reading.
			var ticks = DateTimeOffset.Now.UtcTicks;
			iniTest.Age = ticks;
			var heightBefore = ++iniTest.Height;

			using (var writeStream = new MemoryStream())
			{
				await iniConfig.WriteToStreamAsync(writeStream).ConfigureAwait(false);

				//await iniConfig.WriteAsync().ConfigureAwait(false);

				// Set the not written value to a testable value, this should not be read (and overwritten) by reading the ini file.
				iniTest.NotWritten = TestValueForNonSerialized;

				// Make sure Age is set to some value, so we can see that it is re-read
				iniTest.Age = 2;

				// Make sure we change the value, to see if it's overwritten
				iniTest.Height++;

				// Test reading
				writeStream.Seek(0, SeekOrigin.Begin);
				await iniConfig.ReadFromStreamAsync(writeStream).ConfigureAwait(false);
				//await iniConfig.ReloadAsync(false).ConfigureAwait(false);
				Assert.True(iniTest.SomeValues.ContainsKey("One"));
				Assert.True(iniTest.WindowCornerCutShape.Contains(100));
				// check if the dictionary of lists also has all values again
				Assert.True(iniTest.DictionaryOfLists.ContainsKey("firstValue"));
				Assert.True(iniTest.DictionaryOfLists["firstValue"].Count == 2);

				// Rest of simple tests
				Assert.Equal(Name, iniTest.Name);
				Assert.Equal(FirstName, iniTest.FirstName);
				Assert.Equal(ticks, iniTest.Age);
				Assert.Equal(TestValueForNonSerialized, iniTest.NotWritten);
				Assert.Equal(IniConfigTestEnum.Value1, iniTest.TestWithEnum);
				Assert.Equal(IniConfigTestEnum.Value1, iniTest.TestWithEnumSubValue);
				Assert.Equal(heightBefore, iniTest.Height);
				Assert.True(iniTest.TestEnums.Contains(IniConfigTestEnum.Value2));
			}

			// Check second get, should have same value
			var iniTest2 = iniConfig.Get<IIniConfigTest>();
			Assert.Equal(TestValueForNonSerialized, iniTest2.NotWritten);
		}

		[Fact]
		public async Task TestIniWrongEnumDefault()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigWrongEnumTest>().ConfigureAwait(false);
			Assert.Equal(IniConfigTestEnum.Value1, iniTest.TestWithFalseEnum);
		}
	}
}