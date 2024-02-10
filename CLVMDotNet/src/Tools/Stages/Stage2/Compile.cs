// using CLVMDotNet.CLVM;
//
// namespace CLVMDotNet.Tools.Stages.Stage2;
//
// public static class Compile
// {
//     public static byte[] QUOTE_ATOM => CLVM.Operators.KEYWORD_TO_ATOM()["q"];
//     public static byte[] APPLY_ATOM => CLVM.Operators.KEYWORD_TO_ATOM()["a"];
//     public static byte[] CONS_ATOM => CLVM.Operators.KEYWORD_TO_ATOM()["c"];
//     
//     
//     public static SExp DoComProg(SExp prog, SExp macroLookup, SExp symbolTable, Func<CLVMObject, CLVMObject> runProgram)
//     {
//         // Lower "quote" to "q"
//         prog = LowerQuote(prog, macroLookup, symbolTable, runProgram);
//
//         // Quote atoms
//         if (prog.Nullp() || !prog.Listp())
//         {
//             var atom = prog.AsAtom();
//             if (atom == HelperFunctions.ConvertAtomToBytes("@"))
//             {
//                 return SExp.To(NodePath.TOP.AsShortPath());
//             }
//             foreach (var pair in symbolTable.AsIter())
//             {
//                 var symbol = pair.First().AsAtom();
//                 var value = pair.Rest().First();
//                 if (symbol == atom)
//                 {
//                     return SExp.To(value);
//                 }
//             }
//             return SExp.To(Quote(prog));
//         }
//
//         var operatorObj = prog.First();
//         if (operatorObj.Listp())
//         {
//             // (com ((OP) . RIGHT)) => (a (com (q OP)) 1)
//             var innerExp = Helpers.Eval(SExp.To(new CLVMObject(new List<object> { "com", Quote(operatorObj), Quote(macroLookup), Quote(symbolTable) })), NodePath.TOP.AsShortPath());
//             return SExp.To(new CLVMObject(new List<object> { innerExp }));
//         }
//
//         var asAtom = operatorObj.AsAtom();
//
//         foreach (var macroPair in macroLookup.AsIter())
//         {
//             var macroName = macroPair.First().AsAtom();
//             if (macroName!.SequenceEqual(asAtom))
//             {
//                 var macroCode = macroPair.Rest().First();
//                 var postProg = Helpers.Brun(macroCode, prog.Rest());
//                 return Helpers.Eval(SExp.To(new CLVMObject(new List<object> { "com", postProg, Quote(macroLookup), Quote(symbolTable) })), NodePath.TOP.AsShortPath());
//             }
//         }
//
//         if (COMPILE_BINDINGS.ContainsKey(asAtom))
//         {
//             Func<CLVMObject, MacroLookup, SymbolTable, Func<CLVMObject, CLVMObject>, CLVMObject> f = COMPILE_BINDINGS[asAtom];
//             var postProg = f(prog.Rest(), macroLookup, symbolTable, runProgram);
//             return Helpers.Eval(SExp.To(Quote(postProg)), NodePath.TOP.AsShortPath());
//         }
//
//         if (operatorObj.Equals(QUOTE_ATOM))
//         {
//             return prog;
//         }
//
//         var compiledArgs = prog.Rest().AsIter().Select(arg => DoComProg(arg, macroLookup, symbolTable, runProgram)).ToList();
//         var r = SExp.To(new CLVMObject(new List<object> { operatorObj }.Concat(compiledArgs)));
//
//         if (PASS_THROUGH_OPERATORS.Contains(asAtom) || asAtom.StartsWith("_"))
//         {
//             return r;
//         }
//
//         foreach (var pair in symbolTable.AsPython())
//         {
//             var symbol = pair.First().AsAtom();
//             var value = pair.Rest().First();
//             if (symbol == "*")
//             {
//                 return r;
//             }
//             if (symbol.SequenceEqual(asAtom))
//             {
//                 var newArgs = Helpers.Eval(SExp.To(new CLVMObject(new List<object> { "opt", new CLVMObject(new List<object> { "com", Quote(new CLVMObject(new List<object> { "list" }.Concat(prog.Rest().AsIter()))), Quote(macroLookup), Quote(symbolTable) }) })), TOP.AsPath());
//                 r = SExp.To(new CLVMObject(new List<object> { APPLY_ATOM, value, new CLVMObject(new List<object> { NodePath.Left.AsShortPath(), newArgs }) }));
//                 return r;
//             }
//         }
//
//         throw new SyntaxException($"can't compile , unknown operator");
//         //throw new Exception($"can't compile {Disassemble(prog)}, unknown operator");
//     }
//
//     private static SExp Quote(SExp sexp)
//     {
//         return new SExp(new List<dynamic> { "q", sexp });
//     }
//     
//     public static SExp CompileMacros(SExp args, SExp macroLookup, SExp symbolTable, Func<CLVMObject, CLVMObject> runProgram)
//     {
//         return SExp.To(Helpers.Quote(macroLookup));
//     }
//     
//     public static SExp CompileSymbols(SExp args, SExp macroLookup, SExp symbolTable, Func<CLVMObject, CLVMObject> runProgram)
//     {
//         return SExp.To(Helpers.Quote(symbolTable));
//     }
//     
//     public static SExp CompileQQ(SExp args, SExp macroLookup, SExp symbolTable, Func<CLVMObject, CLVMObject> runProgram, int level = 1)
//     {
//         SExp Com(SExp sexp)
//         {
//             return DoComProg(sexp, macroLookup, symbolTable, runProgram);
//         }
//         
//         var sexp = args.First();
//         if (!sexp.Listp() || sexp.Nullp())
//         {
//             // (qq ATOM) => (q . ATOM)
//             return SExp.To(Helpers.Quote(sexp));
//         }
//
//         if (sexp.Listp() && !sexp.First().Listp())
//         {
//             var op = sexp.First().AsAtom();
//             if (op == HelperFunctions.ConvertAtomToBytes("qq"))
//             {
//                 var subexp = CompileQQ(sexp.Rest(), macroLookup, symbolTable, runProgram, level + 1);
//                 return Com(SExp.To(new CLVMObject(new List<object> { CONS_ATOM, op, new CLVMObject(new List<object> { CONS_ATOM, subexp, Quote(0) }) })), macroLookup, symbolTable, runProgram);
//             }
//             if (op == HelperFunctions.ConvertAtomToBytes("unquote"))
//             {
//                 if (level == 1)
//                 {
//                     // (qq (unquote X)) => X
//                     
//                     //return Com(sexp.Rest().First(), macroLookup, symbolTable, runProgram);
//                 }
//                 var subexp = CompileQQ(sexp.Rest(), macroLookup, symbolTable, runProgram, level - 1);
//                 return Com(SExp.To(new CLVMObject(new List<object> { CONS_ATOM, op, new CLVMObject(new List<object> { CONS_ATOM, subexp, Quote(0) }) })), macroLookup, symbolTable, runProgram);
//             }
//         }
//
//         // (qq (a . B)) => (c (qq a) (qq B))
//         var A = Com(SExp.To(new List<dynamic> { "qq", sexp.First() }));
//         var B = Com(SExp.To(new List<dynamic> { "qq", sexp.Rest() }));
//         return SExp.To(new SExp(new List<dynamic> { CONS_ATOM, A, B }));
//     }
//     
//     public static SExp LowerQuote(SExp prog, SExp? macroLookup = null, SExp? symbolTable = null, Func<CLVMObject, CLVMObject> runProgram = null)
//     {
//         if (prog.Nullp())
//         {
//             return prog;
//         }
//
//         if (prog.Listp())
//         {
//             if (prog.First().AsAtom() == HelperFunctions.ConvertAtomToBytes("quote"))
//             {
//                 // Note: quote should have exactly one arg, so the length of
//                 // quoted list should be 2: "(quote arg)"
//                 if (!prog.Rest().Rest().Nullp())
//                 {
//                     throw new SyntaxException($"Compilation error while compiling  quote takes exactly one argument.");
//                     //throw new SyntaxException($"Compilation error while compiling [{Disassemble(prog)}]. quote takes exactly one argument.");
//                 }
//                 return SExp.To(Quote(LowerQuote(prog.Rest().First())));
//             }
//             else
//             {
//                 return SExp.To((LowerQuote(prog.First()), LowerQuote(prog.Rest())));
//             }
//         }
//         else
//         {
//             return prog;
//         }
//     }
// }