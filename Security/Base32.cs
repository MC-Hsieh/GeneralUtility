using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralUtility.Security
{
    public class Base32
    {
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        public static string Encode(byte[] data)
        {
            StringBuilder result = new StringBuilder((int)Math.Ceiling(data.Length * 8.0 / 5.0));

            int buffer = 0;
            int bufferLength = 0;

            foreach (byte b in data)
            {
                buffer <<= 8;
                buffer |= b;
                bufferLength += 8;

                while (bufferLength >= 5)
                {
                    int index = (buffer >> (bufferLength - 5)) & 0x1F;
                    result.Append(Base32Chars[index]);
                    bufferLength -= 5;
                }
            }

            if (bufferLength > 0)
            {
                buffer <<= (5 - bufferLength);
                int index = buffer & 0x1F;
                result.Append(Base32Chars[index]);
            }

            return result.ToString();
        }
        public static byte[] Decode(string base32)
        {
            int buffer = 0;
            int bufferLength = 0;
            byte[] result = new byte[(int)Math.Ceiling(base32.Length * 5.0 / 8.0)];
            int resultIndex = 0;

            foreach (char c in base32)
            {
                int index = Base32Chars.IndexOf(c);
                if (index < 0)
                {
                    throw new ArgumentException("Invalid Base32 character: " + c);
                }

                buffer <<= 5;
                buffer |= index;
                bufferLength += 5;

                if (bufferLength >= 8)
                {
                    result[resultIndex++] = (byte)(buffer >> (bufferLength - 8));
                    bufferLength -= 8;
                }
            }

            return result;
        }
        public static byte[] ToBytes(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("input");
            }

            input = input.TrimEnd('=');
            int num = input.Length * 5 / 8;
            byte[] array = new byte[num];
            byte b = 0;
            byte b2 = 8;
            int num2 = 0;
            string text = input;
            for (int i = 0; i < text.Length; i++)
            {
                int num3 = CharToValue(text[i]);
                if (b2 > 5)
                {
                    int num4 = num3 << b2 - 5;
                    b = (byte)(b | num4);
                    b2 = (byte)(b2 - 5);
                }
                else
                {
                    int num4 = num3 >> 5 - b2;
                    b = (byte)(b | num4);
                    array[num2++] = b;
                    b = (byte)(num3 << 3 + b2);
                    b2 = (byte)(b2 + 3);
                }
            }

            if (num2 != num)
            {
                array[num2] = b;
            }

            return array;
        }
        public static string ToString(byte[] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            int num = (int)Math.Ceiling((double)input.Length / 5.0) * 8;
            char[] array = new char[num];
            byte b = 0;
            byte b2 = 5;
            int num2 = 0;
            foreach (byte b3 in input)
            {
                b = (byte)(b | (b3 >> 8 - b2));
                array[num2++] = ValueToChar(b);
                if (b2 < 4)
                {
                    b = (byte)((uint)(b3 >> 3 - b2) & 0x1Fu);
                    array[num2++] = ValueToChar(b);
                    b2 = (byte)(b2 + 5);
                }

                b2 = (byte)(b2 - 3);
                b = (byte)((uint)(b3 << (int)b2) & 0x1Fu);
            }

            if (num2 != num)
            {
                array[num2++] = ValueToChar(b);
                while (num2 != num)
                {
                    array[num2++] = '=';
                }
            }

            return new string(array);
        }
        private static int CharToValue(char c)
        {
            if (c < '[' && c > '@')
            {
                return c - 65;
            }

            if (c < '8' && c > '1')
            {
                return c - 24;
            }

            if (c < '{' && c > '`')
            {
                return c - 97;
            }

            throw new ArgumentException("Character is not a Base32 character.", "c");
        }
        private static char ValueToChar(byte b)
        {
            if (b < 26)
            {
                return (char)(b + 65);
            }

            if (b < 32)
            {
                return (char)(b + 24);
            }

            throw new ArgumentException("Byte is not a Base32 value.", "b");
        }
    }
}
