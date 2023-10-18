namespace clvm_dotnet;

/// <summary>
/// This class implements the CLVM Object protocol in the simplest possible way,
/// by just having an "atom" and a "pair" field
/// </summary>
public class CLVMObject
{
    public byte[]? Atom { get; set; }
    public Tuple<CLVMObject,CLVMObject> Pair { get; set; }

    public CLVMObject(dynamic v)
    {
        if (v.GetType() == typeof(Tuple<object, object>))
        {
            Pair = v;
            Atom = null;
        }
        else
        {
            Pair = null;
            Atom = v;
        }
    }
    
    public CLVMObject()
    {
    }
}