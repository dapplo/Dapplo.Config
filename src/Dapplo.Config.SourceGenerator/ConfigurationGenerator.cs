// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dapplo.Config.SourceGenerator
{
    /// <summary>
    /// Source generator for Dapplo.Config configuration interfaces
    /// This generator creates implementations for configuration interfaces at compile-time,
    /// eliminating the need for runtime reflection with DispatchProxy
    /// </summary>
    [Generator]
    public class ConfigurationGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Filter for interface declarations
            var interfaceDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is InterfaceDeclarationSyntax,
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null);

            // Combine with compilation
            var compilationAndInterfaces = context.CompilationProvider.Combine(interfaceDeclarations.Collect());

            // Generate source
            context.RegisterSourceOutput(compilationAndInterfaces,
                static (spc, source) => Execute(source.Left, source.Right!, spc));
        }

        private static InterfaceDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;
            
            // Early filter - must have base list
            if (interfaceDeclaration.BaseList == null || interfaceDeclaration.BaseList.Types.Count == 0)
            {
                return null;
            }

            return interfaceDeclaration;
        }

        private static void Execute(Compilation compilation, ImmutableArray<InterfaceDeclarationSyntax> interfaces, SourceProductionContext context)
        {
            if (interfaces.IsDefaultOrEmpty)
            {
                return;
            }

            // Get the IConfiguration interface symbol to check if interfaces extend it
            var iConfigurationSymbol = compilation.GetTypeByMetadataName("Dapplo.Config.Interfaces.IConfiguration`1");
            var iIniSectionSymbol = compilation.GetTypeByMetadataName("Dapplo.Config.Ini.IIniSection");

            foreach (var interfaceDeclaration in interfaces.Distinct())
            {
                var model = compilation.GetSemanticModel(interfaceDeclaration.SyntaxTree);
                var interfaceSymbol = model.GetDeclaredSymbol(interfaceDeclaration) as INamedTypeSymbol;

                if (interfaceSymbol == null)
                {
                    continue;
                }

                // Check if this interface or any base interface extends IConfiguration or IIniSection
                bool isConfigInterface = IsConfigurationInterface(interfaceSymbol, iConfigurationSymbol, iIniSectionSymbol);
                
                if (!isConfigInterface)
                {
                    continue;
                }

                // Generate the implementation
                var source = GenerateImplementation(interfaceSymbol);
                if (!string.IsNullOrEmpty(source))
                {
                    context.AddSource($"{interfaceSymbol.Name}_Generated.g.cs", SourceText.From(source, Encoding.UTF8));
                }
            }
        }

        private static bool IsConfigurationInterface(INamedTypeSymbol interfaceSymbol, INamedTypeSymbol iConfigurationSymbol, INamedTypeSymbol iIniSectionSymbol)
        {
            if (iIniSectionSymbol != null)
            {
                // Check if implements IIniSection
                if (interfaceSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iIniSectionSymbol)))
                {
                    return true;
                }
            }

            if (iConfigurationSymbol != null)
            {
                // Check if implements IConfiguration<T>
                if (interfaceSymbol.AllInterfaces.Any(i => i.OriginalDefinition != null && SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iConfigurationSymbol)))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GenerateImplementation(INamedTypeSymbol interfaceSymbol)
        {
            var namespaceName = interfaceSymbol.ContainingNamespace.ToDisplayString();
            var interfaceName = interfaceSymbol.Name;
            var className = $"{interfaceName.TrimStart('I')}Generated";

            // Collect all properties from the interface and its base interfaces
            var properties = GetAllProperties(interfaceSymbol);

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// This file is generated by Dapplo.Config.SourceGenerator");
            sb.AppendLine("// It provides a lightweight, reflection-free implementation");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Source-generated implementation for {interfaceName}");
            sb.AppendLine($"    /// This is a lightweight POCO implementation that eliminates runtime reflection");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public sealed class {className} : {interfaceName}, INotifyPropertyChanged");
            sb.AppendLine("    {");
            
            // Generate backing fields for all properties
            foreach (var property in properties)
            {
                var fieldName = GetFieldName(property.Name);
                var propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine($"        private {propertyType} {fieldName};");
            }
            
            sb.AppendLine();
            
            // Generate PropertyChanged event
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Event raised when a property value changes");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public event PropertyChangedEventHandler? PropertyChanged;");
            sb.AppendLine();
            
            // Generate OnPropertyChanged method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Raises the PropertyChanged event");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"propertyName\">Name of the property that changed</param>");
            sb.AppendLine("        private void OnPropertyChanged(string propertyName)");
            sb.AppendLine("        {");
            sb.AppendLine("            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Generate property implementations
            foreach (var property in properties)
            {
                GenerateProperty(sb, property);
            }

            sb.AppendLine();
            // Generate factory method
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Factory method to create an instance of {interfaceName}");
            sb.AppendLine($"        /// This provides a reflection-free way to instantiate the configuration");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        /// <returns>A new instance of {className}</returns>");
            sb.AppendLine($"        public static {interfaceName} Create()");
            sb.AppendLine("        {");
            sb.AppendLine($"            return new {className}();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GetFieldName(string propertyName)
        {
            return $"_{char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";
        }

        private static List<IPropertySymbol> GetAllProperties(INamedTypeSymbol interfaceSymbol)
        {
            var properties = new List<IPropertySymbol>();
            var processed = new HashSet<string>();

            // Get properties from this interface
            foreach (var member in interfaceSymbol.GetMembers())
            {
                if (member is IPropertySymbol property && !property.IsIndexer)
                {
                    if (!processed.Contains(property.Name))
                    {
                        properties.Add(property);
                        processed.Add(property.Name);
                    }
                }
            }

            // Get properties from base interfaces
            foreach (var baseInterface in interfaceSymbol.AllInterfaces)
            {
                foreach (var member in baseInterface.GetMembers())
                {
                    if (member is IPropertySymbol property && !property.IsIndexer)
                    {
                        if (!processed.Contains(property.Name))
                        {
                            properties.Add(property);
                            processed.Add(property.Name);
                        }
                    }
                }
            }

            return properties;
        }

        private static void GenerateProperty(StringBuilder sb, IPropertySymbol property)
        {
            var propertyName = property.Name;
            var propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var fieldName = GetFieldName(propertyName);

            sb.AppendLine();
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Gets or sets the {propertyName} property");
            sb.AppendLine($"        /// </summary>");
            sb.Append($"        public {propertyType} {propertyName}");
            
            // Generate getter/setter based on what the interface defines
            sb.AppendLine();
            sb.AppendLine("        {");
            
            if (property.GetMethod != null)
            {
                sb.AppendLine($"            get => {fieldName};");
            }
            
            if (property.SetMethod != null)
            {
                sb.AppendLine("            set");
                sb.AppendLine("            {");
                sb.AppendLine($"                if (!EqualityComparer<{propertyType}>.Default.Equals({fieldName}, value))");
                sb.AppendLine("                {");
                sb.AppendLine($"                    {fieldName} = value;");
                sb.AppendLine($"                    OnPropertyChanged(nameof({propertyName}));");
                sb.AppendLine("                }");
                sb.AppendLine("            }");
            }
            
            sb.AppendLine("        }");
        }
    }
}
