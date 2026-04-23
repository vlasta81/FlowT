using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace FlowT.Analyzers
{
    /// <summary>
    /// Detects non-thread-safe collections in singleton flow components.
    /// Collections like List, Dictionary, HashSet can cause race conditions when shared between requests.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonThreadSafeCollectionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FlowT002";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Non-thread-safe collection in singleton flow component",
            messageFormat: "Field '{0}' uses non-thread-safe collection '{1}'. Use ConcurrentDictionary, ConcurrentBag, or ImmutableList instead.",
            category: "FlowT.ThreadSafety",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Flow components are singletons shared between concurrent requests. " +
                        "Non-thread-safe collections (List<T>, Dictionary<TKey,TValue>, HashSet<T>) cause race conditions. " +
                        "Use thread-safe alternatives: ConcurrentDictionary, ConcurrentBag, ImmutableList, or readonly collections.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        // Known non-thread-safe collection types
        private static readonly string[] UnsafeCollections = new[]
        {
            "List",
            "Dictionary",
            "HashSet",
            "Queue",
            "Stack",
            "SortedList",
            "SortedSet",
            "SortedDictionary",
            "LinkedList"
        };

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
            foreach (IFieldSymbol field in namedType.GetMembers().OfType<IFieldSymbol>())
            {
                // Skip readonly fields (they're safe if initialized once)
                if (field.IsReadOnly)
                    continue;
                // Check if field type is an unsafe collection
                if (IsUnsafeCollection(field.Type))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        Rule,
                        field.Locations[0],
                        field.Name,
                        GetCollectionTypeName(field.Type));
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
            while (baseType != null)
            {
                if (baseType.Name is "FlowPolicy" or "FlowSpecification")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }

        private bool IsUnsafeCollection(ITypeSymbol type)
        {
            // Check if it's a generic type (List<T>, Dictionary<K,V>, etc.)
            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                string typeName = namedType.ConstructedFrom.Name;
                return UnsafeCollections.Any(unsafeName => typeName.Contains(unsafeName));
            }
            // Check non-generic types (ArrayList, Hashtable, etc.)
            string name = type.Name;
            return UnsafeCollections.Any(unsafeName => name.Contains(unsafeName));
        }

        private string GetCollectionTypeName(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                return namedType.ConstructedFrom.ToDisplayString();
            }
            return type.Name;
        }
    }
}
