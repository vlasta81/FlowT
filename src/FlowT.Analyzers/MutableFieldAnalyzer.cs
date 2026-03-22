using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects mutable fields in singleton flow components.
    /// Mutable fields can cause race conditions when shared between concurrent requests.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MutableFieldAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Mutable field in singleton flow component",
            messageFormat: "Field '{0}' in flow component '{1}' is mutable and can cause race conditions. Make it readonly or use FlowContext storage.",
            category: "FlowT.ThreadSafety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Flow components are registered as singletons and shared between concurrent requests. " +
                        "Mutable fields can cause race conditions. Use readonly fields for dependencies, " +
                        "or store per-request data in FlowContext.");

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
                // Skip readonly, const, static
                if (field.IsReadOnly || field.IsConst || field.IsStatic)
                    continue;
                // Report mutable field
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    field.Locations[0],
                    field.Name,
                    namedType.Name);
                context.ReportDiagnostic(diagnostic);
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
                if (baseType.Name == "FlowPolicy")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
