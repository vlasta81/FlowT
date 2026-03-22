using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects async method invocations without passing CancellationToken.
    /// Cancellation tokens enable operations to be cancelled gracefully.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingCancellationTokenAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT009";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Missing CancellationToken in async call",
            messageFormat: "Async method '{0}' has a CancellationToken parameter that is not being passed. Use 'context.CancellationToken' to enable cancellation.",
            category: "FlowT.AsyncPatterns",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Async operations should propagate CancellationToken to enable graceful cancellation. " +
                        "Use context.CancellationToken when calling async methods that accept a CancellationToken parameter.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;
            // Get the containing method
            MethodDeclarationSyntax method = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method is null)
                return;
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol is null || !IsFlowComponent(methodSymbol.ContainingType))
                return;
            // Get the invoked method symbol
            SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is not IMethodSymbol invokedMethod)
                return;
            // Check if the invoked method has a CancellationToken parameter
            var ctParameter = invokedMethod.Parameters.FirstOrDefault(p => p.Type.Name == "CancellationToken");
            if (ctParameter is null)
                return;
            // Check if CancellationToken is being passed in the call
            SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;
            bool ctPassed = arguments.Any(arg =>
            {
                TypeInfo argType = context.SemanticModel.GetTypeInfo(arg.Expression);
                return argType.Type?.Name == "CancellationToken";
            });

            if (!ctPassed)
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    invocation.GetLocation(),
                    invokedMethod.Name);
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
            while (baseType != null)
            {
                if (baseType.Name == "FlowPolicy")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
