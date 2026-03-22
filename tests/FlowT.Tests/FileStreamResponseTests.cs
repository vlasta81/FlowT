using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FlowT.Tests;

/// <summary>
/// Tests for FileStreamResponse functionality - file downloads and binary streaming.
/// </summary>
public class FileStreamResponseTests : FlowTestBase
{
    #region Basic Functionality Tests

    [Fact]
    public void FileStreamResponse_WithDefaultValues_HasCorrectDefaults()
    {
        // Arrange & Act
        var response = new FileStreamResponse
        {
            Stream = Stream.Null
        };

        // Assert
        Assert.NotNull(response.Stream);
        Assert.Equal("application/octet-stream", response.ContentType);
        Assert.Null(response.FileDownloadName);
        Assert.False(response.EnableRangeProcessing);
        Assert.Null(response.EntityTag);
        Assert.Null(response.LastModified);
    }

    [Fact]
    public void FileStreamResponse_WithCustomValues_StoresCorrectly()
    {
        // Arrange
        var stream = new MemoryStream();
        var lastModified = DateTimeOffset.UtcNow;

        // Act
        var response = new FileStreamResponse
        {
            Stream = stream,
            ContentType = "application/pdf",
            FileDownloadName = "test.pdf",
            EnableRangeProcessing = true,
            EntityTag = "\"12345\"",
            LastModified = lastModified
        };

        // Assert
        Assert.Same(stream, response.Stream);
        Assert.Equal("application/pdf", response.ContentType);
        Assert.Equal("test.pdf", response.FileDownloadName);
        Assert.True(response.EnableRangeProcessing);
        Assert.Equal("\"12345\"", response.EntityTag);
        Assert.Equal(lastModified, response.LastModified);
    }

    [Fact]
    public void FileStreamResponse_Dispose_DisposesStream()
    {
        // Arrange
        var stream = new MemoryStream();
        var response = new FileStreamResponse { Stream = stream };

        // Act
        response.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    #endregion

    #region Flow Integration Tests

    [Fact]
    public async Task FileStreamFlow_ReturnsCorrectFileStream()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DownloadFileHandler>();
        services.AddSingleton<DownloadFileFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<DownloadFileFlow>();

        var request = new DownloadFileRequest { FileName = "test.txt" };

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("text/plain", response.ContentType);
        Assert.Equal("test.txt", response.FileDownloadName);
        Assert.True(response.Stream.CanRead);

        // Read content
        using var reader = new StreamReader(response.Stream);
        var content = await reader.ReadToEndAsync();
        Assert.Equal("Sample file content", content);
    }

    [Fact]
    public async Task FileStreamFlow_WithBinaryData_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DownloadBinaryHandler>();
        services.AddSingleton<DownloadBinaryFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<DownloadBinaryFlow>();

