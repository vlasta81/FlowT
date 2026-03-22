using FlowT;

namespace FlowT.Tests;

public class FlowInterruptTests
{
    [Fact]
    public void Fail_CreatesFailureInterrupt()
    {
        var interrupt = FlowInterrupt<string>.Fail("Validation error");

        Assert.True(interrupt.IsFailure);
        Assert.False(interrupt.IsEarlyReturn);
        Assert.Equal("Validation error", interrupt.Message);
        Assert.Null(interrupt.Response);
        Assert.Equal(400, interrupt.StatusCode);
    }

    [Fact]
    public void Fail_WithCustomStatusCode_UsesProvidedCode()
    {
        var interrupt = FlowInterrupt<string>.Fail("Server error", 500);

        Assert.Equal(500, interrupt.StatusCode);
        Assert.Equal("Server error", interrupt.Message);
    }

    [Fact]
    public void Stop_CreatesEarlyReturnInterrupt()
    {
        var response = "Early result";
        var interrupt = FlowInterrupt<string>.Stop(response);

        Assert.True(interrupt.IsEarlyReturn);
        Assert.False(interrupt.IsFailure);
        Assert.Null(interrupt.Message);
        Assert.Equal("Early result", interrupt.Response);
        Assert.Equal(200, interrupt.StatusCode);
    }

    [Fact]
    public void Stop_WithCustomStatusCode_UsesProvidedCode()
    {
        var interrupt = FlowInterrupt<int>.Stop(42, 201);

        Assert.Equal(201, interrupt.StatusCode);
        Assert.Equal(42, interrupt.Response);
    }

    [Fact]
    public void IsFailure_ReturnsFalse_ForEarlyReturn()
    {
        var interrupt = FlowInterrupt<string>.Stop("Result");

        Assert.False(interrupt.IsFailure);
    }

    [Fact]
    public void IsEarlyReturn_ReturnsFalse_ForFailure()
    {
        var interrupt = FlowInterrupt<string>.Fail("Error");

        Assert.False(interrupt.IsEarlyReturn);
    }

    [Fact]
    public void FlowInterrupt_CanWrapComplexTypes()
    {
        var complexResponse = new TestResponse { Id = 123, Name = "Test" };
        var interrupt = FlowInterrupt<TestResponse>.Stop(complexResponse);

        Assert.Equal(123, interrupt.Response?.Id);
        Assert.Equal("Test", interrupt.Response?.Name);
    }

    [Fact]
    public void FlowInterrupt_WithNullResponse_StillWorksForStop()
    {
        var interrupt = FlowInterrupt<string?>.Stop(null);

        // IsEarlyReturn checks: Response is not null AND Message is null
        // Since Response IS null, IsEarlyReturn should be false
        Assert.False(interrupt.IsEarlyReturn);
        Assert.False(interrupt.IsFailure);
    }

    [Fact]
    public void FlowInterrupt_DefaultStatusCodes_AreCorrect()
    {
        var failure = FlowInterrupt<string>.Fail("Error");
        var earlyReturn = FlowInterrupt<string>.Stop("Result");

        Assert.Equal(400, failure.StatusCode);
        Assert.Equal(200, earlyReturn.StatusCode);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(204)]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(404)]
    [InlineData(500)]
    public void FlowInterrupt_SupportsVariousStatusCodes(int statusCode)
    {
        var interrupt = FlowInterrupt<string>.Fail("Test", statusCode);

        Assert.Equal(statusCode, interrupt.StatusCode);
    }

    // Helper class
    public record TestResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
    }
}
