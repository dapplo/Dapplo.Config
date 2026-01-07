# Dapplo.Config Source Generator

## Overview

This source generator aims to eliminate runtime reflection in Dapplo.Config by generating configuration implementations at compile-time.

## Current Status

The source generator is **functional** and can:
- Detect interfaces that extend `IConfiguration<T>` or `IIniSection`
- Generate classes with property implementations
- Generate `INotifyPropertyChanged` support
- Create factory methods for instantiation

## Limitations

The current implementation generates lightweight POCO classes that:
- ✅ Implement the user-defined properties
- ✅ Support `INotifyPropertyChanged`
- ❌ Do NOT implement all the rich features of Dapplo.Config (transactions, write protection, change tracking, INI file persistence, etc.)

## Usage

### Option 1: Use Generated POCOs (Current)

For simple scenarios where you only need basic property storage and change notification:

```csharp
[IniSection("MyConfig")]
public interface IMyConfig : IIniSection
{
    string Name { get; set; }
    int Age { get; set; }
}

// Use the generated class
var config = MyConfigGenerated.Create();
config.Name = "Test";
```

**Note**: This gives you a lightweight object without file persistence, interceptors, or other advanced features.

### Option 2: Use Existing Reflection-Based API (Recommended for Full Features)

For applications that need the full feature set:

```csharp
// Traditional approach with all features
var config = IniSection<IMyConfig>.Create();
```

## Future Development

To truly eliminate reflection while maintaining all features, the generator should:

1. **Generate Metadata Classes**: Pre-compute `PropertiesInformation` and `GetSetInterceptInformation` at compile-time
2. **Populate Caches**: Initialize the static caches in `ConfigurationBase` with pre-computed metadata
3. **Optimize DispatchProxy**: Consider generating direct property implementations that call into the existing infrastructure

This would provide:
- ✅ Zero reflection at runtime
- ✅ All existing features (transactions, persistence, interceptors, etc.)
- ✅ Backwards compatibility
- ✅ Performance improvements

## Architecture Considerations

The Dapplo.Config library uses a sophisticated architecture:

- **ConfigurationBase**: Provides core property get/set infrastructure with interceptor support
- **DispatchProxy**: Creates dynamic proxies for interfaces at runtime using reflection
- **Interceptor Pattern**: Allows ordered method invocation for features like transactions, change tracking, etc.
- **PropertiesInformation**: Uses reflection to discover properties and their attributes
- **GetSetInterceptInformation**: Uses reflection to discover interceptor methods

Fully replacing this with source-generated code requires substantial architectural changes.

## Recommendations

For now, users should:
1. **Prefer the existing API** for production use - it's mature and feature-complete
2. **Use generated POCOs** only for simple scenarios where reflection is a concern and advanced features aren't needed
3. **Stay tuned** for future versions that will provide full feature parity with zero reflection

## Contributing

To improve the source generator:
1. Focus on generating metadata pre-computation
2. Ensure backwards compatibility
3. Add comprehensive tests
4. Update documentation

## License

Same as Dapplo.Config - MIT License
