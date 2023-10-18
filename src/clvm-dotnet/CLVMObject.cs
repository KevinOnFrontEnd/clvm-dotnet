namespace clvm_dotnet;

/// <summary>
/// This class implements the CLVM Object protocol in the simplest possible way,
/// by just having an "atom" and a "pair" field
/// </summary>
public class CLVMObject
{
    public byte[]? atom { get; set; }
    public Tuple<object?,object?>? pair { get; set; }

    public CLVMObject(dynamic v)
    {
        if (v.GetType() == typeof(Tuple<object, object>))
        {
            pair = v;
            atom = null;
        }
        else
        {
            pair = null;
            atom = v;
        }
    }
}