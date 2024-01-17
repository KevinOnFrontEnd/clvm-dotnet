namespace CLVMDotNet.Tests.SExp;

public class GeneratedTree
{
    public int Depth { get; private set; } = 4;
    public int Val { get; private set; } = 0;

    public GeneratedTree(int depth, int val)
    {
        if (depth < 0)
            throw new ArgumentException("Depth must be greater than or equal to 0.");

        Depth = depth;
        Val = val;
    }

    public byte[] Atom
    {
        get
        {
            if (Depth > 0)
                return null;

            return new byte[] { (byte)Val };
        }
    }

    public Tuple<GeneratedTree, GeneratedTree> Pair
    {
        get
        {
            if (Depth == 0)
                return null;

            int newDepth = Depth - 1;
            return new Tuple<GeneratedTree, GeneratedTree>(new GeneratedTree(newDepth, Val),
                new GeneratedTree(newDepth, Val + (int)Math.Pow(2, newDepth)));
        }
    }
}