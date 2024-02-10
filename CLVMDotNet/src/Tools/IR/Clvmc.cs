// using CLVMDotNet.CLVM;
// using CLVMDotNet.Extensions;
//
// namespace CLVMDotNet.Tools.IR
// {
//     public class Clvmc
//     {
//         public static SExp CompileCLVMText(string text, string[] searchPaths)
//         {
//             var ir_src = IRReader.ReadIR(text);
//             var assembled_sexp = BinUtils.AssembleFromIR(ir_src);
//             var input_sexp = SExp.To((assembled_sexp, Array.Empty<dynamic>()));
//             
//             // everthing above here matches python
//             var tree = HelperFunctions.PrintTree(input_sexp);
//             var result = Program.RunProgram(null, input_sexp);
//             
//             //Need a RunProgramForSearchPaths
//             
//             return result.Item2;
//         }
//
//         public static void CompileCLVM(string inputPath, string outputPath, string[] searchPaths)
//         {
//             
//         }
//         
//         
//         ///
//         /// 
//         //compile_clvm_text
//         //compile_clvm
//         //find_files
//     }
// }