using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.IR
{
    public class Clvmc
    {
        public static dynamic CompileCLVMText(string text, string[] searchPaths)
        {
            var ir_src = IRReader.ReadIR(text);
            var assembled_sexp = BinUtils.AssembleFromIR(ir_src);
            var input_sexp = SExp.To((assembled_sexp, Array.Empty<int>()));
            
            //run_program_for_search_paths
            //run_program
            
            return null;
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