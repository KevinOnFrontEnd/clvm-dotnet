namespace clvm_dotnet;

using System;
using System.Numerics;

public static class casts
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
        int byteCount = (int)((v.GetBitLength() + 7) >> 3);
        if (v == 0)
        {
            return Array.Empty<byte>();
        }
        byte[] result = v.ToByteArray(isBigEndian: true);
        
        // Ensure the returned byte array is minimal
        while (result.Length > 1 && (result[0] == 0xFF && (result[1] & 0x80) != 0 || result[0] == 0))
        {
            byte[] trimmedResult = new byte[result.Length - 1];
            Array.Copy(result, 1, trimmedResult, 0, trimmedResult.Length);
            result = trimmedResult;
        }
        
        return result;
    }

    public static int LimbsForInt(BigInteger v)
    {
        return (int)((v.GetBitLength() + 7) >> 3);
    }
}