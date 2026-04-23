using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects ConfigureAwait(false) usage in flow components, which can cause context loss.
    /// In ASP.NET Core with FlowT, always use ConfigureAwait(true) or omit it entirely.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConfigureAwaitAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT020";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "ConfigureAwait(false) can cause context loss in flow components",
            messageFormat: "Flow component '{0}' uses ConfigureAwait(false) which can lose HttpContext and FlowContext. Remove ConfigureAwait(false) or use ConfigureAwait(true).",
            category: "FlowT.AsyncAwait",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "ConfigureAwait(false) prevents capturing SynchronizationContext, which means HttpContext, FlowContext, " +
                        "and other ambient state will be lost after the await. In ASP.NET Core applications, you should use " +
                        "ConfigureAwait(true) (default behavior) to preserve request context. ConfigureAwait(false) is only " +
                        "beneficial in library code that doesn't need context.");

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
            // Check if this is a method invocation
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;
            // Check if method name is "ConfigureAwait"
            if (memberAccess.Name.Identifier.Text != "ConfigureAwait")
                return;
            // Check if the containing type is a flow component
            INamedTypeSymbol? containingType = context.SemanticModel.GetEnclosingSymbol(invocation.SpanStart) as INamedTypeSymbol
                ?? (context.SemanticModel.GetEnclosingSymbol(invocation.SpanStart) as IMethodSymbol)?.ContainingType;
            if (containingType == null || !IsFlowComponent(containingType))
                return;
            // Check if argument is 'false'
            if (invocation.ArgumentList.Arguments.Count == 1)
            {
                ArgumentSyntax argument = invocation.ArgumentList.Arguments[0];
                if (argument.Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.FalseLiteralExpression))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        invocation.GetLocation(),
                        containingType.Name);
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
