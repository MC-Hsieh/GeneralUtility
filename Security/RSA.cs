using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace GeneralUtility.Security
{
    public class RSA
    {
        public enum StringType
        {
            Hex,
            Base64,
        }

        public static RSAKey CreatRSAKey()
        {
            RSAKey clsReturnRSAKey = new RSAKey();

            using (RSACryptoServiceProvider clsRSA = new RSACryptoServiceProvider(4096))
            {
                // 获取公钥和私钥
                clsReturnRSAKey.strPublicKey = clsRSA.ToXmlString(false); // 获取公钥
                clsReturnRSAKey.strPrivateKey = clsRSA.ToXmlString(true); // 获取私钥
            }
            return clsReturnRSAKey;
        }

        public static byte[] EncryptData(byte[] bData, string strKey, int encryptionBufferSize = 501, int decryptionBufferSize = 512)
        {
            RSACryptoServiceProvider clsRSA = new RSACryptoServiceProvider(4096);
            clsRSA.FromXmlString(strKey);
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[encryptionBufferSize];
                int pos = 0;
                int copyLength = buffer.Length;
                while (true)
                {
                    if (pos + copyLength > bData.Length)
                    {
                        copyLength = bData.Length - pos;
                    }
                    buffer = new byte[copyLength];
                    Array.Copy(bData, pos, buffer, 0, copyLength);
                    pos += copyLength;
                    ms.Write(clsRSA.Encrypt(buffer, false), 0, decryptionBufferSize);
                    Array.Clear(buffer, 0, copyLength);
                    if (pos >= bData.Length)
                    {
                        break;
                    }
                }

                return ms.ToArray();
            }
        }
        public static byte[] DecryptData(byte[] bData, string strKey, int decryptionBufferSize = 512)
        {
            RSACryptoServiceProvider clsRSA = new RSACryptoServiceProvider(4096);
            clsRSA.FromXmlString(strKey);
            using (var ms = new MemoryStream(bData.Length))
            {
                byte[] buffer = new byte[decryptionBufferSize];
                int pos = 0;
                int copyLength = buffer.Length;
                while (true)
                {
                    Array.Copy(bData, pos, buffer, 0, copyLength);
                    pos += copyLength;
                    byte[] resp = clsRSA.Decrypt(buffer, false);
                    ms.Write(resp, 0, resp.Length);
                    Array.Clear(resp, 0, resp.Length);
                    Array.Clear(buffer, 0, copyLength);
                    if (pos >= bData.Length)
                    {
                        break;
                    }
                }
                return ms.ToArray();
            }
        }

        public static string EncryptData2String(byte[] bData, string strKey, StringType eType = StringType.Hex)
        {
            switch (eType)
            {
                case StringType.Hex:
                    byte[] Result = EncryptData(bData, strKey);
                    return BitConverter.ToString(Result).Replace("-", string.Empty);
                case StringType.Base64:
                    return Convert.ToBase64String(EncryptData(bData, strKey));
            }
            return null;
        }

        public static byte[] DecryptData2String(string strCyphertext, string strKey, StringType eType = StringType.Hex)
        {
            byte[] Cyphertext = null;
            switch (eType)
            {
                case StringType.Hex:
                    Cyphertext = Hex2Bytes(strCyphertext);
                    break;
                case StringType.Base64:
                    Cyphertext = Convert.FromBase64String(strCyphertext);
                    break;
            }
            byte[] Result = DecryptData(Cyphertext, strKey);
            return Result;
        }


        private static byte[] Hex2Bytes(string strHex)
        {
            return Enumerable.Range(0, strHex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(strHex.Substring(x, 2), 16))
                             .ToArray();
        }
    }


    public class RSAKey
    {
        public string strPublicKey { get; set; }
        public string strPrivateKey { get; set; }
    }
}
