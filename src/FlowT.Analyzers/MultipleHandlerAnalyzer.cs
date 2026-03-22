using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects when a FlowDefinition's Configure() method calls .Handle&lt;T&gt;() more than once.
    /// A flow must have exactly one terminal handler.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MultipleHandlerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT022";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "FlowDefinition Configure() calls .Handle<T>() more than once",
            messageFormat: "Flow definition '{0}' calls .Handle<T>() {1} times. A flow must have exactly one handler.",
            category: "FlowT.FlowConfiguration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Each FlowDefinition must call .Handle<T>() exactly once in Configure(). " +
                        "Multiple handlers are not supported in a single pipeline. " +
                        "Split into separate flows if multiple handlers are needed.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax method = (MethodDeclarationSyntax)context.Node;

            if (method.Identifier.Text != "Configure")
                return;

            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol is null)
                return;

            if (!InheritsFromFlowDefinition(methodSymbol.ContainingType))
                return;

            List<InvocationExpressionSyntax> handleInvocations = GetHandleInvocations(method);

            if (handleInvocations.Count > 1)
            {
                for (int i = 1; i < handleInvocations.Count; i++)
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        handleInvocations[i].GetLocation(),
                        methodSymbol.ContainingType.Name,
                        handleInvocations.Count);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private List<InvocationExpressionSyntax> GetHandleInvocations(MethodDeclarationSyntax method)
        {
            SyntaxNode? bodyNode = (SyntaxNode?)method.Body ?? method.ExpressionBody;
            if (bodyNode is null)
                return new List<InvocationExpressionSyntax>();

            return bodyNode.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv =>
                {
                    if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
                        return memberAccess.Name.Identifier.Text == "Handle";
                    if (inv.Expression is GenericNameSyntax genericName)
                        return genericName.Identifier.Text == "Handle";
                    return false;
                })
                .ToList();
        }

        private bool InheritsFromFlowDefinition(INamedTypeSymbol type)
        {
            INamedTypeSymbol? baseType = type.BaseType;
            while (baseType is not null)
            {
                if (baseType.Name == "FlowDefinition")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
