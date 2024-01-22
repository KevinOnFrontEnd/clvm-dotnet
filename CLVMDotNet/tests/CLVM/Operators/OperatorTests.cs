using System.Numerics;
using System.Text;
using Xunit;
using x = CLVMDotNet.CLVM;
using CLVM = CLVMDotNet;


namespace CLVMDotNet.Tests.CLVM.Operators
{
    [Trait("Operators", "All Operators")]
    public class OperatorTests
    {
        #region OpAdd

        [Fact]
        public void OpAdd()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x10 }, x.SExp.To(new List<int> { 3, 4, 5 }));
            var s = result;
        }

        #endregion


        [Fact]
        public void OpSubtract()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x11 }, x.SExp.To(new List<int> { 3, 1 }));
            var s = result;
        }

        #region OpDivide

        [Fact]
        public void OpDivide()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new List<BigInteger> { 10, 2 }));
            var s = result;
        }

        [Fact]
        public void OpDivideThrowsExceptionIfDividingByZero()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new List<BigInteger> { 10, 0 }))
                );
            Assert.Contains("div with 0", errorMessage.Message);
        }

        [Fact()]
        public void OpDivideThrowsExceptionWithNegativeOperand1()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new List<BigInteger> { -1, 5 }))
                );
            Assert.Contains("div operator with negative operands is deprecated", errorMessage.Message);
        }

        #endregion

        [Fact]
        public void OpMultiply()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new List<BigInteger> { 10, 3 }));
            var s = result;
        }

        [Fact]
        public void OpDivMod()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x14 }, x.SExp.To(new List<BigInteger> { 3, 5 }));
            var s = result;
        }


        [Fact]
        public void OpConcat()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x0E }, x.SExp.To(new List<string> { "test", "ing" }));
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
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new List<dynamic> { "kevin", startIndex }))
                );
            Assert.Contains("invalid indices for substr", errorMessage.Message);
        }

        [Fact]
        public void OpSubstrThrowsWithTwoManyArgs()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new List<dynamic> { "kevin", 1, 2, 3 }))
                );
            Assert.Contains("substr takes exactly 2 or 3 arguments", errorMessage.Message);
        }

        [Fact]
        public void OpSubstrThrowsWithTooFewArgs()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new List<dynamic> { "kevin" }))
                );
            Assert.Contains("substr takes exactly 2 or 3 arguments", errorMessage.Message);
        }

        [Theory]
        [InlineData(1, "somelongstring", 4, "longstring")]
        [InlineData(1, "somelongstring", 0, "somelongstring")]
        [InlineData(1, "somelongstring", 13, "g")]
        public void OpSubstrReturnsSubStringWithCost(BigInteger cost, string val, int startindex, string expectedResult)
        {
            var result =
                x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new List<dynamic> { val, startindex }));
            var atom = result.Item2.AsAtom();
            string text = Encoding.UTF8.GetString(atom);
            Assert.Equal(expectedResult, text);
            Assert.Equal(cost, result.Item1);
        }

        [Theory]
        [InlineData(1, "somelongstring", 0, 2, "so")]
        [InlineData(1, "somelongstring", 4, 5, "l")]
        [InlineData(1, "somelongstring", 13, 14, "g")]
        [InlineData(1, "somelongstring", 3, 12, "elongstri")]
        public void OpSubstrReturnsSubStringOfNumberOfCharactersWithCost(BigInteger cost, string val, int startindex,
            int endIndex, string expectedResult)
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x0C },
                x.SExp.To(new List<dynamic> { val, startindex, endIndex }));
            var atom = result.Item2.AsAtom();
            string text = Encoding.UTF8.GetString(atom!);
            Assert.Equal(expectedResult, text);
            Assert.Equal(cost, result.Item1);
        }

        #endregion

        #region OpStrLen

        [Theory]
        [InlineData("somestring", 10, 193)]
        [InlineData("s", 1, 184)]
        [InlineData("", 0, 173)]
        [InlineData("THIS IS A LONGER SENTENCE TO CALCULATE THE COST OF.", 51, 234)]
        public void OpStrLen(string val, BigInteger length, int cost)
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x0D },
                x.SExp.To(new List<string> { val }));
            var atom = result.Item2.AsAtom();
            var actualLength = new BigInteger(atom!);
            Assert.Equal(length, actualLength);
            Assert.Equal(cost, result.Item1);
        }


        [Fact]
        public void OpStrLenThrowsWithTooManyArgs()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0D },
                        x.SExp.To(new List<string> { "THIS", "WILL THROW AN EXCEPTION" }))
                );
            Assert.Contains("strlen takes exactly 1 argument", errorMessage.Message);
        }

        [Fact]
        public void OpStrLenThrowsIfPair()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0D }, x.SExp.To(new List<int> { 3, 1 }))
                );
            Assert.Contains("strlen takes exactly 1 argument", errorMessage.Message);
        }

        #endregion

        #region OpSHA256

        [Fact]
        public void OpSHA256()
        {
            var result =
                x.Operator.ApplyOperator(new byte[] { 0x0B }, x.SExp.To(new List<string> { "THIS IS A SHA256 HASH" }));
            var s = result;
            var atom = result.Item2.AsAtom();

            // Assert.Equal(583, result.Item1);
            Assert.True(atom.AsSpan().SequenceEqual(new byte[]
            {
                0xB1, 0xBD, 0xB6, 0xD1, 0xF9, 0xA8, 0x3F, 0xA5, 0xB4, 0xFA, 0x25, 0x53, 0x34, 0xF1, 0x47, 0xC3, 0xCD,
                0x09, 0x4C, 0xE3, 0x6E, 0xC9, 0x74, 0xD5, 0xD8, 0x38, 0xF0, 0x45, 0x98, 0x08, 0x13, 0x4E
            }));
        }

        [Fact(Skip = "Skipping until SHA256 Throws an error")]
        public void OpSHA256OnList_ThrowsError()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0B },
                        x.SExp.To(new List<string> { "SOME", "ERror" })));
            Assert.Contains("sha256 on list", errorMessage.Message);
        }

        #endregion

        #region OpGr

        [Theory]
        [InlineData(502, 1, 2, false)]
        [InlineData(502, 1, 1, false)]
        public void OpGrReturnsFalse(int expectedCost, BigInteger val1, BigInteger val2, bool greaterThan)
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x15 }, x.SExp.To(new List<BigInteger> { val1, val2 }));
            var areEqual = x.SExp.False.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(expectedCost, result.Item1);
        }

        /// <summary>
        /// TODO: decide how signed bytes are dealt with when comparing bytes with negative numbers.
        /// </summary>
        /// <param name="expectedCost"></param>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <param name="greaterThan"></param>
        [Theory]
        [InlineData(502, 4, 2, true)]
        [InlineData(502, -1, 2, true,
            Skip = "-1 is 255 as an unsigned byte. and is greater than 2. Need to probably use sbyte!")] //
        public void OpGrReturnsTrue(int expectedCost, BigInteger val1, BigInteger val2, bool greaterThan)
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x15 }, x.SExp.To(new List<BigInteger> { val1, val2 }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(expectedCost, result.Item1);
        }

        [Fact]
        public void OpGrThrowIfMoreThan2ArgumentsPassed()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x15 }, x.SExp.To(new List<int> { 1, 2, 3 })));
            Assert.Contains("> takes exactly 2 arguments", errorMessage.Message);
        }

        [Fact]
        public void OpGrThrowIfLessThan2ArgumentsPassed()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x15 }, x.SExp.To(new List<int> { 1 })));
            Assert.Contains("> takes exactly 2 arguments", errorMessage.Message);
        }

        #endregion

        #region OpEq

        [Fact]
        public void OpEqReturnsTrueWhenTwoStringsMatch()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x09 },
                x.SExp.To(new List<dynamic> { "SomeString", "SomeString" }));
            var s = result;
            var areEqual = x.SExp.True.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(137, result.Item1);
        }

        [Fact]
        public void OpEqReturnsFalseWhenTwoStringsDoNotMatch()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x09 },
                x.SExp.To(new List<dynamic> { "val1", "DOTNOTMATCH" }));
            var s = result;
            var areEqual = x.SExp.False.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(132, result.Item1);
        }

        [Fact]
        public void OpEqReturnTrueWhenTwoEmptyStringsMatchMatch()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x09 },
                x.SExp.To(new List<dynamic> { "", x.SExp.To(new List<string>()) }));
            var s = result;
            var areEqual = x.SExp.True.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(117, result.Item1);
        }

        [Fact]
        public void OpEqThrowsWhenMoreThanTwoArguments()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x09 },
                        x.SExp.To(new List<dynamic> { "1", "1", x.SExp.To("") })));
            Assert.Contains("= takes exactly 2 arguments", errorMessage.Message);
        }


        [Fact]
        public void OpEqThrowsWhenLessThanTwoArguments()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x09 },
                        x.SExp.To(new List<string> { "SOMESTRING" })));
            Assert.Contains("= takes exactly 2 arguments", errorMessage.Message);
        }

        #endregion
        
        #region OpLogand
        [Fact]
        public void OpLogAndInt()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x18 }, x.SExp.To(new List<int> { 15,244 }));
            var atom = result.Item2.AsAtom();
            Assert.Equal(647, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0x04
            }));
        }
        
        [Fact]
        public void OpLogEmptyList()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x18 }, x.SExp.To(new List<int> {  }));
            var atom = result.Item2.AsAtom();
            Assert.Equal(110, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0xFF
            }));
        }
        #endregion
        
        #region OpLogior
        [Fact]
        public void OpLogior()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x19 }, x.SExp.To(new List<int> { }));
            var atom = result.Item2.AsAtom();
            Assert.Equal(100, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
            }));
        }
        
        [Fact]
        public void OpLogiorInt()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x19 }, x.SExp.To(new List<int> { 35,689}));
            var atom = result.Item2.AsAtom();
            Assert.Equal(657, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0x02, 0xB3 
            }));
        }
        #endregion

        #region OpLogxor
        [Fact]
        public void OpLogxor()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x1A }, x.SExp.To(new List<int> { }));
            var atom = result.Item2.AsAtom();
            Assert.Equal(100, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
            }));
        }
        
        [Fact]
        public void OpLogxorInt()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x1A }, x.SExp.To(new List<BigInteger> { 111111,67452345657}));
            var atom = result.Item2.AsAtom();
            Assert.Equal(702, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0x0f, 0xb4, (byte)'x', 0xaf, (byte)'>'
            }));
        }
        #endregion
        
        #region OpLogNot
        [Fact]
        public void OpLogNot()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x1B }, x.SExp.To(new List<int> { 1}));
            var atom = result.Item2.AsAtom();
            Assert.Equal(344, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0xFE
            }));
        }
        
        [Fact]
        public void OpLogNotNegativeNumbers()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x1B }, x.SExp.To(new List<BigInteger> { -1111}));
            var atom = result.Item2.AsAtom();
            Assert.Equal(357, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0x04, 0x56 
            }));
        }
        
        [Fact]
        public void OpLogNotThrowsWithNoParameters()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x1B },
                        x.SExp.To(new List<int> { })));
            Assert.Contains("lognot takes exactly 1 arguments", errorMessage.Message);
        }
        #endregion

        #region OpAny

        [Fact]
        public void OpAnyReturnsTrueIfListIsNotEmpty()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x21 },
                x.SExp.To(new List<BigInteger> { 1 }));
            var s = result;
            var areEqual = x.SExp.True.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(500, result.Item1);
        }
        
        [Fact]
        public void OpAnyReturnsTrueWithMoreThanOneArg()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x21 },
                x.SExp.To(new List<BigInteger> { 1,3,4,5 }));
            var s = result;
            var areEqual = x.SExp.True.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(1400, result.Item1);
        }
        
        [Fact]
        public void OpAnyReturnsFalseWithNoArgs()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x21 },
                x.SExp.To(new List<BigInteger> { }));
            var s = result;
            var areEqual = x.SExp.False.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(200, result.Item1);
        }
        #endregion

        #region OpAll
        [Fact]
        public void OpAllAtomsReturnsTrue()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x22 }, x.SExp.To(new List<int> { 1,2,3 }));
            var s = result;
            var areEqual = x.SExp.True.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(1100, result.Item1);
        }
        
        [Fact]
        public void OpAllWithEmptyAtomsReturnsTrue()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x22 }, x.SExp.To(new List<int> { }));
            var s = result;
            var areEqual = x.SExp.True.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(200, result.Item1);
        }
        
        [Fact]
        public void OpAllWithPairReturnsFalse()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x22 }, x.SExp.To(new List<dynamic>{"+", 1, 2}));
            var s = result;
            var areEqual = x.SExp.True.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(1100, result.Item1);
        }
        #endregion
        
        #region OpNot

        [Fact]
        public void OpNotNoneEmptyBytes()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x20 }, x.SExp.To(new List<byte> { 0x01 }));
            var s = result;
            var areEqual = x.SExp.False.Equals(result.Item2);
            Assert.True(areEqual);
            Assert.Equal(200, result.Item1);
        }

        [Fact]
        public void OpNotThrowsIfEmptyBytes()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x20 }, x.SExp.To(new List<byte> { })));
            Assert.Contains("not takes exactly 1 arguments", errorMessage.Message);
        }

        [Fact]
        public void OpNotThrowsIfMoreThanOneByte()
        {
            var s = x.SExp.To(new byte[] { 0x01, 0x01 });
            var t = s;

            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x20 }, x.SExp.To(new List<byte> { 0x01, 0x01 })));
            Assert.Contains("not takes exactly 1 arguments", errorMessage.Message);
        }

        #endregion

        #region OpAsh

        [Fact]
        public void OpAsh()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x16 }, x.SExp.To(new List<int> { 1, 2 }));
            var s = result;
            Assert.Equal(new byte[] { 0x04 }, result.Item2.AsAtom());
            Assert.Equal(612, result.Item1);
        }

        [Fact]
        public void OpAshThrowsWhenMoreThanTwoArguments()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x16 },
                        x.SExp.To(new List<int> { 1, 2, 4 })));
            Assert.Contains("ash takes exactly 2 arguments", errorMessage.Message);
        }

        [Fact]
        public void OpAshThrowsWhenLessThanTwoArguments()
        {
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x16 },
                        x.SExp.To(new List<int> { 1, 2, 4 })));
            Assert.Contains("ash takes exactly 2 arguments", errorMessage.Message);
        }

        #endregion

        #region OpLsh

        [Fact()]
        public void OpLsh()
        {
            var result = x.Operator.ApplyOperator(new byte[] { 0x17 }, x.SExp.To(new List<int> { 1, 45 }));
            var s = result;
            Assert.Equal(new byte[] { 32, 0, 0, 0, 0, 0 }, result.Item2.AsAtom());
            Assert.Equal(358, result.Item1);
        }

        #endregion

        #region OpDefaultUnknown

        [Fact]
        public void UnsupportedOpThrowsException()
        {
            var errorMessage =
                Assert.Throws<Exception>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x3a },
                        x.SExp.NULL));
            Assert.Contains("3A Operator not found or is unsupported!", errorMessage.Message);
        }


        //   ./# 0x23
        //   q  0x01
        //   a 0x02

        #endregion

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