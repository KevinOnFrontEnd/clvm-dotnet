namespace clvm_dotnet;

using System;
using System.Numerics;

public static class Casts
{
    public static BigInteger IntFromBytes(byte[] blob)
    {
        int size = blob.Length;
        if (size == 0)
        {
            return BigInteger.Zero;
        }
        return new BigInteger(blob, isBigEndian: true);
    }

    public static byte[] IntToBytes(BigInteger v)
    {
        byte[] byteArray = v.ToByteArray();
        
        if (BitConverter.IsLittleEndian)
        {
            byteArray = byteArray.Reverse().ToArray();
        }

        while (byteArray.Length > 1 && (byteArray[0] == 0xFF || byteArray[0] == 0x00))
        {
            byteArray = byteArray.Skip(1).ToArray();
        }

        if (!v.IsZero)
        {
            if (byteArray[0] >= 0x80)
            {
                byteArray = new byte[] { 0 }.Concat(byteArray).ToArray();
            }
        }
        else
        {
            byteArray = new byte[0];
        }

        return byteArray;
    }

    public static int LimbsForInt(BigInteger v)
    {
        return IntToBytes(v).Length;
    }
}