using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneralUtility
{
    public class CheckSum
    {
        /// <summary> 高階Checksum運算 </summary>
        /// <param name="data"> 資料來源 </param>
        /// <returns>Checksum: high byte, low byte </returns>
        public static byte[] CRC16(byte[] data)
        {
            if (data.Length == 0)
                throw new Exception("Length Error");
            int xda, xdapoly;
            byte i, j, xdabit;
            xda = 0xFFFF;
            xdapoly = 0xA001;
            for (i = 0; i < data.Length; i++)
            {
                xda ^= data[i];         //XOR 運算子
                for (j = 0; j < 8; j++)
                {
                    xdabit = (byte)(xda & 0x01);
                    xda >>= 1;
                    if (xdabit == 1)
                        xda ^= xdapoly;
                }
            }
            byte[] temdata = new byte[2] { (byte)(xda & 0xFF), (byte)(xda >> 8) };
            return temdata;
        }
    }
}
