using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FlowT.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TaskStorageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "FlowT016";
    private const string Category = "Threading";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Task or ValueTask stored in field",
        "Flow component '{0}' stores Task/ValueTask in field '{1}'. Tasks are per-operation and should not be cached in fields shared between requests.",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Storing Task or ValueTask in a field can lead to awaiting the same task multiple times across different requests, " +
                     "causing incorrect behavior. Tasks should be created per-request and awaited immediately.");

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
            return;
        // Check each variable in the field declaration
        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
        {
            IFieldSymbol? fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (fieldSymbol == null)
                continue;
            // Check if the field type is Task or ValueTask
            if (IsTaskLike(fieldSymbol.Type))
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
        if (type.AllInterfaces.Any(i => 
            i.Name == "IFlowHandler" && i.TypeArguments.Length == 2))
            return true;
        // Check if inherits from FlowPolicy<,> or FlowSpecification<,>
        return InheritsFromFlowPolicy(type);
    }

    private static bool InheritsFromFlowPolicy(INamedTypeSymbol type)
    {
        INamedTypeSymbol? baseType = type.BaseType;
        while (baseType is not null)
        {
            if (baseType.Name is "FlowPolicy" or "FlowSpecification" && 
                baseType.TypeArguments.Length == 2)
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static bool IsTaskLike(ITypeSymbol type)
    {
        // Check for Task, Task<T>, ValueTask, ValueTask<T>
        string typeName = type.Name;
        if (typeName != "Task" && typeName != "ValueTask")
            return false;
        // Verify it's from System.Threading.Tasks namespace
        string? ns = type.ContainingNamespace?.ToDisplayString();
        return ns == "System.Threading.Tasks";
    }
}
