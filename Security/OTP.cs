using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GeneralUtility.Security
{
    public class OTP
    {
        public static string GenerateTOTP(string Base32SecretKey, int timeStepSeconds = 30, int digits = 6)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / timeStepSeconds;
            byte[] counterBytes = BitConverter.GetBytes(timestamp);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes); // Ensure big-endian byte order
            }

            byte[] keyBytes = Base32.ToBytes(Base32SecretKey);
            HMACSHA1 hmac = new HMACSHA1(keyBytes);
            byte[] hash = hmac.ComputeHash(counterBytes);

            int offset = hash[hash.Length - 1] & 0x0F;
            int binaryCode = ((hash[offset] & 0x7F) << 24) |
                             ((hash[offset + 1] & 0xFF) << 16) |
                             ((hash[offset + 2] & 0xFF) << 8) |
                             (hash[offset + 3] & 0xFF);

            int totp = binaryCode % (int)Math.Pow(10, digits);

            return totp.ToString().PadLeft(digits, '0');
        }

        public static string GenerateHOTP(string secretKey, long counter, int digits = 6)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            HMACSHA1 hmac = new HMACSHA1(keyBytes);
            byte[] hash = hmac.ComputeHash(counterBytes);

            // Generate the HOTP value
            int offset = hash[hash.Length - 1] & 0x0F;
            int binaryCode = ((hash[offset] & 0x7F) << 24) |
                             ((hash[offset + 1] & 0xFF) << 16) |
                             ((hash[offset + 2] & 0xFF) << 8) |
                             (hash[offset + 3] & 0xFF);

            int hotp = binaryCode % (int)Math.Pow(10, digits);

            return hotp.ToString().PadLeft(digits, '0');
        }
    }
}
