using System.Numerics;

namespace CLVMDotNet.Tools;

public class NodePath
{
    public static NodePath TOP => new NodePath(1);
    public static NodePath Left => TOP.First();
    public static NodePath Right => TOP.Rest();
    public BigInteger ByteCount { get; private set; }
    public byte[] Blob { get; private set; }
    public BigInteger _index { get; private set; }

    public NodePath(BigInteger index)
    {
        if (index < 0)
        {
            var log = index + 1;
            int byteCount = (int)Math.Ceiling(BigInteger.Log(log, 256) / 8);
            byteCount = (byteCount + 7) / 8;
            byte[] blob = index.ToByteArray();
            Array.Reverse(blob);
            byte[] resultBlob = new byte[byteCount + 1];
            Array.Copy(blob, 0, resultBlob, 1, byteCount);
            _index = new BigInteger(resultBlob);
        }
        else
        {
            _index = index;
        }
    }

    public NodePath First()
    {
        BigInteger newIndex = _index * 2;
        return new NodePath(newIndex);
    }

    public NodePath Rest()
    {
        BigInteger newIndex = _index * 2 + 1;
        return new NodePath(newIndex);
    }
    
    public static string ByteArrayToHexString(byte[] byteArray)
    {
        return string.Concat(byteArray.Select(b => b.ToString("X2")));
    }

    public byte[] AsShortPath()
    {
        BigInteger index = _index;
        int byteCount = Math.Max(1, (int)Math.Ceiling(BigInteger.Log(index, 256) / 8));
        byte[] byteArray = index.ToByteArray();
        Array.Reverse(byteArray); // Reverse the byte array if needed

        // Ensure the result has the correct byteCount
        if (byteArray.Length > byteCount)
        {
            Array.Resize(ref byteArray, byteCount);
        }
        else if (byteArray.Length < byteCount)
        {
            // If the byte array is shorter than required, add leading zeros
            byte[] paddedArray = new byte[byteCount];
            byteArray.CopyTo(paddedArray, byteCount - byteArray.Length);
            byteArray = paddedArray;
        }

        return byteArray;
    }

    public NodePath Add(NodePath otherNode)
    {
        BigInteger resultIndex = ComposePaths(_index, otherNode._index);
        return new NodePath(resultIndex);
    }


    /// <summary>
    /// The binary representation of a path is a 1 (which means "stop"), followed by the path as binary digits,
    /// where 0 is "left" and 1 is "right".
    /// 
    /// Look at the diagram at the top for these examples.
    ///
    /// Example: 9 = 0b1001, so right, left, left
    /// Example: 10 = 0b1010, so left, right, left
    ///
    /// How it works: we write both numbers as binary. We ignore the terminal in path_0, since it's
    /// not the terminating condition anymore. We shift path_1 enough places to OR in the rest of path_0.
    ///
    /// Example: path_0 = 9 = 0b1001, path_1 = 10 = 0b1010.
    ///
    /// Shift path_1 three places (so there is room for 0b001) to 0b1010000.
    /// Then OR in 0b001 to yield 0b1010001 = 81, which is right, left, left, left, right, left.
    /// </summary>
    /// <param name="path0"></param>
    /// <param name="path1"></param>
    /// <returns></returns>
    public BigInteger ComposePaths(BigInteger path0, BigInteger path1)
    {
        BigInteger mask = 1;
        BigInteger temp_path = path0;

        while (temp_path > 1)
        {
            path1 <<= 1;
            mask <<= 1;
            temp_path >>= 1;
        }

        mask -= 1;
        BigInteger path = path1 | (path0 & mask);

        return path;
    }
    
    public override string ToString()
    {
        return $"NodePath: {_index}";
    }
}