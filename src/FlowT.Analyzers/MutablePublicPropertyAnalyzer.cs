using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FlowT.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MutablePublicPropertyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "FlowT015";
    private const string Category = "Threading";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Mutable public or internal property in flow component",
        "Flow component '{0}' has mutable {1} property '{2}'. Public/internal properties allow external mutation causing race conditions. Make it readonly or private.",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Mutable public or internal properties in singleton flow components allow external code to modify state, causing race conditions between concurrent requests. Properties should be readonly, use init accessor, or be private.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        PropertyDeclarationSyntax propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        IPropertySymbol? propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
        if (propertySymbol is null)
            return;
        // Check if this is in a flow component
        INamedTypeSymbol containingType = propertySymbol.ContainingType;
        if (containingType is null || !IsFlowComponent(containingType))
            return;
        // Skip static properties (handled by FlowT004)
        if (propertySymbol.IsStatic)
            return;
        // Check if property is public or internal AND has a setter
        if (propertySymbol.DeclaredAccessibility != Accessibility.Public && propertySymbol.DeclaredAccessibility != Accessibility.Internal)
            return;
        // Check if property has a mutable setter
        if (!HasMutableSetter(propertySymbol))
            return;
        string accessibility = propertySymbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
        Diagnostic diagnostic = Diagnostic.Create(
            Rule,
            propertyDeclaration.Identifier.GetLocation(),
            containingType.Name,
            accessibility,
            propertySymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool HasMutableSetter(IPropertySymbol property)
    {
        // No setter at all = readonly
        if (property.SetMethod == null)
            return false;
        // Init-only setter = safe
        if (property.SetMethod.IsInitOnly)
            return false;
        // Has regular setter = mutable
        return true;
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
}
