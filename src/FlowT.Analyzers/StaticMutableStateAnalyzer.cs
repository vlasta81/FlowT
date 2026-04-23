using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects static mutable fields in flow components.
    /// Static mutable state is shared across all instances and threads, causing severe race conditions.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticMutableStateAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT004";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Static mutable field in flow component",
            messageFormat: "Static field '{0}' in flow component '{1}' is mutable. Static fields are shared across all instances and threads. Make it readonly or use thread-safe alternatives.",
            category: "FlowT.ThreadSafety",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Static mutable fields are shared across ALL instances and threads in the application. " +
                        "This creates severe race conditions and data corruption. " +
                        "Use 'static readonly' for immutable data, or ConcurrentDictionary for mutable shared state.");

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
                // Only check static fields
                if (!field.IsStatic)
                    continue;
                // Skip readonly and const (they're safe)
                if (field.IsReadOnly || field.IsConst)
                    continue;
                // Report static mutable field
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
                if (baseType.Name is "FlowPolicy" or "FlowSpecification")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
