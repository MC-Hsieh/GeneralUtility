using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace GeneralUtility.Security
{
    public class Hash
    {
        public enum EncryptType
        {
            SHA1,
            SHA256,
            SHA384,
            SHA512,
            MD5,
            RIPEMD160,
        }

        /// <summary>HASH加密</summary>
        /// <param name="eType">加密方法</param>
        /// <param name="strData">加密資料</param>
        /// <returns>回傳HEX字串值</returns>
        public static string EncryptToHexString(EncryptType eType, string strData)
        {
            byte[] HashData = Encoding.UTF8.GetBytes(strData);
            return EncryptToHexString(eType, HashData);
        }

        /// <summary>HASH加密</summary>
        /// <param name="eType">加密方法</param>
        /// <param name="FileStream">加密串流</param>
        /// <returns>回傳HEX字串值</returns>
        public static string EncryptToHexString(EncryptType eType, Stream FileStream , string strNote = "")
        {   
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] NoteData = Encoding.UTF8.GetBytes(strNote);
                FileStream.CopyTo(ms);
                byte[] FileBytes = ms.ToArray();
                FileBytes = FileBytes.Concat(NoteData).ToArray();
                return EncryptToHexString(eType, FileBytes);
            }
        }

        /// <summary>HASH加密</summary>
        /// <param name="eType">加密方法</param>
        /// <param name="HashDatas">加密資料</param>
        /// <returns>回傳HEX字串值</returns>
        public static string EncryptToHexString(EncryptType eType, byte[] HashDatas)
        {
            byte[] Result = EncryptToBytes(eType,HashDatas);
            return BitConverter.ToString(Result).Replace("-", string.Empty);
        }

        /// <summary>HASH加密</summary>
        /// <param name="eType">加密方法</param>
        /// <param name="HashDatas">加密資料</param>
        /// <returns>回傳Byte陣列值</returns>
        public static byte[] EncryptToBytes(EncryptType eType, byte[] HashDatas)
        {
            byte[] Result = null;

            switch (eType)
            {
                case EncryptType.SHA1:
                    SHA1Managed SHA1 = new SHA1Managed();
                    Result = SHA1.ComputeHash(HashDatas);
                    break;
                case EncryptType.SHA256:
                    SHA256Managed SHA256 = new SHA256Managed();
                    Result = SHA256.ComputeHash(HashDatas);
                    break;
                case EncryptType.SHA384:
                    SHA384Managed SHA384 = new SHA384Managed();
                    Result = SHA384.ComputeHash(HashDatas);
                    break;
                case EncryptType.SHA512:
                    SHA512Managed SHA512 = new SHA512Managed();
                    Result = SHA512.ComputeHash(HashDatas);
                    break;
                case EncryptType.MD5:
                    MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
                    Result = MD5.ComputeHash(HashDatas);
                    break;
                case EncryptType.RIPEMD160:
                    RIPEMD160Managed RIPEMD160 = new RIPEMD160Managed();
                    Result = RIPEMD160.ComputeHash(HashDatas);
                    break;
                    
                    
            }
            return Result;
        }

        /// <summary>HASH加密</summary>
        /// <param name="eType">加密方法</param>
        /// <param name="HashDatas">加密字串</param>
        /// <returns>回傳</returns>
        public static byte[] EncryptToBytes(EncryptType eType, string strData)
        {
            byte[] HashData = Encoding.UTF8.GetBytes(strData);
            return EncryptToBytes(eType, HashData);
        }

        /// <summary>HASH加密</summary>
        /// <param name="eType">加密方法</param>
        /// <param name="HashDatas">加密資料</param>
        /// <returns>回傳值Base64</returns>
        public static string EncryptToBase64(EncryptType eType, byte[] HashDatas)
        {
            return Convert.ToBase64String(EncryptToBytes(eType, HashDatas));
        }

        /// <summary>HASH加密</summary>
        /// <param name="eType">加密方法</param>
        /// <param name="HashDatas">加密資料</param>
        /// <returns>回傳值Base64</returns>
        public static string EncryptToBase64(EncryptType eType, string HashDatas)
        {
            byte[] HashData = Encoding.UTF8.GetBytes(HashDatas);
            return Convert.ToBase64String(EncryptToBytes(eType, HashData));
        }

        /// <summary>HASH加密</summary>
        /// <param name="eType">加密方法</param>
        /// <param name="HashDatas">加密資料</param>
        /// <returns>回傳值Base64</returns>
        public static string EncryptToBase64(EncryptType eType, Stream FileStream, string strNote = "")
        {
            byte[] HashData = Encoding.UTF8.GetBytes(EncryptToHexString(eType, FileStream, strNote));
            return Convert.ToBase64String(EncryptToBytes(eType, HashData));
        }
    }

}
