using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects when a FlowPlugin-derived type is stored in an instance field of a singleton flow component.
    /// FlowPlugin instances have PerFlow lifetime and must be resolved via context.Plugin&lt;T&gt;().
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FlowPluginCapturingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT021";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "FlowPlugin stored in singleton field",
            messageFormat: "Flow component '{0}' stores FlowPlugin type '{1}' in field '{2}'. FlowPlugin instances are PerFlow — use context.Plugin<T>() instead.",
            category: "FlowT.ThreadSafety",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "FlowPlugin instances are created once per FlowContext (PerFlow lifetime). " +
                        "Storing them in an instance field of a singleton component shares one plugin instance " +
                        "across all concurrent requests, causing data leaks and race conditions. " +
                        "Use context.Plugin<T>() to resolve the plugin per-request.");

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
                if (InheritsFromFlowPlugin(field.Type))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        field.Locations[0],
                        namedType.Name,
                        field.Type.Name,
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

        private bool InheritsFromFlowPlugin(ITypeSymbol type)
        {
            INamedTypeSymbol? current = (type as INamedTypeSymbol)?.BaseType;
            while (current is not null)
            {
                if (current.Name == "FlowPlugin")
                    return true;
                current = current.BaseType;
            }
            return false;
        }
    }
}
