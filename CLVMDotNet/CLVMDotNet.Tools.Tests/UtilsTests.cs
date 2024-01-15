using System.Numerics;
using CLVMDotNet.Tools.IR;
using Xunit;

namespace CLVMDotNet.Tools.Tests;

public class UtilsTests
{
    [Theory]
    [InlineData("1129270867", "CONS")]
    [InlineData("1314212940", "NULL")]
    [InlineData("4804180", "INT")]
    [InlineData("4736344", "HEX")]
    [InlineData("20820", "QT")]
    [InlineData("4477268", "DQT")]
    [InlineData("5460308", "SQT")]
    [InlineData("5462349", "SYM")]
    [InlineData("20304", "OP")]
    [InlineData("1129268293", "CODE")]
    [InlineData("1313817669", "NODE")]
    public void Convert_to_base256_matches_python(string expected, string sym)
    {
        BigInteger expectedNumber = BigInteger.Parse(expected);
        Assert.Equal(expectedNumber, Utils.ConvertToBase256(sym));
    }
}