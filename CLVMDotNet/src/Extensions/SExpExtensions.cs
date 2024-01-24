using CLVMDotNet.CLVM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CLVMDotNet.Extensions;

public static class SExpExtensions
{
    public static string AsJSON(this SExp sexp)
    {
        var sourceJObject = JsonConvert.SerializeObject(sexp);
        return sourceJObject;
    }
}