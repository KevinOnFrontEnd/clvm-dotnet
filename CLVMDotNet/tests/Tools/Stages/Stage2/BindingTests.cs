using CLVMDotNet.Tools.Stages.Stage2;
using Xunit;

namespace CLVMDotNet.Tests.Tools.Stages.Stage2;

public class BindingTests
{
    [Fact]
    public void TestBrun()
    {
        // Arrange
        
        // Act
        var brun = Bindings.Brun;
        
        // Assert
        Assert.Null(brun.Atom);
        Assert.NotNull(brun.Pair);
        Assert.True(brun.AsPair()!.Item1.AsAtom()!.SequenceEqual(new byte[] { 0x02 }));
        Assert.Null(brun.AsPair()!.Item1.Pair);
        Assert.Null(brun.AsPair()!.Item2.Atom);
        Assert.NotNull(brun.AsPair()!.Item2.Pair);
        Assert.True(brun.AsPair()!.Item2.AsPair()!.Item1.AsAtom()!.SequenceEqual(new byte[] { 0x02}));
        Assert.Null(brun.AsPair()!.Item2.AsPair()!.Item2.Atom);
        Assert.NotNull(brun.AsPair()!.Item2.AsPair()!.Item2.Pair);
        Assert.True(brun.AsPair()!.Item2.AsPair()!.Item2.AsPair()!.Item1.AsAtom()!.SequenceEqual(new byte []{ 0x03}));
        Assert.Empty(brun.AsPair()!.Item2.AsPair()!.Item2.AsPair()!.Item2.AsAtom()!);
    }
    
    [Fact(Skip = "Skipping for now")]
    public void TestRun()
    {
        // Arrange
        
        // Act
        var run = Bindings.Run;
        
        // Assert
        Assert.Null(run.Atom);
        Assert.True(run.AsPair()!.Item1.AsAtom()!.SequenceEqual(new byte[] { 0x02 }));
        Assert.Null(run.AsPair()!.Item1.Pair);
        Assert.Null(run.AsPair()!.Item2.Atom);
        Assert.NotNull(run.AsPair()!.Item2.Pair);
        Assert.NotNull(run.AsPair()!.Item2.AsPair()!.Item2);
        Assert.NotNull(run.AsPair()!.Item2.AsPair()!.Item1);
        
        
    }
}