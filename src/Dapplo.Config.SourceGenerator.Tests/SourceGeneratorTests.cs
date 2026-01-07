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
            Assert.Equal("Test", config.Name);
            Assert.Equal(42, config.Age);
            Assert.True(config.IsEnabled);
            
            // Test property changes
            config.Name = "NewName";
            Assert.Equal("NewName", config.Name);
            
            config.Age = 100;
            Assert.Equal(100, config.Age);
            
            config.IsEnabled = false;
            Assert.False(config.IsEnabled);
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
