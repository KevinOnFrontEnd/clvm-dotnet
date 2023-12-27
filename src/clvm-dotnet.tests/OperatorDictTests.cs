// using Xunit;
//
// namespace clvm_dotnet.tests;
//
// public class operatordicttests
// {
//     [Fact]
//     public void TestOperatorDictConstructor()
//     {
//         // Constructing should fail if quote or apply are not specified,
//         // either by object property or by keyword argument.
//         // Note that they cannot be specified in the operator dictionary itself.
//         var d = new Dictionary<int, string> { { 1, "hello" }, { 2, "goodbye" } };
//
//         Assert.Throws<AttributeNotFoundException>(() => new OperatorDict(d));
//         Assert.Throws<AttributeNotFoundException>(() => new OperatorDict(d, apply: 1));
//         Assert.Throws<AttributeNotFoundException>(() => new OperatorDict(d, quote: 1));
//
//         var o = new OperatorDict(d, apply: 1, quote: 2);
//
//         // Why does the constructed Operator dict contain entries for "apply":1 and "quote":2 ?
//         // assert d == o
//         Assert.Equal(1, o.ApplyAtom);
//         Assert.Equal(2, o.QuoteAtom);
//
//         // Test construction from an already existing OperatorDict
//         var o2 = new OperatorDict(o);
//         Assert.Equal(1, o2.ApplyAtom);
//         Assert.Equal(2, o2.QuoteAtom);
//     }
// }