using CLVMDotNet.CLVM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CLVMDotNet.Tests.Helpers;

public static class JSONHelpers
{
    public static string ConvertToJSON(this SExp sexp)
    {
        return JsonConvert.SerializeObject(sexp);
    }
    
    public static async Task AssertExpectedJSONResult(this SExp sexp, Object expectObject)
    {
        var targetJObject = JObject.FromObject(expectObject);
        var sourceJObject = JObject.FromObject(sexp);

        if (!JToken.DeepEquals(sourceJObject, targetJObject))
        {
            //actual doesn't match expected properties, values
            foreach (var targetProp in targetJObject.Properties())
            {
                JProperty source = sourceJObject.Property(targetProp.Name);
                if (source == null)
                {
                    throw new Exception($"expceted property: {targetProp.Name}  - {targetProp.Value} but not present");
                }

                if (!JToken.DeepEquals(source.Value, targetProp.Value))
                {
                    throw new Exception($"{targetProp.Name} value is {source.Value}, expected {targetProp.Value} but got { source.Value}");
                }
            }

            //extra property on response
            foreach (var sourceProp in sourceJObject.Properties())
            {
                JProperty source = targetJObject.Property(sourceProp.Name);
                if (source == null)
                {
                    throw new Exception($"extra property on response : {sourceProp.Name}  - {sourceProp.Value}");
                }
            }

        }
        else
        {
            Console.WriteLine("Objects are same");
        }
        Assert.Equal(1, 1);
    }
}