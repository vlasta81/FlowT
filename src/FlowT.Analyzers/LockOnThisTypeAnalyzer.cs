using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects lock statements on 'this' or 'typeof(T)'.
    /// Locking on publicly accessible objects can cause deadlocks.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LockOnThisTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT008";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Lock on 'this' or 'typeof(T)' in flow component",
            messageFormat: "Locking on '{0}' is unsafe. External code can acquire the same lock causing deadlocks. Use a private readonly object instead.",
            category: "FlowT.ThreadSafety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Locking on 'this' or 'typeof(T)' exposes the lock to external code, which can cause deadlocks. " +
                        "Use a private readonly object (e.g., 'private readonly object _syncLock = new();') for locking.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeLockStatement, SyntaxKind.LockStatement);
        }

        private void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            LockStatementSyntax lockStatement = (LockStatementSyntax)context.Node;
            // Get the method's containing type
            MethodDeclarationSyntax method = lockStatement.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method == null)
                return;
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null || !IsFlowComponent(methodSymbol.ContainingType))
                return;
            ExpressionSyntax expression = lockStatement.Expression;
            string lockTarget = string.Empty;
            // Check for lock(this)
            if (expression is ThisExpressionSyntax)
            {
                lockTarget = "this";
            }
            // Check for lock(typeof(T))
            else if (expression is TypeOfExpressionSyntax)
            {
                lockTarget = "typeof(T)";
            }
            if (!string.IsNullOrEmpty(lockTarget))
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    expression.GetLocation(),
                    lockTarget);
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
