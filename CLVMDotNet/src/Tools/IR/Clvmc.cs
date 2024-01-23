using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.IR
{
    public class Clvmc
    {
        public static SExp CompileCLVMText(string text, string[] searchPaths)
        {
            var ir_src = IRReader.ReadIR(text);
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            var assembled_sexp = BinUtils.AssembleFromIR(ir_src);
            var input_sexp = SExp.To((assembled_sexp, Array.Empty<dynamic>()));

            var result = Program.RunProgram(null, input_sexp);
            return result.Item2;
        }

        public static void CompileCLVM(string inputPath, string outputPath, string[] searchPaths)
        {
            
        }
        
        
        ///
        /// 
        //compile_clvm_text
        //compile_clvm
        //find_files
    }
}