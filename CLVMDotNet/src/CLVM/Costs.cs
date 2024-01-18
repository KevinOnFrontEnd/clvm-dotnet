namespace CLVMDotNet.CLVM
{

    public static class Costs
    {
        public const int IF_COST = 33;
        public const int CONS_COST = 50;
        public const int FIRST_COST = 30;
        public const int REST_COST = 30;
        public const int LISTP_COST = 19;

        public const int MALLOC_COST_PER_BYTE = 10;

        public const int ARITH_BASE_COST = 99;
        public const int ARITH_COST_PER_BYTE = 3;
        public const int ARITH_COST_PER_ARG = 320;

        public const int LOG_BASE_COST = 100;
        public const int LOG_COST_PER_BYTE = 3;
        public const int LOG_COST_PER_ARG = 264;
        public const int GRS_BASE_COST = 117;
        public const int GRS_COST_PER_BYTE = 1;

        public const int EQ_BASE_COST = 117;
        public const int EQ_COST_PER_BYTE = 1;

        public const int GR_BASE_COST = 498;
        public const int GR_COST_PER_BYTE = 2;

        public const int DIVMOD_BASE_COST = 1116;
        public const int DIVMOD_COST_PER_BYTE = 6;

        public const int DIV_BASE_COST = 988;
        public const int DIV_COST_PER_BYTE = 4;

        public const int SHA256_BASE_COST = 87;
        public const int SHA256_COST_PER_ARG = 134;
        public const int SHA256_COST_PER_BYTE = 2;

        public const int POINT_ADD_BASE_COST = 101094;
        public const int POINT_ADD_COST_PER_ARG = 1343980;

        public const int PUBKEY_BASE_COST = 1325730;
        public const int PUBKEY_COST_PER_BYTE = 38;

        public const int MUL_BASE_COST = 92;
        public const int MUL_COST_PER_OP = 885;
        public const int MUL_LINEAR_COST_PER_BYTE = 6;
        public const int MUL_SQUARE_COST_PER_BYTE_DIVIDER = 128;

        public const int STRLEN_BASE_COST = 173;
        public const int STRLEN_COST_PER_BYTE = 1;

        public const int PATH_LOOKUP_BASE_COST = 40;
        public const int PATH_LOOKUP_COST_PER_LEG = 4;
        public const int PATH_LOOKUP_COST_PER_ZERO_BYTE = 4;

        public const int CONCAT_BASE_COST = 142;
        public const int CONCAT_COST_PER_ARG = 135;
        public const int CONCAT_COST_PER_BYTE = 3;

        public const int BOOL_BASE_COST = 200;
        public const int BOOL_COST_PER_ARG = 300;

        public const int ASHIFT_BASE_COST = 596;
        public const int ASHIFT_COST_PER_BYTE = 3;

        public const int LSHIFT_BASE_COST = 277;
        public const int LSHIFT_COST_PER_BYTE = 3;

        public const int LOGNOT_BASE_COST = 331;
        public const int LOGNOT_COST_PER_BYTE = 3;

        public const int APPLY_COST = 90;
        public const int QUOTE_COST = 20;
    }
}