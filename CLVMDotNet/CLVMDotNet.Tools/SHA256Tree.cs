using System.Security.Cryptography;

namespace CLVMDotNet.Tools;

public static class SHA256Tree
{
    public static byte[] Sha256Tree(SExp v)
    {
        if (v.Pair != null)
        {
            byte[] left = Sha256Tree(v.Pair.Item1);
            byte[] right = Sha256Tree(v.Pair.Item2);
            byte[] s = new byte[1] { 0x02 };
            s = CombineByteArrays(s, left, right);
            return Sha256Hash(s);
        }
        else
        {
            byte[] s = new byte[1] { 0x01 };
            s = CombineByteArrays(s, v.Atom);
            return Sha256Hash(s);
        }
    }
    
    private static byte[] CombineByteArrays(params byte[][] arrays)
    {
        int totalLength = arrays.Sum(array => array.Length);
        byte[] result = new byte[totalLength];
        int currentIndex = 0;

        foreach (byte[] array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, currentIndex, array.Length);
            currentIndex += array.Length;
        }

        return result;
    }
    
    private static byte[] Sha256Hash(byte[] input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(input);
        }
    }
}