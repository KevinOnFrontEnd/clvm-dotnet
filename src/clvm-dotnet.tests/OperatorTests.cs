using System.Numerics;
using Xunit;

namespace clvm_dotnet.tests;

public class OperatorTests
{
    private bool handlerCalled = false;
    
    private Tuple<int, SExp> UnknownHandler(byte[] name, SExp args)
    {
        handlerCalled = true;
        Assert.Equal(new byte[] { 0xff, 0xff, (byte)(0x1337 & 0xFF) }, name);
        Assert.Equal(SExp.To((Int32)1337), args);
        return Tuple.Create(42, SExp.To(new byte[] { 0x66, 0x6f, 0x6f, 0x62, 0x61, 0x72 }));
    }

     // [Fact]
     // public void TestUnknownOp()
     // {
     //     Assert.Throws<EvalError>(() =>  OPERATOR_LOOKUP(new byte[] { 0xff, 0xff, 0x1337 }, SExp.To(1337)));
     //     var od = new OperatorDict(OPERATOR_LOOKUP, unknownOpHandler: (name, args) => UnknownHandler(name, args));
     //     var result = od(new byte[] { 0xff, 0xff, 0x1337 }, SExp.To(1337));
     //     Assert.True(handlerCalled);
     //     Assert.Equal(42, result.Item1);
     //     Assert.Equal(SExp.To(new byte[] { 0x66, 0x6f, 0x6f, 0x62, 0x61, 0x72 }), result.Item2);
     // }
//
     // [Fact]
     // public void TestPlus()
     // {
     //     Console.WriteLine(OperatorDict.OPERATOR_LOOKUP);
     //     Assert.Equal(OperatorDict.OPERATOR_LOOKUP(KEYWORD_TO_ATOM["+"], SExp.To(new List<int> { 3, 4, 5 }))[1], SExp.To((Int32)12));
     // }
//
     // [Fact]
     // public void TestUnknownOpReserved()
     // {
     //     // any op that starts with ffff is reserved, and results in a hard failure
     //     Assert.Throws<EvalError>(() => DefaultUnknownOp(new byte[] { 0xff, 0xff }, SExp.Null()));
     //
     //     foreach (var suffix in new byte[][] { new byte[] { 0xff }, new byte[] { 0x30 }, new byte[] { 0x00 }, new byte[] { 0xcc, 0xcc, 0xfe, 0xed, 0xfa, 0xce } })
     //     {
     //         Assert.Throws<EvalError>(() => DefaultUnknownOp(new byte[] { 0xff, 0xff }.Concat(suffix).ToArray(), SExp.Null()));
     //     }
     //
     //     Assert.Throws<EvalError>(() => Assert.Equal(DefaultUnknownOp(new byte[0], SExp.Null()), Tuple.Create(1, SExp.Null())));
     //
     //     // a single ff is not sufficient to be treated as a reserved opcode
     //     Assert.Equal(DefaultUnknownOp(new byte[] { 0xff }, SExp.Null()), Tuple.Create(CONCAT_BASE_COST, SExp.Null()));
     //
     //     // leading zeroes count, and this does not count as a ffff-prefix
     //     // the cost is 0xffff00 = 16776960
     //     Assert.Equal(DefaultUnknownOp(new byte[] { 0x00, 0xff, 0xff, 0x00, 0x00 }, SExp.Null()), Tuple.Create(16776961, SExp.Null()));
     // }

     [Fact]
     public void TestUnknownOpsLastBits()
     {
         // The last byte is ignored for no-op unknown ops
         foreach (var suffix in new byte[][] { new byte[] { 0x3f }, new byte[] { 0x0f }, new byte[] { 0x00 }, new byte[] { 0x2c } })
         {
             // the cost is unchanged by the last byte
             Assert.Equal(OperatorDict.DefaultUnknownOp(new byte[] { 0x3c }.Concat(suffix).ToArray(), SExp.NULL), Tuple.Create(61, SExp.NULL));
         }
     }
}