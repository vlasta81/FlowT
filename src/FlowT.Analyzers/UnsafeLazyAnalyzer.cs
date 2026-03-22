using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FlowT.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnsafeLazyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "FlowT018";
    private const string Category = "Threading";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Lazy<T> without thread-safe mode in flow component",
        "Flow component '{0}' uses Lazy<T> in field '{1}' without explicit thread-safe mode. Specify LazyThreadSafetyMode.ExecutionAndPublication or LazyThreadSafetyMode.PublicationOnly.",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Lazy<T> without explicit thread-safety mode defaults to ExecutionAndPublication, but this should be explicit in singleton components. " +
                     "Specify LazyThreadSafetyMode to make thread-safety intent clear and prevent accidental use of None mode.");

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
            if (fieldSymbol is null)
                continue;
            // Check if the field type is Lazy<T>
            if (!IsLazyType(fieldSymbol.Type))
                continue;
            // Check if there's an initializer with constructor call
            if (variable.Initializer?.Value is ObjectCreationExpressionSyntax objectCreation)
            {
                // Check if LazyThreadSafetyMode is specified
                if (!HasThreadSafetyModeArgument(objectCreation, context))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        variable.GetLocation(),
                        containingType.Name,
                        fieldSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else
            {
                // Field declared without initializer or with non-constructor initializer
                // This is also problematic as we can't verify thread-safety
                Diagnostic diagnostic = Diagnostic.Create(
                    Rule,
                    variable.GetLocation(),
                    containingType.Name,
                    fieldSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool HasThreadSafetyModeArgument(ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext context)
    {
        if (objectCreation.ArgumentList is null)
            return false;
        // Check if any argument is LazyThreadSafetyMode enum
        foreach (ArgumentSyntax argument in objectCreation.ArgumentList.Arguments)
        {
            TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(argument.Expression);
            if (typeInfo.Type?.Name == "LazyThreadSafetyMode" && typeInfo.Type.ContainingNamespace?.ToDisplayString() == "System.Threading")
            {
                return true;
            }
            // Also check for member access like LazyThreadSafetyMode.ExecutionAndPublication
            if (argument.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
                if (symbolInfo.Symbol?.ContainingType?.Name == "LazyThreadSafetyMode")
                {
                    return true;
                }
            }
        }
        return false;
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
            if (baseType.Name is "FlowPolicy" or "FlowSpecification" && baseType.TypeArguments.Length == 2)
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static bool IsLazyType(ITypeSymbol type)
    {
        // Check for Lazy<T>
        if (type is not INamedTypeSymbol namedType)
            return false;
        if (namedType.Name != "Lazy")
            return false;
        // Verify it's from System namespace
        return namedType.ContainingNamespace?.ToDisplayString() == "System";
    }
}
