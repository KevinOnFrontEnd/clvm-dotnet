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
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x10 }, x.SExp.To(new List<int> { 3, 4, 5 }));
            
            // Assert
        }

        #endregion


        [Fact]
        public void OpSubtract()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x11 }, x.SExp.To(new List<int> { 3, 1 }));
            
            // Assert
        }

        #region OpDivide

        [Fact]
        public void OpDivide()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new List<BigInteger> { 10, 2 }));

            // Assert
        }

        [Fact]
        public void OpDivideThrowsExceptionIfDividingByZero()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new List<BigInteger> { 10, 0 }))
                );
            
            // Assert
            Assert.Contains("div with 0", errorMessage.Message);
        }

        [Fact()]
        public void OpDivideThrowsExceptionWithNegativeOperand1()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new List<BigInteger> { -1, 5 }))
                );
            
            // Assert
            Assert.Contains("div operator with negative operands is deprecated", errorMessage.Message);
        }

        #endregion

        [Fact]
        public void OpMultiply()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x12 }, x.SExp.To(new List<BigInteger> { 10, 3 }));

            // Assert
        }

        [Fact]
        public void OpDivMod()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x14 }, x.SExp.To(new List<BigInteger> { 3, 5 }));

            // Assert
        }

        [Fact]
        public void OpConcat()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x0E }, x.SExp.To(new List<string> { "test", "ing" }));

            // Assert
        }

        #region OpSubStr

        [Theory]
        [InlineData(-1)]
        [InlineData(10)]
        public void OpSubstrThrowsWhenOutOfBounds(int startIndex)
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new List<dynamic> { "kevin", startIndex }))
                );
            
            // Assert
            Assert.Contains("invalid indices for substr", errorMessage.Message);
        }

        [Fact]
        public void OpSubstrThrowsWithTwoManyArgs()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new List<dynamic> { "kevin", 1, 2, 3 }))
                );
            
            // Assert
            Assert.Contains("substr takes exactly 2 or 3 arguments", errorMessage.Message);
        }

        [Fact]
        public void OpSubstrThrowsWithTooFewArgs()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new List<dynamic> { "kevin" }))
                );
            
            // Assert
            Assert.Contains("substr takes exactly 2 or 3 arguments", errorMessage.Message);
        }

        [Theory]
        [InlineData("1", "somelongstring", 4, "longstring")]
        [InlineData("1", "somelongstring", 0, "somelongstring")]
        [InlineData("1", "somelongstring", 13, "g")]
        public void OpSubstrReturnsSubStringWithCost(string stringCost, string val, int startindex, string expectedResult)
        {
            // Arrange
            BigInteger cost = BigInteger.Parse(stringCost);
            
            // Act
            var result =
                x.Operator.ApplyOperator(new byte[] { 0x0C }, x.SExp.To(new List<dynamic> { val, startindex }));
            var atom = result.Item2.AsAtom();
            string text = Encoding.UTF8.GetString(atom);
            
            // Assert
            Assert.Equal(expectedResult, text);
            Assert.Equal(cost, result.Item1);
        }

        [Theory]
        [InlineData("1", "somelongstring", 0, 2, "so")]
        [InlineData("1", "somelongstring", 4, 5, "l")]
        [InlineData("1", "somelongstring", 13, 14, "g")]
        [InlineData("1", "somelongstring", 3, 12, "elongstri")]
        public void OpSubstrReturnsSubStringOfNumberOfCharactersWithCost(string stringCost, string val, int startindex,
            int endIndex, string expectedResult)
        {
            // Arrange
            BigInteger cost = BigInteger.Parse(stringCost);
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x0C },
                x.SExp.To(new List<dynamic> { val, startindex, endIndex }));
            var atom = result.Item2.AsAtom();
            string text = Encoding.UTF8.GetString(atom!);
            
            // Assert
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
        public void OpStrLen(string val, int length, int cost)
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x0D },
                x.SExp.To(new List<string> { val }));
            var atom = result.Item2.AsAtom();
            var actualLength = new BigInteger(atom!);
            
            // Assert
            Assert.Equal(length, actualLength);
            Assert.Equal(cost, result.Item1);
        }


        [Fact]
        public void OpStrLenThrowsWithTooManyArgs()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0D },
                        x.SExp.To(new List<string> { "THIS", "WILL THROW AN EXCEPTION" }))
                );
            
            // Assert
            Assert.Contains("strlen takes exactly 1 argument", errorMessage.Message);
        }

        [Fact]
        public void OpStrLenThrowsIfPair()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0D }, x.SExp.To(new List<int> { 3, 1 }))
                );
            
            // Assert
            Assert.Contains("strlen takes exactly 1 argument", errorMessage.Message);
        }

        #endregion

        #region OpSHA256

        [Fact]
        public void OpSHA256()
        {
            // Arrange
            
            // Act
            var result =
                x.Operator.ApplyOperator(new byte[] { 0x0B }, x.SExp.To(new List<string> { "THIS IS A SHA256 HASH" }));
            var atom = result.Item2.AsAtom();

            // Assert
            Assert.Equal(583, result.Item1);
            Assert.True(atom.AsSpan().SequenceEqual(new byte[]
            {
                0xB1, 0xBD, 0xB6, 0xD1, 0xF9, 0xA8, 0x3F, 0xA5, 0xB4, 0xFA, 0x25, 0x53, 0x34, 0xF1, 0x47, 0xC3, 0xCD,
                0x09, 0x4C, 0xE3, 0x6E, 0xC9, 0x74, 0xD5, 0xD8, 0x38, 0xF0, 0x45, 0x98, 0x08, 0x13, 0x4E
            }));
        }

        [Fact(Skip = "Skipping until SHA256 Throws an error")]
        public void OpSHA256OnList_ThrowsError()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0B },
                        x.SExp.To(new List<string> { "SOME", "ERror" })));
            
            // Assert
            Assert.Contains("sha256 on list", errorMessage.Message);
        }

        #endregion
        
        #region GrBytes
        [Theory]
        [InlineData(119, "a", "b")]
        [InlineData(131, "testing", "testing")]
        public void OpGrBytesReturnsFalse(int expectedCost, string val1, string val2)
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x0A }, x.SExp.To(new List<string> { val1, val2 }));
            var areEqual = x.SExp.False.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(expectedCost, result.Item1);
        }
        
        [Theory]
        [InlineData(119, "b", "a")]
        public void OpGrBytesReturnsTrue(int expectedCost, string val1, string val2)
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x0A }, x.SExp.To(new List<string> { val1, val2 }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(expectedCost, result.Item1);
        }
        
        [Fact]
        public void OpGrBytesThrowsWithMoreThanTwoParameters()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x0A },
                        x.SExp.To(new List<String> { "val1", "val", "val3" })));
            
            // Assert
            Assert.Contains(">s takes exactly 2 arguments", errorMessage.Message);
        }
        
        //TODO: Add test to throw when OpGrBytes is called on a pair
        #endregion
        
        #region OpGr

        [Theory]
        [InlineData(502, "1", "2", false)]
        [InlineData(502, "1", "1", false)]
        public void OpGrReturnsFalse(int expectedCost, string strVal1, string strVal2, bool greaterThan)
        {
            // Arrange
            BigInteger val1 = BigInteger.Parse(strVal1);
            BigInteger val2 = BigInteger.Parse(strVal2);
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x15 }, x.SExp.To(new List<BigInteger> { val1, val2 }));
            var areEqual = x.SExp.False.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(expectedCost, result.Item1);
        }
        
        [Theory]
        [InlineData(502, "4", "2", true)]
        [InlineData(502, "-1", "2", true,
            Skip = "-1 is 255 as an unsigned byte. and is greater than 2. Need to probably use sbyte!")] //
        public void OpGrReturnsTrue(int expectedCost, string strVal1, string strVal2, bool greaterThan)
        {
            // Arrange
            BigInteger val1 = BigInteger.Parse(strVal1);
            BigInteger val2 = BigInteger.Parse(strVal2);
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x15 }, x.SExp.To(new List<BigInteger> { val1, val2 }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(expectedCost, result.Item1);
        }

        [Fact]
        public void OpGrThrowIfMoreThan2ArgumentsPassed()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x15 }, x.SExp.To(new List<int> { 1, 2, 3 })));
            
            // Assert
            Assert.Contains("> takes exactly 2 arguments", errorMessage.Message);
        }

        [Fact]
        public void OpGrThrowIfLessThan2ArgumentsPassed()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x15 }, x.SExp.To(new List<int> { 1 })));
            
            // Assert
            Assert.Contains("> takes exactly 2 arguments", errorMessage.Message);
        }

        #endregion

        #region OpEq

        [Fact]
        public void OpEqReturnsTrueWhenTwoStringsMatch()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x09 },
                x.SExp.To(new List<dynamic> { "SomeString", "SomeString" }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(137, result.Item1);
        }

        [Fact]
        public void OpEqReturnsFalseWhenTwoStringsDoNotMatch()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x09 },
                x.SExp.To(new List<dynamic> { "val1", "DOTNOTMATCH" }));
            var areEqual = x.SExp.False.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(132, result.Item1);
        }

        [Fact]
        public void OpEqReturnTrueWhenTwoEmptyStringsMatchMatch()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x09 },
                x.SExp.To(new List<dynamic> { "", x.SExp.To(new List<string>()) }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(117, result.Item1);
        }

        [Fact]
        public void OpEqThrowsWhenMoreThanTwoArguments()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x09 },
                        x.SExp.To(new List<dynamic> { "1", "1", x.SExp.To("") })));
            
            // Assert
            Assert.Contains("= takes exactly 2 arguments", errorMessage.Message);
        }


        [Fact]
        public void OpEqThrowsWhenLessThanTwoArguments()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x09 },
                        x.SExp.To(new List<string> { "SOMESTRING" })));
            
            // Assert
            Assert.Contains("= takes exactly 2 arguments", errorMessage.Message);
        }

        #endregion

        #region OpLogand

        [Fact]
        public void OpLogAndInt()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x18 }, x.SExp.To(new List<int> { 15, 244 }));
            var atom = result.Item2.AsAtom();
            
            // Assert
            Assert.Equal(647, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0x04
            }));
        }

        [Fact]
        public void OpLogEmptyList()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x18 }, x.SExp.To(new List<int> { }));
            var atom = result.Item2.AsAtom();
            
            // Assert
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
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x19 }, x.SExp.To(new List<int> { }));
            var atom = result.Item2.AsAtom();
            
            // Assert
            Assert.Equal(100, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
            }));
        }

        [Fact]
        public void OpLogiorInt()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x19 }, x.SExp.To(new List<int> { 35, 689 }));
            var atom = result.Item2.AsAtom();
            
            // Assert
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
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x1A }, x.SExp.To(new List<int> { }));
            var atom = result.Item2.AsAtom();
            
            // Assert
            Assert.Equal(100, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
            }));
        }

        [Fact]
        public void OpLogxorInt()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x1A },
                x.SExp.To(new List<BigInteger> { 111111, 67452345657 }));
            var atom = result.Item2.AsAtom();
            
            // Assert
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
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x1B }, x.SExp.To(new List<int> { 1 }));
            var atom = result.Item2.AsAtom();
            
            // Assert
            Assert.Equal(344, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0xFE
            }));
        }

        [Fact]
        public void OpLogNotNegativeNumbers()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x1B }, x.SExp.To(new List<BigInteger> { -1111 }));
            var atom = result.Item2.AsAtom();
            
            // Assert
            Assert.Equal(357, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0x04, 0x56
            }));
        }

        [Fact]
        public void OpLogNotThrowsWithNoParameters()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x1B },
                        x.SExp.To(new List<int> { })));
            
            // Assert
            Assert.Contains("lognot takes exactly 1 arguments", errorMessage.Message);
        }

        #endregion

        #region OpPointAdd

        #endregion

        #region OpPubkeyForExp

        [Fact()]
        public void OpPubKeyForExp()
        {
            // Arrange
            
            // Act
            var result =
                x.Operator.ApplyOperator(new byte[] { 0x1E }, x.SExp.To(new List<String> { "this is a test" }));
            var atom = result.Item2.AsAtom();
            
            // Assert
            Assert.Equal(1326742, result.Item1);
            Assert.True(atom!.SequenceEqual(new byte[]
            {
                0xB3, 0xFD, 0x19, 0xF6, 0xB1, 0xA7, 0x59, 0xB9, 0x6E, 0x98, 0xE7, 0x45, 0x6F, 0x2F, 0x3F, 0x0C, 0x45,
                0xB0, 0xA7, 0xA1, 0x24, 0x3F, 0xF9, 0x40, 0x90, 0xFE, 0xFC, 0x51, 0x6C, 0x1B, 0x92, 0x9B,
                0x33, 0xB4, 0xF0, 0xC1, 0xC0, 0xF9, 0xBF, 0xEE, 0xD7, 0xB3, 0xC9, 0xC4, 0xFB, 0xB6, 0x00, 0x31
            }));
        }
        
        [Fact]
        public void OpPubKeyThrowsWithNoArgumentsp()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x1E }, x.SExp.To(new List<String> { })));
            
            // Assert
            Assert.Contains("pubkey_for_exp takes exactly 1 arguments", errorMessage.Message);

        }
        #endregion

        #region OpAny

        [Fact]
        public void OpAnyReturnsTrueIfListIsNotEmpty()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x21 },
                x.SExp.To(new List<BigInteger> { 1 }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(500, result.Item1);
        }

        [Fact]
        public void OpAnyReturnsTrueWithMoreThanOneArg()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x21 },
                x.SExp.To(new List<BigInteger> { 1, 3, 4, 5 }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(1400, result.Item1);
        }

        [Fact]
        public void OpAnyReturnsFalseWithNoArgs()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x21 },
                x.SExp.To(new List<BigInteger> { }));
            var areEqual = x.SExp.False.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(200, result.Item1);
        }

        #endregion

        #region OpAll

        [Fact]
        public void OpAllAtomsReturnsTrue()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x22 }, x.SExp.To(new List<int> { 1, 2, 3 }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(1100, result.Item1);
        }

        [Fact]
        public void OpAllWithEmptyAtomsReturnsTrue()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x22 }, x.SExp.To(new List<int> { }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(200, result.Item1);
        }

        [Fact]
        public void OpAllWithPairReturnsFalse()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x22 }, x.SExp.To(new List<dynamic> { "+", 1, 2 }));
            var areEqual = x.SExp.True.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(1100, result.Item1);
        }

        #endregion

        #region OpNot

        [Fact]
        public void OpNotNoneEmptyBytes()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x20 }, x.SExp.To(new List<byte> { 0x01 }));
            var areEqual = x.SExp.False.Equals(result.Item2);
            
            // Assert
            Assert.True(areEqual);
            Assert.Equal(200, result.Item1);
        }

        [Fact]
        public void OpNotThrowsIfEmptyBytes()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x20 }, x.SExp.To(new List<byte> { })));
            
            // Assert
            Assert.Contains("not takes exactly 1 arguments", errorMessage.Message);
        }

        [Fact]
        public void OpNotThrowsIfMoreThanOneByte()
        {
            // Arrange
            
            // Act
            var s = x.SExp.To(new byte[] { 0x01, 0x01 });

            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x20 }, x.SExp.To(new List<byte> { 0x01, 0x01 })));
            
            // Assert
            Assert.Contains("not takes exactly 1 arguments", errorMessage.Message);
        }

        #endregion

        #region OpAsh

        [Fact]
        public void OpAsh()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x16 }, x.SExp.To(new List<int> { 1, 2 }));

            // Assert
            Assert.Equal(new byte[] { 0x04 }, result.Item2.AsAtom());
            Assert.Equal(612, result.Item1);
        }

        [Fact]
        public void OpAshThrowsWhenMoreThanTwoArguments()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x16 },
                        x.SExp.To(new List<int> { 1, 2, 4 })));
            Assert.Contains("ash takes exactly 2 arguments", errorMessage.Message);
        }

        [Fact]
        public void OpAshThrowsWhenLessThanTwoArguments()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<x.EvalError>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x16 },
                        x.SExp.To(new List<int> { 1, 2, 4 })));
            
            // Assert
            Assert.Contains("ash takes exactly 2 arguments", errorMessage.Message);
        }

        #endregion

        #region OpLsh

        [Fact()]
        public void OpLsh()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x17 }, x.SExp.To(new List<int> { 1, 45 }));

            // Assert
            Assert.Equal(new byte[] { 32, 0, 0, 0, 0, 0 }, result.Item2.AsAtom());
            Assert.Equal(358, result.Item1);
        }

        #endregion

        #region OpDefaultUnknown

        [Fact]
        public void UnsupportedOpThrowsException()
        {
            // Arrange
            
            // Act
            var errorMessage =
                Assert.Throws<Exception>(() =>
                    x.Operator.ApplyOperator(new byte[] { 0x3a },
                        x.SExp.NULL));
            
            // Assert
            Assert.Contains("3A Operator not found or is unsupported!", errorMessage.Message);
        }

        [Fact]
        public void OpDefaultUnknownAtom()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x02 }, x.SExp.To(new List<int> { 1, 2 }));

            // Assert
            Assert.Null(result.Item2.AsAtom());
            Assert.Equal(1, result.Item1);
        }
        
        [Fact]
        public void OpDefaultUnknownQuote()
        {
            // Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x01 }, x.SExp.To(new List<int> { 1, 2 }));

            // Assert
            Assert.Null(result.Item2.AsAtom());
            Assert.Equal(1, result.Item1);
        }
        
        [Fact]
        public void OpDefaultUnknownDot()
        {
            //Arrange
            
            // Act
            var result = x.Operator.ApplyOperator(new byte[] { 0x23 }, x.SExp.To(new List<int> { 1, 2 }));

            // Assert
            Assert.Null(result.Item2.AsAtom());
            Assert.Equal(1, result.Item1);
        }
        
        //TODO: Determine how apply operator can be called to call OpdefaultUnknown with op other than 0x01,0x02,0x23
        #endregion
    }
}