using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects synchronous file I/O calls (File.ReadAllText, File.WriteAllText, etc.) inside async flow component methods.
    /// These block the thread pool and reduce throughput under load.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SynchronousFileIOAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT024";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Synchronous file I/O in async flow method",
            messageFormat: "Flow component '{0}' calls synchronous '{1}' in async method '{2}'. Use the async equivalent to avoid blocking the thread pool.",
            category: "FlowT.AsyncPatterns",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Synchronous file I/O operations (File.ReadAllText, File.ReadAllBytes, File.WriteAllText, etc.) " +
                        "block the thread pool thread in async flow methods, reducing throughput under load. " +
                        "Use the corresponding async methods (File.ReadAllTextAsync, File.ReadAllBytesAsync, " +
                        "File.WriteAllTextAsync, etc.) with await instead.");

        private static readonly string[] SynchronousFileMethods = new[]
        {
            "ReadAllText",
            "ReadAllBytes",
            "ReadAllLines",
            "WriteAllText",
            "WriteAllBytes",
            "WriteAllLines",
            "AppendAllText",
            "AppendAllLines",
        };

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
            if (!SynchronousFileMethods.Contains(methodName))
                return;

            SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
            IMethodSymbol? invokedMethod = symbolInfo.Symbol as IMethodSymbol;
            if (invokedMethod is null)
                return;

            if (invokedMethod.ContainingType?.Name != "File")
                return;

            MethodDeclarationSyntax? method = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method is null)
                return;

            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol is null || !methodSymbol.IsAsync)
                return;

            if (!IsFlowComponent(methodSymbol.ContainingType))
                return;

            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                invocation.GetLocation(),
                methodSymbol.ContainingType.Name,
                $"File.{methodName}",
                methodSymbol.Name);

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
                if (baseType.Name == "FlowPolicy")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
