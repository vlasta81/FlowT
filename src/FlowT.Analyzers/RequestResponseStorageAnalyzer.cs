using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects when Request or Response objects are stored in instance fields.
    /// Request/Response are per-request data and must not be captured in singleton components.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RequestResponseStorageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT007";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Request or Response object stored in field",
            messageFormat: "Flow component '{0}' stores request/response type '{1}' in field '{2}'. Request/Response are per-request data and must not be captured.",
            category: "FlowT.ThreadSafety",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Request and Response objects contain per-request data. " +
                        "Storing them in fields of singleton components will leak data between requests. " +
                        "Use them only as method parameters and return values.");

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

            if (!IsFlowComponent(namedType, out ITypeSymbol? requestType, out ITypeSymbol? responseType))
                return;

            foreach (IFieldSymbol field in namedType.GetMembers().OfType<IFieldSymbol>())
            {
                // Check if field type matches the Request or Response type
                if (SymbolEqualityComparer.Default.Equals(field.Type, requestType))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        field.Locations[0],
                        namedType.Name,
                        requestType.Name,
                        field.Name);
                    context.ReportDiagnostic(diagnostic);
                }
                else if (SymbolEqualityComparer.Default.Equals(field.Type, responseType))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        field.Locations[0],
                        namedType.Name,
                        responseType.Name,
                        field.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsFlowComponent(INamedTypeSymbol type, out ITypeSymbol? requestType, out ITypeSymbol? responseType)
        {
            requestType = null;
            responseType = null;
            // Check IFlowHandler<TRequest, TResponse>
            INamedTypeSymbol? handlerInterface = type.AllInterfaces.FirstOrDefault(i => i.Name == "IFlowHandler" && i.TypeArguments.Length == 2);
            if (handlerInterface is not null)
            {
                requestType = handlerInterface.TypeArguments[0];
                responseType = handlerInterface.TypeArguments[1];
                return true;
            }
            // Check FlowPolicy<TRequest, TResponse>
            INamedTypeSymbol? baseType = type.BaseType;
            while (baseType is not null)
            {
                if (baseType.Name == "FlowPolicy" && baseType.TypeArguments.Length == 2)
                {
                    requestType = baseType.TypeArguments[0];
                    responseType = baseType.TypeArguments[1];
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
