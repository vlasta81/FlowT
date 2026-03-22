using BenchmarkDotNet.Attributes;
using FlowT;

namespace FlowT.Benchmarks;

/// <summary>
/// Detailed benchmarks comparing FlowContext performance with and without named keys.
/// Measures the overhead of CompositeKey vs default key.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class NamedKeysComparisonBenchmarks
{
    private FlowContext _context = null!;
    private readonly string _testString = "Test Value";
    private readonly User _testUser = new("John", 30);

    [GlobalSetup]
    public void Setup()
    {
        _context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };
    }

    // ============================================
    // Set Operations Comparison
    // ============================================

    [Benchmark(Baseline = true, Description = "Set without key (baseline)")]
    public void Set_WithoutKey()
    {
        _context.Set(_testString);
    }

    [Benchmark(Description = "Set with named key")]
    public void Set_WithNamedKey()
    {
        _context.Set(_testString, "mykey");
    }

    [Benchmark(Description = "Set with empty string key")]
    public void Set_WithEmptyKey()
    {
        _context.Set(_testString, "");
    }

    // ============================================
    // TryGet Operations Comparison
    // ============================================

    [Benchmark(Description = "TryGet without key (baseline)")]
    public bool TryGet_WithoutKey()
    {
        _context.Set(_testString);
        return _context.TryGet<string>(out _);
    }

    [Benchmark(Description = "TryGet with named key")]
    public bool TryGet_WithNamedKey()
    {
        _context.Set(_testString, "mykey");
        return _context.TryGet<string>(out _, "mykey");
    }

    // ============================================
    // Round-trip Comparison
    // ============================================

    [Benchmark(Description = "Set + TryGet without key")]
    public string RoundTrip_WithoutKey()
    {
        _context.Set(_testString);
        _context.TryGet<string>(out var result);
        return result;
    }

    [Benchmark(Description = "Set + TryGet with named key")]
    public string RoundTrip_WithNamedKey()
    {
        _context.Set(_testString, "mykey");
        _context.TryGet<string>(out var result, "mykey");
        return result;
    }

    // ============================================
    // Multiple Values Scenarios
    // ============================================

    [Benchmark(Description = "Store 5 values without keys")]
    public void StoreMultiple_WithoutKeys()
    {
        _context.Set("string value");
        _context.Set(42);
        _context.Set(_testUser);
        _context.Set(new List<string>());
        _context.Set(DateTime.UtcNow);
    }

    [Benchmark(Description = "Store 5 values (same type) with named keys")]
    public void StoreMultiple_SameType_WithNamedKeys()
    {
        _context.Set("value1", "key1");
        _context.Set("value2", "key2");
        _context.Set("value3", "key3");
        _context.Set("value4", "key4");
        _context.Set("value5", "key5");
    }

    // ============================================
    // Cache Scenarios
    // ============================================

    [Benchmark(Description = "Single cache without key")]
    public Dictionary<int, string> SingleCache_WithoutKey()
    {
        return _context.GetOrAdd(() => new Dictionary<int, string>());
    }

    [Benchmark(Description = "3 caches with named keys")]
    public void ThreeCaches_WithNamedKeys()
    {
        var cache1 = _context.GetOrAdd(() => new Dictionary<int, string>(), "users");
        var cache2 = _context.GetOrAdd(() => new Dictionary<int, string>(), "products");
        var cache3 = _context.GetOrAdd(() => new Dictionary<int, string>(), "orders");
    }

    // ============================================
    // Real-world Scenarios
    // ============================================

    [Benchmark(Description = "Multi-user scenario (without named keys)")]
    public void MultiUser_WithoutNamedKeys()
    {
        // Need wrapper types
        _context.Set(new AdminUserWrapper(_testUser));
        _context.Set(new GuestUserWrapper(_testUser));
        _context.Set(new SystemUserWrapper(_testUser));

        _context.TryGet<AdminUserWrapper>(out var admin);
        _context.TryGet<GuestUserWrapper>(out var guest);
        _context.TryGet<SystemUserWrapper>(out var system);
    }

    [Benchmark(Description = "Multi-user scenario (with named keys)")]
    public void MultiUser_WithNamedKeys()
    {
        // Direct storage
        _context.Set(_testUser, "admin");
        _context.Set(_testUser, "guest");
        _context.Set(_testUser, "system");

        _context.TryGet<User>(out var admin, "admin");
        _context.TryGet<User>(out var guest, "guest");
        _context.TryGet<User>(out var system, "system");
    }

    [Benchmark(Description = "Configuration scenario (without named keys)")]
    public void Configuration_WithoutNamedKeys()
    {
        _context.Set(new RetryConfig { MaxRetries = 3 });
        _context.Set(new TimeoutConfig { Seconds = 30 });
        _context.Set(new LoggingConfig { Level = "Debug" });

        _context.TryGet<RetryConfig>(out var retry);
        _context.TryGet<TimeoutConfig>(out var timeout);
        _context.TryGet<LoggingConfig>(out var logging);
    }

    [Benchmark(Description = "Configuration scenario (with named keys)")]
    public void Configuration_WithNamedKeys()
    {
        // Using generic Config class with named keys
        _context.Set(new Config { Value = "3" }, "retry");
        _context.Set(new Config { Value = "30" }, "timeout");
        _context.Set(new Config { Value = "Debug" }, "logging");

        _context.TryGet<Config>(out var retry, "retry");
        _context.TryGet<Config>(out var timeout, "timeout");
        _context.TryGet<Config>(out var logging, "logging");
    }

    // ============================================
    // Key Complexity Impact
    // ============================================

    [Benchmark(Description = "Short key (3 chars)")]
    public void ShortKey()
    {
        _context.Set(_testString, "key");
        _context.TryGet<string>(out _, "key");
    }

    [Benchmark(Description = "Medium key (20 chars)")]
    public void MediumKey()
    {
        _context.Set(_testString, "my_configuration_key");
        _context.TryGet<string>(out _, "my_configuration_key");
    }

    [Benchmark(Description = "Long key (50 chars)")]
    public void LongKey()
    {
        _context.Set(_testString, "very_long_descriptive_key_name_with_underscores_123");
        _context.TryGet<string>(out _, "very_long_descriptive_key_name_with_underscores_123");
    }

    // ============================================
    // Test Data Types
    // ============================================

    public record User(string Name, int Age);

    public record Config
    {
        public string Value { get; init; } = "";
    }

    // Wrapper types for comparison
    public record AdminUserWrapper(User User);
    public record GuestUserWrapper(User User);
    public record SystemUserWrapper(User User);

    public class RetryConfig
    {
        public int MaxRetries { get; init; }
    }

    public class TimeoutConfig
    {
        public int Seconds { get; init; }
    }

    public class LoggingConfig
    {
        public string Level { get; init; } = "";
    }
}
