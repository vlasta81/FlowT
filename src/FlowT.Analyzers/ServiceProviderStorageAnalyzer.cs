using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects when IServiceProvider is stored in instance fields.
    /// Singleton components will capture the root provider instead of scoped providers.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ServiceProviderStorageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT012";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "IServiceProvider stored in singleton flow component",
            messageFormat: "Flow component '{0}' stores IServiceProvider in field '{1}'. Singleton will capture root provider. Use 'context.Service<T>()' or 'context.Services' instead.",
            category: "FlowT.DependencyInjection",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Singleton flow components that capture IServiceProvider in constructor will get the root provider, not scoped providers. " +
                        "This means scoped services won't be per-request. " +
                        "Use FlowContext.Service<T>() or FlowContext.Services inside HandleAsync/CheckAsync methods instead.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            INamedTypeSymbol namedType = (INamedTypeSymbol)context.Symbol;
            if (!IsFlowComponent(namedType))
                return;
            foreach (IFieldSymbol field in namedType.GetMembers().OfType<IFieldSymbol>())
            {
                // Check if field type is IServiceProvider
                if (IsServiceProvider(field.Type))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        field.Locations[0],
                        namedType.Name,
                        field.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsFlowComponent(INamedTypeSymbol type)
        {
            return type.AllInterfaces.Any(i =>
                i.Name == "IFlowHandler" ||
                i.Name == "IFlowSpecification") ||
                InheritsFromFlowPolicy(type);
        }

        private bool InheritsFromFlowPolicy(INamedTypeSymbol type)
        {
            INamedTypeSymbol? baseType = type.BaseType;
            while (baseType is not null)
            {
                if (baseType.Name is "FlowPolicy" or "FlowSpecification")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }

        private bool IsServiceProvider(ITypeSymbol type)
        {
            // Check if it's IServiceProvider interface
            if (type.TypeKind == TypeKind.Interface && type.Name == "IServiceProvider")
                return true;
            // Check if it implements IServiceProvider
            if (type is INamedTypeSymbol namedType)
            {
                return namedType.AllInterfaces.Any(i => i.Name == "IServiceProvider");
            }
            return false;
        }
    }
}
