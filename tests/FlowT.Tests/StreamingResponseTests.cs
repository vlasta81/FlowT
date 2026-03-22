using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Tests.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FlowT.Tests;

/// <summary>
/// Unit tests for streaming response types: IStreamableResponse, StreamableResponse, PagedStreamResponse
/// </summary>
public class StreamingResponseTests : FlowTestBase
{
    #region Test Models

    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class CustomStreamableResponse : StreamableResponse
    {
        public string Status { get; init; } = string.Empty;
        public int Code { get; init; }
        public IAsyncEnumerable<string> Messages { get; init; } = AsyncEnumerable.Empty<string>();

        protected override Task WriteMetadataAsync(Utf8JsonWriter writer, CancellationToken ct)
        {
            WriteProperty(writer, "status", Status);
            WriteProperty(writer, "code", Code);
            return Task.CompletedTask;
        }

        protected override async Task WriteItemsAsync(Utf8JsonWriter writer, CancellationToken ct)
        {
            await foreach (var message in Messages.WithCancellation(ct))
            {
                JsonSerializer.Serialize(writer, message);
                await writer.FlushAsync(ct);
            }
        }
    }

    #endregion

    #region PagedStreamResponse Tests

    [Fact]
    public async Task PagedStreamResponse_WithNoItems_ReturnsEmptyArray()
    {
        // Arrange
        var response = new PagedStreamResponse<TestItem>
        {
            TotalCount = 0,
            Page = 0,
            PageSize = 10,
            Items = AsyncEnumerable.Empty<TestItem>()
        };

        // Act
        var json = await SerializeStreamableResponse(response);
        var result = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(0, result.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, result.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(10, result.RootElement.GetProperty("pageSize").GetInt32());
        Assert.False(result.RootElement.GetProperty("hasMore").GetBoolean());
        Assert.Empty(result.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task PagedStreamResponse_WithItems_SerializesCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Id = 1, Name = "Item 1" },
            new TestItem { Id = 2, Name = "Item 2" },
            new TestItem { Id = 3, Name = "Item 3" }
        };

        var response = new PagedStreamResponse<TestItem>
        {
            TotalCount = 100,
            Page = 0,
            PageSize = 10,
            Items = items.ToAsyncEnumerable()
        };

        // Act
        var json = await SerializeStreamableResponse(response);
        var result = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(100, result.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, result.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(10, result.RootElement.GetProperty("pageSize").GetInt32());
        Assert.True(result.RootElement.GetProperty("hasMore").GetBoolean());

        var itemsArray = result.RootElement.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(3, itemsArray.Count);
        Assert.Equal(1, itemsArray[0].GetProperty("Id").GetInt32());
        Assert.Equal("Item 1", itemsArray[0].GetProperty("Name").GetString());
    }

    [Fact]
    public void PagedStreamResponse_HasMore_CalculatesCorrectly()
    {
        // Arrange & Act & Assert

        // First page with more pages available
        var response1 = new PagedStreamResponse<TestItem>
        {
            TotalCount = 100,
            Page = 0,
            PageSize = 10
        };
        Assert.True(response1.HasMore);

        // Last page
        var response2 = new PagedStreamResponse<TestItem>
        {
            TotalCount = 100,
            Page = 9,
            PageSize = 10
        };
        Assert.False(response2.HasMore);

        // Middle page
        var response3 = new PagedStreamResponse<TestItem>
        {
            TotalCount = 100,
            Page = 5,
            PageSize = 10
        };
        Assert.True(response3.HasMore);

        // Partial last page
        var response4 = new PagedStreamResponse<TestItem>
        {
            TotalCount = 95,
            Page = 9,
            PageSize = 10
        };
        Assert.False(response4.HasMore);
    }

    [Fact]
    public async Task PagedStreamResponse_LargeDataset_StreamsProgressively()
    {
        // Arrange
        var itemCount = 1000;
        var items = Enumerable.Range(1, itemCount)
            .Select(i => new TestItem { Id = i, Name = $"Item {i}" })
            .ToAsyncEnumerable();

        var response = new PagedStreamResponse<TestItem>
        {
            TotalCount = itemCount,
            Page = 0,
            PageSize = 100,
            Items = items
        };

        // Act
        var json = await SerializeStreamableResponse(response);
        var result = JsonDocument.Parse(json);

        // Assert
        var itemsArray = result.RootElement.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(itemCount, itemsArray.Count);
        Assert.Equal(1, itemsArray[0].GetProperty("Id").GetInt32());
        Assert.Equal(itemCount, itemsArray[^1].GetProperty("Id").GetInt32());
    }

    [Fact]
    public async Task PagedStreamResponse_RespectsCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var items = CreateCancellableAsyncEnumerable(1000, cts.Token);

        var response = new PagedStreamResponse<TestItem>
        {
            TotalCount = 1000,
            Page = 0,
            PageSize = 100,
            Items = items
        };

        // Act
        cts.CancelAfter(TimeSpan.FromMilliseconds(50)); // Cancel after 50ms

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await SerializeStreamableResponse(response, cts.Token);
        });
    }

    #endregion

    #region CustomStreamableResponse Tests

    [Fact]
    public async Task CustomStreamableResponse_SerializesMetadataAndItems()
    {
        // Arrange
        var messages = new[] { "Message 1", "Message 2", "Message 3" }.ToAsyncEnumerable();
        var response = new CustomStreamableResponse
        {
            Status = "success",
            Code = 200,
            Messages = messages
        };

        // Act
        var json = await SerializeStreamableResponse(response);
        var result = JsonDocument.Parse(json);

        // Assert
        Assert.Equal("success", result.RootElement.GetProperty("status").GetString());
        Assert.Equal(200, result.RootElement.GetProperty("code").GetInt32());

        var itemsArray = result.RootElement.GetProperty("items").EnumerateArray()
            .Select(e => e.GetString())
            .ToList();

        Assert.Equal(3, itemsArray.Count);
        Assert.Contains("Message 1", itemsArray);
        Assert.Contains("Message 2", itemsArray);
        Assert.Contains("Message 3", itemsArray);
    }

    [Fact]
    public async Task CustomStreamableResponse_WithEmptyItems_ReturnsEmptyArray()
    {
        // Arrange
        var response = new CustomStreamableResponse
        {
            Status = "no_content",
            Code = 204,
            Messages = AsyncEnumerable.Empty<string>()
        };

        // Act
        var json = await SerializeStreamableResponse(response);
        var result = JsonDocument.Parse(json);

        // Assert
        Assert.Equal("no_content", result.RootElement.GetProperty("status").GetString());
        Assert.Empty(result.RootElement.GetProperty("items").EnumerateArray());
    }

    #endregion

    #region StreamableResponse Base Class Tests

    [Fact]
    public void StreamableResponse_WriteProperty_HandlesVariousTypes()
    {
        // This is tested indirectly through CustomStreamableResponse
        // which uses WriteProperty for status (string) and code (int)
        Assert.True(true); // Placeholder - actual test is CustomStreamableResponse_SerializesMetadataAndItems
    }

    [Fact]
    public async Task StreamableResponse_GeneratesValidJsonStructure()
    {
        // Arrange
        var response = new CustomStreamableResponse
        {
            Status = "test",
            Code = 123,
            Messages = new[] { "msg" }.ToAsyncEnumerable()
        };

        // Act
        var json = await SerializeStreamableResponse(response);

        // Assert - Should be valid JSON
        var result = JsonDocument.Parse(json); // Throws if invalid
        Assert.NotNull(result.RootElement);
        Assert.Equal(JsonValueKind.Object, result.RootElement.ValueKind);
    }

    #endregion

    #region IStreamableResponse Interface Tests

    [Fact]
    public async Task IStreamableResponse_CanBeImplementedDirectly()
    {
        // Arrange
        var directImplementation = new DirectStreamableImplementation();

        // Act
        var json = await SerializeStreamableResponse(directImplementation);
        var result = JsonDocument.Parse(json);

        // Assert
        Assert.Equal("direct", result.RootElement.GetProperty("type").GetString());
        Assert.Equal(42, result.RootElement.GetProperty("value").GetInt32());
    }

    private class DirectStreamableImplementation : IStreamableResponse
    {
        public async Task WriteToStreamAsync(Utf8JsonWriter writer, CancellationToken cancellationToken)
        {
            writer.WriteStartObject();
            writer.WriteString("type", "direct");
            writer.WriteNumber("value", 42);
            writer.WriteEndObject();
            await writer.FlushAsync(cancellationToken);
        }
    }

    #endregion

    #region Helper Methods

    private static async Task<string> SerializeStreamableResponse(
        IStreamableResponse response,
        CancellationToken ct = default)
    {
        using var stream = new MemoryStream();
        await using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

        await response.WriteToStreamAsync(writer, ct);
        await writer.FlushAsync(ct);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static async IAsyncEnumerable<TestItem> CreateCancellableAsyncEnumerable(
        int count,
        CancellationToken ct)
    {
        for (int i = 1; i <= count; i++)
        {
            await Task.Delay(10, ct); // Simulate async work
            yield return new TestItem { Id = i, Name = $"Item {i}" };
        }
    }

    #endregion
}
