namespace CLVMDotNet;

public static class Keywords
{
         
    public static string[] KEYWORDS = new string[]
    {
        // core opcodes 0x01-x08
        ". q a i c f r l x ",

        // opcodes on atoms as strings 0x09-0x0f
        "= >s sha256 substr strlen concat . ",

        // opcodes on atoms as ints 0x10-0x17
        "+ - * / divmod > ash lsh ",

        // opcodes on atoms as vectors of bools 0x18-0x1c
        "logand logior logxor lognot . ",

        // opcodes for bls 1381 0x1d-0x1f
        "point_add pubkey_for_exp . ",

        // bool opcodes 0x20-0x23
        "not any all . ",

        // misc 0x24
        "softfork "
    };

    public static Dictionary<byte[], string> KEYWORD_FROM_ATOM  => InitializeKeywordFromAtom();
    public static Dictionary<string, byte[]> KEYWORD_TO_ATOM => InitializeKeywordToAtom();

    private static Dictionary<byte[], string> InitializeKeywordFromAtom()
    {
        var keywordFromAtom = new Dictionary<byte[], string>();

        for (int k = 0; k < KEYWORDS.Length; k++)
        {
            byte[] keyBytes = BitConverter.GetBytes(k);
            keywordFromAtom[keyBytes] = KEYWORDS[k];
        }

        return keywordFromAtom;
    }

    private static Dictionary<string, byte[]> InitializeKeywordToAtom()
    {
        var keywordToAtom = new Dictionary<string, byte[]>();

        for (int k = 0; k < KEYWORDS.Length; k++)
        {
            keywordToAtom[KEYWORDS[k]] = BitConverter.GetBytes(k);
        }

        return keywordToAtom;
    }

}