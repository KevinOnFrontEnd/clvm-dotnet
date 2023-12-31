using Xunit;

namespace clvm_dotnet.tests;

public class BitMaskTests
{
    [Fact]
    public void TestMsbMask()
    {
        Assert.Equal(0x00, HelperFunctions.MSBMask(0x00));
        Assert.Equal(0x01, HelperFunctions.MSBMask(0x01));
        Assert.Equal(0x02, HelperFunctions.MSBMask(0x02));
        Assert.Equal(0x04, HelperFunctions.MSBMask(0x04));
        Assert.Equal(0x08, HelperFunctions.MSBMask(0x08));
        Assert.Equal(0x10, HelperFunctions.MSBMask(0x10));
        Assert.Equal(0x20, HelperFunctions.MSBMask(0x20));
        Assert.Equal(0x40, HelperFunctions.MSBMask(0x40));
        Assert.Equal(0x80, HelperFunctions.MSBMask(0x80));
        Assert.Equal(0x40, HelperFunctions.MSBMask(0x44));
        Assert.Equal(0x20, HelperFunctions.MSBMask(0x2A));
        Assert.Equal(0x80, HelperFunctions.MSBMask(0xFF));
        Assert.Equal(0x08, HelperFunctions.MSBMask(0x0F));
    }
}