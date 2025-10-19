using System.Text.Json;
using HueBuildStatus.Api.Features.Webhooks;

namespace HueBuildStatus.Tests;

public class JsonHelperTests
{
    [Fact]
    public void FindJsonProperty_ObjectWithProperty_ReturnsValue()
    {
        // Arrange
        var json = """{"status": "completed"}""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "status");

        // Assert
        Assert.Equal("completed", result);
    }

    [Fact]
    public void FindJsonProperty_ObjectWithoutProperty_ReturnsNull()
    {
        // Arrange
        var json = """{"other": "value"}""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "status");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindJsonProperty_ArrayWithProperty_ReturnsValue()
    {
        // Arrange
        var json = """[{"status": "completed"}]""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "status");

        // Assert
        Assert.Equal("completed", result);
    }

    [Fact]
    public void FindJsonProperty_ArrayWithoutProperty_ReturnsNull()
    {
        // Arrange
        var json = """[{"other": "value"}]""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "status");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindJsonProperty_NestedObject_ReturnsValue()
    {
        // Arrange
        var json = """{"workflow_run": {"status": "completed"}}""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "status");

        // Assert
        Assert.Equal("completed", result);
    }

    [Fact]
    public void FindJsonProperty_NestedArray_ReturnsValue()
    {
        // Arrange
        var json = """{"items": [{"status": "completed"}]}""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "status");

        // Assert
        Assert.Equal("completed", result);
    }

    [Fact]
    public void FindJsonProperty_DeeplyNested_ReturnsValue()
    {
        // Arrange
        var json = """{"level1": {"level2": {"status": "completed"}}}""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "status");

        // Assert
        Assert.Equal("completed", result);
    }


    [Fact]
    public void FindJsonProperty_NullValue_ReturnsNull()
    {
        // Arrange
        var json = """{"status": null}""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "status");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindJsonProperty_NumberValue_ReturnsNull()
    {
        // Arrange
        var json = """{"count": 42}""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "count");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindJsonProperty_BooleanValue_ReturnsNull()
    {
        // Arrange
        var json = """{"success": true}""";
        var doc = JsonDocument.Parse(json);
        var element = doc.RootElement;

        // Act
        var result = JsonHelper.FindJsonProperty(element, "success");

        // Assert
        Assert.Null(result);
    }
}