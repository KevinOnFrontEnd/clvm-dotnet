using Xunit;
using clvm = clvm_dotnet;

namespace clvm_dotnet.tests.Serialize;

[Trait("Serialize", "Common")]
public class CommonTests
{
    public static void CheckSerde(dynamic s)
    {
        var v = clvm.SExp.To(s);
        var b = v.AsBin();
        var v1 = clvm.Serialize.SexpFromStream(new MemoryStream(b));
        var isEqual = v.Equals(v1);
        if (!isEqual)
        {
            Console.WriteLine($"{v}: {b.Length} {BitConverter.ToString(b)} {v1}");
            System.Diagnostics.Debugger.Break();
            b = v.AsBin();
            v1 = clvm.Serialize.SexpBufferFromStream(new MemoryStream(b));
        }
        Assert.True(isEqual);
    }
    
    // [Fact]
    // public void EmptyString()
    // {
    //     CheckSerde(Array.Empty<byte>());
    // }
    
    // [Fact]
    // public void TestSingleBytes()
    // {
    //     for (int _ = 0; _ < 256; _++)
    //     {
    //         byte[] byteArray = new byte[] { (byte)_ };
    //         CheckSerde(byteArray);
    //     }
    // }
    
    [Fact]
     public void TestShortLists()
     {
         for (int _ = 8; _ < 36; _ += 8)
         {
             for (int size = 1; size < 5; size++)
             {
                 CheckSerde(Enumerable.Repeat(_, size).ToArray());
             }
         }
     }   

    // [Fact]
    // public void TestConsBox()
    // {
    //     CheckSerde(Tuple.Create<object?, object?>(null, null));
    //     //CheckSerde(Tuple.Create<object, object>(null, new object[] { 1, 2, 30, 40, 600, Tuple.Create<object, object>(null, 18) }));
    //     //CheckSerde(Tuple.Create<object, object>(100, Tuple.Create<object, object>(TEXT, Tuple.Create<object, object>(30, Tuple.Create<object, object>(50, Tuple.Create<object, object>(90, Tuple.Create<object, object>(TEXT, TEXT + TEXT)))))));
    // }

    // [Fact]
    // public void TestPlus()
    // {
    //     Assert.AreEqual(OPERATOR_LOOKUP(KEYWORD_TO_ATOM["+"], SExp.To(new int[] { 3, 4, 5 }))[1], SExp.To(12));
    // }
}