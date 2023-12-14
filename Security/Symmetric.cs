using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace GeneralUtility.Security
{
    public class Symmetric
    {
        public enum Type
        {
            AES,
            DES,
        }
        public enum StringType
        {
            Hex,
            Base64,
        }

        /// <summary>  對稱加密  </summary>  
        /// <param name="eType">加密方式</param>  
        /// <param name="Plaintext">明文</param>  
        /// <param name="strKey">密鑰 </param>  
        /// <param name="strIV">密鑰向量 </param>  
        /// <returns>回傳加密Byte陣列</returns>  
        public static byte[] Encrypt(Type eType, byte[] Plaintext, string strKey, string strIV)
        {
            if (Plaintext == null) return null;
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, GetEncryptor(eType, strKey, strIV), CryptoStreamMode.Write))
            {
                cs.Write(Plaintext, 0, Plaintext.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }

        /// <summary>  對稱加密  </summary>
        /// <param name="eType">加密方式</param>
        /// <param name="Plaintext">明文</param>
        /// <param name="strKey">密鑰</param>
        /// <param name="strIV">密鑰向量</param>
        /// <param name="eStringType">輸出格式</param>
        /// <returns>回傳字串</returns>
        public static string Encrypt2String(Type eType, byte[] Plaintext, string strKey, string strIV, StringType eStringType)
        {
            switch (eStringType)
            {
                case StringType.Hex:
                    byte[] Result = Encrypt(eType, Plaintext, strKey, strIV);
                    return BitConverter.ToString(Result).Replace("-", string.Empty);
                case StringType.Base64:
                    return Convert.ToBase64String(Encrypt(eType, Plaintext, strKey, strIV));
            }
            return null;
        }

        /// <summary>  對稱加密  </summary>
        /// <param name="eType">加密方式</param>
        /// <param name="Plaintext">明文</param>
        /// <param name="strKey">密鑰</param>
        /// <param name="strIV">密鑰向量</param>
        /// <param name="eStringType">輸出格式</param>
        /// <returns>回傳字串</returns>
        public static string Encrypt2String(Type eType, string strPlaintext, string strKey, string strIV, StringType eStringType)
        {
            byte[] Plaintext = Encoding.UTF8.GetBytes(strPlaintext);
            switch (eStringType)
            {
                case StringType.Hex:
                    byte[] Result = Encrypt(eType, Plaintext, strKey, strIV);
                    return BitConverter.ToString(Result).Replace("-", string.Empty);
                case StringType.Base64:
                    return Convert.ToBase64String(Encrypt(eType, Plaintext, strKey, strIV));
            }
            return null;
        }

        /// <summary>  對稱檔案加密  </summary>
        /// <param name="eType">加密方式</param>
        /// <param name="SourcePath">未加密檔案位置</param>
        /// <param name="EncryptPath">加密檔案位置</param>
        /// <param name="strKey">密鑰</param>
        /// <param name="strIV">密鑰向量</param>
        /// <param name="eStringType">輸出格式</param>
        /// <returns>回傳是否成功</returns>
        public static bool EncryptFile(string SourcePath, string EncryptPath, Type eType, string strKey, string strIV)
        {
            try
            {
                using (FileStream sourceStream = new FileStream(SourcePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] dataByteArray = new byte[sourceStream.Length];
                    sourceStream.Read(dataByteArray, 0, dataByteArray.Length);
                    sourceStream.Close();

                    using (FileStream encryptStream = new FileStream(EncryptPath, FileMode.Create, FileAccess.Write))
                    {
                        using (CryptoStream cs = new CryptoStream(encryptStream, GetEncryptor(eType, strKey, strIV), CryptoStreamMode.Write))
                        {
                            cs.Write(dataByteArray, 0, dataByteArray.Length);
                            cs.FlushFinalBlock();
                        }
                        
                        encryptStream.Close();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>  對稱檔案加密  </summary>
        /// <param name="eType">加密方式</param>
        /// <param name="SourcePath">未加密檔案位置</param>
        /// <param name="EncryptPath">加密檔案位置</param>
        /// <param name="strKey">密鑰</param>
        /// <param name="strIV">密鑰向量</param>
        /// <param name="eStringType">輸出格式</param>
        /// <returns>回傳是否成功</returns>
        public static bool EncryptFile(string EncryptPath, string[] Plaintexts, Type eType, string strKey, string strIV)
        {
            try
            {
                StringBuilder clsStringBuilder = new StringBuilder();
                foreach (string strPerLine in Plaintexts)
                {
                    clsStringBuilder.AppendLine(strPerLine);
                }
                string strSave = clsStringBuilder.ToString();
                byte[] bPlainText = Encoding.UTF8.GetBytes(strSave);

                using (FileStream encryptStream = new FileStream(EncryptPath, FileMode.Create, FileAccess.Write))
                {
                    using (CryptoStream cs = new CryptoStream(encryptStream, GetEncryptor(eType, strKey, strIV), CryptoStreamMode.Write))
                    {
                        cs.Write(bPlainText, 0, bPlainText.Length);
                        cs.FlushFinalBlock();
                    }

                    encryptStream.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>  對稱解密  </summary>  
        /// <param name="eType">加密方式</param>  
        /// <param name="Cyphertext">密文</param>  
        /// <param name="strKey">密鑰 </param>  
        /// <param name="strIV">密鑰向量 </param>  
        /// <returns>回傳解密Byte陣列</returns>  
        public static byte[] Decrypt(Type eType, byte[] Cyphertext, string strKey, string strIV)
        {
            if (Cyphertext == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, GetDecryptor(eType, strKey, strIV), CryptoStreamMode.Write))
                {
                    cs.Write(Cyphertext, 0, Cyphertext.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>  對稱解密  </summary>
        /// <param name="eType">加密方式</param>
        /// <param name="strCyphertext">密文</param>
        /// <param name="strKey">密鑰</param>
        /// <param name="strIV">密鑰向量</param>
        /// <param name="eStringType">字串格式</param>
        /// <returns>輸出解密字串</returns>
        public static string Decrypt2String(Type eType, string strCyphertext, string strKey, string strIV, StringType eStringType)
        {
            byte[] Cyphertext = null;
            switch (eStringType)
            {
                case StringType.Hex:
                    Cyphertext = Hex2Bytes(strCyphertext);
                    break;
                case StringType.Base64:
                    Cyphertext = Convert.FromBase64String(strCyphertext);
                    break;
            }
            byte[] Result = Decrypt(eType, Cyphertext, strKey, strIV);
            return Encoding.UTF8.GetString(Result);
        }

        /// <summary>  對稱解密  </summary>
        /// <param name="eType">加密方式</param>
        /// <param name="strCyphertext">密文</param>
        /// <param name="strKey">密鑰</param>
        /// <param name="strIV">密鑰向量</param>
        /// <param name="eStringType">字串格式</param>
        /// <returns>輸出解密陣列</returns>
        public static byte[] Decrypt2StringB(Type eType, string strCyphertext, string strKey, string strIV, StringType eStringType)
        {
            byte[] Cyphertext = null;
            switch (eStringType)
            {
                case StringType.Hex:
                    Cyphertext = Hex2Bytes(strCyphertext);
                    break;
                case StringType.Base64:
                    Cyphertext = Convert.FromBase64String(strCyphertext);
                    break;
            }
            byte[] Result = Decrypt(eType, Cyphertext, strKey, strIV);
            return Result;
        }

        /// <summary>  對稱檔案解密  </summary>
        /// <param name="eType">加密方式</param>
        /// <param name="SourcePath">加密檔案位置</param>
        /// <param name="EncryptPath">解密檔案位置</param>
        /// <param name="strKey">密鑰</param>
        /// <param name="strIV">密鑰向量</param>
        /// <param name="eStringType">輸出格式</param>
        /// <returns>回傳是否成功</returns>
        public static bool DecryptFile(string EncryptPath, string DecryptPath, Type eType, string strKey, string strIV)
        {
            try
            {
                using (FileStream EncryptStream = new FileStream(EncryptPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] dataByteArray = new byte[EncryptStream.Length];
                    EncryptStream.Read(dataByteArray, 0, dataByteArray.Length);
                    EncryptStream.Close();

                    using (FileStream DecryptStream = new FileStream(DecryptPath, FileMode.Create, FileAccess.Write))
                    {
                        using (CryptoStream cs = new CryptoStream(DecryptStream, GetDecryptor(eType, strKey, strIV), CryptoStreamMode.Write))
                        {
                            cs.Write(dataByteArray, 0, dataByteArray.Length);
                            cs.FlushFinalBlock();
                        }
                        
                        DecryptStream.Close();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>  對稱檔案解密  </summary>
        /// <param name="eType">加密方式</param>
        /// <param name="SourcePath">加密檔案位置</param>
        /// <param name="EncryptPath">解密檔案位置</param>
        /// <param name="strKey">密鑰</param>
        /// <param name="strIV">密鑰向量</param>
        /// <param name="eStringType">輸出格式</param>
        /// <returns>回傳是否成功</returns>
        public static string[] DecryptFile(string EncryptPath, Type eType, string strKey, string strIV)
        {
            try
            {
                using (FileStream EncryptStream = new FileStream(EncryptPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] dataByteArray = new byte[EncryptStream.Length];
                    EncryptStream.Read(dataByteArray, 0, dataByteArray.Length);
                    EncryptStream.Close();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, GetDecryptor(eType, strKey, strIV), CryptoStreamMode.Write))
                        {
                            cs.Write(dataByteArray, 0, dataByteArray.Length);
                            cs.FlushFinalBlock();
                        }
                        char[] ClearLine = new char[]{'\r', '\n'};
                        string[] strData = Encoding.UTF8.GetString(ms.ToArray()).Split(ClearLine,StringSplitOptions.RemoveEmptyEntries);
                        ms.Close();
                        return strData;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static ICryptoTransform GetDecryptor(Type eType, string strKey, string strIV)
        {
            byte[] Keys;
            byte[] IVs;
            byte[] key;
            byte[] iv;

            switch (eType)
            {
                case Type.AES:
                    Keys = new byte[32];              //Key最大長度
                    IVs = new byte[16];               //IV最大長度
                    AesCryptoServiceProvider Aes = new AesCryptoServiceProvider();
                    key = Hash.EncryptToBytes(Hash.EncryptType.SHA256, strKey);
                    iv = Hash.EncryptToBytes(Hash.EncryptType.SHA256, strIV);
                    //長度修正
                    for (int i = 0; i < 16; i++)
                    {
                        int index = (i * 2);
                        IVs[i] = iv[index];
                    }
                    Aes.Key = key;
                    Aes.IV = IVs;
                    return Aes.CreateDecryptor();
                case Type.DES:
                    Keys = new byte[8];              //Key最大長度
                    IVs = new byte[8];               //IV最大長度
                    DESCryptoServiceProvider Des = new DESCryptoServiceProvider();
                    key = Hash.EncryptToBytes(Hash.EncryptType.SHA256, strKey);
                    iv = Hash.EncryptToBytes(Hash.EncryptType.SHA256, strIV);
                    //長度修正
                    for (int i = 0; i < 8; i++)
                    {
                        int index = (i * 4);
                        Keys[i] = key[index];
                        IVs[i] = iv[index];
                    }
                    Des.Key = Keys;
                    Des.IV = IVs;
                    return Des.CreateDecryptor();
            }
            return null;
        }

        private static ICryptoTransform GetEncryptor(Type eType, string strKey, string strIV)
        {
            byte[] Keys;
            byte[] IVs;
            byte[] key;
            byte[] iv;

            switch (eType)
            {
                case Type.AES:
                    Keys = new byte[32];              //Key最大長度
                    IVs = new byte[16];               //IV最大長度
                    AesCryptoServiceProvider Aes = new AesCryptoServiceProvider();
                    key = Hash.EncryptToBytes(Hash.EncryptType.SHA256, strKey);
                    iv = Hash.EncryptToBytes(Hash.EncryptType.SHA256, strIV);
                    //長度修正
                    for (int i = 0; i < 16; i++)
                    {
                        int index = (i * 2);
                        IVs[i] = iv[index];
                    }
                    Aes.Key = key;
                    Aes.IV = IVs;
                    return Aes.CreateEncryptor();
                case Type.DES:
                    Keys = new byte[8];              //Key最大長度
                    IVs = new byte[8];               //IV最大長度
                    DESCryptoServiceProvider Des = new DESCryptoServiceProvider();
                    key = Hash.EncryptToBytes(Hash.EncryptType.SHA256, strKey);
                    iv = Hash.EncryptToBytes(Hash.EncryptType.SHA256, strIV);
                    //長度修正
                    for (int i = 0; i < 8; i++)
                    {
                        int index = (i * 4);
                        Keys[i] = key[index];
                        IVs[i] = iv[index];
                    }
                    Des.Key = Keys;
                    Des.IV = IVs;
                    return Des.CreateEncryptor();
            }
            return null;
        }

        private static byte[] Hex2Bytes(string strHex)
        {
            return Enumerable.Range(0, strHex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(strHex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static void aaaa()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);
        }
    }
}
