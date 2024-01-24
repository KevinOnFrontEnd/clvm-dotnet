namespace CLVMDotNet.CLVM
{
    public static class Keywords
    {
        public static string[] KEYWORDS = new string[]
        {
            // core opcodes 0x01-x08
            ".",
            "q",
            "a",
            "i",
            "c",
            "f",
            "r",
            "l",
            "x",

            // opcodes on atoms as strings 0x09-0x0f
            "=",
            ">s",
            "sha256",
            "substr",
            "strlen",
            "concat",
            //"."

            // opcodes on atoms as ints 0x10-0x17
            "+",
            "-",
            "*",
            "/",
            "divmod",
            ">",
            "ash",
            "lsh",

            // opcodes on atoms as vectors of bools 0x18-0x1c
            "logand",
            "logior",
            "logxor",
            "lognot",
            // ".",

            // opcodes for bls 1381 0x1d-0x1f
            "point_add",
            "pubkey_for_exp",
            // ".",

            // bool opcodes 0x20-0x23
            "not",
            "any",
            "all",
            ".",

            // misc 0x24
            "softfork"
        };

        public static Dictionary<byte, string> KEYWORD_FROM_ATOM => InitializeKeywordFromAtom();
        public static Dictionary<string, byte> KEYWORD_TO_ATOM => InitializeKeywordToAtom();

        private static Dictionary<byte, string> InitializeKeywordFromAtom()
        {
            var keywordFromAtom = new Dictionary<byte, string>();
            keywordFromAtom.Add(0x01, "q");
            keywordFromAtom.Add(0x02, "a");
            keywordFromAtom.Add(0x03, "i");
            keywordFromAtom.Add(0x04, "c");
            keywordFromAtom.Add(0x05, "f");
            keywordFromAtom.Add(0x06, "r");
            keywordFromAtom.Add(0x07, "l");
            keywordFromAtom.Add(0x08, "x");
            keywordFromAtom.Add(0x09, "=");
            keywordFromAtom.Add(0x0a, ">s");
            keywordFromAtom.Add(0x0b, "sha256");
            keywordFromAtom.Add(0x0c, "substr");
            keywordFromAtom.Add(0x0d, "strlen");
            keywordFromAtom.Add(0x0e, "concat");
            keywordFromAtom.Add(0x0f, ".");
            keywordFromAtom.Add(0x10, "+");
            keywordFromAtom.Add(0x11, "-");
            keywordFromAtom.Add(0x12, "*");
            keywordFromAtom.Add(0x13, "/");
            keywordFromAtom.Add(0x14, "divmod");
            keywordFromAtom.Add(0x15, ">");
            keywordFromAtom.Add(0x16, "ash");
            keywordFromAtom.Add(0x17, "lsh");
            keywordFromAtom.Add(0x18, "logand");
            keywordFromAtom.Add(0x19, "logior");
            keywordFromAtom.Add(0x1a, "logxor");
            keywordFromAtom.Add(0x1b, "lognot");
            keywordFromAtom.Add(0x1c, ".");
            keywordFromAtom.Add(0x1d, "point_add");
            keywordFromAtom.Add(0x1e, "pubkey_for_exp");
            keywordFromAtom.Add(0x1f, ".");
            keywordFromAtom.Add(0x20, "not");
            keywordFromAtom.Add(0x21, "any");
            keywordFromAtom.Add(0x22, "all");
            keywordFromAtom.Add(0x23, ".");
            return keywordFromAtom;
        }

        private static Dictionary<string, byte> InitializeKeywordToAtom()
        {
            var keywordToAtom = new Dictionary<string, byte>();
            keywordToAtom.Add(".", 0x00);
            keywordToAtom.Add("q", 0x01);
            keywordToAtom.Add("a", 0x02);
            keywordToAtom.Add("i", 0x03);
            keywordToAtom.Add("c", 0x04);
            keywordToAtom.Add("f", 0x05);
            keywordToAtom.Add("r", 0x06);
            keywordToAtom.Add("l", 0x07);
            keywordToAtom.Add("x", 0x08);
            keywordToAtom.Add("=", 0x09);
            keywordToAtom.Add(">s", 0x0a);
            keywordToAtom.Add("sha256", 0x0b);
            keywordToAtom.Add("substr", 0x0c);
            keywordToAtom.Add("strlen", 0x0d);
            keywordToAtom.Add("concat", 0x0e);
            keywordToAtom.Add("+", 0x10);
            keywordToAtom.Add("-", 0x11);
            keywordToAtom.Add("*", 0x12);
            keywordToAtom.Add("/", 0x13);
            keywordToAtom.Add("divmod", 0x14);
            keywordToAtom.Add(">", 0x15);
            keywordToAtom.Add("ash", 0x16);
            keywordToAtom.Add("lsh", 0x17);
            keywordToAtom.Add("logand", 0x18);
            keywordToAtom.Add("logior", 0x19);
            keywordToAtom.Add("logxor", 0x1a);
            keywordToAtom.Add("lognot", 0x1b);
            keywordToAtom.Add("point_add", 0x1d);
            keywordToAtom.Add("pubkey_for_exp", 0x1e);
            keywordToAtom.Add("not", 0x20);
            keywordToAtom.Add("any", 0x21);
            keywordToAtom.Add("all", 0x22);
            keywordToAtom.Add("softfork", 0x24);
            return keywordToAtom;
        }
    }
}