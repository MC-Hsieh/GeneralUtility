using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace GeneralUtility.IO
{
    /// <summary>讀寫檔案相關工具。</summary>
    public static class XFile
    {

        #region " Definition "

        /// <summary>檔案編碼格式。</summary>
        public enum Encode
        {
            /// <summary>編碼格式 UTF7 。</summary>
            UTF7,
            /// <summary>編碼格式 UTF8 。</summary>
            UTF8,
            /// <summary>編碼格式 UTF32 。</summary>
            UTF32,
            /// <summary>編碼格式 Unicode 。</summary>
            Unicode,
            /// <summary>預設。</summary>
            Default,
            /// <summary>編碼格式 ASCII 。</summary>
            ASCII,
            /// <summary>編碼格式 BigEndianUnicode 。</summary>
            BigEndianUnicode,
        }

        /// <summary>刪除檔案日期類型。</summary>
        public enum DeleteDateType
        {
            /// <summary>當前時間日期之前。</summary>
            Before,
            /// <summary>當前時間日期之後。</summary>
            After
        }

        /// <summary>資料夾時間型態。</summary>
        public enum FolderTimeType
        {
            /// <summary>資料夾建立日期。</summary>
            CreateTime,
            /// <summary>資料夾最後存取日期。</summary>
            LastAccessTime,
            /// <summary>資料夾最後寫入日期。</summary>
            LastWriteTime,
        }

        /// <summary>檔案大小單位。</summary>
        public enum FileSizeUnitType
        {
            /// <summary>Byte。</summary>
            Byte,
            /// <summary>KB。</summary>
            KB,
            /// <summary>MB。</summary>
            MB,
            /// <summary>GB。</summary>
            GB,
        }

        #endregion

        #region " Properties "

        /// <summary>取得，硬碟鎖定物件。</summary>
        public static object DiskLock
        {
            get { return XFile.g_objDiskLock; }
        }
        private static object g_objDiskLock = new object();

        #endregion

        #region " Methods - Write/Read/Move/Create/Delete with File/Folder/Serialize"

        /// <summary>
        /// 移除非法檔名字元，請勿使用含路徑檔名。
        /// </summary>
        /// <param name="strFileName">檔名。</param>
        /// <returns>合法檔名字串。</returns>
        /// <remarks>請勿使用含路徑檔名。</remarks>
        public static string RemoveInvalidFileNameChar(string strFileName)
        {
            string strLegalPath = strFileName;
            char[] cIllegalChars=Path.GetInvalidFileNameChars();
            foreach (var cIllegalChar in cIllegalChars)
            {
                strLegalPath = strLegalPath.Replace(string.Format("{0}",cIllegalChar), "");
            }
            return strLegalPath;
        }

        /// <summary>
        /// 取得隨機名稱檔名。
        /// </summary>
        /// <returns>隨機檔名。</returns>
        public static string GetRandomFileName()
        {
            return Path.GetRandomFileName();
        }


        public static bool CheckCreateFolder(string strFolder)
        {
            bool bResult = true;
            try
            {
                string[] strPaths = strFolder.Split(new char[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);
                string strPath = strPaths[0];

                if (!IsExist_Folder(strPath)) CreateFolder(strPath);
                for (int i = 1; i < strPaths.Length; i++)
                {
                    strPath += "\\" + strPaths[i];
                    if (!IsExist_Folder(strPath)) CreateFolder(strPath);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// 建立一資料夾。
        /// </summary>
        /// <returns>回傳目前指定的資料夾底下有多少個資料夾</returns>
        public static bool CreateFolder(string strFolder)
        {
            bool bResult = true;

            try
            {
                // 確保資料夾是否存在
                if (!Directory.Exists(strFolder))
                {
                    Directory.CreateDirectory(strFolder);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// 建立一檔案。
        /// </summary>
        /// <returns></returns>
        public static bool CreateFile(string strFile)
        {
            bool bResult = true;

            try
            {
                // 確保資料夾是否存在
                if (!File.Exists(strFile))
                {
                    File.Create(strFile).Close();
                }
            }
            catch (Exception ex)
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// 進行資料存檔，當該路徑原本就有資料時，此資料群將會累加到原本文件中，並且有換行的動作。
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名</param>
        /// <param name="strFileData">加入此資料，並且加入後會進行換行</param>
        public static void AppendToFile(string strPath, List<string> strFileData)
        {
            AppendToFile(strPath, strFileData.ToArray());
        }

        /// <summary>
        /// 進行資料存檔，當該路徑原本就有資料時，此資料群將會累加到原本文件中，並且有換行的動作。
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名</param>
        /// <param name="strFileData">加入此資料，並且加入後會進行換行</param>
        public static void AppendToFile(string strPath, string[] strFileData)
        {
            if (strFileData != null)
            {
                lock (g_objDiskLock)
                {
                    try
                    {
                        StringBuilder clsStringBuilder = new StringBuilder();
                        foreach (string strPerLine in strFileData)
                        {
                            clsStringBuilder.AppendLine(strPerLine);
                        }
                        using (StreamWriter clsStreamWriter = new StreamWriter(strPath, true, Encoding.Default))
                        {
                            clsStreamWriter.Write(clsStringBuilder.ToString());
                            clsStreamWriter.Close();
                        }
                        clsStringBuilder.Clear();
                        clsStringBuilder = null;
                    }
                    catch (Exception ex)
                    {
                        XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                    }
                }
            }
        }

        /// <summary>
        /// 進行資料存檔，當該路徑原本就有資料時，此資料群將會累加到原本文件中，如要換行請將 bNewLine = true。
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名</param>
        /// <param name="strFileData">加入此資料</param>
        /// <param name="bNewLine">是否換行，預設不換行。</param>
        public static void AppendToFile(string strPath, string strFileData, bool bNewLine = false)
        {
            if (strFileData != null)
            {
                lock (g_objDiskLock)
                {
                    try
                    {
                        StringBuilder clsStringBuilder = new StringBuilder();
                        strFileData = bNewLine ? (strFileData + Environment.NewLine) : strFileData;
                        clsStringBuilder.Append(strFileData);
                        using (StreamWriter clsStreamWriter = new StreamWriter(strPath, true, Encoding.Default))
                        {
                            clsStreamWriter.Write(clsStringBuilder.ToString());
                            clsStreamWriter.Close();
                        }
                        clsStringBuilder.Clear();
                        clsStringBuilder = null;
                    }
                    catch (Exception ex)
                    {
                        XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                    }
                }
            }
        }

        /// <summary>
        /// 單純進行資料存檔。
        /// <para><b>注意！如果該路徑原本就存在，將會蓋掉原本資料!!</b></para>
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名。</param>
        /// <param name="strFileDatas">資料內容。</param>
        /// <param name="eEncode">編碼格式，預設為 Encode.Default 。</param>
        public static void WriteFile(string strPath, string strFileDatas, Encode eEncode = Encode.Default)
        {
            WriteFile(strPath, new string[] { strFileDatas }, eEncode);
        }

        /// <summary>
        /// 單純進行資料存檔。
        /// <para><b>注意！如果該路徑原本就存在，將會蓋掉原本資料!!</b></para>
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名。</param>
        /// <param name="strFileDatas">資料內容。</param>
        /// <param name="eEncode">編碼格式，預設為 Encode.Default 。</param>
        public static void WriteFile(string strPath, List<string> strFileDatas, Encode eEncode = Encode.Default)
        {
            WriteFile(strPath, strFileDatas.ToArray(), eEncode);
        }

        /// <summary>
        /// 單純進行資料存檔。
        /// <para><b>注意！如果該路徑原本就存在，將會蓋掉原本資料!!</b></para>
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名。</param>
        /// <param name="strFileDatas">資料內容。</param>
        /// <param name="eEncode">編碼格式，預設為 Encode.Default 。</param>
        public static void WriteFile(string strPath, string[] strFileDatas, Encode eEncode = Encode.Default)
        {
            if (strFileDatas != null)
            {
                Encoding eEncodeing;
                switch (eEncode)
                {
                    case Encode.ASCII:
                        eEncodeing = Encoding.ASCII;
                        break;
                    case Encode.BigEndianUnicode:
                        eEncodeing = Encoding.BigEndianUnicode;
                        break;
                    case Encode.Default:
                        eEncodeing = Encoding.Default;
                        break;
                    case Encode.Unicode:
                        eEncodeing = Encoding.Unicode;
                        break;
                    case Encode.UTF32:
                        eEncodeing = Encoding.UTF32;
                        break;
                    case Encode.UTF7:
                        eEncodeing = Encoding.UTF7;
                        break;
                    case Encode.UTF8:
                        eEncodeing = Encoding.UTF8;
                        break;
                    default:
                        eEncodeing = Encoding.UTF8;
                        break;
                }

                lock (g_objDiskLock)
                {
                    try
                    {
                        StringBuilder clsStringBuilder = new StringBuilder();
                        foreach (string strPerLine in strFileDatas)
                        {
                            clsStringBuilder.AppendLine(strPerLine);
                        }
                        using (StreamWriter clsStreamWriter = new StreamWriter(strPath, false, eEncodeing))
                        {
                            string a = clsStringBuilder.ToString();
                            byte[] bb = Encoding.UTF8.GetBytes(a);
                            char[] cc = Encoding.UTF8.GetChars(bb);
                            clsStreamWriter.Write(cc);
                            //clsStreamWriter.Write(clsStringBuilder.ToString());
                            clsStreamWriter.Close();
                        }
                        clsStringBuilder.Clear();
                        clsStringBuilder = null;

                        // 之前的做法如下，有時候會突然把檔案給咬住，即使程式關閉了也會無法釋放掉，所以換成上面的寫法。
                        // 此段註解務必留下。
                        //File.WriteAllLines(strPath, strFileDatas, e);
                    }
                    catch (Exception ex)
                    {
                        XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                    }
                }
            }
        }

        /// <summary>
        /// 將物件寫成Binary序列化到指定位置。
        /// </summary>
        /// <param name="strPath">>文件路徑。</param>
        /// <param name="clsXmlObject">欲寫入物件。</param>
        /// <returns>錯誤碼。</returns>
        /// <remarks>
        /// <para>使用Binary序列化，物件屬性基本使用基本資料型態。</para>
        /// <para>需序列化物件要設定 [Serializable] 屬性，否則會報錯。</para>
        /// <para>大部分都可使用使用Binary序列化，唯獨屬性也可以。</para>
        /// </remarks>
        public static bool WriteBinarySerialize(string strPath, object clsXmlObject)
        {
            bool bResult = true;
            try
            {
                using (FileStream clsFileStream = new FileStream(strPath, FileMode.Create))
                {
                    BinaryFormatter clsBinaryFormatter = new BinaryFormatter();
                    clsBinaryFormatter.Serialize(clsFileStream, clsXmlObject);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex) + "檔案寫入失敗。");
            }
            return bResult;
        }

        /// <summary>
        /// 讀取Binary序列化文件轉換成物件。
        /// </summary>
        /// <typeparam name="T">物件型態(類別)。</typeparam>
        /// <param name="strFilePath">文件路徑。</param>
        /// <param name="clsObject">輸出物件。</param>
        /// <returns>錯誤碼。</returns>
        public static bool ReadBinarySerialize<T>(string strFilePath, ref T clsObject)
        {
            bool bResult = true;

            T clsReadObjet;
            try
            {
                if (XFile.IsExist_File(@strFilePath))
                {
                    using (FileStream clsFileStream = new FileStream(strFilePath, FileMode.Open))
                    {
                        BinaryFormatter clsBinaryFormatter = new BinaryFormatter();
                        clsReadObjet = (T)clsBinaryFormatter.Deserialize(clsFileStream);
                    }
                    if (clsReadObjet != null)
                        clsObject = clsReadObjet;
                }
                else
                {
                    bResult = false;
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex) + "檔案讀取失敗。");
            }
            return bResult;
        }

        /// <summary>
        /// 將物件寫成Xml序列化到指定位置。
        /// </summary>
        /// <typeparam name="T">物件型態(類別)。</typeparam>
        /// <param name="strPath">文件路徑。</param>
        /// <param name="clsXmlObject">欲寫入物件。</param>
        /// <returns>錯誤碼。</returns>
        /// <remarks>
        /// <para>使用Xml序列化，物件屬性基本使用基本資料型態。</para>
        /// <para>目前測物件有 List、static、const、方法 都可以使用Xml序列化</para>
        /// <para>若屬性用到其他類別，須確認是否都有實作物件內所有屬性的 "空"建構子</para>
        /// <para>未實作 "空"建構子虛實作會導致Xml序列化錯誤。</para>
        /// <para>且唯獨屬性將不可XML序列化需使用Binary序列化。</para>
        /// </remarks>
        public static bool WriteXmlSerialize<T>(string strPath, T clsXmlObject)
        {
            bool bResult = true;

            XmlWriterSettings clsXmlSettings = null;
            try
            {
                XmlSerializer clsXmlSerializer = new XmlSerializer(typeof(T));
                clsXmlSettings = new XmlWriterSettings();
                clsXmlSettings.NewLineHandling = NewLineHandling.None;
                clsXmlSettings.Indent = true;
                clsXmlSettings.Encoding = Encoding.UTF8;

                using (MemoryStream stream = new MemoryStream())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stream, clsXmlSettings))
                    {
                        clsXmlSerializer.Serialize(xmlWriter, clsXmlObject);
                        xmlWriter.Close();
                        byte[] bStreamBuffer = stream.GetBuffer();
                        int iLength = Array.IndexOf(bStreamBuffer,(byte)0);
                        stream.Close();
                        byte[] bHead = Encoding.UTF8.GetPreamble();
                        bool bCompare = true;
                        if (bStreamBuffer.Length >= bHead.Length)
                        {
                            for (int iCount = 0; iCount < bHead.Length; iCount++)
                            {
                                if (bHead[iCount] != bStreamBuffer[iCount])
                                {
                                    bCompare = false;
                                    break;
                                }
                            }
                            if (bCompare)
                            {
                                if (iLength == -1) iLength = bStreamBuffer.Length; 
                                using (FileStream fileStream = new FileStream(strPath, FileMode.Create))
                                {
                                    fileStream.Write(bStreamBuffer, bHead.Length, iLength - bHead.Length);
                                    fileStream.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex) + "檔案寫入失敗。");
            }
            return bResult;
        }

        /// <summary>
        /// 將物件寫成Xml序列化將物件寫成Xml序列化並轉成ByteArray
        /// </summary>
        /// <typeparam name="T">物件型態(類別)。</typeparam>
        /// <param name="strPath">文件路徑。</param>
        /// <param name="clsXmlObject">欲寫入物件。</param>
        /// <returns>錯誤碼。</returns>
        /// <remarks>
        /// <para>使用Xml序列化，物件屬性基本使用基本資料型態。</para>
        /// <para>目前測物件有 List、static、const、方法 都可以使用Xml序列化</para>
        /// <para>若屬性用到其他類別，須確認是否都有實作物件內所有屬性的 "空"建構子</para>
        /// <para>未實作 "空"建構子虛實作會導致Xml序列化錯誤。</para>
        /// <para>且唯獨屬性將不可XML序列化需使用Binary序列化。</para>
        /// </remarks>
        public static byte[] WriteXmlSerializeToArray<T>(T clsXmlObject)
        {
            XmlWriterSettings clsXmlSettings = null;
            byte[] bStreamBuffer;
            try
            {                
                XmlSerializer clsXmlSerializer = new XmlSerializer(typeof(T));
                clsXmlSettings = new XmlWriterSettings();
                clsXmlSettings.NewLineHandling = NewLineHandling.None;
                clsXmlSettings.Indent = true;
                clsXmlSettings.Encoding = Encoding.UTF8;

                using (MemoryStream stream = new MemoryStream())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stream, clsXmlSettings))
                    {
                        clsXmlSerializer.Serialize(xmlWriter, clsXmlObject);
                        xmlWriter.Close();
                        bStreamBuffer = stream.GetBuffer();
                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex) + "檔案寫入失敗。");
                return null;
            }
            return bStreamBuffer;
        }

        /// <summary>
        /// 讀取Xml序列化文件轉換成物件。
        /// </summary>
        /// <typeparam name="T">物件型態(類別)。</typeparam>
        /// <param name="strFilePath">文件路徑。</param>
        /// <param name="clsObject">輸出物件。</param>
        /// <returns>錯誤碼。</returns>
        /// <remarks>
        /// <para>使用Xml序列化，物件屬性基本使用基本資料型態。</para>
        /// <para>目前測物件有 List、static、const、方法 都可以使用Xml序列化</para>
        /// <para>若屬性用到其他類別，須確認是否都有實作物件內所有屬性的 "空"建構子</para>
        /// <para>未實作 "空"建構子虛實作會導致Xml序列化錯誤。</para>
        /// </remarks>
        public static bool ReadXmlSerialize<T>(string strFilePath, ref T clsObject)
        {
            bool bResult = true;
            T clsReadObjet;
            try
            {
				if (XFile.IsExist_File(@strFilePath))
				{
					XmlSerializer clsXmlSerializer = new XmlSerializer(typeof(T));
					using (XmlReader clsXmlReader = XmlReader.Create(@strFilePath))
					{
						clsReadObjet = (T)clsXmlSerializer.Deserialize(clsXmlReader);
					}
					if (clsReadObjet != null)
						clsObject = clsReadObjet;
				}
				else
				{
                    bResult = false;
				}
            }
            catch (Exception ex)
            {
                bResult = false;;
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex) + "檔案讀取失敗。");
            }
            return bResult;
        }

        /// <summary>
        /// 讀取Xml序列化ByteArray轉換成物件。
        /// </summary>
        /// <typeparam name="T">物件型態(類別)。</typeparam>
        /// <param name="strFilePath">文件路徑。</param>
        /// <param name="clsObject">輸出物件。</param>
        /// <returns>錯誤碼。</returns>
        /// <remarks>
        /// <para>使用Xml序列化，物件屬性基本使用基本資料型態。</para>
        /// <para>目前測物件有 List、static、const、方法 都可以使用Xml序列化</para>
        /// <para>若屬性用到其他類別，須確認是否都有實作物件內所有屬性的 "空"建構子</para>
        /// <para>未實作 "空"建構子虛實作會導致Xml序列化錯誤。</para>
        /// </remarks>
        public static bool ReadXmlSerialize<T>(byte[] bStreamBuffer, ref T clsObject)
        {
            bool bResult = true;
            T clsReadObjet;
            try
            {
                if (bStreamBuffer != null)
                {
                    MemoryStream stream = new MemoryStream(bStreamBuffer); 
                    XmlSerializer clsXmlSerializer = new XmlSerializer(typeof(T));
                    using (XmlReader clsXmlReader = XmlReader.Create(stream))
                    {
                        clsReadObjet = (T)clsXmlSerializer.Deserialize(clsXmlReader);
                    }
                    if (clsReadObjet != null)
                        clsObject = clsReadObjet;
                }
                else
                {
                    bResult = false;
                }
            }
            catch (Exception ex)
            {
                bResult = false; ;
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex) + "檔案讀取失敗。");
            }
            return bResult;
        }

        /// <summary>
        /// 將物件寫成序列化格式並壓縮檔案到指定位置，適合大型資料或影像資料。
        /// </summary>
        /// <typeparam name="T">物件型態(類別)。</typeparam>
        /// <param name="strPath">文件路徑。</param>
        /// <param name="clsXmlObject">欲寫入物件。</param>
        /// <returns>錯誤碼。</returns>
        public static bool WriteZipSerializer<T>(string strPath, T clsObject)
        {
            bool bResult = true;
            try
            {
                DataContractSerializer clsDataContractSerializer = new DataContractSerializer(clsObject.GetType());

                using (FileStream clsFileStream = new FileStream(strPath, FileMode.Create))
                {
                    using (GZipStream clsZip = new GZipStream(clsFileStream, CompressionMode.Compress))
                    {
                        clsDataContractSerializer.WriteObject(clsZip, clsObject);
                    }
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex) + "檔案寫入失敗。");
            }
            return bResult;
        }

        /// <summary>
        /// 讀取ZipSerializer序列化文件轉換成物件。
        /// </summary>
        /// <typeparam name="T">物件型態(類別)。</typeparam>
        /// <param name="strFilePath">文件路徑。</param>
        /// <param name="clsObject">輸出物件。</param>
        /// <returns>錯誤碼。</returns>
        public static bool ReadZipSerializer<T>(string strFilePath, ref T clsObject)
        {
            bool bResult = true;
            T clsReadObjet;
            try
            {
                if (XFile.IsExist_File(@strFilePath))
                {
                    DataContractSerializer clsDataContractSerializer = new DataContractSerializer(clsObject.GetType());
                    using (FileStream clsFileStream = new FileStream(strFilePath, FileMode.Open))
                    {
                        using (GZipStream clsZip = new GZipStream(clsFileStream, CompressionMode.Decompress))
                        {
                            clsReadObjet = (T)clsDataContractSerializer.ReadObject(clsZip);
                        }
                    }
                    if (clsReadObjet != null)
                        clsObject = clsReadObjet;
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex) + "檔案讀取失敗。");
            }
            return bResult;
        }

        /// <summary>
        /// 直接讀取資料檔案。再以陣列方式回傳檔案內容。<para></para>
        /// 更換原先使用 File.ReadAllLine() 的寫法，<para></para>
        /// 由於 ADJ 專案遇到神奇的檔案被鎖住的事件，造成無法對檔案的內容讀取，<para></para>
        /// 故改以此非同步串流讀取的方式讀取檔案，能夠越過檔案被鎖住的狀況。<para></para>
        /// 2014.04.15 by Alex 。<para></para>
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名。</param>
        /// <returns>回傳檔案內容，以String[]的方式承接。</returns>
        public static String[] ReadFile(string strPath)
        {
            //lock (g_objDiskLock)  // 測試在讀取檔案時不做 lock 是否會有問題
            {
                FileStream clsFileStream = null;
                StreamReader clsStreamReader = null;
                try
                {
                    if (File.Exists(strPath))
                    {
                        clsFileStream = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        {
                            clsStreamReader = new StreamReader(clsFileStream, Encoding.Default);
                            List<string> strContents = new List<string>();
                            while (!clsStreamReader.EndOfStream)
                            {
                                strContents.Add(clsStreamReader.ReadLine());
                            }
                            clsStreamReader.Close();
                            clsFileStream.Close();
                            return strContents.ToArray();
                        }
                    }
                    else return null;
                }
                catch (Exception ex)
                {
                    XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                    if (clsStreamReader != null) clsStreamReader.Close();
                    if (clsFileStream != null) clsFileStream.Close();
                    return null;
                }
            }
        }

        /// <summary>
        /// 直接讀取資料檔案。再以陣列方式回傳檔案內容。<para></para>
        /// 更換原先使用 File.ReadAllLine() 的寫法，<para></para>
        /// 由於 ADJ 專案遇到神奇的檔案被鎖住的事件，造成無法對檔案的內容讀取，<para></para>
        /// 故改以此非同步串流讀取的方式讀取檔案，能夠越過檔案被鎖住的狀況。<para></para>
        /// 2014.04.15 by Alex 。<para></para>
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名。</param>
        /// <param name="eEncoding"> Encoding 。</param>
        /// <returns>回傳檔案內容，以String[]的方式承接。</returns>
        public static String[] ReadFile(string strPath, Encoding eEncoding)
        {
            //lock (g_objDiskLock)  // 測試在讀取檔案時不做 lock 是否會有問題
            {
                FileStream clsFileStream = null;
                StreamReader clsStreamReader = null;
                try
                {
                    if (File.Exists(strPath))
                    {
                        clsFileStream = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        {
                            clsStreamReader = new StreamReader(clsFileStream, eEncoding);
                            List<string> strContents = new List<string>();
                            while (!clsStreamReader.EndOfStream)
                            {
                                strContents.Add(clsStreamReader.ReadLine());
                            }
                            clsStreamReader.Close();
                            clsFileStream.Close();
                            return strContents.ToArray();
                        }
                    }
                    else return null;
                }
                catch (Exception ex)
                {
                    XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                    if (clsStreamReader != null) clsStreamReader.Close();
                    if (clsFileStream != null) clsFileStream.Close();
                    return null;
                }
            }
        }

        /// <summary>
        /// 單純進行資料存檔並加密。<para></para>
        /// <para><b>注意！如果該路徑原本就存在，將會蓋掉原本資料!!</b></para>
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名。</param>
        /// <param name="strFileDatas">資料內容。</param>
        public static void WriteSecurityFile(string strPath, List<string> strFileDatas, Encode eEncode = Encode.Default ,string strKey = "")
        {
            WriteSecurityFile(strPath, strFileDatas.ToArray(), eEncode, strKey);
        }

        /// <summary>
        /// 單純進行資料存檔並加密。<para></para>
        /// <para><b>注意！如果該路徑原本就存在，將會蓋掉原本資料!!</b></para>
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名。</param>
        /// <param name="strFileDatas">資料內容。</param>
        /// <param name="eEncode">編碼格式 (預設值為： Encode.Default ) 。</param>
        public static void WriteSecurityFile(string strPath, string[] strFileDatas, Encode eEncode = Encode.Default , string strKey = "")
        {
            if (strFileDatas != null)
            {
                Encoding e;
                switch (eEncode)
                {
                    case Encode.ASCII:
                        e = Encoding.ASCII;
                        break;
                    case Encode.BigEndianUnicode:
                        e = Encoding.BigEndianUnicode;
                        break;
                    case Encode.Default:
                        e = Encoding.Default;
                        break;
                    case Encode.Unicode:
                        e = Encoding.Unicode;
                        break;
                    case Encode.UTF32:
                        e = Encoding.UTF32;
                        break;
                    case Encode.UTF7:
                        e = Encoding.UTF7;
                        break;
                    case Encode.UTF8:
                        e = Encoding.UTF8;
                        break;
                    default:
                        e = Encoding.UTF8;
                        break;
                }

                lock (g_objDiskLock)
                {
                    try
                    {
                        StringBuilder clsStringBuilder = new StringBuilder();
                        foreach (string strPerLine in strFileDatas)
                        {
                            clsStringBuilder.AppendLine(strPerLine);
                        }
                        using (StreamWriter clsStreamWriter = new StreamWriter(strPath, false, e))
                        {
                            clsStreamWriter.Write(clsStringBuilder.ToString());
                            clsStreamWriter.Close();
                        }
                        clsStringBuilder.Clear();
                        clsStringBuilder = null;
                        XFileSecurity.EncryptFile(strPath, strKey);
                    }
                    catch (Exception ex)
                    {
                        XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                    }
                }
            }
        }

        /// <summary>
        /// 直接讀取加密資料檔案。再以陣列方式回傳檔案內容。<para></para>
        /// </summary>
        /// <param name="strPath">完整路徑及副檔名。</param>
        /// <returns>回傳檔案內容，以String[]的方式承接。</returns>
        public static String[] ReadSecurityFile(string strPath ,string strKey = "")
        {
            lock (g_objDiskLock)
            {
                FileStream clsFileStream = null;
                StreamReader clsStreamReader = null;

                try
                {
                    if (XFileSecurity.IsEncryptFile(strPath))
                    {
                        string strTempFile = Path.GetTempFileName();

                        File.Copy(strPath, strTempFile, true);
                        XFileSecurity.DecryptFile(strTempFile, strKey);

                        List<string> strContents = new List<string>();
                        using (clsFileStream = new FileStream(strTempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (clsStreamReader = new StreamReader(clsFileStream, Encoding.Default))
                            {
                                while (!clsStreamReader.EndOfStream)
                                {
                                    strContents.Add(clsStreamReader.ReadLine());
                                }
                            }
                        }
                        File.Delete(strTempFile);
                        return strContents.ToArray();
                    }
                    else return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ReadFile] - " + " : " + ex.Message);
                    if (clsStreamReader != null) clsStreamReader.Close();
                    if (clsFileStream != null) clsFileStream.Close();
                    return null;
                }
            }
        }

        /// <summary>刪除一資料夾或檔案。</summary>
        /// <returns>回傳是否執行成功。</returns>
        public static bool Delete(string strPath)
        {
            bool bResult = true;

            lock (g_objDiskLock)
            {
                try
                {
                    // 確保資料夾是否存在
                    if (Directory.Exists(strPath))
                    {
                        XFile.DeleteFilesByDate(strPath, DateTime.Now, DeleteDateType.Before, true, true);
                    }
                    else if (File.Exists(strPath))
                    {
                        File.Delete(strPath);
                    }
                }
                catch (Exception ex)
                {
                    bResult = false;
                    XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                }
            }
            return bResult;
        }

        /// <summary>移動資料夾位置 。
        /// <para>移動相同路徑可達到更名的效果 。</para></summary>
        /// <param name="strSourceFolder">資料夾來源路徑 。</param>
        /// <param name="strTargetPath">資料夾目標路徑 。</param>
        /// <returns>XErrorCode 。</returns>
        public static bool MoveFolder(string strSourceFolder, string strTargetPath)
        {
            bool bResult = true;
            lock (g_objDiskLock)
            {
                try
                {
                    // 確保資料夾是否存在
                    if (Directory.Exists(strSourceFolder))
                    {
                        Directory.Move(strSourceFolder, strTargetPath);
                    }
                    else
                    {
                        XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), strSourceFolder + "資料夾不存在。");
                    }
                }
                catch (Exception ex)
                {
                    bResult = false;
                    XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                }
            }
            return bResult;
        }

        /// <summary>移動檔案位置 。
        /// <para>移動相同路徑可達到更名的效果 。</para></summary>
        /// <param name="strSourceFile">檔案來源路徑 。</param>
        /// <param name="strTargetFile">檔案目標路徑 。</param>
        /// <returns>XErrorCode 。</returns>
        public static bool MoveFile(string strSourceFile, string strTargetFile)
        {
            bool bResult = true;
            lock (g_objDiskLock)
            {
                try
                {
                    // 確保檔案是否存在
                    if (File.Exists(strSourceFile))
                    {
                        File.Move(strSourceFile, strTargetFile);
                    }
                    else
                    {
                        XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), strSourceFile + "檔案不存在。");
                    }
                }
                catch (Exception ex)
                {
                    bResult = false;
                    XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                }
            }
            return bResult;
        }

        #endregion

        #region " Methods - Find File/Folder "

        /// <summary>
        /// NTFS索引方式遍歷指定硬碟所有檔案。
        /// </summary>
        private static class USNJournal
        {

            #region " Properties "
            private static IntPtr g_pINVALID_HANDLE_VALUE = new IntPtr(-1);
            private const uint g_iGENERIC_READ = 0x80000000;
            private const int g_iFILE_SHARE_READ = 0x1;
            private const int g_iFILE_SHARE_WRITE = 0x2;
            private const int g_iOPEN_EXISTING = 3;
            private const int g_iFILE_READ_ATTRIBUTES = 0x80;
            private const int g_iFILE_NAME_IINFORMATION = 9;
            private const int g_iFILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
            private const int g_iFILE_OPEN_FOR_BACKUP_INTENT = 0x4000;
            private const int g_iFILE_OPEN_BY_FILE_ID = 0x2000;
            private const int g_iFILE_OPEN = 0x1;
            private const int g_iOBJ_CASE_INSENSITIVE = 0x40;
            private const int g_iFSCTL_ENUM_USN_DATA = 0x900b3;

            private static IntPtr g_pFilePointer;
            private static IntPtr g_pBuffer;
            private static int g_iBufferSize;
            private static string g_strDriveLetter;

            [StructLayout(LayoutKind.Sequential)]
            private struct MFT_ENUM_DATA
            {
                public long StartFileReferenceNumber;
                public long LowUsn;
                public long HighUsn;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct USN_RECORD
            {
                public int iRecordLength;
                public short iMajorVersion;
                public short iMinorVersion;
                public long iFileReferenceNumber;
                public long iParentFileReferenceNumber;
                public long iUsn;
                public long iTimeStamp;
                public int iReason;
                public int iSourceInfo;
                public int iSecurityId;
                public FileAttributes eFileAttributes;
                public short iFileNameLength;
                public short iFileNameOffset;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct IO_STATUS_BLOCK
            {
                public int iStatus;
                public int iInformation;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct UNICODE_STRING
            {
                public short iLength;
                public short iMaximumLength;
                public IntPtr pBuffer;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct OBJECT_ATTRIBUTES
            {
                public int iLength;
                public IntPtr pRootDirectory;
                public IntPtr pObjectName;
                public int iAttributes;
                public int iSecurityDescriptor;
                public int iSecurityQualityOfService;
            }

            public struct FSNode
            {
                public long iFRN;
                public long iParentFRN;
                public string strFileName;
                public bool bIsFile;
                public bool bIsDirectory;
                public bool bIsHidden;

                public FSNode(long FRN, long ParentFSN, string FileName, bool IsFile, bool IsDirectory, bool IsHidden)
                {
                    iFRN = FRN;
                    iParentFRN = ParentFSN;
                    strFileName = FileName;
                    bIsFile = IsFile;
                    bIsHidden = IsHidden;
                    bIsDirectory = IsDirectory;
                }
            }

            #endregion

            #region " Methods "
            //// MFT_ENUM_DATA
            [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
            private static extern bool DeviceIoControl(IntPtr pDevice, int iIoControlCode, ref MFT_ENUM_DATA tlpInBuffer, int iInBufferSize, IntPtr pOutBuffer, int iOutBufferSize, ref int iBytesReturned, IntPtr pOverlapped);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern IntPtr CreateFile(string strFileName, uint iDesiredAccess, int iShareMode, IntPtr pSecurityAttributes, int iCreationDisposition, int iFlagsAndAttributes, IntPtr pTemplateFile);

            [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
            private static extern Int32 CloseHandle(IntPtr pObject);

            [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
            private static extern int NtCreateFile(ref IntPtr pFileHandle, int iDesiredAccess, ref OBJECT_ATTRIBUTES tObjectAttributes, ref IO_STATUS_BLOCK tIoStatusBlock, int iAllocationSize, int iFileAttribs, int iSharedAccess, int iCreationDisposition, int iCreateOptions, int iEaBuffer,
            int iEaLength);

            [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
            private static extern int NtQueryInformationFile(IntPtr pFileHandle, ref IO_STATUS_BLOCK tIoStatusBlock, IntPtr pFileInformation, int iLength, int iFileInformationClass);

            /// <summary>開啟硬碟。</summary>
            private static IntPtr OpenVolume()
            {

                IntPtr pCJ = default(IntPtr);

                pCJ = CreateFile("\\\\.\\" + g_strDriveLetter, g_iGENERIC_READ, g_iFILE_SHARE_READ | g_iFILE_SHARE_WRITE, IntPtr.Zero, g_iOPEN_EXISTING, 0, IntPtr.Zero);

                return pCJ;

            }

            /// <summary>釋放資源。</summary>
            public static void Cleanup()
            {
                if (g_pFilePointer != IntPtr.Zero)
                {
                    // 斷開句柄。
                    CloseHandle(g_pFilePointer);
                    g_pFilePointer = g_pINVALID_HANDLE_VALUE;
                }

                if (g_pBuffer != IntPtr.Zero)
                {
                    //釋放記憶體。
                    Marshal.FreeHGlobal(g_pBuffer);
                    g_pBuffer = IntPtr.Zero;
                }

            }

            /// <summary>
            /// 搜尋此磁碟所有文件。
            /// </summary>
            /// <param name="clsAllFiles"></param>
            /// <param name="strDriveLetter"></param>
            public static void ScanAll(ref Dictionary<long, FSNode> clsAllFiles, string strDriveLetter)
            {
                var tUsnRecord = default(USN_RECORD);
                var tMFT = default(MFT_ENUM_DATA);
                int iDwRetBytes = 0;
                int iCb = 0;
                bool bIsFile = false;
                bool bIsDirectory = false;
                bool bIsHidden = false;
                string strFileName;

                IntPtr pUsnRecord;
                IntPtr pFileName;

                tMFT.StartFileReferenceNumber = 0;
                tMFT.LowUsn = 0;
                tMFT.HighUsn = long.MaxValue;

                g_pBuffer = (IntPtr)0;

                // Assign buffer size
                g_iBufferSize = 65536;
                //64KB

                // Allocate a buffer to use for reading records.
                g_pBuffer = Marshal.AllocHGlobal(g_iBufferSize);

                // correct path
                strDriveLetter = strDriveLetter.TrimEnd('\\');

                g_strDriveLetter = strDriveLetter;

                // Open the volume handle 
                g_pFilePointer = OpenVolume();

                // Check if the volume handle is valid.
                if (g_pFilePointer == g_pINVALID_HANDLE_VALUE)
                {
                    throw new Exception("無法使用句柄打開硬碟。");
                }

                do
                {
                    if (DeviceIoControl(g_pFilePointer, g_iFSCTL_ENUM_USN_DATA, ref tMFT, Marshal.SizeOf(tMFT), g_pBuffer, g_iBufferSize, ref iDwRetBytes, IntPtr.Zero))
                    {
                        iCb = iDwRetBytes;
                        // Pointer to the first record
                        pUsnRecord = (IntPtr)(g_pBuffer.ToInt32() + 8);

                        while ((iDwRetBytes > 8))
                        {
                            // Copy pointer to USN_RECORD structure.
                            tUsnRecord = (USN_RECORD)Marshal.PtrToStructure(pUsnRecord, tUsnRecord.GetType());

                            pFileName = (IntPtr)(pUsnRecord.ToInt32() + tUsnRecord.iFileNameOffset);

                            // The filename within the USN_RECORD.
                            strFileName = Marshal.PtrToStringUni(pFileName, tUsnRecord.iFileNameLength / 2);

                            bIsDirectory = tUsnRecord.eFileAttributes.HasFlag(FileAttributes.Directory);
                            bIsFile = !tUsnRecord.eFileAttributes.HasFlag(FileAttributes.Directory);
                            bIsHidden = tUsnRecord.eFileAttributes.HasFlag(FileAttributes.Hidden);
                            clsAllFiles.Add(tUsnRecord.iFileReferenceNumber, new FSNode(tUsnRecord.iFileReferenceNumber, tUsnRecord.iParentFileReferenceNumber, strFileName, bIsFile, bIsDirectory, bIsHidden));

                            // Pointer to the next record in the buffer.
                            pUsnRecord = (IntPtr)(pUsnRecord.ToInt32() + tUsnRecord.iRecordLength);

                            iDwRetBytes -= tUsnRecord.iRecordLength;
                        }

                        // The first 8 bytes is always the start of the next USN.
                        tMFT.StartFileReferenceNumber = Marshal.ReadInt64(g_pBuffer, 0);
                    }
                    else
                    {
                        break; // TODO: might not be correct. Was : Exit Do
                    }

                } while (!(iCb <= 8));
            }

            #endregion

        }

        /// <summary>
        /// 指定資料夾的位置，去抓取底下有多少個資料夾，並且獲取該名稱。
        /// </summary>
        /// <param name="strFolder">資料夾路徑。</param>
        /// <param name="strExtension">指定搜尋的附檔名。</param>
        /// <returns>回傳目前指定的資料夾底下有多少個資料夾。</returns>
        public static List<string> GetFilesNameFromFolder(string strFolder, string strExtension = "*.*")
        {
            List<string> strSubFolders = new List<string>();			// Create return data

            try
            {
                // 確保資料夾是否存在
                if (!Directory.Exists(strFolder))
                {
                    return strSubFolders;   //為避免搜尋的資料夾權限若是唯讀狀態時，若再另建立新資料夾會造成系統錯誤之情況
                }

                // 設定要讀取的資料夾
                DirectoryInfo clsTarget = new DirectoryInfo(strFolder);

                // 取得目標底下的資料夾內的資料夾名稱  GetDirectories
                foreach (FileInfo clsRecipe in clsTarget.GetFiles(strExtension))
                {
                    if (clsRecipe.Name != "")
                    {
                        strSubFolders.Add(clsRecipe.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return strSubFolders;
        }

        /// <summary>
        /// 取得路徑底下所有檔案路徑。
        /// </summary>
        /// <param name="strFolder">資料夾路徑。</param>
        /// <param name="strExtension">指定搜尋的附檔名。</param>
        /// <returns>回傳檔案路徑列表。</returns>
        public static List<string> GetAllFilesPathFromFolder(string strFolder, string strExtension = "*.*")
        {
            List<string> strAllPathes = new List<string>();			// Create return data

            try
            {
                // 確保資料夾是否存在
                if (!Directory.Exists(strFolder))
                {
                    return strAllPathes;   //為避免搜尋的資料夾權限若是唯讀狀態時，若再另建立新資料夾會造成系統錯誤之情況
                }

                // 設定要讀取的資料夾
                DirectoryInfo clsTarget = new DirectoryInfo(strFolder);

                // 取得目標底下的資料夾內的資料檔案名稱
                foreach (FileInfo clsFile in clsTarget.GetFiles(strExtension))
                {
                    if (clsFile.Name != "") strAllPathes.Add(strFolder + "\\" + clsFile.Name);
                }

                // 取得目標底下的資料夾內的資料檔案名稱
                List<string> strFolderNames = GetFoldersNameFromFolder(strFolder);
                foreach (string folderName in strFolderNames)
                {
                    strAllPathes.AddRange(GetAllFilesPathFromFolder(strFolder + "\\" + folderName, strExtension));
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return strAllPathes;
        }

        /// <summary>
        /// 回傳指定資料夾底下的檔案，並包含路徑。
        /// </summary>
        /// <param name="strFolder">指定搜尋的資料夾。</param>
        /// <param name="strExtension">指定搜尋的附檔名。</param>
        /// <returns></returns>
        public static List<string> GetFilesPathFromFolder(string strFolder, string strExtension = "*.*")
        {
            List<string> strSubFolderFiles = new List<string>();			// Create return data

            try
            {
                // 確保資料夾是否存在
                if (!Directory.Exists(strFolder))
                {
                    return strSubFolderFiles;
                }

                // 設定要讀取的資料夾
                DirectoryInfo clsDirectoryInfo = new DirectoryInfo(strFolder);

                // 取得目標底下的資料夾內的資料檔案名稱
                foreach (FileInfo clsFileInfo in clsDirectoryInfo.GetFiles(strExtension))
                {
                    if (clsFileInfo.Name != "")
                    {
                        strSubFolderFiles.Add(strFolder + "\\" + clsFileInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return strSubFolderFiles;
        }

        /// <summary>
        /// 回傳指定資料夾底下的檔案。
        /// </summary>
        /// <param name="strFolder">指定搜尋的資料夾。</param>
        /// <param name="strExtension">指定搜尋的附檔名。</param>
        /// <returns></returns>
        public static List<string> GetFileNamesPathFromFolder(string strFolder, string strExtension = "*.*")
        {
            List<string> strSubFolderFiles = new List<string>();			// Create return data

            try
            {
                // 確保資料夾是否存在
                if (!Directory.Exists(strFolder))
                {
                    return strSubFolderFiles;
                }

                // 設定要讀取的資料夾
                DirectoryInfo clsDirectoryInfo = new DirectoryInfo(strFolder);

                // 取得目標底下的資料夾內的資料檔案名稱
                foreach (FileInfo clsFileInfo in clsDirectoryInfo.GetFiles(strExtension))
                {
                    if (clsFileInfo.Name != "")
                    {
                        strSubFolderFiles.Add(clsFileInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return strSubFolderFiles;
        }



        /// <summary>指定資料夾的位置，去抓取底下有多少個檔案路徑。</summary>
        /// <param name="strFolder">資料夾路徑。</param>
        /// <param name="clsFileSizeLimit">欲抓取的檔案大小限制範圍，只抓取這個 A~B 數值範圍內的檔案（單位：Byte）。</param>
        /// <returns>回傳目前指定的資料夾底下有多少個符合檔案大小限制的檔案檔名。</returns>
        public static List<string> GetFilePathFromFolder(string strFolder, long iMinByte , long iMaxByte)
        {
            List<string> strAllNames = new List<string>();			// Create return data
            try
            {
                // 確保資料夾是否存在
                if (!Directory.Exists(strFolder))
                {
                    return strAllNames;   //為避免搜尋的資料夾權限若是唯讀狀態時，若再另建立新資料夾會造成系統錯誤之情況
                }

                // 設定要讀取的資料夾
                DirectoryInfo clsTarget = new DirectoryInfo(strFolder);

                // 取得目標底下的資料夾內的資料檔案名稱
                foreach (FileInfo clsFile in clsTarget.GetFiles())
                {
                    if (clsFile.Length >= iMinByte && clsFile.Length <= iMaxByte)
                    {
                        strAllNames.Add(clsFile.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return strAllNames;
        }

        /// <summary>
        /// 取得指定硬碟下所有檔案。
        /// </summary>
        /// <param name="strDriveLetter">硬碟名稱。(例如: C:\\ 或 D:\\)</param>
        /// <returns>輸出結果。</returns>
        public static List<string> GetDirveAllFiles(string strDriveLetter)
        {
            string strFullPath;
            USNJournal.FSNode tParentFSNode;

            List<string> strFileResults = new List<string>();
            Dictionary<long, USNJournal.FSNode> clsAllFiles = new Dictionary<long, USNJournal.FSNode>();

            try
            {
                USNJournal.ScanAll(ref clsAllFiles, strDriveLetter);

                // Resolve all paths for Files
                //clsAllFiles = clsAllFiles.Where(kvp => kvp.Value.IsFile && !kvp.Value.IsHidden).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                foreach (USNJournal.FSNode tFSNode in clsAllFiles.Values.Where(tElement => tElement.bIsFile && !tElement.bIsHidden))
                {
                    strFullPath = tFSNode.strFileName;
                    tParentFSNode = tFSNode;

                    if (Path.HasExtension(strFullPath))//是檔案
                    {
                        while (clsAllFiles.TryGetValue(tParentFSNode.iParentFRN, out tParentFSNode))
                        {
                            strFullPath = tParentFSNode.strFileName + "\\" + strFullPath;
                        }

                        strFullPath = strDriveLetter + "\\" + strFullPath;
                        strFileResults.Add(strFullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }

            return strFileResults;
        }

        /// <summary>
        /// 取得指定硬碟下所有資料夾。
        /// </summary>
        /// <param name="strDriveLetter">硬碟代號。</param>
        /// <returns>輸出結果。</returns>
        public static List<string> GetDirveAllFolders(string strDriveLetter)
        {
            string strFullPath;
            USNJournal.FSNode tParentFSNode;
            List<string> clsResults = new List<string>();

            try
            {
                strDriveLetter = Path.GetPathRoot(strDriveLetter);



                //Dictionary<long, clsUSNJournal.FSNode> clsFiles;
                Dictionary<long, USNJournal.FSNode> clsAllFiles = new Dictionary<long, USNJournal.FSNode>();

                USNJournal.ScanAll(ref clsAllFiles, strDriveLetter);

                // Resolve all paths for Files
                //clsAllFiles = clsAllFiles.Where(kvp => kvp.Value.IsFile && !kvp.Value.IsHidden).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                foreach (USNJournal.FSNode tFSNode in clsAllFiles.Values.Where(o => o.bIsDirectory && !o.bIsHidden))
                {
                    strFullPath = tFSNode.strFileName;
                    tParentFSNode = tFSNode;

                    while (clsAllFiles.TryGetValue(tParentFSNode.iParentFRN, out tParentFSNode))
                    {
                        strFullPath = tParentFSNode.strFileName + "\\" + strFullPath;//string.Concat(oParentFSNode.FileName, "\\", sFullPath);
                    }

                    strFullPath = strDriveLetter + "\\" + strFullPath;//string.Concat(szDriveLetter, "\\", sFullPath);
                    clsResults.Add(strFullPath);//yield return strFullPath;
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return clsResults;
        }

        /// <summary>
        /// 搜尋。指定路徑下關鍵字檔案。
        /// </summary>
        /// <param name="strPath">欲搜尋路徑。</param>
        /// <param name="strKeyword">關鍵字(支援正則表達式)。</param>
        /// <param name="bDeepSearch">深度搜尋(包含路徑下所有資料夾)。</param>
        /// <param name="strExtension">副檔名(例：*.jpg, *.exe ...etc)</param>
        /// <returns>搜尋結果(完整路徑)。</returns>
        public static List<string> GetKeywordFiles(string strPath, string strKeyword, bool bDeepSearch, string strExtension = "*.*")
        {
            List<string> clsAllFiles;
            List<string> clsMatchFolders = new List<string>();

            try
            {
                if (!Directory.Exists(strPath))
                    return clsMatchFolders;

                if (bDeepSearch)
                {
                    clsAllFiles = XFile.GetAllFilesPathFromFolder(strPath, strExtension);
                }
                else
                {
                    clsAllFiles = XFile.GetFilesPathFromFolder(strPath, strExtension);
                }

                foreach (string strFile in clsAllFiles)
                {
                    if (Regex.IsMatch(Path.GetFileNameWithoutExtension(strFile), strKeyword))
                        clsMatchFolders.Add(strFile);
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }

            return clsMatchFolders;
        }

        /// <summary>
        /// NTFS 索引方式過濾指定磁碟內關鍵字檔案。
        /// </summary>
        /// <param name="strDrive">磁碟名稱或路徑。</param>
        /// <param name="strKeyword">關鍵字(支援正則表達式)。</param>
        /// <param name="strExtenstionIn">副檔名(ex:.jpg; *.jpg)</param>
        /// <returns>過濾結果(完整路徑)。</returns>
        public static List<string> GetKeywordFiles(string strDrive, string strKeyword, string strExtenstionIn = "*.*")
        {
            string strDriveLetter;
            string strFileName;
            string strExtension;
            List<string> strFileResults;
            List<string> strMatchFiles = new List<string>();

            try
            {
                strDriveLetter = Path.GetPathRoot(strDrive);
                strFileName = "";
                strExtension = "";
                strExtenstionIn = Path.GetExtension(strExtenstionIn);
                strFileResults = GetDirveAllFiles(strDriveLetter);

                // Resolve all paths for Files
                foreach (string strFilePath in strFileResults)
                {
                    if (Path.HasExtension(strFilePath))//是檔案
                    {
                        strFileName = Path.GetFileNameWithoutExtension(strFilePath);
                        strExtension = Path.GetExtension(strFilePath);

                        if (Regex.IsMatch(strFileName, strKeyword))//比較檔名
                        {
                            if (strExtenstionIn == ".*")//所有副檔名
                            {
                                strMatchFiles.Add(strFilePath);
                            }
                            else//指定副檔名
                            {
                                if (strExtension == strExtenstionIn)//比較副檔名
                                {
                                    strMatchFiles.Add(strFilePath);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }

            return strMatchFiles;
        }

        /// <summary>
        /// 搜尋。指定路徑下關鍵字資料夾。
        /// </summary>
        /// <param name="strPath">欲搜尋路徑。</param>
        /// <param name="strKeyword">關鍵字(支援正則表達式)。</param>
        /// <param name="bDeepSearch">深度搜尋(包含路徑下所有資料夾)。</param>
        /// <returns>搜尋結果(完整路徑)。</returns>
        public static List<string> GetKeywordFolders(string strPath, string strKeyword, bool bDeepSearch)
        {
            string strFileName;
            List<string> clsAllFolders = new List<string>();
            List<string> clsMatchFolders = new List<string>();

            if (!Directory.Exists(strPath))
                return clsMatchFolders;

            try
            {
                if (bDeepSearch)
                {
                    GetAllFoldersFromFolder(ref clsAllFolders, strPath);
                }
                else
                {
                    foreach (string strFolderPath in Directory.GetDirectories(strPath))
                    {
                        clsAllFolders.Add(strFolderPath);
                    }
                }

                foreach (var strFolder in clsAllFolders)
                {
                    strFileName = Path.GetFileNameWithoutExtension(strFolder);

                    if (Regex.IsMatch(strFileName, strKeyword))
                        clsMatchFolders.Add(strFolder);
                }

            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }

            return clsMatchFolders;
        }

        /// <summary>
        /// NTFS索引方式過濾指定磁碟內關鍵字資料夾。
        /// </summary>
        /// <param name="strPath">磁碟名稱或路徑。</param>
        /// <param name="strKeyword">關鍵字(支援正則表達式)。</param>
        /// <returns>過濾結果(完整路徑)。</returns>
        public static List<string> GetKeywordFolders(string strPath, string strKeyword)
        {
            string strDriveLetter;
            string strFolderName;
            List<string> strAllFolders;
            List<string> strMatchFolders = new List<string>();

            try
            {
                strDriveLetter = Path.GetPathRoot(strPath);

                strAllFolders = GetDirveAllFolders(strDriveLetter);

                // Resolve all paths for Files
                foreach (string strFolderPath in strAllFolders)
                {
                    strFolderName = strFolderPath.Substring(strFolderPath.LastIndexOf('\\') + 1);

                    if (Regex.IsMatch(strFolderName, strKeyword))
                    {
                        strMatchFolders.Add(strFolderPath);
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }

            return strMatchFolders;
        }

        /// <summary>
        /// 指定資料夾的位置，去抓取底下有多少個資料夾，並且獲取該名稱。
        /// </summary>
        /// <param name="strFolder">資料夾路徑。</param>
        /// <returns>回傳目前指定的資料夾底下有多少個資料夾。</returns>
        public static List<string> GetFoldersNameFromFolder(string strFolder)
        {
            List<string> strSubFolders = new List<string>();			// Create return data

            try
            {
                // 確保資料夾是否存在
                if (!Directory.Exists(strFolder))
                {
                    return strSubFolders;
                }

                // 設定要讀取的資料夾
                DirectoryInfo target = new DirectoryInfo(strFolder);

                // 取得目標底下的資料夾內的資料夾名稱  GetDirectories
                foreach (DirectoryInfo recipe in target.GetDirectories())
                {
                    if (recipe.Name != "")
                    {
                        strSubFolders.Add(recipe.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return strSubFolders;
        }

        /// <summary>
        /// 指定路徑，去抓取底下所有資料夾，並且獲取完整路徑。
        /// </summary>
        /// <param name="clsAllFolders">累計資料夾容器。</param>
        /// <param name="strFolder">資料夾路徑。</param>
        public static void GetAllFoldersFromFolder(ref List<string> clsAllFolders, string strFolder)
        {
            try
            {
                foreach (string strFolderPath in Directory.GetDirectories(strFolder))
                {
                    clsAllFolders.Add(strFolderPath);
                    GetAllFoldersFromFolder(ref clsAllFolders, strFolderPath);
                }
            }
            catch { }
        }

        /// <summary>取得，應用程式啟動起始路徑。</summary>
        public static string ApplicationStartupPath
        {
            get { return XFile.g_strApplication_StartupPath; }
        }

        private static readonly string g_strApplication_StartupPath = Application.StartupPath;

        /// <summary>
        /// 指定的檔案是否存在，利用這個功能，使得其他Class不需要using就能使用Files的功能。
        /// </summary>
        /// <param name="strPath">檔案路徑。</param>
        /// <returns>回傳Bool值，檔案是否存在。</returns>
        public static bool IsExist_File(ref string strPath)
        {
            bool isExist = File.Exists(strPath);
            if (!isExist)
            {
                string anotherPath = ("\\" + strPath).Replace(@"\\", @"\");

                // 如果找不到指定的檔案，那就多加一層判斷主程式端的位置。
                if (anotherPath.Length > 1 && anotherPath.Substring(1, 1) == "\\")
                    anotherPath = g_strApplication_StartupPath + anotherPath.Substring(1, anotherPath.Length - 1);
                else
                    anotherPath = g_strApplication_StartupPath + anotherPath;

                isExist = File.Exists(anotherPath);
                if (isExist) strPath = anotherPath;
            }
            return isExist;
        }

        /// <summary>
        /// 指定的檔案是否存在，利用這個功能，使得其他Class不需要using就能使用Files的功能。
        /// </summary>
        /// <param name="strPath">檔案路徑。</param>
        /// <returns>回傳Bool值，檔案是否存在。</returns>
        public static bool IsExist_File(string strPath)
        {
            bool isExist = File.Exists(strPath);
            if (!isExist)
            {
                string anotherPath = ("\\" + strPath).Replace(@"\\", @"\");

                // 如果找不到指定的檔案，那就多加一層判斷主程式端的位置。
                if (anotherPath.Length > 1 && anotherPath.Substring(1, 1) == "\\")
                    anotherPath = g_strApplication_StartupPath + anotherPath.Substring(1, anotherPath.Length - 1);
                else
                    anotherPath = g_strApplication_StartupPath + anotherPath;

                isExist = File.Exists(anotherPath);
                if (isExist) strPath = anotherPath;
            }
            return isExist;
        }

        /// <summary>
        /// 資料夾是否存在
        /// </summary>
        /// <param name="strPath">資料夾路徑。</param>
        /// <returns>存在則回傳 True 。</returns>
        public static bool IsExist_Folder(string strPath)
        {
            return Directory.Exists(strPath);
        }

        #endregion

        #region " Methods - Application Function "

        /// <summary>
        /// 依照日期時間刪除檔案功能。
        /// <para><b>利用此函數砍掉的檔案將不會放置於資源回收筒，所以請小心使用!!</b></para>
        /// </summary>
        /// <remarks>
        /// 建議呼叫前請自行確認資料夾是否存在，否則一旦進入之後發現不存在，會另外回報錯誤，這部份請自行考量回報機制。
        /// </remarks>
        /// <param name="strFolderPath">給予主目錄來進行刪除檔案的動作。</param>
        /// <param name="tTime">參考的時間點。</param>
        /// <param name="eDeleteType">要刪除的檔案是在參考點往前的時間或往後的時間。</param>
        /// <param name="bNeedDeleteEmptyFolder">如果資料夾內部檔案已經清空了，是否還要刪除資料夾。</param>
        /// <param name="bSearchAllSubfolders">是否要搜索資料夾內部更多的資料夾。</param>
        /// <param name="strExtension">副檔名格式。</param>
        /// <example>
        /// <code>
        /// private void Example()
        /// {
        ///		// 時間的設定可以利用以下方式，即可取得上一個月的時間點。
        ///		DateTime tTime = DateTime.Now.AddMonths(-1);  
        ///		
        ///		// 也可以取得往前20天的時間點。
        ///		DateTime tTime2 = DateTime.Now.AddDays(-20);
        ///		
        ///		// 刪除 20 天前的檔案
        ///		DeleteFilesByDate(@"D:/Test",tTime2,DeleteDateType.Before);
        ///	}
        /// </code>
        /// </example>
        public static void DeleteFilesByDate(string strFolderPath, DateTime tTime, DeleteDateType eDeleteType,
                                                                bool bNeedDeleteEmptyFolder = false, bool bSearchAllSubfolders = false,
                                                                string strExtension = "*.*")
        {
            try
            {
                // 先確認資料夾是否存在
                if (IsExist_Folder(strFolderPath))
                {
                    List<string> strFiles = GetFilesPathFromFolder(strFolderPath, strExtension);
                    foreach (string strFile in strFiles)
                    {
                        DateTime tFileTime = File.GetCreationTime(strFile);
                        switch (eDeleteType)
                        {
                            case DeleteDateType.After:
                                if (tFileTime.Subtract(tTime).TotalMilliseconds >= 0)
                                {
                                    File.Delete(strFile);
                                }
                                break;
                            case DeleteDateType.Before:
                                if (tFileTime.Subtract(tTime).TotalMilliseconds <= 0)
                                {
                                    File.Delete(strFile);
                                }
                                break;
                        }
                    }

                    // 判斷是否要做更多的資料夾，如果是True代表這個資料夾內部的所有資料夾都要看!
                    if (bSearchAllSubfolders)
                    {
                        List<string> strFolders = GetFoldersNameFromFolder(strFolderPath);
                        foreach (string strFolder in strFolders)
                        {
                            DeleteFilesByDate(strFolderPath + "\\" + strFolder, tTime, eDeleteType, bNeedDeleteEmptyFolder, bSearchAllSubfolders, strExtension);
                        }
                    }

                    // 最後進行確認，如果資料夾內部無資料，就砍資料夾
                    if (bNeedDeleteEmptyFolder)
                    {
                        strFiles = GetAllFilesPathFromFolder(strFolderPath);
                        List<string> strFolders = GetFoldersNameFromFolder(strFolderPath);
                        if ((strFiles.Count + strFolders.Count) == 0)
                        {
                            Directory.Delete(strFolderPath);
                        }
                    }
                }
                else
                {
                    XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), strFolderPath + " of folder isn't exist.");
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
        }

        /// <summary>
        /// 砍檔的呼叫指令。(利用此函數砍掉的檔案將不會放置於資源回收筒，所以請小心使用!!)<para></para>
        /// 依照設定日期刪除資料夾，可設定日期 之前 或 之後以及資料夾時間模式 如 資料夾建立日期、資料夾最後存取日期、資料夾最後寫入日期。<para></para>
        /// </summary>
        /// <param name="strFolderPath">欲刪除資料夾路徑。</param>
        /// <param name="tTime">參考的時間點。</param>
        /// <param name="eFolderTimeType">資料夾時間模式。</param>
        /// <param name="eDeleteType">要刪除的資料夾是在參考點往前的時間或往後的時間。</param>
        /// <param name="bRefSubFolder">是否要也參考資料夾下的子資料夾。。</param>
        /// <remarks>會以遞迴的方式刪出資料夾。</remarks>
        public static void DeleteFolderByDate(string strFolderPath, DateTime tTime, FolderTimeType eFolderTimeType, DeleteDateType eDeleteType,
                                                                bool bRefSubFolder = false)
        {
            try
            {
                if (XFile.IsExist_Folder(strFolderPath))
                {
                    //取得父資料夾時間
                    DateTime tFolderTime;
                    switch (eFolderTimeType)
                    {
                        case FolderTimeType.CreateTime:
                            tFolderTime = Directory.GetCreationTime(@strFolderPath);
                            break;
                        case FolderTimeType.LastAccessTime:
                            tFolderTime = Directory.GetLastAccessTime(@strFolderPath);
                            break;
                        default:
                        case FolderTimeType.LastWriteTime:
                            tFolderTime = Directory.GetLastWriteTime(@strFolderPath);
                            break;
                    }


                    // 依照 存取時間 與建立時間 決定 遞迴應該要先作還是後作
                    switch (eFolderTimeType)
                    {
                        // 如果是建立時間的話要優先由內往外作
                        case FolderTimeType.CreateTime:
                            //優先從最裡面的資料夾開始判定，如果不符合就一個一個刪除
                            if (bRefSubFolder)
                            {
                                // 有可能子資料夾已經被砍掉了，所以要判斷。
                                if (Directory.Exists(strFolderPath))
                                {
                                    //取得子資料夾時間
                                    string[] strSubfolders = Directory.GetDirectories(strFolderPath);
                                    foreach (string strFolderName in strSubfolders)
                                    {
                                        DeleteFolderByDate(strFolderName, tTime, eFolderTimeType, eDeleteType, bRefSubFolder);
                                    }
                                }
                            }
                            //刪除資料夾
                            switch (eDeleteType)
                            {
                                case DeleteDateType.Before:
                                    if (tFolderTime.Subtract(tTime).TotalMilliseconds <= 0)
                                    {
                                        Directory.Delete(strFolderPath, true);
                                    }
                                    break;
                                case DeleteDateType.After:
                                    if (tFolderTime.Subtract(tTime).TotalMilliseconds >= 0)
                                    {
                                        Directory.Delete(strFolderPath, true);
                                    }
                                    break;
                            }
                            break;
                        // 如果是存取時間的話只要看最外層就好
                        case FolderTimeType.LastAccessTime:
                        case FolderTimeType.LastWriteTime:
                            //刪除資料夾
                            switch (eDeleteType)
                            {
                                case DeleteDateType.Before:
                                    if (tFolderTime.Subtract(tTime).TotalMilliseconds <= 0)
                                    {
                                        Directory.Delete(strFolderPath, true);
                                    }
                                    break;
                                case DeleteDateType.After:
                                    if (tFolderTime.Subtract(tTime).TotalMilliseconds >= 0)
                                    {
                                        Directory.Delete(strFolderPath, true);
                                    }
                                    break;
                            }

                            // 是否偵測子資料夾
                            if (bRefSubFolder)
                            {
                                // 有可能子資料夾已經被砍掉了，所以要判斷。
                                if (Directory.Exists(strFolderPath))
                                {
                                    //取得子資料夾時間
                                    string[] strSubfolders = Directory.GetDirectories(strFolderPath);
                                    foreach (string strFolderName in strSubfolders)
                                    {
                                        DeleteFolderByDate(strFolderName, tTime, eFolderTimeType, eDeleteType, bRefSubFolder);
                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), strFolderPath + " of folder isn't exist.");
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
        }

        /// <summary>清空資料夾內所有檔案。</summary>
        /// <param name="strFolder">資料夾路徑。</param>
        public static void ClearFolder(string strFolder)
        {
            try
            {
                DateTime clsDateTime = DateTime.Now;
                if (IsExist_Folder(strFolder))
                    XFile.DeleteFilesByDate(strFolder, clsDateTime, XFile.DeleteDateType.Before, false, true);
                else
                    XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), strFolder + " of folder isn't exist.");
            }
            catch (Exception ex)
            {
                string strEx = ex.Message;
                XStatus.Report(XStatus.Type.IO, MethodInfo.GetCurrentMethod(), strFolder + " of folder isn't exist.");
            }
        }

        /// <summary>
        /// 取得資料夾內容大小。
        /// </summary>
        /// <param name="strFolderPath">資料夾路徑。</param>
        /// <param name="eSizeUnitType">回傳檔案大小單位。</param>
        /// <returns>回傳資料夾內容大小。</returns>
        public static long GetFolderSize(string strFolderPath, FileSizeUnitType eSizeUnitType = FileSizeUnitType.MB)
        {
            long dDataSize = 0;
            if (Directory.Exists(strFolderPath))
            {
                foreach (string strFName in System.IO.Directory.GetFileSystemEntries(strFolderPath))
                {
                    //判斷是檔案還是資料夾,是資料夾的話要一直找下去,直到找不到其他資料夾為止
                    if (System.IO.File.Exists(strFName))
                    {
                        FileInfo clsFileInfo = new FileInfo(strFName);
                        dDataSize = clsFileInfo.Length + dDataSize;
                    }
                    else
                    {
                        dDataSize = GetFolderSize(@strFName, FileSizeUnitType.Byte) + dDataSize;
                    }
                }
            }
            switch (eSizeUnitType)
            {

                case FileSizeUnitType.Byte:
                    dDataSize = dDataSize;
                    break;
                case FileSizeUnitType.KB:
                    dDataSize = dDataSize >> 10;
                    break;
                default:
                case FileSizeUnitType.MB:
                    dDataSize = dDataSize >> 20;
                    break;
                case FileSizeUnitType.GB:
                    dDataSize = dDataSize >> 30;
                    break;
            }
            return dDataSize;
        }

        /// <summary>
        /// 指定來源和目標資料夾的位置，複製來源資料夾所有檔案至目標資料夾。
        /// </summary>
        /// <param name="strSourcePath">來源資料夾的位置。</param>
        /// <param name="strTargetPath">目標資料夾的位置。</param>
        /// <param name="strFilters">指定副檔名 如 "*.jpeg,*.jpg"。</param>
        /// <param name="bDeepClone">是否深層複製。</param>
        /// <remarks>深層複製 是指複製來源底下所有檔案，淺層複製 是指只複製來源內容下檔案，不另外往下資料夾做複製。</remarks>
        public static void CopyDir(string strSourcePath, string strTargetPath, string strFilters = "*.*", bool bDeepClone = true)
        {
            try
            {
                if (XFile.IsExist_Folder(@strSourcePath))
                {
                    string strFileName;
                    string strDeepFileName;
                    string strDeepSourcePath;
                    string strDeepTargetPath;
                    //檔案
                    foreach (string strFile in Directory.GetFiles(@strSourcePath).Where(s => strFilters.Contains(Path.GetExtension(s).ToLower())))
                    {
                        strFileName = Path.GetFileName(strFile);
                        string[] strFolders = Directory.GetDirectories(@strSourcePath);
                        if (bDeepClone && strFolders.Length > 0)
                        {
                            foreach (string strFolder in strFolders)
                            {
                                strDeepFileName = Path.GetFileName(strFolder);
                                strDeepSourcePath = Path.Combine(@strSourcePath, strDeepFileName);
                                strDeepTargetPath = Path.Combine(@strTargetPath, strDeepFileName);
                                Directory.CreateDirectory(@strDeepTargetPath);
                                //遞回
                                CopyDir(@strDeepSourcePath, @strDeepTargetPath, strFilters, bDeepClone);
                            }
                        }
                        //複製檔案
                        string strDestFile = Path.Combine(strTargetPath, strFileName);
                        File.Copy(@strFile, strDestFile, true);
                    }
                }
                else
                {
                    XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), "複製資料夾來源不存在。");
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Vision, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
        }

        /// <summary>指定來源和目標資料夾的位置，複製來源資料夾所有檔案至目標資料夾。。</summary>
        /// <param name="strSourcePath">來源資料夾。</param>
        /// <param name="strTargetPath">目標資料夾。</param>
        /// <returns>回傳是否成功，成功為 True ，失敗為 False 。</returns>
        public static bool CopyDir(string strSourcePath, string strTargetPath)
        {
            try
            {
                #region 判斷來源資料夾是否存在:不存在return false
                if (!Directory.Exists(strSourcePath))
                {
                    MessageBox.Show("資料夾不存在");
                    return false;
                }
                #endregion

                #region 判斷目的資料夾是否存在:若不存在，建立一個@
                if (!Directory.Exists(strTargetPath))
                {
                    Directory.CreateDirectory(strTargetPath);
                }
                #endregion

                #region 目的路徑最後若沒有"\\"，要補上
                if (strTargetPath[strTargetPath.Length - 1] != Path.DirectorySeparatorChar)
                {
                    strTargetPath = strTargetPath + Path.DirectorySeparatorChar;
                }
                #endregion

                #region 複製檔案到指定路徑
                string[] strFileList = Directory.GetFileSystemEntries(strSourcePath); // 取得來源資料夾內所有檔案的路徑
                foreach (string strFile in strFileList)
                {
                    // 若目前檔案路徑為資料夾，要再往下層搜尋檔案並複製
                    if (Directory.Exists(strFile))
                    {
                        CopyDir(strFile, strTargetPath + Path.GetFileName(strFile)); // (遞迴-往子資料夾處理)
                    }
                    else
                    {
                        File.Copy(strFile, strTargetPath + Path.GetFileName(strFile), true); // 複製檔案到指定路徑
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return false;
            }

            return true;
        }

        /// <summary>複製檔案，如果目標路徑資料夾不存在則建立。</summary>
        /// <param name="strSourcePath">來源檔案。</param>
        /// <param name="strTargetPath">目標檔案。</param>
        /// <param name="bOverWrite">如果目標檔案存在是否複寫（預設： true ）。</param>
        /// <returns>複製是否成功。</returns>
        public static bool CopyFile(string strSourcePath, string strTargetPath, bool bOverWrite = true)
        {
            try
            {

                if (!File.Exists(strSourcePath))
                {
                    MessageBox.Show("檔案不存在");
                    return false;
                }

                if (!Directory.Exists(Path.GetDirectoryName(strTargetPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(strTargetPath));
                }

                File.Copy(strSourcePath, strTargetPath, bOverWrite); // 複製檔案到指定路徑

            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return false;
            }

            return true;
        }

		/// <summary>取得最後一層資料夾名稱，不包含路徑部份。</summary>
		/// <param name="strSourcePath">原始路徑。</param>
		/// <returns>資料夾名稱。</returns>
		public static string GetFolderNameWithoutPath(string strSourcePath)
		{
			string strFolderName = "";
			try
			{
				strFolderName = Path.GetFileName(Path.GetDirectoryName(strSourcePath));
			}
			catch (Exception ex)
			{
				XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
				return "";
			}

			return strFolderName;
		}

        #endregion

    }

    /// <summary>用於檔案加解密(DEC加解密) 只要是檔案都可以加密解密 (文字、圖片 ... )。</summary>
    /// <example>
    /// <code>
    ///    private void Example()
    ///    {
    ///        // =====================================
    ///        // 檔案加密 
    ///        // =====================================
    ///        // 先寫出一份檔案
    ///        string strFilename = @"D:\MyData.txt";
    ///        StreamWriter clsWrite = new StreamWriter(strFilename, false, Encoding.Default);
    ///        List &#60;string> strDatas = new List &#60;string>();
    ///
    ///        strDatas.Add("這是測試");
    ///        strDatas.Add("你要加密的檔案可以是文字檔也可以是圖片");
    ///
    ///        foreach (string strLine in strDatas)
    ///        {
    ///            clsWrite.WriteLine(strLine);
    ///        }
    ///        clsWrite.Close();
    ///
    ///        // 你可以把硬碟的指定檔案加密後存到另外一個檔案去
    ///        XFileSecurity.EncryptFile(strFilename, @"D:\test.tmp");
    ///        // 或者把原本的檔案讀進來並且加密後存回去
    ///        XFileSecurity.EncryptFile(strFilename);
    ///
    ///        // =====================================
    ///        // 檔案解密 
    ///        // =====================================
    ///
    ///        // 如果有使用未加識別碼的編碼檔案可以先加上識別碼再進行解碼 (過時)
    ///        XFileSecurity.ConvertToNewEncryptFile(strFilename);
    ///        // 檔案解碼 來源一定要是加密檔案，否則不予處理
    ///        XFileSecurity.DecryptFile(strFilename);
    ///        // 檔案解碼 也可以寫到其他的檔案去
    ///        XFileSecurity.DecryptFile(strFilename, @"D:\MyData.txt");
    ///    }
    /// </code>
    /// </example>
    public class XFileSecurity
    {
        /// <summary> 加密鑰匙，至少要八個字元以上才可以 </summary>
        private static string g_strKey = "!aoi2013";

        /// <summary> 對稱演算法的初始化向量，至少要八個字元以上才可以</summary>
        private static string g_strInitialVector = "!aoi2013";

        private static byte[] g_bytePassword = new byte[] { 0x19, 0x86, 0x03, 0x16 };

        /// <summary> 對檔案進行加密動作 </summary>
        /// <param name="strSrcFilePath">來源檔案名稱</param>
        /// <param name="strEncryptFilePath">加密檔案名稱</param>
        /// <returns>true:加密成功 false:加密失敗或者此檔案已經加密過了</returns>
        public static bool EncryptFile(string strSrcFilePath, string strEncryptFilePath, string strKey = "")
        {
            bool bResult = true;

            if (strKey == "")
                strKey = g_strKey;

            if (string.IsNullOrEmpty(strSrcFilePath) || string.IsNullOrEmpty(strEncryptFilePath))
            {
                return false;
            }
            if (!File.Exists(strSrcFilePath))
            {
                return false;
            }
            if (IsEncryptFile(strSrcFilePath))
            {
                return false;
            }

            DESCryptoServiceProvider clsDesService = new DESCryptoServiceProvider();
            byte[] byteKey = Encoding.ASCII.GetBytes(strKey);
            byte[] byteIv = Encoding.ASCII.GetBytes(g_strInitialVector);

            clsDesService.Key = byteKey;
            clsDesService.IV = byteIv;

            try
            {
                using (FileStream clsSrcStream = new FileStream(strSrcFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream clsTagStream = new FileStream(strEncryptFilePath, FileMode.Create, FileAccess.Write))
                    {
                        //檔案加密
                        byte[] byteDataArray = new byte[clsSrcStream.Length];
                        clsSrcStream.Read(byteDataArray, 0, byteDataArray.Length);

                        using (CryptoStream clsStream = new CryptoStream(clsTagStream, clsDesService.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            clsStream.Write(byteDataArray, 0, byteDataArray.Length);
                            clsStream.FlushFinalBlock();
                        }
                    }
                }

                // 加入 Password 識別碼
                AddPassword(strEncryptFilePath);
            }
            catch (System.Exception ex)
            {
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return bResult;
        }

        /// <summary> 對檔案進行加密動作 </summary>
        /// <param name="strFilePath">檔案名稱</param>
        /// <returns>ture:加密成功  false:加密失敗或者此檔案已經是加密的檔案了</returns>
        public static bool EncryptFile(string strFilePath, string strKey = "")
        {
            bool bResult = true;

            if (strKey == "")
                strKey = g_strKey;

            if (string.IsNullOrEmpty(strFilePath))
            {
                return false;
            }
            if (!File.Exists(strFilePath))
            {
                return false;
            }
            if (IsEncryptFile(strFilePath))
            {
                return false;
            }

            // 暫存檔路徑
            string strTmpPath = Path.GetTempFileName();

            try
            {
                // 複製成站存檔
                File.Copy(strFilePath, strTmpPath, true);
                // 將屬性設為隱藏
                File.SetAttributes(strTmpPath, FileAttributes.Hidden);
                // 檔案加密
                EncryptFile(strTmpPath, strFilePath, strKey);
                // 刪除暫存檔
                File.Delete(strTmpPath);
            }
            catch (System.Exception ex)
            {
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return bResult;
        }


        /// <summary> 檔案解碼，要注意你用的 Key 與 IV 是否正確，如果不正確的話是無法解密的</summary>
        /// <param name="strEncryptFilePath">加碼檔案名稱</param>
        /// <param name="strDecryptFilePath">解碼檔案名稱</param>
        /// <returns>true:解碼成功   false:解碼失敗、者檔案不存在或此檔案不是加密檔</returns>
        public static bool DecryptFile(string strEncryptFilePath, string strDecryptFilePath, string strKey = "")
        {
            bool bResult = true;

            if (strKey == "")
                strKey = g_strKey;

            if (string.IsNullOrEmpty(strEncryptFilePath) || string.IsNullOrEmpty(strDecryptFilePath))
            {
                return false;
            }
            if (!File.Exists(strEncryptFilePath))
            {
                return false;
            }

            if (!IsEncryptFile(strEncryptFilePath))
            {
                return false;
            }

            // 移除 Password
            //RemovePassword(strEncryptFilePath);

            DESCryptoServiceProvider clsDesService = new DESCryptoServiceProvider();
            byte[] byteKey = Encoding.ASCII.GetBytes(strKey);
            byte[] byteInitialVector = Encoding.ASCII.GetBytes(g_strInitialVector);

            clsDesService.Key = byteKey;
            clsDesService.IV = byteInitialVector;

            try
            {


                using (FileStream clsSrcStream = new FileStream(strEncryptFilePath, FileMode.Open, FileAccess.Read))
                using (FileStream clsTagStream = new FileStream(strDecryptFilePath, FileMode.Create, FileAccess.Write))
                {

                    byte[] byteDataArray = new byte[clsSrcStream.Length];
                    clsSrcStream.Read(byteDataArray, 0, byteDataArray.Length - 4);
                    using (CryptoStream clsStream = new CryptoStream(clsTagStream, clsDesService.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        clsStream.Write(byteDataArray, 0, byteDataArray.Length -4);
                        clsStream.FlushFinalBlock();
                    }
                }
            }
            catch (System.Exception ex)
            {
                bResult = false ;
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return bResult;
        }

        /// <summary> 對檔案進行解密動作 </summary>
        /// <param name="strFilePath">檔案名稱</param>
        /// <returns>true:解密成功   false:解碼失敗、者檔案不存在或此檔案不是加密檔</returns>
        public static bool DecryptFile(string strFilePath, string strKey = "")
        {
            bool bResult = true;

            if (strKey == "")
                strKey = g_strKey;

            if (string.IsNullOrEmpty(strFilePath))
            {
                return false;
            }
            if (!File.Exists(strFilePath))
            {
                return false;
            }
            if (!IsEncryptFile(strFilePath))
            {
                return false;
            }

            // 暫存檔路徑
            string strTmpPath = Path.GetTempFileName();

            try
            {
                // 複製成暫存檔
                File.Copy(strFilePath, strTmpPath, true);
                // 檔案解密
                DecryptFile(strTmpPath, strFilePath, strKey);
                // 將屬性設為隱藏
                File.SetAttributes(strTmpPath, FileAttributes.Hidden);
                // 刪除暫存檔
                XFile.Delete(strTmpPath);
            }
            catch (Exception ex)
            {
               bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return bResult;
        }

        /// <summary>
        /// 加入有加密的識別碼
        /// </summary>
        /// <param name="strFilePath">檔案路徑</param>
        private static void AddPassword(string strFilePath)
        {
            using (FileStream clsSrcStream = new FileStream(strFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                clsSrcStream.Seek(clsSrcStream.Length, 0);
                clsSrcStream.Write(g_bytePassword, 0, g_bytePassword.Length);
            }
        }

        /// <summary>
        /// 移除最後一行的識別碼
        /// </summary>
        /// <param name="strFilePath">檔案路徑</param>
        private static void RemovePassword(string strFilePath)
        {
            byte[] byteDataArray;

            using (FileStream clsSrcStream = new FileStream(strFilePath, FileMode.Open, FileAccess.Read))
            {
                byteDataArray = new byte[clsSrcStream.Length];
                clsSrcStream.Read(byteDataArray, 0, byteDataArray.Length);
            }

            using (FileStream clsSrcStream = new FileStream(strFilePath, FileMode.Create, FileAccess.Write))
            {
                clsSrcStream.Write(byteDataArray, 0, byteDataArray.Length - 4);
            }
        }

        /// <summary>
        /// 判斷這個檔案是否有加密
        /// </summary>
        /// <param name="strFilePath">檔案名稱</param>
        /// <returns>true:此為加密檔   false:不是加密檔或檔案不存在</returns>
        public static bool IsEncryptFile(string strFilePath)
        {
            if (!File.Exists(strFilePath))
            {
                return false;
            }

            bool bIsEncrypt = true;
            using (FileStream clsSrcStream = new FileStream(strFilePath, FileMode.Open, FileAccess.Read))
            {

                byte[] byteDataArray = new byte[clsSrcStream.Length];
                clsSrcStream.Read(byteDataArray, 0, byteDataArray.Length);

                if (byteDataArray.Count() - 4 < 0)
                {
                    return false;
                }

                for (int i = 0; i < g_bytePassword.Count(); i++)
                {
                    if (byteDataArray[byteDataArray.Length - 4 + i] != g_bytePassword[i])
                    {
                        bIsEncrypt = false;
                        break;
                    }
                }
            }

            return bIsEncrypt;
        }

        /// <summary>
        /// 將有加密但是沒有加入識別碼的加密檔案添加識別碼
        /// </summary>
        /// <param name="strFilePath">檔案路徑</param>
        /// <returns>true:轉換成功 false:轉換失敗，或者本檔案本來就已經包含識別碼</returns>
        public static bool ConvertToNewEncryptFile(string strFilePath)
        {
            if (IsEncryptFile(strFilePath))
            {
                return false;
            }
            else
            {
                AddPassword(strFilePath);
                return true;
            }
        }

        #region " Example "

        private void Example()
        {
            // =====================================
            // 檔案加密 
            // =====================================
            // 先寫出一份檔案
            string strFilename = @"D:\MyData.txt";
            StreamWriter clsWrite = new StreamWriter(strFilename, false, Encoding.Default);
            List<string> strDatas = new List<string>();

            strDatas.Add("這是測試");
            strDatas.Add("你要加密的檔案可以是文字檔也可以是圖片");

            foreach (string strLine in strDatas)
            {
                clsWrite.WriteLine(strLine);
            }
            clsWrite.Close();

            // 你可以把硬碟的指定檔案加密後存到另外一個檔案去
            XFileSecurity.EncryptFile(strFilename, @"D:\test.tmp");
            // 或者把原本的檔案讀進來並且加密後存回去
            XFileSecurity.EncryptFile(strFilename);

            // =====================================
            // 檔案解密 
            // =====================================

            // 如果有使用未加識別碼的編碼檔案可以先加上識別碼再進行解碼 (過時)
            XFileSecurity.ConvertToNewEncryptFile(strFilename);
            // 檔案解碼 來源一定要是加密檔案，否則不予處理
            XFileSecurity.DecryptFile(strFilename);
            // 檔案解碼 也可以寫到其他的檔案去
            XFileSecurity.DecryptFile(strFilename, @"D:\MyData.txt");
        }

        #endregion

    }

    /// <summary> 標準系統設定檔工具，寫出的格式將為
    /// <para>[Sation]</para>
    /// <para>Key = Value</para>
    /// </summary>
    /// <example>
    /// <code>
    /// private void Example()
    /// {
    ///     // Example by Frank Jian.
    ///
    ///     // 建立時要給路徑,在此模擬寫入資料到ini檔案中。
    ///     XIni clsIni = new XIni(@"D:\123.ini");
    ///     clsIni.Write("Section1", "Key", "26");
    ///     clsIni.Write("Section1", "a~y", "20");
    ///     clsIni.Write("Section2", "a~y", "25");
    ///
    ///     // 建立時要給路徑,在此模擬讀取ini檔案中的資料。
    ///     XIni clsIni2 = new XIni(@"D:\123.ini");
    ///     string value = clsIni2.Read("Section1", "Key");
    /// }
    /// </code>
    /// </example>
    public class XIni
    {

        #region " Property "

        /// <summary> 取得，設定檔路徑 </summary>
        public string IniPath { get { return g_strIniPath; } }
        private string g_strIniPath;

        #endregion

        #region " Operation "

        [DllImport("kernel32")]			// Ini 檔案的撰寫
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]			// Ini 檔案的讀取
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]			// Ini 檔案的讀取 (整數)
        private static extern int GetPrivateProfileInt(string section, string key, int def, string filePath);

        #endregion

        #region " Methods - New"

        public XIni(string strIniPath)
        {
            g_strIniPath = strIniPath;
        }

        #endregion

        #region " Methods - Write "

        /// <summary> 撰寫INI文件中的特定子區塊 </summary> 
        public void Write(string strSection, string strKey, string strValue)
        {
            WritePrivateProfileString(strSection, strKey, strValue, g_strIniPath);
        }

        /// <summary>撰寫INI文件中的特定子區塊 (整數)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="iValue">整數值</param>
        public void WriteInteger(string strSection, string strKey, int iValue)
        {
            string strValue = iValue.ToString();
            WritePrivateProfileString(strSection, strKey, strValue, g_strIniPath);
        }

        /// <summary>撰寫INI文件中的特定子區塊 (浮點數)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="fValue">浮點數值</param>
        public void WriteFloat(string strSection, string strKey, float fValue)
        {
            string strValue = fValue.ToString();
            WritePrivateProfileString(strSection, strKey, strValue, g_strIniPath);
        }

        public void WriteFloat(string strSection, string strKey, double dValue)
        {
            string strValue = dValue.ToString();
            WritePrivateProfileString(strSection, strKey, strValue, g_strIniPath);
        }

        /// <summary>撰寫INI文件中的特定子區塊 (浮點數)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="dValue">浮點數值</param>
        public void WriteDouble(string strSection, string strKey, double dValue)
        {
            string strValue = dValue.ToString();
            WritePrivateProfileString(strSection, strKey, strValue, g_strIniPath);

        }

        /// <summary>撰寫INI文件中的特定子區塊 (布林值)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="bValue">布林值</param>
        public void WriteBoolean(string strSection, string strKey, bool bValue)
        {
            if (bValue)
            {
                string sValue = "True";
                WritePrivateProfileString(strSection, strKey, sValue, g_strIniPath);
            }
            else
            {
                string sValue = "False";
                WritePrivateProfileString(strSection, strKey, sValue, g_strIniPath);
            }
        }

        /// <summary>撰寫INI文件中的特定子區塊 (字串)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="strValue">字串</param>
        public void WriteString(string strSection, string strKey, string strValue)
        {
            Write(strSection, strKey, strValue);
        }

        #endregion

        #region " Methods - Read "

        /// <summary> 讀取INI文件中的特定子區塊 </summary> 
        /// <param name="strSection">項目名稱(如 [TypeName] )</param> 
        /// <param name="strKey">資料群</param> 
        /// <param name="iSize">字串序列的長度 (預設 500)</param> 
        /// <returns>回傳"="後面的字串</returns> 
        public string Read(string strSection, string strKey, int iSize = 500)
        {
            StringBuilder receive = new StringBuilder(iSize);
            int i = GetPrivateProfileString(strSection, strKey, "", receive, iSize, g_strIniPath);
            return receive.ToString();
        }

        /// <summary>讀取INI文件中的特定子區塊 (整數)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="iDefaultValue">預設值</param>
        /// <returns>回傳"="後面的整數</returns>
        public int ReadInteger(string strSection, string strKey, int iDefaultValue)
        {
            int iResult = GetPrivateProfileInt(strSection, strKey, iDefaultValue, g_strIniPath);
            return iResult;
        }

        /// <summary>讀取INI文件中的特定子區塊 (浮點數)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="fDefaultValue">預設值</param>
        /// <returns>回傳"="後面的浮點數</returns>
        public float ReadFloat(string strSection, string strKey, float fDefaultValue, int iSize = 500)
        {
            float fResult = 0;
            StringBuilder sResult = new StringBuilder(iSize);
            string sDefault = fDefaultValue + "";
            GetPrivateProfileString(strSection, strKey, sDefault, sResult, iSize, g_strIniPath);
            if (!float.TryParse(sResult.ToString(), out fResult))
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), "轉換 float 失敗！");
            }
            return fResult;
        }

        public float ReadFloat(string strSection, string strKey, double dDefaultValue, int iSize = 500)
        {
            float fResult = 0;
            StringBuilder sResult = new StringBuilder(iSize);
            string sDefault = dDefaultValue + "";
            GetPrivateProfileString(strSection, strKey, sDefault, sResult, iSize, g_strIniPath);
            if (!float.TryParse(sResult.ToString(), out fResult))
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), "轉換 float 失敗！");
            }
            return fResult;
        }

        /// <summary>讀取INI文件中的特定子區塊 (浮點數)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="dDefaultValue">預設值</param>
        /// <returns>回傳"="後面的浮點數</returns>
        public double ReadDouble(string strSection, string strKey, double dDefaultValue, int iSize = 500)
        {
            double dResult = 0;
            StringBuilder sResult = new StringBuilder(iSize);
            string sDefault = dDefaultValue + "";
            GetPrivateProfileString(strSection, strKey, sDefault, sResult, iSize, g_strIniPath);
            if (!double.TryParse(sResult.ToString(), out dResult))
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), "轉換 double 失敗！");
            }
            return dResult;
        }


        /// <summary>讀取INI文件中的特定子區塊 (浮點數)</summary>
        /// <param name="sSection">項目名稱(如 [TypeName] )</param>
        /// <param name="sKey">資料群</param>
        /// <param name="bDefaultValue">預設值</param>
        /// <returns>回傳"="後面的True/False</returns>
        public bool ReadBoolean(string sSection, string sKey, bool bDefaultValue, int iSize = 500)
        {
            bool bResult;
            StringBuilder sResult = new StringBuilder(iSize);
            if (bDefaultValue)
            {
                string sDefault = "True";
                GetPrivateProfileString(sSection, sKey, sDefault, sResult, iSize, g_strIniPath);
            }
            else
            {
                string sDefault = "False";
                GetPrivateProfileString(sSection, sKey, sDefault, sResult, iSize, g_strIniPath);
            }

            if (sResult.ToString().ToUpper() == "TRUE" || sResult.ToString() == "1")
            {
                bResult = true;
            }
            else
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>讀取INI文件中的特定子區塊 (字串)</summary>
        /// <param name="strSection">項目名稱(如 [TypeName] )</param>
        /// <param name="strKey">資料群</param>
        /// <param name="strDefaultValue">預設值</param>
        /// <returns>回傳"="後面的字串</returns>
        public string ReadString(string strSection, string strKey, string strDefaultValue, int iSize = 500)
        {
            StringBuilder strResult = new StringBuilder(iSize);
            GetPrivateProfileString(strSection, strKey, strDefaultValue, strResult, iSize, g_strIniPath);
            return strResult.ToString();
        }

        #endregion

        #region " Methods - Delete "

        /// <summary>刪除INI文件中的特定的子區塊</summary>
        /// <param name="strSection">項目名稱([XXXX])</param>
        /// <param name="strKey">資料群</param>
        public void DeleteKey(string strSection, string strKey)
        {
            WritePrivateProfileString(strSection, strKey, null, g_strIniPath);
        }

        /// <summary>刪除INI文件中的特定的區塊</summary>
        /// <param name="strSection">項目名稱([XXXX])</param>
        public void DeleteSection(string strSection)
        {
            WritePrivateProfileString(strSection, null, null, g_strIniPath);
        }

        #endregion

        #region " Example "

        private void Example()
        {
            // Example by Frank Jian.

            // 建立時要給路徑,在此模擬寫入資料到ini檔案中。
            XIni clsIni = new XIni(@"D:\123.ini");
            clsIni.Write("Section1", "Key", "26");
            clsIni.Write("Section1", "a~y", "20");
            clsIni.Write("Section2", "a~y", "25");

            // 建立時要給路徑,在此模擬讀取ini檔案中的資料。
            XIni clsIni2 = new XIni(@"D:\123.ini");
            string value = clsIni2.Read("Section1", "Key");
        }

        #endregion

    }

    /// <summary>序列化/反序列化</summary>
    public class XmlSerialization
    {
        /// <summary>序列化資料轉成 Xml 格式。</summary>
        /// <typeparam name="T">型別，可能為自訂的 Class or 其他基本型別。</typeparam>
        /// <param name="strPath">檔案路徑。</param>
        /// <param name="clsData">物件資料。</param>
        /// <returns>執行結果狀態碼 XErrorCode 。</returns>
        public static bool Serialize<T>(string strPath, T clsData)
        {
            bool bResult = true;

            XmlWriterSettings clsXmlSettings = null;
            XmlWriter clsXmlWriter = null;
            XmlSerializer clsXmlSerializer = null;
            XmlSerializerNamespaces clsXmlSerializerNamespaces = null;
            try
            {
                clsXmlSettings = new XmlWriterSettings();
                clsXmlSettings.NewLineHandling = NewLineHandling.None;
                clsXmlSettings.Indent = true;

                using (clsXmlWriter = XmlWriter.Create(strPath, clsXmlSettings))
                {
                    clsXmlSerializer = new XmlSerializer(clsData.GetType());
                    clsXmlSerializerNamespaces = new XmlSerializerNamespaces();
                    clsXmlSerializerNamespaces.Add(string.Empty, string.Empty);
                    clsXmlSerializer.Serialize(clsXmlWriter, clsData, clsXmlSerializerNamespaces);
                    clsXmlWriter.Close();
                }
            }
            catch (Exception ex)
            {
                if (clsXmlWriter != null) clsXmlWriter.Close();
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            finally
            {
                clsXmlSettings = null;
                clsXmlWriter = null;
                clsXmlSerializer = null;
                clsXmlSerializerNamespaces = null;
            }
            return bResult;
        }

        /// <summary>反序列化 Xml 文件中的資料轉成物件資料。。</summary>
        /// <typeparam name="T">型別，可能為自訂的 Class or 其他基本型別。</typeparam>
        /// <param name="strPath">檔案路徑。</param>
        /// <param name="clsData">物件資料。</param>
        /// <returns>執行結果狀態碼 XErrorCode 。</returns>
        public static bool Deserialize<T>(string strPath, ref T clsData)
        {
            bool bResult = true;

            object clsTempData = null;
            MemoryStream clsMemoryStream = null;
            XmlSerializer clsXmlSerializer = null;
            XmlReader clsXmlReader = null;
            UTF8Encoding clsEncoding = null;
            try
            {
                clsEncoding = new UTF8Encoding();
                using (clsMemoryStream = new MemoryStream(clsEncoding.GetBytes(File.ReadAllText(strPath))))
                {
                    clsXmlSerializer = new XmlSerializer(typeof(T));
                    clsTempData = (T)clsXmlSerializer.Deserialize(clsMemoryStream);
                    if (clsTempData != null)
                    {
                        clsData = (T)clsTempData;
                    }
                    else
                    {
                        clsData = default(T);
                    }
                    clsMemoryStream.Close();
                }
            }
            catch (Exception ex)
            {
                if (clsXmlReader != null)
                {
                    clsXmlReader.Close();
                }

                if (clsMemoryStream != null)
                {
                    clsMemoryStream = null;
                }
                bResult = false;
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            finally
            {
                clsXmlSerializer = null;
                clsXmlReader = null;
                clsMemoryStream = null;
            }
            return bResult;
        }
    }

    /// <summary>
    /// 用來紀錄你要得資訊內容，紀錄方式可以依照時間作切割分檔
    /// </summary>
    /// <example>
    /// <code>
    /// public void Example()
    /// {
    ///     // Example by Frank Jian.
    ///     // 寫入資料夾，檔案名稱，未來檔案會以Day, Hour or Minute分成不同檔案名稱去做存檔的動作。
    ///     XLog clsTestLog = new XLog(@"D:\要記錄的資料夾位置", "給一個檔案名稱.txt", XLog.LogRecordGroup.Minute, "Vision的記錄檔");
    ///
    ///     // 可以重新設定存檔的型態，看是要一個小時存一個檔案還是一天、一分鐘，如果選none就是不分檔案，會全部存在同一個，小心爆掉!!
    ///     clsTestLog.LogRecordType = LogRecordGroup.Hours;
    ///
    ///     // 可以設定紀錄內容的時間格式! 設定的方式有下面這幾類型。
    ///     clsTestLog.SetDateFormat();
    ///     clsTestLog.SetDateFormat("MMddHHmmss");
    ///     clsTestLog.SetDateFormat("", false, false, true, true, true, false, false);
    ///     clsTestLog.SetDateFormat("_", false, false, true, true, false, false, false);
    ///
    ///     // 要記錄單行的時候~
    ///     clsTestLog.WriteLog("單行就是直接放一個string就好");
    ///
    ///     // 要紀錄附帶自訂格式變數的字串時
    ///     int iA = 3, iB = 2;
    ///     clsTestLog.WriteLog("{0} = {1} + {2}", iA + iB, iA, iB);
    ///
    ///     // 要記錄多行的時候~ (多行的話必須是一個 Array,&#60; List > 的型式)
    ///     List &#60;string> strLog = new List &#60;string>();
    ///     strLog.Add(DateTime.Now.Millisecond.ToString());
    ///     strLog.Add(DateTime.Now.Millisecond.ToString());
    ///     strLog.Add(DateTime.Now.Date.ToString());
    ///
    ///     clsTestLog.WriteLog(strLog.ToArray());
    /// }
    /// </code>
    /// </example>
    public class XLog
    {
        #region" Definities "

        /// <summary>Log 紀錄方式。</summary>
        public enum LogRecordType
        {
            /// <summary>以分為單位切割檔案。</summary>
            Minute,
            /// <summary>以小時為單位切割檔案。</summary>
            Hour,
            /// <summary>以天為單位切割檔案。</summary>
            Day,
        }

        #endregion

        #region " Properties "

        /// <summary>取得或設定，Log紀錄方式。</summary>
        public LogRecordType eLogRecordType
        {
            get { return g_eLogRecordType; }
            set { g_eLogRecordType = value; }
        }
        private LogRecordType g_eLogRecordType;

        //private string g_strArrayProString;

        /// <summary>取得或設定，儲存資料夾路徑。</summary>
        public string Folder
        {
            get { return g_strLogFolder; }
            set
            {
                g_strLogFolder = value;
                if (!XFile.IsExist_Folder(g_strLogFolder))
                {
                    XFile.CreateFolder(g_strLogFolder);
                }
            }
        }
        private string g_strLogFolder;

        /// <summary>取得或設定，檔案名稱。</summary>
        public string FileName
        {
            get { return g_strLogFileName; }
            set { g_strLogFileName = value; }
        }
        private string g_strLogFileName;

        /// <summary> 取得或設定，物件描述 </summary>
        public string Description
        {
            get { return g_strDescription; }
            set { g_strDescription = value; }
        }
        private string g_strDescription;

        private string g_strDateFormat = "yyyy/MM/dd HH:mm:ss.fff";

        private string g_strExecutionName = "";

        private string g_strWindowName = "";

        private uint g_uiDeviceID = 0;

        #endregion

        #region " Methods - New "

        /// <summary>
        /// XLog建構子。
        /// </summary>
        /// <param name="strFolder">存檔的完整路徑。</param>
        /// <param name="strFileName">存檔的檔名。</param>
        /// <param name="eLogRecordType">存檔的區間。</param>
        /// <param name="strDestript">Log的描述。</param>
        public XLog(string strFolder, string strFileName, LogRecordType eLogRecordType)
        {
            LogFolderSetup(strFolder, strFileName);
            g_eLogRecordType = eLogRecordType;
        }

        #endregion

        #region " Methods - Write Log "

        /// <summary>設定 Log 資料夾路徑與檔名。</summary>
        /// <param name="strFolder">資料夾路徑，不存在時會自動建立。</param>
        /// <param name="strFileName">檔名。</param>
        public void LogFolderSetup(string strFolder, string strFileName)
        {
            strFolder = (strFolder + @"\").Replace(@"\\", @"\");    // 自動在後面的字元加入"\"的字元
            g_strLogFolder = strFolder;

            if (!XFile.IsExist_Folder(g_strLogFolder)) XFile.CreateFolder(g_strLogFolder);

            g_strLogFileName = strFileName;
        }

        /// <summary>寫出 Log 。</summary>
        /// <param name="strMessage">資訊。</param>
        public void WriteLog(string strMessage)
        {
            string[] strMessages = new string[1] { strMessage };
            WriteLog(strMessages);
        }

        /// <summary>寫出 Log 。</summary>
        /// <param name="strFormat">寫出格式。</param>
        /// <param name="objPrarams">變數。</param>
        public void WriteLog(string strFormat, params object[] objPrarams)
        {
            string[] strMessages = new string[1] { string.Format(strFormat, objPrarams) };
            WriteLog(strMessages);
        }

        /// <summary>寫出 Log 。</summary>
        /// <param name="strMessage">陣列字串。</param>
        public void WriteLog(string[] strMessage)
        {
            string strDateString = DateTime.Now.ToString(g_strDateFormat) + "\t";
            int iMessageLenght = strMessage.Length;
            if (iMessageLenght > 0)
            {
                strMessage[0] = strDateString + strMessage[0];

                for (int iMessageLine = 1; iMessageLine < iMessageLenght; iMessageLine++)
                {
                    strMessage[iMessageLine] = strMessage[iMessageLine];
                }

                string strProFileName = string.Empty;
                switch (g_eLogRecordType)
                {
                    case LogRecordType.Day:
                        strProFileName = DateTime.Now.ToString("yyyyMMdd");
                        break;

                    case LogRecordType.Hour:
                        strProFileName = DateTime.Now.ToString("yyyyMMddHH");
                        break;

                    case LogRecordType.Minute:
                        strProFileName = DateTime.Now.ToString("yyyyMMddHHmm");
                        break;
                }
                XFile.AppendToFile(g_strLogFolder + strProFileName + g_strLogFileName, strMessage);
            }
        }

        public void SetDateFormat(string strSpiltChar, bool bMillisecond = true, bool bSecond = true, bool bMinute = true, bool bHour = true,
                                                    bool bDay = true, bool bMonth = true, bool bYear = true)
        {
            List<string> strDateFormat = new List<string>();
            if (bYear) strDateFormat.Add("yyyy");
            if (bMonth) strDateFormat.Add("MM");
            if (bDay) strDateFormat.Add("dd");
            if (bHour) strDateFormat.Add("HH");
            if (bMinute) strDateFormat.Add("mm");
            if (bSecond) strDateFormat.Add("ss");
            if (bMillisecond) strDateFormat.Add("fff");
            g_strDateFormat = String.Join(strSpiltChar, strDateFormat);
            g_strDateFormat = g_strDateFormat + "";
            SetDateFormat(g_strDateFormat);
        }

        #endregion

    }
}