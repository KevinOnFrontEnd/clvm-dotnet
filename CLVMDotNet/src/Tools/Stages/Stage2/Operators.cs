using System.Numerics;
using System.Text;
using CLVMDotNet.CLVM;
using CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Operators
{
    public static Tuple<BigInteger, SExp> DoRead(SExp args)
    {
        var filename = args.First().AsAtom();
        string filepath = Encoding.UTF8.GetString(filename);
        string s = File.ReadAllText(filepath);
        var irSexp = SExp.To(IR.IRReader.ReadIR(s));
        var sexp = BinUtils.AssembleFromIR(irSexp);
        return new Tuple<BigInteger, SExp>(1, sexp);
    }

    public static Tuple<BigInteger, SExp> DoWrite(SExp args)
    {
        var filename = args.First().AsAtom();
        string filepath = Encoding.UTF8.GetString(filename);
        string s = File.ReadAllText(filepath);
        var irSexp = SExp.To(IR.IRReader.ReadIR(s));
        var sexp = BinUtils.AssembleFromIR(irSexp);
        return new Tuple<BigInteger, SExp>(1, sexp);
    }


    
    public static SExp RunProgramProgramForSearchPaths(string[] searchPaths)
    {
        Dictionary<string, OperatorDict.DictDelegate> BINDINGS = new Dictionary<string, OperatorDict.DictDelegate>();
        BINDINGS["com"] = (op, sexp) => DoRead(sexp);
        BINDINGS["opt"] = (op, sexp) => DoRead(sexp);
        BINDINGS["_full_path_for_name"] = (op, sexp) => DoFullPathForName(sexp);
        BINDINGS["_read"] = (op, sexp) => DoRead(sexp);
        BINDINGS["_write"] = (op, sexp) => DoWrite(sexp);

        return null;
        
        Tuple<BigInteger, SExp> DoFullPathForName(SExp args)
        {
            var filename = args.First().AsAtom();
        
            foreach (string path in searchPaths)
            {
                string filePath = Path.Combine("", "");
                if (File.Exists(filePath))
                {
                    var sexp = SExp.To("test");
                    return new Tuple<BigInteger, SExp>(1, sexp);
                }
            }
            throw new EvalError($"can't open {filename}");
        }
    }
}