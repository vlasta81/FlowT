using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects direct IServiceProvider access (GetService, GetRequiredService) inside flow components.
    /// Prefer context.Service&lt;T&gt;() and context.TryService&lt;T&gt;() for clean, testable service resolution.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DirectServiceProviderAccessAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT025";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Direct IServiceProvider access in flow component",
            messageFormat: "Flow component '{0}' calls '{1}' directly on IServiceProvider. Use context.Service<T>() or context.TryService<T>() for clean scoped resolution.",
            category: "FlowT.BestPractices",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Directly calling GetService<T>() or GetRequiredService<T>() on IServiceProvider is a service locator pattern. " +
                        "FlowContext.Service<T>() and FlowContext.TryService<T>() are the preferred way to resolve services in flow components. " +
                        "They automatically use the correct scoped provider and keep the code testable.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            string methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "GetService" && methodName != "GetRequiredService")
                return;

            // Verify the receiver type implements IServiceProvider
            TypeInfo receiverTypeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
            ITypeSymbol? receiverType = receiverTypeInfo.Type;
            if (receiverType is null)
                return;

            bool isServiceProvider = receiverType.Name == "IServiceProvider" ||
                receiverType.AllInterfaces.Any(i => i.Name == "IServiceProvider");

            if (!isServiceProvider)
                return;

            MethodDeclarationSyntax? method = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method is null)
                return;

            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol is null || !IsFlowComponent(methodSymbol.ContainingType))
                return;

            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                invocation.GetLocation(),
                methodSymbol.ContainingType.Name,
                $"{methodName}<T>()");

            context.ReportDiagnostic(diagnostic);
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
