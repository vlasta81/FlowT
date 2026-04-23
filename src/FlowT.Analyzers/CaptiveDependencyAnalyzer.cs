using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects when singleton flow components capture scoped services in their constructors.
    /// This causes the "captive dependency" anti-pattern where scoped services are disposed after the first request.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CaptiveDependencyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT003";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Captive scoped dependency in singleton flow component",
            messageFormat: "Flow component '{0}' captures scoped service '{1}' in constructor. Use 'context.Service<{1}>()' instead.",
            category: "FlowT.DependencyInjection",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Singleton flow components (handlers, policies, specifications) must not capture scoped services in constructors. " +
                        "Scoped services are disposed after the first request, causing ObjectDisposedException. " +
                        "Use context.Service<T>() inside HandleAsync/CheckAsync methods instead.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        // Known scoped service types (common EF Core and ASP.NET Core types)
        private static readonly string[] KnownScopedTypes = new[]
        {
            "DbContext",
            "IHttpContextAccessor",
            "HttpContext",
            "HttpRequest",
            "HttpResponse",
            "SignInManager",
            "UserManager"
        };

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
        {
            ConstructorDeclarationSyntax constructor = (ConstructorDeclarationSyntax)context.Node;
            INamedTypeSymbol? containingType = context.SemanticModel.GetDeclaredSymbol(constructor)?.ContainingType;
            if (containingType is null || !IsFlowComponent(containingType))
            {
                return;
            }
            // Check each parameter
            foreach (ParameterSyntax parameter in constructor.ParameterList.Parameters)
            {
                IParameterSymbol? parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameter);
                if (parameterSymbol is null)
                {
                    continue;
                }
                ITypeSymbol parameterType = parameterSymbol.Type;
                if (IsScopedService(parameterType))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        parameter.GetLocation(),
                        containingType.Name,
                        parameterType.Name);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsFlowComponent(INamedTypeSymbol type)
        {
            // Check if implements IFlowHandler, IFlowSpecification, or inherits from FlowPolicy
            return type.AllInterfaces.Any(i => i.Name == "IFlowHandler" || i.Name == "IFlowSpecification") || InheritsFromFlowPolicy(type);
        }

        private bool InheritsFromFlowPolicy(INamedTypeSymbol type)
        {
            INamedTypeSymbol? baseType = type.BaseType;
            while (baseType is not null)
            {
                if (baseType.Name is "FlowPolicy" or "FlowSpecification")
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }

        private bool IsScopedService(ITypeSymbol type)
        {
            // Check against known scoped types
            string typeName = type.Name;
            if (KnownScopedTypes.Any(known => typeName == known))
            {
                return true;
            }
            // Check if type inherits from DbContext
            INamedTypeSymbol? baseType = type.BaseType;
            while (baseType is not null)
            {
                if (baseType.Name == "DbContext")
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}
