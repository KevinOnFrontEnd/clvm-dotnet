using System.Numerics;

namespace CLVMDotNet.Tools;

public class NodePath
{
    public static NodePath TOP => new NodePath();
    public static NodePath Left => NodePath.First();
    public static NodePath Right => NodePath.Rest();
    public  BigInteger ByteCount { get; private set; }
    public byte[] Blob { get; private set; }
    public int _index { get; private set; }

    public NodePath(int index = 0)
    {
        if (index < 0)
        {
            //bytecount
            //blob
            //index
        }
        else
        {
            _index = index;
        }
    }

    public static NodePath First()
    {
        return null;
    }

    public static NodePath Rest()
    {
        return null;
    }
    
    
    //ComposePath
    //AsShortPath
    //Add
    //Str
    //Repr
}