using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects when a FlowDefinition's Configure() method does not call .Handle&lt;T&gt;().
    /// Every flow must terminate with exactly one handler.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingHandlerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT011";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "FlowDefinition Configure() is missing .Handle<T>()",
            messageFormat: "Flow definition '{0}' does not call .Handle<T>() in Configure(). Every flow must terminate with a handler.",
            category: "FlowT.FlowConfiguration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Every FlowDefinition must call .Handle<T>() exactly once in Configure() to specify the terminal handler. " +
                        "A flow without a handler has no business logic and will throw at runtime.");

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

            if (CountHandleInvocations(method) == 0)
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    method.Identifier.GetLocation(),
                    methodSymbol.ContainingType.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private int CountHandleInvocations(MethodDeclarationSyntax method)
        {
            SyntaxNode? bodyNode = (SyntaxNode?)method.Body ?? method.ExpressionBody;
            if (bodyNode is null)
                return 0;

            return bodyNode.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Count(inv =>
                {
                    if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
                        return memberAccess.Name.Identifier.Text == "Handle";
                    if (inv.Expression is GenericNameSyntax genericName)
                        return genericName.Identifier.Text == "Handle";
                    return false;
                });
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
