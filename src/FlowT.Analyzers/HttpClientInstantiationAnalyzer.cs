using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects direct HttpClient instantiation (new HttpClient()) inside flow components.
    /// Use IHttpClientFactory.CreateClient() to avoid socket exhaustion.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HttpClientInstantiationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT023";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "HttpClient instantiated directly in flow component",
            messageFormat: "Flow component '{0}' creates HttpClient directly with 'new HttpClient()'. Use IHttpClientFactory.CreateClient() to avoid socket exhaustion.",
            category: "FlowT.BestPractices",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Creating HttpClient with 'new HttpClient()' in flow components causes socket exhaustion because " +
                        "HttpClient instances are not properly pooled. Inject IHttpClientFactory and use CreateClient() instead, " +
                        "which correctly manages the underlying HttpMessageHandler pool.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        }

        private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            ObjectCreationExpressionSyntax objectCreation = (ObjectCreationExpressionSyntax)context.Node;

            if (GetTypeName(objectCreation.Type) != "HttpClient")
                return;

            MethodDeclarationSyntax? method = objectCreation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method is null)
                return;

            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol is null || !IsFlowComponent(methodSymbol.ContainingType))
                return;

            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                objectCreation.GetLocation(),
                methodSymbol.ContainingType.Name);

            context.ReportDiagnostic(diagnostic);
        }

        private string GetTypeName(TypeSyntax type)
        {
            if (type is IdentifierNameSyntax identifier)
                return identifier.Identifier.Text;
            if (type is QualifiedNameSyntax qualified)
                return qualified.Right.Identifier.Text;
            return string.Empty;
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
