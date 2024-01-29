using System.Globalization;
using System.Numerics;
using CLVMDotNet.Tools;
using Xunit;

namespace CLVMDotNet.Tests.Tools;

public class NodePathTests
{
    public NodePath Reset(NodePath n)
    {
        byte[] pathBlob = n.AsShortPath();
        BigInteger index = new BigInteger(pathBlob);
        NodePath newNodePath = new NodePath(index);
        return newNodePath;
    }
    
    
    public static bool IsValidBinaryString(string binaryString)
    {
        foreach (char c in binaryString)
        {
            if (c != '0' && c != '1')
            {
                return false;
            }
        }
        return true;
    }
    
    public bool CmpToBits(NodePath n, string bits)
    {
        byte[] pathBlob = n.AsShortPath();
        var hex = NodePath.ByteArrayToHexString(pathBlob);
        BigInteger nAsInt = BigInteger.Parse(hex, NumberStyles.HexNumber);
        
        if (!IsValidBinaryString(bits))
        {
            throw new ArgumentException("Invalid binary string", nameof(bits));
        }

        BigInteger result = 0;
        for (int i = bits.Length - 1, j = 0; i >= 0; i--, j++)
        {
            if (bits[i] == '1')
            {
                result += BigInteger.Pow(2, j);
            }
        }

        return nAsInt == result;
    }
    
    [Fact]
    public void TestNodePath()
    {
        // Arrange
        var left_right_left = NodePath.Left
            .Add(NodePath.Right)
            .Add(NodePath.Left);
        var top = NodePath.TOP;
        var n = top;
        n.Add(NodePath.Left);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("01", NodePath.ByteArrayToHexString(path));
    }
    
    [Fact]
    public void TestTopPlusLeft()
    {
        // Arrange
        var n = NodePath.TOP
            .Add(NodePath.Left);

        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("02", NodePath.ByteArrayToHexString(path));
    }
    
    [Fact]
    public void TestToplusLeftPlusRight()
    {
        // Arrange
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("06", NodePath.ByteArrayToHexString(path));
    }
    
    [Fact]
    public void TestToplusLeftPlusRightPlusRight()
    {
        // Arrange
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Right);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("0E", NodePath.ByteArrayToHexString(path));
    }
    
    [Fact]
    public void TestToplusLeftPlusRightPlusRightPlusLeft()
    {
        // Arrange
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Right)
            .Add(NodePath.Left);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("16", NodePath.ByteArrayToHexString(path));
    }
    
    [Fact]
    public void TestToplusLeftPlusRightPlusRightPlusLeftPlusLeft()
    {
        // Arrange
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Right)
            .Add(NodePath.Left)
            .Add(NodePath.Left);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("26", NodePath.ByteArrayToHexString(path));
    }
    
    [Fact]
    public void TestToplusLeftPlusRightPlusRightPlusLeftPlusLeftPlusLeft()
    {
        // Arrange
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Right)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Left);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("46", NodePath.ByteArrayToHexString(path));
    }

    [Fact] public void TestToplusLeftPlusRightPlusRightPlusLeftPlusLeftPlusLeftPlusRight()
    {
        // Arrange
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Right)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Right);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.Equal("C6", NodePath.ByteArrayToHexString(path));
    }
    
    [Fact] public void TestToplusLeftPlusRightPlusRightPlusLeftPlusLeftPlusLeftPlusRightPlusLeft()
    {
        // Arrange
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Right)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Left);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.True(CmpToBits(n, "101000110"));
    }
    
    [Fact] public void TestToplusLeftPlusRightPlusRightPlusLeftPlusLeftPlusLeftPlusRightPlusLeftPlusLeftRightLeft()
    {
        // Arrange
        var LeftRightLeft = NodePath.Left
            .Add(NodePath.Right)
            .Add(NodePath.Left);
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Right)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Left)
            .Add(LeftRightLeft);
        
        // Act
        var path = n.AsShortPath();
        
        // Assert
        Assert.True(CmpToBits(n, "101001000110"));
    }
    
    [Fact] public void AddLeftRightLeft()
    {
        // Arrange
        var leftRightLeft = NodePath.Left
            .Add(NodePath.Right)
            .Add(NodePath.Left);
        
        var n = NodePath.TOP
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Right)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Left)
            .Add(NodePath.Right)
            .Add(NodePath.Left)
            .Add(leftRightLeft)
            .Add(leftRightLeft)
            .Add(leftRightLeft);
        
        // Act
        var isSameBits = CmpToBits(n, "101001001001000110");

        // Assert
        Assert.True(isSameBits);
    }
}