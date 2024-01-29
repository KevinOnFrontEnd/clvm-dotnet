using CLVMDotNet.Tools;
using Xunit;

namespace CLVMDotNet.Tests.Tools;

public class NodePathTests
{
    [Fact]
    public void TestNodePath()
    {
        // Arrange
        var top = NodePath.TOP;
        var n = top;
        n.Add(NodePath.Left);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("01", NodePath.ByteArrayToHexString(path));
    }
}