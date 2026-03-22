using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FlowT.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CancellationTokenSourceStorageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "FlowT013";
    private const string Category = "Threading";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "CancellationTokenSource stored in field",
        "Flow component '{0}' stores CancellationTokenSource in field '{1}'. CancellationTokenSource is per-request and must not be shared between requests.",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "CancellationTokenSource stored in a field will be shared between concurrent requests, causing cancellation to affect all requests. Create CancellationTokenSource per-request instead.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        FieldDeclarationSyntax fieldDeclaration = (FieldDeclarationSyntax)context.Node;        
        // Check if this is in a flow component
        INamedTypeSymbol? containingType = context.ContainingSymbol?.ContainingType;
        if (containingType is null || !IsFlowComponent(containingType))
        {
            return;
        }
        // Check each variable in the field declaration
        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
        {
            IFieldSymbol? fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (fieldSymbol == null)
            {
                continue;
            }
            // Check if the field type is CancellationTokenSource
            if (IsCancellationTokenSource(fieldSymbol.Type))
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    variable.GetLocation(),
                    containingType.Name,
                    fieldSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsFlowComponent(INamedTypeSymbol type)
    {
        // Check if implements IFlowHandler<,>
        if (type.AllInterfaces.Any(i => i.Name == "IFlowHandler" && i.TypeArguments.Length == 2)) return true;
        // Check if inherits from FlowPolicy<,> or FlowSpecification<,>
        return InheritsFromFlowPolicy(type);
    }

    private static bool InheritsFromFlowPolicy(INamedTypeSymbol type)
    {
        INamedTypeSymbol? baseType = type.BaseType;
        while (baseType is not null)
        {
            if (baseType.Name is "FlowPolicy" or "FlowSpecification" && baseType.TypeArguments.Length == 2)
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static bool IsCancellationTokenSource(ITypeSymbol type)
    {
        // Check exact type name and namespace
        return type.Name == "CancellationTokenSource" && type.ContainingNamespace?.ToDisplayString() == "System.Threading";
    }
}
