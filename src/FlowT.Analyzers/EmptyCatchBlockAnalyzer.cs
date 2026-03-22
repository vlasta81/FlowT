using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects empty catch blocks that swallow exceptions.
    /// Empty catch blocks hide errors and make debugging difficult.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyCatchBlockAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT014";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Empty catch block swallows exceptions",
            messageFormat: "Empty catch block swallows exceptions in method '{0}'. Log the exception or re-throw it.",
            category: "FlowT.ErrorHandling",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Empty catch blocks hide exceptions and make debugging difficult. " +
                        "Always log exceptions or re-throw them. If you intentionally want to ignore an exception, " +
                        "add a comment explaining why.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
        }

        private void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            CatchClauseSyntax catchClause = (CatchClauseSyntax)context.Node;
            // Get the containing method
            MethodDeclarationSyntax method = catchClause.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method == null)
                return;
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null || !IsFlowComponent(methodSymbol.ContainingType))
                return;
            // Check if catch block is empty (no statements)
            if (catchClause.Block.Statements.Count == 0)
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    catchClause.CatchKeyword.GetLocation(),
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
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
