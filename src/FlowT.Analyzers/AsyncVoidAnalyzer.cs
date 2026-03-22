using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects async void methods in flow components.
    /// Async void methods cannot be awaited and exceptions cannot be caught.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncVoidAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT005";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Async void method in flow component",
            messageFormat: "Method '{0}' is async void. Exceptions cannot be caught. Use async Task or async ValueTask instead.",
            category: "FlowT.AsyncPatterns",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Async void methods do not allow exceptions to be caught and cannot be awaited. " +
                        "Use async Task or async ValueTask for fire-and-forget scenarios. " +
                        "Consider using context.PublishInBackground() for background tasks.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax method = (MethodDeclarationSyntax)context.Node;
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null)
            {
                return;
            }
            // Check if containing type is a flow component
            if (!IsFlowComponent(methodSymbol.ContainingType))
            {
                return;
            }
            // Check if method is async void
            if (methodSymbol.IsAsync && methodSymbol.ReturnsVoid)
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    method.Identifier.GetLocation(),
                    methodSymbol.Name);
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
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
