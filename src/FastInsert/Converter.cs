using System;

namespace FastInsert
{
    public class Converter
    {
        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new ArgumentException();

            var arr = new byte[hex.Length >> 1];

            for (var i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte) ((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
            }

            return arr;
        }

        private static int GetHexVal(int hex)
        {
            var val = hex;
            return val - (val < 58
                    ? 48
                    : val < 97
                        ? 55
                        : 87
                );
        }
    }
}
