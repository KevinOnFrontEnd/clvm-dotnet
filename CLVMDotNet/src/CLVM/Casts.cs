using System.Net.Sockets;
using System.Numerics;

namespace CLVMDotNet.CLVM
{
    public static class Casts
    {
        public static BigInteger IntFromBytes(byte[] blob)
        {
            int size = blob.Length;
            if (size == 0)
            {
                return BigInteger.Zero;
            }

            return new BigInteger(blob, isBigEndian: true);
        }

        /// <summary>
        /// In python integers are dynamically sized, so working out the number of bytes required in
        /// c# needs to first see if the number will fit into a number of datatypes.
        ///
        /// 0-255 (byte)
        /// 
        ///
        /// This is required to see if the biginteger parameter is able to fit into a smaller
        /// datatype and converting them to bytes.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static byte[] IntToBytes(BigInteger v)
        {
            if (v == 0)
            {
                byte[] emptyByteArray = new byte[0];
                return emptyByteArray;
            }
            
            //v can fit into a byte 0-255 (unsigned)
            if (v >= byte.MinValue && v <= byte.MaxValue)
            {
                var intValue = (byte)v;
                var b = (byte)0x00;
                if (v < 128)
                {
                    byte[] byteArray = new[] { intValue };
                    return byteArray;
                }
                else
                {
                    byte[] byteArray = new[] { b,intValue };
                    return byteArray;
                }
            }

            //v can fit into an sbyte -128 to 127 (signed)
            if (v >= sbyte.MinValue && v <= sbyte.MaxValue)
            {
                sbyte sbyteValue = (sbyte)v;
                byte byteValue = (byte)sbyteValue;
                byte[] byteArray = new[] { byteValue };
                return byteArray;
            }

            //v can fit into a short -32,768 to 32,767 (signed)
            if (v >= short.MinValue && v <= short.MaxValue)
            {
                short shortValue = (short)v;
                byte[] byteArray = BitConverter.GetBytes(shortValue);
                Array.Reverse(byteArray);
                return byteArray;
            }

            //v can fit into a long -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807)
            if (v >= long.MinValue && v <= long.MaxValue)
            {
                if (v == 0)
                {
                    return new byte[0];
                }

                byte[] result = v.ToByteArray();

                // if (result[0] == 0x00)
                // {
                //     // Remove leading 0x00 byte if present
                //     byte[] minimalResult = new byte[result.Length - 1];
                //     Array.Copy(result, 1, minimalResult, 0, minimalResult.Length);
                //     return minimalResult;
                // }
                result = result.Reverse().ToArray();
                return result;
            }
            //python equivalent of numbers larger than a long is a bigInteger
            else
            {
                byte[] byteArray = v.ToByteArray();

                if (BitConverter.IsLittleEndian)
                {
                    byteArray = byteArray.Reverse().ToArray();
                }

                while (byteArray.Length > 1 && (byteArray[0] == 0xFF || byteArray[0] == 0x00))
                {
                    byteArray = byteArray.Skip(1).ToArray();
                }
                
                return byteArray;
            }
        }


        public static int LimbsForInt(BigInteger v)
        {
            return IntToBytes(v).Length;
        }
    }
}