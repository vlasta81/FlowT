using BenchmarkDotNet.Attributes;
using FlowT;

namespace FlowT.Benchmarks;

/// <summary>
/// Benchmarks for FlowContext operations (Set, Get, TryGet, GetOrAdd).
/// Tests the performance of the type-safe dictionary and TypeKey optimization.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class FlowContextBenchmarks
{
    private FlowContext _context = null!;
    private readonly string _testString = "Test Value";
    private readonly int _testInt = 42;
    private readonly ComplexObject _complexObject = new("Test", 100);

    [GlobalSetup]
    public void Setup()
    {
        _context = new FlowContext
        {
            Services = null,
            CancellationToken = CancellationToken.None
        };
    }

    [Benchmark(Description = "Set<string> single value")]
    public void Set_String()
    {
        _context.Set(_testString);
    }

    [Benchmark(Description = "Set<int> single value")]
    public void Set_Int()
    {
        _context.Set(_testInt);
    }

    [Benchmark(Description = "Set<ComplexObject> single value")]
    public void Set_ComplexObject()
    {
        _context.Set(_complexObject);
    }

    [Benchmark(Description = "Set + TryGet<string> success")]
    public string Set_And_TryGet_String()
    {
        _context.Set(_testString);
        _context.TryGet<string>(out var result);
        return result;
    }

    [Benchmark(Description = "Set + TryGet<string> (returns bool)")]
    public bool Set_And_TryGet_String_Bool()
    {
        _context.Set(_testString);
        return _context.TryGet<string>(out _);
    }

    [Benchmark(Description = "GetOrAdd<List<string>> (cold)")]
    public List<string> GetOrAdd_Cold()
    {
        var ctx = new FlowContext { Services = null, CancellationToken = CancellationToken.None };
        return ctx.GetOrAdd(() => new List<string>());
    }

    [Benchmark(Description = "GetOrAdd<List<string>> (warm)")]
    public List<string> GetOrAdd_Warm()
    {
        return _context.GetOrAdd(() => new List<string>());
    }

    [Benchmark(Description = "Multiple types interleaved")]
    public void MultipleTypes_Interleaved()
    {
        _context.Set("String1");
        _context.Set(42);
        _context.Set(_complexObject);
        _context.Set("String2");
        _context.TryGet<string>(out _);
        _context.TryGet<int>(out _);
        _context.TryGet<ComplexObject>(out _);
    }

    [Benchmark(Description = "Push/Pop scope")]
    public void Push_And_Pop_Scope()
    {
        _context.Set("Original");
        using (_context.Push("Temporary"))
        {
            _context.TryGet<string>(out _);
        }
        _context.TryGet<string>(out _);
    }

    [Benchmark(Description = "10 different types Set+TryGet", OperationsPerInvoke = 20)]
    public void TenDifferentTypes()
    {
        _context.Set("string");
        _context.Set(1);
        _context.Set(2L);
        _context.Set(3.14);
        _context.Set(true);
        _context.Set(new ComplexObject("test", 1));
        _context.Set(new List<string>());
        _context.Set(new Dictionary<string, int>());
        _context.Set(Guid.NewGuid());
        _context.Set(DateTime.UtcNow);

        _context.TryGet<string>(out _);
        _context.TryGet<int>(out _);
        _context.TryGet<long>(out _);
        _context.TryGet<double>(out _);
        _context.TryGet<bool>(out _);
        _context.TryGet<ComplexObject>(out _);
        _context.TryGet<List<string>>(out _);
        _context.TryGet<Dictionary<string, int>>(out _);
        _context.TryGet<Guid>(out _);
        _context.TryGet<DateTime>(out _);
    }

    // ============================================
    // Named Keys Benchmarks
    // ============================================

    [Benchmark(Description = "Set<string> with named key")]
    public void Set_String_WithNamedKey()
    {
        _context.Set(_testString, "mykey");
    }

    [Benchmark(Description = "Set + TryGet with named key")]
    public string Set_And_TryGet_WithNamedKey()
    {
        _context.Set(_testString, "mykey");
        _context.TryGet<string>(out var result, "mykey");
        return result;
    }

    [Benchmark(Description = "Multiple named keys same type")]
    public void MultipleNamedKeys_SameType()
    {
        _context.Set("Admin User", "admin");
        _context.Set("Guest User", "guest");
        _context.Set("System User", "system");

        _context.TryGet<string>(out _, "admin");
        _context.TryGet<string>(out _, "guest");
        _context.TryGet<string>(out _, "system");
    }

    [Benchmark(Description = "GetOrAdd with named key (warm)")]
    public List<string> GetOrAdd_WithNamedKey_Warm()
    {
        return _context.GetOrAdd(() => new List<string>(), "cache1");
    }

    [Benchmark(Description = "Push/Pop with named key")]
    public void Push_And_Pop_WithNamedKey()
    {
        _context.Set("Original", "key");
        using (_context.Push("Temporary", "key"))
        {
            _context.TryGet<string>(out _, "key");
        }
        _context.TryGet<string>(out _, "key");
    }

    [Benchmark(Description = "3 caches with named keys")]
    public void ThreeCaches_WithNamedKeys()
    {
        var cache1 = _context.GetOrAdd(() => new Dictionary<int, string>(), "users");
        var cache2 = _context.GetOrAdd(() => new Dictionary<int, string>(), "products");
        var cache3 = _context.GetOrAdd(() => new Dictionary<int, string>(), "orders");

        cache1[1] = "User1";
        cache2[1] = "Product1";
        cache3[1] = "Order1";
    }

    [Benchmark(Description = "Default vs Named key comparison", OperationsPerInvoke = 4)]
    public void DefaultVsNamedKey_Comparison()
    {
        // Default key
        _context.Set(_testString);
        _context.TryGet<string>(out _);

        // Named key
        _context.Set(_testString, "key");
        _context.TryGet<string>(out _, "key");
    }

    public record ComplexObject(string Name, int Value);
}
