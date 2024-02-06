using System.Text;
using Xunit;
using clvm = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.SExp
{

    [Trait("SExp", "To")]
    public class To
    {
        [Fact]
        public void builds_correct_tree()
        {
            // Arrange
            
            // Act
            var s = clvm.SExp.To(new List<dynamic> { "+", 1, 2 });
            var tree = clvm.HelperFunctions.PrintTree(s);
            
            // Assert
            Assert.Equal("(43 (1 (2 () )))", tree);
        }

        [Fact]
        public void test_case_1()
        {
            // Arrange
            
            // Act
            var sexp = clvm.SExp.To(Encoding.UTF8.GetBytes("foo"));
            var t1 = clvm.SExp.To(new List<dynamic> { 1, sexp });
            
            // Assert
            clvm.HelperFunctions.ValidateSExp(t1);
        }

        [Fact]
        public void NumberAtomIsSet()
        {
            // Arrange
            
            // Act
            var a = clvm.SExp.To(1);
            var atom = a.AsAtom();
            
            // Assert
            Assert.Equal(new byte[] { 0x01}, atom);
        }
        
        [Fact]
        public void StringAtomIsSet()
        {
            // Arrange
            
            // Act
            var a = clvm.SExp.To("somestring");
            var atom = a.AsAtom();
            
            // Assert
            Assert.Equal(new byte[] { 115, 111, 109, 101, 115,116, 114, 105, 110, 103}, atom);
        }
        
        
        [Fact]
        public void TestListConversions()
        {
            // Arrange
            
            // Act
            var a = clvm.SExp.To(new List<int> { 1, 2, 3 });
            string expectedOutput = "(1 (2 (3 () )))";
            string result = clvm.HelperFunctions.PrintTree(a);
            
            // Assert
            Assert.Equal(expectedOutput, result);
        }

        [Fact]
        public void TestNullConversions()
        {
            // Arrange
            
            // Act
            var a = clvm.SExp.To(null);
            byte[] expected = Array.Empty<byte>();
            
            // Assert
            Assert.Equal(expected, a.AsAtom());
        }

        [Fact]
        public void empty_list_conversions()
        {
            // Arrange
            
            // Act
            var a = clvm.SExp.To(new object[] { });
            byte[] expected = new byte[0];
            
            // Assert
            Assert.Equal(expected, a.AsAtom());
        }

        [Fact]
        public void int_conversions()
        {
            // Arrange
            
            // Act
            var a = clvm.SExp.To(1337);
            byte[] expected = new byte[] { 0x5, 0x39 };
            
            // Assert
            Assert.Equal(expected, a.AsAtom());
        }


        [Fact]
        // SExp provides a view on top of a tree of arbitrary types, as long as
        // those types implement the CLVMObject protocol. This is an example of
        // a tree that's generate
        //
        // There is a subtle differences between the python version and the 
        // c# version of representing trees. 
        public void arbitrary_underlying_tree()
        {
            // Arrange
            var gentree1 = new GeneratedTree(5, 0);
            var gentree2 = new GeneratedTree(3, 0);
            var gentree3 = new GeneratedTree(3, 10);
            
            // Act
            var tree1 = clvm.SExp.To(gentree1);
            var tree2 = clvm.SExp.To(gentree2);
            var tree3 = clvm.SExp.To(gentree3);
            
            // Assert
            Assert.Equal(clvm.HelperFunctions.PrintTree(tree1),
                "(((((0 1 )(2 3 ))((4 5 )(6 7 )))(((8 9 )(10 11 ))((12 13 )(14 15 ))))((((16 17 )(18 19 ))((20 21 )(22 23 )))(((24 25 )(26 27 ))((28 29 )(30 31 )))))");
            Assert.Equal(clvm.HelperFunctions.PrintTree(tree2), "(((0 1 )(2 3 ))((4 5 )(6 7 )))");
            Assert.Equal(clvm.HelperFunctions.PrintTree(tree3), "(((10 11 )(12 13 ))((14 15 )(16 17 )))");
        }
    }
}