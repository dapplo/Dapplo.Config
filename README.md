Dapplo.Config
=============

- Documentation can be found [here](http://www.dapplo.net/blocks/Dapplo.Config) (soon)
- Current build status: [![Build status](https://ci.appveyor.com/api/projects/status/ujfvy3g49g6a1d1q?svg=true)](https://ci.appveyor.com/project/dapplo/dapplo-config)
- Coverage Status: [![Coverage Status](https://coveralls.io/repos/github/dapplo/Dapplo.Config/badge.svg?branch=master)](https://coveralls.io/github/dapplo/Dapplo.Config?branch=master)
- NuGet package: [![NuGet package](https://badge.fury.io/nu/Dapplo.Config.svg)](https://badge.fury.io/nu/Dapplo.Config)

Dapplo.Config is a project which helps you to extend your application with a configuration.
This can be found on NuGet!

As it was build for [Greenshot](https://github.com/greenshot/greenshot), the main focus was on having .ini suport.
It was also very important that Greenshot plug-ins are able to store their information into the same file, and keep the complexity for the developer as little as possible.


# Ini-files

## Quick start:

An .ini has one or more sections, you can find details on this [here in Wikipedia](https://en.wikipedia.org/wiki/INI_file).
These sections are represented by .NET interfaces which extend IIniSection.
You will need to implement these interface and place the properties for your settings in here.
You can have "as many" IIniSection implementing interfaces as you need, it makes sense to logically spread the information so they fit to a certain behaviour or function.
The section in the .ini file will have the name of the interface or you can use an Attribute to specify this (adviced) 

Example of a simple section interface:
```
public interface ICoreConfiguration : IIniSection
{
  string MyProperty {get;set;}
}
```

Every application needs a IniConfig instance, this represent the .ini which is read/written.
As soon as you have an instance of IniConfig, you will **need** to register your interface(s) with it.
This way the properties can be serialized to or deserialized from the file.

An example of your initial code:
```
var iniConfig = new IniConfig("<application>", "<ini-file-name-without-extension>");
var coreConfiguration = await iniConfig.RegisterAndGetAsync<ICoreConfiguration>().ConfigureAwait(false);
```
As the file needs to be read from a filesystem, we don't want to block so the RegisterAndGetAsync method is Async.
There are many ways to do this, but I just want to describe a simple how-to.

Now the important parts:
RegisterAndGetAsync will give you an **instance** of your ICoreConfiguration. What? Yes!!
You will get an implementation which is generated for you, this has many advantanges as the framework can add additional functionality. (later more).

Second: the instance you get is a singleton, you will get always get the same instance, no mather how often you ask for it.
You can have classes assign the instance to a static variable which you can use to read and write to the properties:
```
public class MyClass {
  private static readonly ICoreConfiguration CoreConfiguration = IniConfig.Current().Get<>(ICoreConfiguration);
  
  public void MyMethod() {
    Debug.WriteLine(CoreConfiguration.MyProperty);
  }
```
In this part IniConfig.Current() is used to get a reference to the one and only instance of IniConfig which was created, this won't work when you have multiples... in this case you will need to call Get("<application>", "<ini-file-name-without-extension>").

Now you can also read and write values via the CoreConfiguration variable!

## Auto save

The framework registers an event-handler on AppDomain.CurrentDomain.ProcessExit, which tries to save when your application stops.
Although saving is done automatically after 1 second (is configurable) you should if possible make sure the WriteAsync() method is called on your IniConfig **if** you know you have changes. If a process is terminated, the framework can't write your changes.

## The properties of your interfaces

The framework supports a lot of types for your properties:
* All primitives, like (u)int/(u)short/(u)long/bool etc
* strings ofcourse
* Enums
* Collections of primitives
* Dictionary of key/value with the primitives (yes)
* Interfaces for collections, the framework has a mapping of e.g. IList -> List etc.

Maybe you find something which really makes sense to have and doesn't work? Create an issue for it.

### Attributes for IIniSection

* DescriptionAttribute: having this attribute on a property will create a comment about the section in the .ini, this can help IT staff or users to understand your settings.
* IniSectionAttribute: this Dapplo.Config attribute can be used to specify the name of the ini-section (so inside the file) and if any (de-)serialization errors should be ignored (default = false).

### Attributes for Properties

The way the properties are serialized/deserialized to your .ini file depends on attributes you annotate on your properties.
You can assist by specifying converters, and if defaults need to be in the file.

* DefaultValueAttribute: As you cannot have initializers inside an interface, the DefaultValueAttribute can be used to specify what the default value(s) show be.
* DescriptionAttribute: having this attribute on a property will create a comment about the line in the .ini, this can help IT staff or users to understand your settings.
* DataMemberAttribute: this can be used to specify if a property with the default value should be written to the file. (use EmitDefaultValue=true)
* IniPropertyBehaviorAttribute: this is a Dapplo.Config Attribute, and can be used to suppress reading / writing, and ignore errors.
* TypeConverterAttribute: This can be used to specify which TypeConverter should be used to convert string -> property when reading and property->string when writing.


## Additional functionality

As the framework generates an implementation of your interface, using Dapplo.InterfaceImpl, I added the possibility to extended this with additional functionality.
You can use this simply by extending your interface with additional interfaces.

Currently the following is available but can be extended:
* INotifyPropertyChanged: this will add the PropertyChanged event, and generate events for every property that you changed.
* INotifyPropertyChanging: this will add the PropertyChanging event, and generate events for every property that you are changing.
* IHasChanges: this will add a possibility to detect if you changed a value since the start or a reset.
* IDescription: this will add method to get the value of a DescriptionAttribute of a property.

## Weak-types access

You can also access the values in our .ini without going through the interfaces, this could be something you want to do when you want to have a settings UI (I will build one soon).

All values are parsed into "IniValue" instances, which have many details on the value.
If the ICoreConfiguration interface had a IniSectionAttribute to specify the section name "Core", you could access your properties like this:

```
iniConfig["Core"]["MyProperty"].Value = "a new value for MyProperty";
```
There are also enumerating possiblities, to foreach over all IIniSections and their properties.


# Language (text resources)

Additionally to .ini support, the framework was extended with functionality for translations.
This was also written for Greenshot, and is already very usuable. Documentation will follow.
The functionality is in the LanguageLoader, and in general the usage is similar to the IniConfig.
Your interface needs to extend ILanguage, and only have getters.


# Registry access

This is work in progress, but the same idea as having an .NET interface map to a .ini, I also added a mapping into the registry.
More to come...


# Managed Extension Framework (MEF) Support

There is another Dapplo repository (Dapplo.Addons](https://github.com/dapplo/Dapplo.Addons) which makes it possible to import IIniSection interfaces into your MEF controlled classes. The idea of that project is making it easy to add extensions to your application. In this case MEF can really help...


Notice:
**This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.**
