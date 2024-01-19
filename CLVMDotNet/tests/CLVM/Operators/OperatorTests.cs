using System.Numerics;
using System.Text;
using Xunit;
using x = CLVMDotNet.CLVM;
using CLVM = CLVMDotNet;


namespace CLVMDotNet.Tests.CLVM.Operators
{
    [Trait("Operators","All Operators")]
    public class OperatorTests
    {
        [Fact]
        public void OpAdd()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x10 }, x.SExp.To(new int[] { 3, 4, 5 }));
            var s = result;
        }


        [Fact]
        public void OpSubtract()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x11 }, x.SExp.To(new int[] { 3, 1 }));
            var s = result;
        }

        [Fact]
        public void OpDivide()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new BigInteger[] { 10, 2 }));
            var s = result;
        }

        [Fact]
        public void OpMultiply()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new BigInteger[] { 10, 3 }));
            var s = result;
        }
        
        [Fact]
        public void OpDivMod()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x14 }, x.SExp.To(new BigInteger[] { 3, 5 }));
            var s = result;
        }

        
        
        
        [Fact]
        public void OpConcat()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x0E }, x.SExp.To(new string[] { "test", "ing" }));
            var s = result;
        }
        
        #region OpSubStr

        [Theory]
        [InlineData(-1)]
        [InlineData(10)]
        public void OpSubstrThrowsWhenOutOfBounds(int startIndex)
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new dynamic[] { "kevin", startIndex }))
                );
            Assert.Contains("invalid indices for substr", errorMessage.Message);
        }

        [Fact]
        public void OpSubstrThrowsWithTwoManyArgs()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new dynamic[] { "kevin", 1,2,3 }))
                );
            Assert.Contains("substr takes exactly 2 or 3 arguments", errorMessage.Message);
        }
        
        [Fact]
        public void OpSubstrThrowsWithTooFewArgs()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new dynamic[] { "kevin" }))
                );
            Assert.Contains("substr takes exactly 2 or 3 arguments", errorMessage.Message);
        }
        
        [Theory]
        [InlineData(1,"somelongstring",4,"longstring" )]
        [InlineData(1,"somelongstring",0,"somelongstring" )]
        [InlineData(1,"somelongstring",13,"g" )]
        public void OpSubstrReturnsSubStringWithCost(BigInteger cost, string val, int startindex, string expectedResult)
        {
            var result =x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new dynamic[] { val, startindex }));
            var atom = result.Item2.AsAtom();
            string text = Encoding.UTF8.GetString(atom);
            Assert.Equal(expectedResult, text);
            Assert.Equal(cost, result.Item1);
        }
        
        [Theory]
        [InlineData(1,"somelongstring",0,2,"so" )]
        [InlineData(1,"somelongstring",4,5,"l" )]
        [InlineData(1,"somelongstring",13,14,"g" )]
        [InlineData(1,"somelongstring",3,12,"elongstri" )]
        public void OpSubstrReturnsSubStringOfNumberOfCharactersWithCost(BigInteger cost, string val, int startindex,int endIndex, string expectedResult)
        {
            var result =x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new dynamic[] { val, startindex, endIndex }));
            var atom = result.Item2.AsAtom();
            string text = Encoding.UTF8.GetString(atom);
            Assert.Equal(expectedResult, text);
            Assert.Equal(cost, result.Item1);
        }
        #endregion

        

        [Fact]
        public void OpNot()
        {
            
        }
        
        
        
        // private bool handlerCalled = false;
        // private Tuple<int, SExp> UnknownHandler(byte[] name, SExp args)
        // {
        //     handlerCalled = true;
        //     Assert.Equal(new byte[] { 0xff, 0xff, (byte)(0x1337 & 0xFF) }, name);
        //     Assert.Equal(SExp.To((Int32)1337), args);
        //     return Tuple.Create(42, SExp.To(new byte[] { 0x66, 0x6f, 0x6f, 0x62, 0x61, 0x72 }));
        // }

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
        //     Console.WriteLine(OperatorDict.OPERATOR_LOOKUP(1,1));
        //     Assert.Equal(OperatorDict.OPERATOR_LOOKUP(KEYWORD_TO_ATOM["+"], SExp.To(new List<int> { 3, 4, 5 }))[1], SExp.To((Int32)12));
        // }

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

        // [Fact]
        // public void TestUnknownOpsLastBits()
        // {
        //     // The last byte is ignored for no-op unknown ops
        //     foreach (var suffix in new byte[][]
        //                  { new byte[] { 0x3f }, new byte[] { 0x0f }, new byte[] { 0x00 }, new byte[] { 0x2c } })
        //     {
        //         // the cost is unchanged by the last byte
        //         Assert.Equal(
        //             OperatorDict.DefaultUnknownOp(new byte[] { 0x3c }.Concat(suffix).ToArray(), CLVM.SExp.NULL),
        //             Tuple.Create(61, CLVM.SExp.NULL));
        //     }
        // }
    }
}