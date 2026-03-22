using FlowT;

namespace FlowT.Tests;

/// <summary>
/// Tests for FlowContext named key feature.
/// Demonstrates storing multiple values of the same type under different keys.
/// </summary>
public class FlowContextNamedKeysTests
{
    [Fact]
    public void Set_WithNamedKey_StoresMultipleValuesOfSameType()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        context.Set("Admin User", "admin");
        context.Set("Guest User", "guest");
        context.Set("Default User"); // No key = default

        context.TryGet<string>(out var admin, "admin");
        context.TryGet<string>(out var guest, "guest");
        context.TryGet<string>(out var defaultUser);

        Assert.Equal("Admin User", admin);
        Assert.Equal("Guest User", guest);
        Assert.Equal("Default User", defaultUser);
    }

    [Fact]
    public void TryGet_WithWrongKey_ReturnsFalse()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        context.Set("Value", "key1");

        bool found = context.TryGet<string>(out var value, "key2");

        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void GetOrAdd_WithNamedKey_WorksCorrectly()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        var list1 = context.GetOrAdd(() => new List<string>(), "cache1");
        var list2 = context.GetOrAdd(() => new List<string>(), "cache2");
        var list1Again = context.GetOrAdd(() => new List<string>(), "cache1");

        Assert.NotSame(list1, list2); // Different keys = different instances
        Assert.Same(list1, list1Again); // Same key = same instance
    }

    [Fact]
    public void Push_WithNamedKey_RestoresCorrectValue()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        context.Set("Original", "key1");
        context.Set("Other", "key2");

        using (context.Push("Temporary", "key1"))
        {
            context.TryGet<string>(out var temp, "key1");
            context.TryGet<string>(out var other, "key2");

            Assert.Equal("Temporary", temp);
            Assert.Equal("Other", other); // key2 should be unaffected
        }

        context.TryGet<string>(out var restored, "key1");
        Assert.Equal("Original", restored);
    }

    [Fact]
    public void NamedKeys_WithDifferentTypes_WorkIndependently()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        // Same key name, different types
        context.Set("String value", "key");
        context.Set(42, "key");
        context.Set(true, "key");

        context.TryGet<string>(out var str, "key");
        context.TryGet<int>(out var num, "key");
        context.TryGet<bool>(out var flag, "key");

        Assert.Equal("String value", str);
        Assert.Equal(42, num);
        Assert.True(flag);
    }

    [Fact]
    public void GetOrAdd_WithArg_AndNamedKey_WorksCorrectly()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        var result1 = context.GetOrAdd(10, x => x * 2, "calc1");
        var result2 = context.GetOrAdd(20, x => x * 2, "calc2");
        var result1Again = context.GetOrAdd(999, x => x * 2, "calc1"); // Should return cached 20

        Assert.Equal(20, result1);
        Assert.Equal(40, result2);
        Assert.Equal(20, result1Again); // Factory not called, returns cached
    }

    [Fact]
    public void Set_Overwrites_PreviousValueWithSameKey()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        context.Set("First", "key");
        context.Set("Second", "key");

        context.TryGet<string>(out var value, "key");

        Assert.Equal("Second", value);
    }

    [Fact]
    public void NamedKeys_AreThreadSafe()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            string key = $"key{i % 10}"; // 10 different keys
            context.Set($"Value{i}", key);
            context.TryGet<string>(out var value, key);
        })).ToArray();

        Task.WaitAll(tasks);

        // Verify all keys exist
        for (int i = 0; i < 10; i++)
        {
            bool found = context.TryGet<string>(out _, $"key{i}");
            Assert.True(found);
        }
    }

    [Fact]
    public void DefaultKey_AndNamedKey_AreIndependent()
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        context.Set("Default Value");
        context.Set("Named Value", "mykey");

        context.TryGet<string>(out var defaultVal);
        context.TryGet<string>(out var namedVal, "mykey");

        Assert.Equal("Default Value", defaultVal);
        Assert.Equal("Named Value", namedVal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("key")]
    [InlineData("very-long-key-name-with-special-chars-!@#$%")]
    public void Set_WorksWithVariousKeyNames(string? keyName)
    {
        var context = new FlowContext
        {
            Services = null!,
            CancellationToken = CancellationToken.None
        };

        context.Set("Test Value", keyName);
        bool found = context.TryGet<string>(out var value, keyName);

        Assert.True(found);
        Assert.Equal("Test Value", value);
    }
}
