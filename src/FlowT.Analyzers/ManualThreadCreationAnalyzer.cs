using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FlowT.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ManualThreadCreationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "FlowT017";
    private const string Category = "Threading";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Manual Thread creation in flow component",
        "Flow component '{0}' creates Thread manually. Use Task.Run() or ThreadPool instead for better resource management.",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Creating threads manually with 'new Thread()' bypasses the thread pool and allocates ~1MB stack per thread. " +
                     "Use Task.Run() or ThreadPool.QueueUserWorkItem() instead for better performance and resource management.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        ObjectCreationExpressionSyntax objectCreation = (ObjectCreationExpressionSyntax)context.Node;        
        // Check if this is in a flow component
        INamedTypeSymbol? containingType = context.ContainingSymbol?.ContainingType;
        if (containingType is null || !IsFlowComponent(containingType))
            return;
        // Get the type being created
        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
        if (typeInfo.Type is null)
            return;
        // Check if creating a Thread
        if (typeInfo.Type.Name == "Thread" && typeInfo.Type.ContainingNamespace?.ToDisplayString() == "System.Threading")
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                objectCreation.GetLocation(),
                containingType.Name);
            context.ReportDiagnostic(diagnostic);
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
}
