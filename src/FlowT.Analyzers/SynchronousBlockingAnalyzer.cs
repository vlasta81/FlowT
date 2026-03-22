using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects synchronous blocking calls (.Wait(), .Result, .GetAwaiter().GetResult(), Thread.Sleep) in async methods.
    /// These can cause deadlocks and reduce performance.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SynchronousBlockingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT010";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Synchronous blocking in async method",
            messageFormat: "Using '{0}' blocks the thread in async method '{1}'. Use 'await Task.Delay()' instead to prevent thread pool starvation.",
            category: "FlowT.AsyncPatterns",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Synchronous blocking calls (.Wait(), .Result, .GetAwaiter().GetResult(), Thread.Sleep()) in async methods can cause deadlocks and thread pool starvation. " +
                        "Use 'await' and 'await Task.Delay()' instead to maintain asynchronous flow.");

        private static readonly DiagnosticDescriptor ThreadSleepRule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Thread.Sleep in async method",
            messageFormat: "Using 'Thread.Sleep()' blocks the thread pool in async method '{0}'. Use 'await Task.Delay()' instead.",
            category: "FlowT.AsyncPatterns",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Thread.Sleep() in async methods blocks expensive thread pool threads causing thread starvation. " +
                        "Use 'await Task.Delay()' instead which is non-blocking.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule, ThreadSleepRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            MemberAccessExpressionSyntax memberAccess = (MemberAccessExpressionSyntax)context.Node;

            // Get the containing method
            MethodDeclarationSyntax method = memberAccess.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method is null)
                return;
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol is null || !methodSymbol.IsAsync || !IsFlowComponent(methodSymbol.ContainingType))
                return;
            string memberName = memberAccess.Name.Identifier.Text;
            // Check for .Result property access
            if (memberName == "Result")
            {
                TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
                if (IsTaskLike(typeInfo.Type))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        memberAccess.GetLocation(),
                        ".Result",
                        methodSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

            // Get the containing method
            MethodDeclarationSyntax method = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method is null)
                return;
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol is null || !methodSymbol.IsAsync || !IsFlowComponent(methodSymbol.ContainingType))
                return;
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                string memberName = memberAccess.Name.Identifier.Text;
                // Check for Thread.Sleep()
                if (memberName == "Sleep")
                {
                    SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
                    if (symbolInfo.Symbol is IMethodSymbol sleepMethod)
                    {
                        INamedTypeSymbol containingType = sleepMethod.ContainingType;
                        if (containingType?.Name == "Thread" && containingType.ContainingNamespace?.ToDisplayString() == "System.Threading")
                        {
                            Diagnostic diagnostic = Diagnostic.Create(
                                ThreadSleepRule,
                                invocation.GetLocation(),
                                methodSymbol.Name);
                            context.ReportDiagnostic(diagnostic);
                            return;
                        }
                    }
                }
                // Check for .Wait() or .GetAwaiter().GetResult()
                if (memberName == "Wait" || memberName == "GetResult")
                {
                    TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
                    if (IsTaskLike(typeInfo.Type) || IsAwaiter(typeInfo.Type))
                    {
                        Diagnostic diagnostic = Diagnostic.Create(
                            Rule,
                            invocation.GetLocation(),
                            $".{memberName}()",
                            methodSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
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
                if (baseType.Name == "FlowPolicy")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }

        private bool IsTaskLike(ITypeSymbol? type)
        {
            if (type is null)
                return false;
            string typeName = type.Name;
            return typeName == "Task" || typeName == "ValueTask";
        }

        private bool IsAwaiter(ITypeSymbol? type)
        {
            if (type is null)
                return false;
            return type.Name.Contains("Awaiter");
        }
    }
}
