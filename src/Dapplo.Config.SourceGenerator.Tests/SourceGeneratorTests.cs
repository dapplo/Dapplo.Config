// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Dapplo.Config.SourceGenerator.Tests
{
    /// <summary>
    /// Tests for the source generator
    /// </summary>
    public class SourceGeneratorTests
    {
        [Fact]
        public void TestSourceGeneratedConfiguration()
        {
            // Test that the source generator created a class
            var config = TestConfigGenerated.Create();
            
            Assert.NotNull(config);
            // Note: DefaultValue attributes are not currently implemented in generated code
            // Properties will have type defaults (null, 0, false)
            Assert.Null(config.Name);
            Assert.Equal(0, config.Age);
            Assert.False(config.IsEnabled);
            
            // Test property changes
            config.Name = "NewName";
            Assert.Equal("NewName", config.Name);
            
            config.Age = 100;
            Assert.Equal(100, config.Age);
            
            config.IsEnabled = true;
            Assert.True(config.IsEnabled);
        }
        
        [Fact]
        public void TestPropertyChangedEvent()
        {
            var config = TestConfigGenerated.Create();
            
            string changedPropertyName = null;
            config.PropertyChanged += (sender, args) =>
            {
                changedPropertyName = args.PropertyName;
            };
            
            config.Name = "Changed";
            Assert.Equal("Name", changedPropertyName);
            
            config.Age = 50;
            Assert.Equal("Age", changedPropertyName);
        }
    }
}
