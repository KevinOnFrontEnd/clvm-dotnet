using Xunit;
using clvm = CLVMDotNet;

namespace CLVMDotNet.Tests.SExp;
[Trait("SExp", "AsBin")]
public class AsBin
{
    [Theory]
    [InlineData(new byte[]{ 0xFF, 0x80, 0x80  }, new int[] {0})] 
    [InlineData(new byte[]{ 0xFF, 0x01, 0x80  }, new int[] {1})] 
    [InlineData(new byte[] { 0xFF, 0x08, 0xFF, 0x08, 0x80}, new int[] {8,8})]
    //[InlineData(new byte[] { 0xFF, 0x82, 0x01, 0x00, 0xFF, 0x82, 0x01, 0x00, 0x80 }, new int[]  {256,256})]  
    // [InlineData(new byte[] { 8, 9 }, new byte[]   {512,512,512})]  
    // [InlineData(new byte[] { 8, 9 }, new byte[]  {1024,1024,1024,1024})]  
    // [InlineData(new byte[] { 8, 9 }, new List<int>  {2048,248,2048,2048,2048})]  
    public void sexp_AsBinIsCorrectOrder(byte[] expected, dynamic  sexp_list)
    {
        var v = clvm.SExp.To(sexp_list);
        var bytes = v.AsBin();
        Assert.Equal(expected, bytes );
    }
}