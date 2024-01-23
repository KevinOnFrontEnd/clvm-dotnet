namespace CLVMDotNet.CLVM;

public class Program
{
    //RunProgram
    //TraversePath
    //SwapOp
    //ConsOp
    //ApplyOp
    //to_pre_eval_op

    public static byte MSBMask(byte inputByte)
    {
        inputByte |= (byte)(inputByte >> 1);
        inputByte |= (byte)(inputByte >> 2);
        inputByte |= (byte)(inputByte >> 4);
        return (byte)((inputByte + 1) >> 1);
    }
}