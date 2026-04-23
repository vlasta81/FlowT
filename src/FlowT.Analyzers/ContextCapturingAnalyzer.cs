using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects when FlowContext is stored in instance fields.
    /// FlowContext is per-request and must not be captured in singleton components.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ContextCapturingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT006";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "FlowContext stored in instance field",
            messageFormat: "Flow component '{0}' stores FlowContext in field '{1}'. Context is per-request and must not be captured.",
            category: "FlowT.ThreadSafety",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "FlowContext is created per-request and contains request-specific data. " +
                        "Storing it in an instance field of a singleton component will leak data between requests. " +
                        "Use FlowContext only as a method parameter.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
                // Check if field type is FlowContext
                if (field.Type.Name == "FlowContext")
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
    }
}
