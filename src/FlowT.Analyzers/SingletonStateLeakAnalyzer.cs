using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects when singleton flow components have instance state that could be modified by multiple threads.
    /// This catches patterns like mutable collections, counters, caches that aren't thread-safe.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SingletonStateLeakAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT019";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Potential state leak in singleton flow component",
            messageFormat: "Flow component '{0}' has mutable instance state '{1}' that can leak data between requests. Singleton components share state across all requests - use thread-safe alternatives or FlowContext.",
            category: "FlowT.ThreadSafety",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Singleton flow components (handlers, policies, specifications) are shared between all concurrent requests. " +
                        "Any mutable instance state (fields, properties, auto-properties) will be visible to all requests simultaneously, " +
                        "causing data leaks and race conditions. Use thread-safe collections (ConcurrentDictionary, ConcurrentBag) " +
                        "or store per-request data in FlowContext.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        // Common mutable types that indicate state leak risk
        private static readonly string[] MutableStateTypes =
        [
            "StringBuilder",
            "MemoryStream",
            "StreamWriter",
            "StreamReader",
            "HttpClient",
            "Stopwatch",
            "Timer",
            "Random",
            "ArraySegment"
        ];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            INamedTypeSymbol namedType = (INamedTypeSymbol)context.Symbol;

            if (!IsFlowComponent(namedType))
                return;
            // Check all fields
            foreach (IFieldSymbol field in namedType.GetMembers().OfType<IFieldSymbol>())
            {
                // Skip static, readonly, const
                if (field.IsStatic || field.IsReadOnly || field.IsConst)
                    continue;
                // Check if it's a potentially leaky mutable type
                if (IsMutableStateType(field.Type))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        field.Locations[0],
                        namedType.Name,
                        field.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // Check all auto-properties (they create backing fields)
            foreach (IPropertySymbol property in namedType.GetMembers().OfType<IPropertySymbol>())
            {
                // Skip readonly properties, init-only setters
                if (property.IsReadOnly || property.SetMethod == null)
                    continue;
                // Check if setter is init-only
                if (property.SetMethod.IsInitOnly)
                    continue;
                // Skip static properties
                if (property.IsStatic)
                    continue;
                // Check if it's a potentially leaky mutable type
                if (IsMutableStateType(property.Type))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        property.Locations[0],
                        namedType.Name,
                        property.Name);
                    context.ReportDiagnostic(diagnostic);
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
                if (baseType.Name is "FlowPolicy" or "FlowSpecification")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }

        private bool IsMutableStateType(ITypeSymbol type)
        {
            string typeName = type.Name;
            // Check against known mutable state types
            if (MutableStateTypes.Contains(typeName))
                return true;
            // Check for arrays (always mutable)
            if (type.TypeKind == TypeKind.Array)
                return true;
            // Check for reference types that aren't thread-safe collections
            // (List, Dictionary, Queue, Stack, etc. - already caught by NonThreadSafeCollectionAnalyzer)
            return false;
        }
    }
}