        var request = new DownloadBinaryRequest { DataSize = 1024 };

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("application/octet-stream", response.ContentType);
        Assert.Equal(1024, response.Stream.Length);
    }

    [Fact]
    public async Task FileStreamFlow_WithRangeSupport_EnablesRangeProcessing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DownloadVideoHandler>();
        services.AddSingleton<DownloadVideoFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<DownloadVideoFlow>();

        var request = new DownloadVideoRequest { VideoId = "video123" };

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("video/mp4", response.ContentType);
        Assert.True(response.EnableRangeProcessing);
        Assert.NotNull(response.EntityTag);
        Assert.NotNull(response.LastModified);
        Assert.True(response.Stream.CanSeek); // Required for range processing
    }

    [Fact]
    public async Task FileStreamFlow_WithEmptyFile_ReturnsEmptyStream()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DownloadEmptyFileHandler>();
        services.AddSingleton<DownloadEmptyFileFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<DownloadEmptyFileFlow>();

        var request = new EmptyFileRequest();

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(0, response.Stream.Length);
    }

    #endregion

    #region Large File Simulation Tests

    [Fact]
    public async Task FileStreamFlow_WithLargeFile_UsesConstantMemory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DownloadLargeFileHandler>();
        services.AddSingleton<DownloadLargeFileFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<DownloadLargeFileFlow>();

        var request = new LargeFileRequest { SizeMB = 10 };

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10 * 1024 * 1024, response.Stream.Length);
        Assert.True(response.Stream.CanSeek);

        // Verify we can read from stream without loading entire file
        var buffer = new byte[4096];
        var bytesRead = await response.Stream.ReadAsync(buffer, 0, buffer.Length);
        Assert.Equal(4096, bytesRead);

        // Stream position should be at 4096, not end of file (proves streaming)
        Assert.Equal(4096, response.Stream.Position);
    }

    #endregion

    #region Content Type Tests

    [Theory]
    [InlineData("test.pdf", "application/pdf")]
    [InlineData("test.jpg", "image/jpeg")]
    [InlineData("test.mp4", "video/mp4")]
    [InlineData("test.zip", "application/zip")]
    [InlineData("test.json", "application/json")]
    public async Task FileStreamFlow_DetectsCorrectContentType(string fileName, string expectedContentType)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DetectContentTypeHandler>();
        services.AddSingleton<DetectContentTypeFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<DetectContentTypeFlow>();

        var request = new DetectContentTypeRequest { FileName = fileName };

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.Equal(expectedContentType, response.ContentType);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task FileStreamFlow_RespectsCancellationToken()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<SlowFileHandler>();
        services.AddSingleton<SlowFileFlow>();

        var provider = services.BuildServiceProvider();

        using var cts = new CancellationTokenSource();
        var context = CreateContext(provider, cts.Token);
        var flow = provider.GetRequiredService<SlowFileFlow>();

        var request = new SlowFileRequest();

        // Act & Assert
        cts.Cancel(); // Cancel immediately
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await flow.ExecuteAsync(request, context);
        });
    }

    #endregion

    #region Test Models and Handlers

    // Basic file download
    public record DownloadFileRequest { public string FileName { get; init; } = ""; }

    public class DownloadFileFlow : FlowDefinition<DownloadFileRequest, FileStreamResponse>
    {
        protected override void Configure(IFlowBuilder<DownloadFileRequest, FileStreamResponse> flow)
        {
            flow.Handle<DownloadFileHandler>();
        }
    }

    public class DownloadFileHandler : IFlowHandler<DownloadFileRequest, FileStreamResponse>
    {
        public ValueTask<FileStreamResponse> HandleAsync(DownloadFileRequest request, FlowContext context)
        {
            var content = "Sample file content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            return ValueTask.FromResult(new FileStreamResponse
            {
                Stream = stream,
                ContentType = "text/plain",
                FileDownloadName = request.FileName
            });
        }
    }

    // Binary data download
    public record DownloadBinaryRequest { public int DataSize { get; init; } }

    public class DownloadBinaryFlow : FlowDefinition<DownloadBinaryRequest, FileStreamResponse>
    {
        protected override void Configure(IFlowBuilder<DownloadBinaryRequest, FileStreamResponse> flow)
        {
            flow.Handle<DownloadBinaryHandler>();
        }
    }

    public class DownloadBinaryHandler : IFlowHandler<DownloadBinaryRequest, FileStreamResponse>
    {
        public ValueTask<FileStreamResponse> HandleAsync(DownloadBinaryRequest request, FlowContext context)
        {
            var data = new byte[request.DataSize];
            new Random(42).NextBytes(data); // Deterministic random data
            var stream = new MemoryStream(data);

            return ValueTask.FromResult(new FileStreamResponse
            {
                Stream = stream,
                ContentType = "application/octet-stream"
            });
        }
    }

    // Video streaming with range support
    public record DownloadVideoRequest { public string VideoId { get; init; } = ""; }

    public class DownloadVideoFlow : FlowDefinition<DownloadVideoRequest, FileStreamResponse>
    {
        protected override void Configure(IFlowBuilder<DownloadVideoRequest, FileStreamResponse> flow)
        {
            flow.Handle<DownloadVideoHandler>();
        }
    }

    public class DownloadVideoHandler : IFlowHandler<DownloadVideoRequest, FileStreamResponse>
    {
        public ValueTask<FileStreamResponse> HandleAsync(DownloadVideoRequest request, FlowContext context)
        {
            // Simulate video file
            var videoData = new byte[1024 * 1024]; // 1 MB
            var stream = new MemoryStream(videoData);

            return ValueTask.FromResult(new FileStreamResponse
            {
                Stream = stream,
                ContentType = "video/mp4",
                EnableRangeProcessing = true,
                EntityTag = $"\"{request.VideoId}\"",
                LastModified = DateTimeOffset.UtcNow.AddDays(-1)
            });
        }
    }

    // Empty file
    public record EmptyFileRequest;

    public class DownloadEmptyFileFlow : FlowDefinition<EmptyFileRequest, FileStreamResponse>
    {
        protected override void Configure(IFlowBuilder<EmptyFileRequest, FileStreamResponse> flow)
        {
            flow.Handle<DownloadEmptyFileHandler>();
        }
    }

    public class DownloadEmptyFileHandler : IFlowHandler<EmptyFileRequest, FileStreamResponse>
    {
        public ValueTask<FileStreamResponse> HandleAsync(EmptyFileRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new FileStreamResponse
            {
                Stream = new MemoryStream(),
                ContentType = "text/plain",
                FileDownloadName = "empty.txt"
            });
        }
    }

    // Large file
    public record LargeFileRequest { public int SizeMB { get; init; } }

    public class DownloadLargeFileFlow : FlowDefinition<LargeFileRequest, FileStreamResponse>
    {
        protected override void Configure(IFlowBuilder<LargeFileRequest, FileStreamResponse> flow)
        {
            flow.Handle<DownloadLargeFileHandler>();
        }
    }

    public class DownloadLargeFileHandler : IFlowHandler<LargeFileRequest, FileStreamResponse>
    {
        public ValueTask<FileStreamResponse> HandleAsync(LargeFileRequest request, FlowContext context)
        {
            var sizeBytes = request.SizeMB * 1024 * 1024;
            var stream = new MemoryStream(new byte[sizeBytes]);

            return ValueTask.FromResult(new FileStreamResponse
            {
                Stream = stream,
                ContentType = "application/octet-stream",
                EnableRangeProcessing = true
            });
        }
    }

    // Content type detection
    public record DetectContentTypeRequest { public string FileName { get; init; } = ""; }

    public class DetectContentTypeFlow : FlowDefinition<DetectContentTypeRequest, FileStreamResponse>
    {
        protected override void Configure(IFlowBuilder<DetectContentTypeRequest, FileStreamResponse> flow)
        {
            flow.Handle<DetectContentTypeHandler>();
        }
    }

    public class DetectContentTypeHandler : IFlowHandler<DetectContentTypeRequest, FileStreamResponse>
    {
        public ValueTask<FileStreamResponse> HandleAsync(DetectContentTypeRequest request, FlowContext context)
        {
            var contentType = GetContentType(request.FileName);
            var stream = new MemoryStream(new byte[100]);

            return ValueTask.FromResult(new FileStreamResponse
            {
                Stream = stream,
                ContentType = contentType,
                FileDownloadName = request.FileName
            });
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".mp4" => "video/mp4",
                ".zip" => "application/zip",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }
    }

    // Slow file (for cancellation testing)
    public record SlowFileRequest;

    public class SlowFileFlow : FlowDefinition<SlowFileRequest, FileStreamResponse>
    {
        protected override void Configure(IFlowBuilder<SlowFileRequest, FileStreamResponse> flow)
        {
            flow.Handle<SlowFileHandler>();
        }
    }

    public class SlowFileHandler : IFlowHandler<SlowFileRequest, FileStreamResponse>
    {
        public async ValueTask<FileStreamResponse> HandleAsync(SlowFileRequest request, FlowContext context)
        {
            // Simulate slow file generation
            await Task.Delay(TimeSpan.FromSeconds(10), context.CancellationToken);

            return new FileStreamResponse
            {
                Stream = new MemoryStream(),
                ContentType = "text/plain"
            };
        }
    }

    #endregion
}
