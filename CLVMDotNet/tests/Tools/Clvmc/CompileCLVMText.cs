using CLVMDotNet.CLVM;
using CLVMDotNet.Tests.Helpers;
using Xunit;

namespace CLVMDotNet.Tests.Tools.Clvmc;

public class CompileCLVMText
{
    [Fact]
    public void RunBasicProgram()
    {
        var result = CLVMDotNet.Tools.IR.Clvmc.CompileCLVMText("(/ 10 2)", Array.Empty<string>());


        var sexp = SExp.To(new List<dynamic> { "+", 1, 2 });
        var json = sexp.ConvertToJSON();
        
        var rs = result;
    }
}