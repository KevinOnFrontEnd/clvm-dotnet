namespace clvm_dotnet;

/// <summary>
/// This class implements the CLVM Object protocol in the simplest possible way,
/// by just having an "atom" and a "pair" field
/// </summary>
public class CLVMObject
{
    public byte[] Atom { get;  set; }
    public Tuple<object, object> Pair { get;  set; }

    public CLVMObject(object v)
    {
        Console.WriteLine("initiating CLVMObject class");
        if (v is CLVMObject clvmObj)
        {
            Atom = clvmObj.Atom;
            Pair = clvmObj.Pair;
        }
        else if (v is Tuple<object, object> tuple)
        {
            if (tuple.Item1 == null || tuple.Item2 == null)
            {
                throw new ArgumentException("Tuples must not contain null values.");
            }

            Pair = tuple;
            Atom = null;
        }
        else
        {
            Atom = HelperFunctions.ConvertAtomToBytes(v); // Implement your conversion logic
            Pair = null;
        }
    }
    
    public CLVMObject()
    {
    }

    public byte[] AsAtom()
    {
        return Atom;
    }
}