using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace GeneralUtility.Net
{
    public class XFtp
    {
        private string FTPCONSTR = @"ftp://10.84.141.106:21";//FTP的服務器地址，格式為ftp://192.168.1.234:8021/。ip地址和端口換成自己的，這些建議寫在配置文檔中，方便修改
        private string FTPUSERNAME = "Root";//FTP服務器的用户名
        private string FTPPASSWORD = "Chieh";//FTP服務器的密碼

        private delegate void updateui(long rowCount, int i, ProgressBar PB);
        public void upui(long rowCount, int i, ProgressBar PB)
        {
            try
            {
                PB.Value = i;
            }
            catch { }
        }

        public XFtp(string strFTPPath, string strUserName, string strPassword)
        {
            FTPCONSTR = strFTPPath;
            FTPUSERNAME = strUserName;
            FTPPASSWORD = strPassword;
        }

        #region "Upload"

        /// <summary>上傳文檔到遠程ftp</summary>
        /// <param name="strPath">本地的檔案位置</param>
        /// <param name="strFilename">上傳文檔名稱</param>
        /// <returns></returns>
        public bool UploadFile(string strPath, string strFilename)
        {
            string erroinfo = "";
            FileInfo clsFileInfo = new FileInfo(strPath);
            strPath = FTPCONSTR + "/" + strFilename;//這個路徑是我要傳到ftp目錄下的這個目錄下
            FtpWebRequest ReqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(strPath));
            ReqFtp.UseBinary = true;
            ReqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
            ReqFtp.KeepAlive = false;
            ReqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            ReqFtp.ContentLength = clsFileInfo.Length;

            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = clsFileInfo.OpenRead();
            try
            {
                Stream strm = ReqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},無法完成上傳", ex.Message);
                return false;
            }
        }

        /// <summary>上傳</summary>
        /// <param name="path">本地的文檔目錄</param>
        /// <param name="name">文檔名稱</param>
        /// <param name="pb">進度條</param>
        /// <returns></returns>
        public bool UploadFile(string path, string strFilename, ProgressBar pb)
        {
            string erroinfo = "";
            float percent = 0;
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            path = FTPCONSTR + "/" + strFilename;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            int allbye = (int)f.Length;
            if (pb != null)
            {
                pb.Maximum = (int)allbye;
            }
            int startbye = 0;
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    startbye = contentLen + startbye;
                    if (pb != null)
                    {
                        pb.Value = (int)startbye;
                    }
                    contentLen = fs.Read(buff, 0, buffLength);
                    percent = (float)startbye / (float)allbye * 100;
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},無法完成上傳", ex.Message);
                return false;
            }
        }

        public bool UploadFolder(string strLocalFolder, string strFtpFolder)
        {
            if (!GeneralUtility.IO.XFile.IsExist_Folder(strLocalFolder)) return false;

            XFTPDirectoryDetails clsXFDD = FtpGetFileList(strFtpFolder);
            if (clsXFDD != null) FtpDeleteFolderDeep(strFtpFolder);
            if (FtpCreatFolder(strFtpFolder))
            {
                List<string> strFolders = GeneralUtility.IO.XFile.GetFoldersNameFromFolder(strLocalFolder);
                List<string> strFiles = GeneralUtility.IO.XFile.GetFileNamesPathFromFolder(strLocalFolder);
                foreach (string strFile in strFiles)
                {
                    UploadFile(strLocalFolder + "//" + strFile, strFtpFolder + "/" + strFile);
                }
                foreach (string strFolder in strFolders)
                {
                    UploadFolder(strLocalFolder + "//" + strFolder, strFtpFolder + "/" + strFolder);
                }
                return true;
            }
            return false;
        }

        #endregion

        #region "Download"

        ////上面的代碼實現了從ftp服務器下載文檔的功能
        public Stream Download(string strFtpFilePath)
        {
            Stream ftpStream = null;
            FtpWebResponse response = null;
            try
            {
                strFtpFilePath = strFtpFilePath.Replace("\\", "/");
                string strUrl = FTPCONSTR + strFtpFilePath;
                FtpWebRequest ReqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(strUrl));
                ReqFtp.UseBinary = true;
                ReqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                response = (FtpWebResponse)ReqFtp.GetResponse();
                ftpStream = response.GetResponseStream();
            }
            catch (Exception ex)
            {
                if (response != null) response.Close();
            }
            return ftpStream;
        }

        /// <summary>從ftp服務器下載文檔的功能</summary>
        /// <param name="ftpfilepath">ftp下載的地址</param>
        /// <param name="filePath">保存到本地的路徑名稱</param>
        /// <returns></returns>
        public bool Download(string ftpfilepath, string filePath, bool IsOverWrite = true)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    if (IsOverWrite) File.Delete(filePath);
                    else return false;
                }
                ftpfilepath = ftpfilepath.Replace("\\", "/");
                string url = FTPCONSTR + ftpfilepath;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                FileStream outputStream = new FileStream(filePath, FileMode.Create);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},無法下載", ex.Message);
                return false;
            }
        }

        /// <summary>從ftp服務器下載文檔的功能----帶進度條</summary>
        /// <param name="ftpfilepath">ftp下載的地址</param>
        /// <param name="filePath">保存本地的地址</param>
        /// <param name="fileName">保存的名字</param>
        /// <param name="pb">進度條引用</param>
        /// <returns></returns>
        public bool Download(string ftpfilepath, string filePath, ProgressBar pb)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            FileStream outputStream = null;
            try
            {
                filePath = filePath.Replace("我的電腦\\", "");
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch { }

                }
                ftpfilepath = ftpfilepath.Replace("\\", "/");
                string url = FTPCONSTR + ftpfilepath;
                reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                response = (FtpWebResponse)reqFtp.GetResponse();
                ftpStream = response.GetResponseStream();
                long cl = FtpGetFileSize(url);
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                outputStream = new FileStream(filePath, FileMode.Create);

                float percent = 0;
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                    percent = (float)outputStream.Length / (float)cl * 100;
                    if (percent <= 100) if (pb != null) pb.Invoke(new updateui(upui), new object[] { cl, (int)percent, pb });
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (reqFtp != null) reqFtp.Abort();
                if (response != null) response.Close();
                if (ftpStream != null) ftpStream.Close();
                if (outputStream != null) outputStream.Close();
            }
        }

        /// <summary>從ftp服務器下載整個資料夾功能</summary>
        /// <param name="ftpFolderPath">Ftp目錄</param>
        /// <param name="strLoaclPath">本地端</param>
        /// <returns></returns>
        public bool DownloadFolder(string ftpFolderPath, string strLoaclPath)
        {
            if (!GeneralUtility.IO.XFile.IsExist_Folder(strLoaclPath)) GeneralUtility.IO.XFile.CreateFolder(strLoaclPath);

            XFTPDirectoryDetails clsXFDD = FtpGetFileList(ftpFolderPath);
            if (clsXFDD != null)
            {
                foreach (string strFiles in clsXFDD.Files)
                {
                    Download(ftpFolderPath + "/" + strFiles, strLoaclPath + "//" + strFiles);
                }

                foreach (string strFolders in clsXFDD.Folders)
                {
                    DownloadFolder(ftpFolderPath + "/" + strFolders, strLoaclPath + "//" + strFolders);
                }
                return true;
            }
            return false;
        }

        #endregion

        #region "Function"

        /// <summary>獲得文檔大小</summary>
        /// <param name="url">FTP文檔的完全路徑</param>
        /// <returns></returns>
        public long FtpGetFileSize(string url)
        {

            long fileSize = 0;
            try
            {
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                reqFtp.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                fileSize = response.ContentLength;
                response.Close();
            }
            catch (Exception ex)
            {
            }
            return fileSize;
        }

        /// <summary>在ftp服務器上創建文檔目錄</summary>
        /// <param name="dirName">文檔目錄</param>
        /// <returns></returns>
        public bool FtpCreatFolder(string strFolderName)
        {
            try
            {
                if (FtpFolderExists(strFolderName)) return true;
                string url = FTPCONSTR + strFolderName;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        /// <summary>判斷ftp上的文檔目錄是否存在</summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool FtpFolderExists(string strFolderName)
        {
            strFolderName = FTPCONSTR + strFolderName;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(strFolderName));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
            reqFtp.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse resFtp = null;
            try
            {
                resFtp = (FtpWebResponse)reqFtp.GetResponse();
                FtpStatusCode code = resFtp.StatusCode;//OpeningData
                resFtp.Close();
                return true;
            }
            catch
            {
                if (resFtp != null)
                {
                    resFtp.Close();
                }
                return false;
            }
        }

        /// <summary>從ftp服務器刪除文檔的功能</summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool FtpDeleteFile(string fileName)
        {
            try
            {
                string url = FTPCONSTR + fileName;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.KeepAlive = false;
                reqFtp.Method = WebRequestMethods.Ftp.DeleteFile;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>從ftp服務器刪除目錄的功能</summary>
        /// <param name="strFolder"></param>
        /// <returns></returns>
        public bool FtpDeleteFolder(string strFolder)
        {
            try
            {
                string url = FTPCONSTR + strFolder;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.KeepAlive = false;
                reqFtp.Method = WebRequestMethods.Ftp.RemoveDirectory;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>取得目錄底下所有檔案列表</summary>
        /// <param name="strURL">目錄路徑</param>
        /// <returns>檔案列表</returns>
        public XFTPDirectoryDetails FtpGetFileList(string strURL)
        {
            XFTPDirectoryDetails clsXFDD = new XFTPDirectoryDetails();
            StringBuilder result = new StringBuilder();
            WebResponse response = null;
            StreamReader reader = null;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FTPCONSTR + strURL);
                request.UseBinary = true;
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                request.KeepAlive = false;
                request.UsePassive = false;
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    string[] strSubs = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strSubs.Length == 9)
                    {
                        if (strSubs[0].Substring(0, 1) == "d")
                            clsXFDD.Folders.Add(strSubs[8]);
                        else
                            clsXFDD.Files.Add(strSubs[8]);
                    }
                    line = reader.ReadLine();
                }
                return clsXFDD;
            }
            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                return null;
            }
        }

        /// <summary>從ftp服務器深度刪除目錄的功能</summary>
        /// <param name="strFolder"></param>
        /// <returns></returns>
        public bool FtpDeleteFolderDeep(string strFolder)
        {
            XFTPDirectoryDetails clsXFDD = FtpGetFileList(strFolder);

            if (clsXFDD != null)
            {
                foreach (string strFilePath in clsXFDD.Files)
                {
                    FtpDeleteFile(strFolder + "/" + strFilePath);
                }
                foreach (string strFolderPath in clsXFDD.Folders)
                {
                    FtpDeleteFolderDeep(strFolder + "/" + strFolderPath);
                }
                FtpDeleteFolder(strFolder);
                return true;
            }
            return false;
        }

        #endregion
    }

    public class XFTPDirectoryDetails
    {
        public List<string> Folders = new List<string>();
        public List<string> Files = new List<string>();
    }

    public class FtpFileInfo
    {
        public string Name;
        public double Side;
        public bool IsDir;
        public DateTime ModifiedTime;
    }

    public class FtpEventArgs : EventArgs
    {
        public int Progress { get; set; }
        public string ResultStatus { get; set; }
    }

    public class FtpClient
    {
        #region 'Definition'

        /// <summary> get / set Ftp Host ip </summary>
        public string Host
        {
            get { return _strHost; }
            set { _strHost = value; }
        }
        private string _strHost;

        /// <summary> get / set Ftp User Name </summary>
        public string User
        {
            get { return _strUser; }
            set { _strUser = value; }
        }
        private string _strUser;

        /// <summary> get / set Ftp User Password </summary>
        public string Pass
        {
            get { return _strPass; }
            set { _strPass = value; }
        }
        private string _strPass;

        /// <summary> the result status of progress </summary>
        protected string _strResultStatus;

        /// <summary> get / set the Total size for actions </summary>
        public double Total
        {
            get { return _dTotalSize; }
        }
        protected double _dTotalSize = 0;

        /// <summary> get / set the size was read by action </summary>
        public double Read
        {
            get { return _dReadSize; }
        }
        protected double _dReadSize = 0;

        /// <summary> the percentage of progress </summary>
        protected int _iProgress = 0;

        /// <summary> get bool : FTP是否在運行中 </summary>
        public bool Processing
        {
            get { return _bProcessing; }
        }
        protected bool _bProcessing = false;

        #endregion

        /// <summary> 初始化 FTPClient 類別的新執行個體 </summary>
        /// <param name="hostIP">ftp ip address</param>
        /// <param name="userName">ftp user name</param>
        /// <param name="password">ftp password</param>
        public FtpClient(string hostIP, string userName, string password)
        {
            Host = hostIP;
            User = userName;
            Pass = password;
        }

        /// <summary> 列出目標地址中的檔案及資料夾明細 </summary>
        /// <param name="folder_uri">ftp 目標地址 (完整絕對路徑)</param>
        /// <returns>string[] 檔案及資料夾明細</returns>
        /// 08-10-11  12:02PM       <DIR>          Version2
        /// 06-25-09  02:41PM            144700153 image34.gif
        public string[] ListDirectoryDetails(string folder_uri)
        {
            StringBuilder result = new StringBuilder();

            try
            {
                /* Create an FTP Request */
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(folder_uri);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(User, Pass);
                /* Specify the Type of FTP Request */
                ftpRequest.KeepAlive = false;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftpRequest.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);//中文檔名
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }

                // to remove the trailing '' '' 
                if (result.ToString() != "")
                {
                    result.Remove(result.ToString().LastIndexOf("\n"), 1);
                }
                reader.Close();
                response.Close();
                return (result.ToString() == "") ? new string[] { } : result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                return new string[] { };
            }
        }

        /// <summary> 以XFtpFileInfo class的方式列出目標地址中的所有檔案及資料夾明細 </summary>
        /// <param name="folder_uri">ftp 目標地址 (完整絕對路徑)</param>
        /// <returns>XFtpFileInfo[] 檔案及資料夾明細</returns>
        public FtpFileInfo[] ItemsDetail_InDirectory(string folder_uri)
        {
            List<FtpFileInfo> ObjectList = new List<FtpFileInfo>();

            string pattern = @"^(\d+-\d+-\d+\s+\d+:\d+(?:AM|PM))\s+(<DIR>|\d+)\s+(.+)$";
            Regex regex = new Regex(pattern);
            IFormatProvider culture = CultureInfo.GetCultureInfo("en-us");

            string[] strDirDetails = ListDirectoryDetails(folder_uri);
            foreach (string stritem in strDirDetails)
            {
                Match match = regex.Match(stritem);

                FtpFileInfo info = new FtpFileInfo();

                info.Name = match.Groups[3].Value;
                info.Side = (match.Groups[2].Value != "<DIR>") ? long.Parse(match.Groups[2].Value) : 0;
                info.IsDir = match.Groups[2].Value == "<DIR>";
                info.ModifiedTime = DateTime.ParseExact(match.Groups[1].Value, "MM-dd-yy  hh:mmtt", culture, DateTimeStyles.None);

                ObjectList.Add(info);
            }

            return ObjectList.ToArray();
        }

        /// <summary> 以XFtpFileInfo class的方式列出目標地址中的檔案明細 </summary>
        /// <param name="folder_uri">ftp 目標地址 (完整絕對路徑)</param>
        /// <returns>XFtpFileInfo[] 檔案明細</returns>
        public FtpFileInfo[] FilesDetail_InDirectory(string folder_uri)
        {
            List<FtpFileInfo> clsList = ItemsDetail_InDirectory(folder_uri).ToList();
            for (int i = clsList.Count - 1; i >= 0; i--)
            {
                if (clsList[i].IsDir)
                {
                    clsList.RemoveAt(i);
                }
            }
            return clsList.ToArray();
        }

        /// <summary> 以XFtpFileInfo class的方式列出目標地址中的資料夾明細 </summary>
        /// <param name="folder_uri">ftp 目標地址 (完整絕對路徑)</param>
        /// <returns>XFtpFileInfo[] 資料夾明細</returns>
        public FtpFileInfo[] DirsDetail_InDirectory(string folder_uri)
        {
            List<FtpFileInfo> clsList = ItemsDetail_InDirectory(folder_uri).ToList();
            for (int i = clsList.Count - 1; i >= 0; i--)
            {
                if (!clsList[i].IsDir)
                {
                    clsList.RemoveAt(i);
                }
            }
            return clsList.ToArray();
        }

        /// <summary> 列出目標地址中的所有檔案和資料夾名稱 </summary>
        /// <param name="folder_uri">ftp 目標地址 (完整絕對路徑)</param>
        /// <returns>string[] FilesName</returns>
        public string[] Items_Name_InDirectory(string folder_uri)
        {
            FtpFileInfo[] clsList = ItemsDetail_InDirectory(folder_uri);
            string[] strReturn = new string[clsList.Length];
            for (int i = 0; i < clsList.Length; i++)
            {
                strReturn[i] = clsList[i].Name;
            }
            return strReturn;
        }

        /// <summary> 列出目標地址中的所有檔案名稱 </summary>
        /// <param name="folder_uri">ftp 目標地址 (完整絕對路徑)</param>
        /// <returns>string[] FilesName</returns>
        public string[] FilesName_InDirectory(string folder_uri)
        {
            FtpFileInfo[] clsList = FilesDetail_InDirectory(folder_uri);
            string[] strReturn = new string[clsList.Length];
            for (int i = 0; i < clsList.Length; i++)
            {
                strReturn[i] = clsList[i].Name;
            }
            return strReturn;
        }

        /// <summary> 列出目標地址中的所有檔案名稱 </summary>
        /// <param name="folder_uri">ftp 目標地址 (完整絕對路徑)</param>
        /// <returns>string[] DirsName</returns>
        public string[] DirsName_InDirectory(string folder_uri)
        {
            FtpFileInfo[] clsList = DirsDetail_InDirectory(folder_uri);
            string[] strReturn = new string[clsList.Length];
            for (int i = 0; i < clsList.Length; i++)
            {
                strReturn[i] = clsList[i].Name;
            }
            return strReturn;
        }

        /// <summary> 在FTP伺服器中建立一個新的資料夾 (內部呼叫) </summary>
        /// <param name="Folder_uri">要建立的資料夾絕對路徑地址，包含資料夾名稱</param>
        /// <returns>true:資料夾建立 ; false:資料夾建立發生錯誤</returns>
        private bool createdirectory(string Folder_uri)
        {
            try
            {
                /* Create an FTP Request */
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(Folder_uri);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(User, Pass);
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                /* Establish Return Communication with the FTP Server */
                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                SetStatus(ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary> 在FTP伺服器中建立資料夾(可多層建立) </summary>
        /// <param name="Folder_uri">要建立的資料夾絕對路徑地址，包含資料夾名稱</param>
        /// <returns>true:資料夾建立 ; false:資料夾建立發生錯誤</returns>
        public bool CreateDirectory(string Folder_uri)
        {
            string strRelativeUri = Folder_uri.Replace(_strHost, "");
            string[] strUriPart = strRelativeUri.Trim('/').Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string strDirUriCreate = _strHost.Trim('/');
            bool result = true;

            foreach (string part in strUriPart)
            {
                strDirUriCreate += "/" + part;

                if (!IsDirectoryExist(strDirUriCreate))
                {
                    result = createdirectory(strDirUriCreate);
                }

                if (!result) break;
            }

            return result;
        }

        /// <summary> 刪除Ftp伺服器中的資料夾，包含子資料夾及檔案 </summary>
        /// <param name="Folder_uri">欲被刪除的資料夾絕對路徑地址</param>
        /// <returns>true:資料夾已經刪除或資料夾不存在; false:資料夾刪除過程發送錯誤</returns>
        public bool RemoveDirectory(string Folder_uri)
        {
            /* cheack 目錄是否存在 */
            if (!IsDirectoryExist(Folder_uri)) return true;  //如果不存在 返回

            /* cheack demo in uri */
            FtpFileInfo[] folder_items = ItemsDetail_InDirectory(Folder_uri);   // 取得目標資料夾內的所有物件名稱
            List<string> items_file_uri = new List<string>();
            List<string> items_folder_uri = new List<string>();

            if (folder_items.Length != 0)
            {
                foreach (FtpFileInfo Demo_name in folder_items)
                {
                    string Full_Uri = Folder_uri + "/" + Demo_name.Name;   // 物件的完整地址

                    if (Demo_name.IsDir)     // 物件是否為資料夾
                        items_folder_uri.Add(Full_Uri);
                    else
                        items_file_uri.Add(Full_Uri);
                }

                if (items_folder_uri.Count > 0)
                    foreach (string uri in items_folder_uri)        // 子資料夾遞迴
                        RemoveDirectory(uri);

                if (items_file_uri.Count > 0)                       // 檔案刪除
                    Delete_Files(items_file_uri.ToArray());
            }

            /* delete the empty Directory */
            try
            {
                /* Create an FTP Request */
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(Folder_uri);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(User, Pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                /* Establish Return Communication with the FTP Server */
                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                SetStatus(ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary> 刪除單一檔案 </summary>
        /// <param name="file_uri">欲刪除目標檔案的ftp完整地址，包含副檔名</param>
        public void Delete_File(string file_uri)
        {
            string[] Uris = new string[1] { file_uri };
            Delete_Files(Uris);
        }

        /// <summary> 刪除多個檔案 </summary>
        /// <param name="file_uri">包含所有要刪除檔案完整路徑的陣列,包含副檔名</param>
        public void Delete_Files(string[] file_uri)
        {
            try
            {
                foreach (string uri in file_uri)
                {
                    /* Create an FTP Request */
                    FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
                    /* Log in to the FTP Server with the User Name and Password Provided */
                    ftpRequest.Credentials = new NetworkCredential(User, Pass);
                    /* Specify the Type of FTP Request */
                    ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    /* Establish Return Communication with the FTP Server */
                    FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                    /* Resource Cleanup */
                    ftpResponse.Close();
                    ftpRequest = null;
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.ToString());
            }
            return;
        }

        /// <summary> 使用測試檔案大小來判斷是否為檔案 </summary>
        /// <param name="uri">FTP完整路徑</param>
        /// <returns>true: 可以取得檔案大小,判斷為檔案 ; false: 無法取得檔案大小,判斷為非檔案</returns>
        public bool IsFileExists(string uri)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(User, Pass);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
                return true;        // 如果連結錯誤也會回傳false
            }
            catch (WebException)
            {
                return false;   // 不是檔案,但可能為其他錯誤, 不一定為資料夾
            }
        }

        /// <summary> 判断当前目录下指定的子目录是否存在 </summary>
        /// <param name="RemoteDirectoryUri">指定的目录URI (完成路徑)</param>
        public bool IsDirectoryExist(string uri)
        {
            string strDir = uri.TrimEnd('/');
            string strDirName = strDir.Substring(strDir.LastIndexOf("/") + 1); //資料夾名稱
            string strParentDirUri = strDir.Remove(strDir.LastIndexOf("/"));
            string[] dirList = DirsName_InDirectory(strParentDirUri);//获取子目录
            return dirList.Contains(strDirName);
        }

        /// <summary> 取得FTP目標資料夾內所有包含子資料夾內的檔案明細 </summary>
        /// <param name="folder_uri">目標資料夾URI</param>
        /// <param name="Uri_FilesList"> 紀錄資料夾下所有檔案URI的儲存空間 </param>
        public void GetFileListUri(string folder_uri, ref List<string> Uri_FilesList)
        {
            string[] Items_file = FilesName_InDirectory(folder_uri);
            string[] Items_folder = DirsName_InDirectory(folder_uri);

            if (Items_file.Length > 0)
            {
                foreach (string item in Items_file)
                {
                    string full_uri = folder_uri + "/" + item;
                    Uri_FilesList.Add(full_uri);         /* 輸出檔案完整路徑 */
                }
            }
            if (Items_folder.Length > 0)
            {
                foreach (string item in Items_folder)
                {
                    GetFileListUri(folder_uri + "/" + item, ref Uri_FilesList);       /* 進入下層資料夾 */
                }
            }
        }

        /// <summary> 取得本地資料夾中所有包含子資料夾內的子檔案明細 </summary>
        /// <param name="folder_path">要上傳資料夾絕對路徑</param>
        /// <param name="Path_FilesList"> 紀錄資料夾下所有檔案路徑的儲存空間 </param>
        public void GetFileListPath(string folder_path, ref List<string> Path_FilesList)
        {
            DirectoryInfo directory = new DirectoryInfo(folder_path);
            FileInfo[] list_file = directory.GetFiles();
            DirectoryInfo[] list_folder = directory.GetDirectories();
            if (list_file.Length > 0)
            {
                foreach (FileInfo file in list_file)
                {
                    Path_FilesList.Add(file.FullName);
                }
            }
            if (list_folder.Length > 0)
            {
                foreach (DirectoryInfo folder in list_folder)
                {
                    GetFileListPath(folder.FullName, ref Path_FilesList);
                }
            }
        }

        // <自定義事件>
        /// <summary>
        /// 宣告狀態委託
        /// </summary>
        public delegate void StatusEventHandler(object sender, FtpEventArgs e);
        /// <summary>
        /// 宣告狀態事件
        /// </summary>
        public event StatusEventHandler StatusNOT_OK;
        /// <summary>
        /// 修改狀態內容
        /// </summary>
        /// <param name="Status">狀態的內容文字</param>
        protected virtual void SetStatus(string Status)
        {
            _strResultStatus = Status;
            if (_strResultStatus != "OK")
            {
                if (StatusNOT_OK != null)
                {
                    _bProcessing = false;
                    StatusNOT_OK(this, new FtpEventArgs() { Progress = _iProgress, ResultStatus = _strResultStatus }); /* 事件被触发 */
                }
            }
        }

        // <自定義事件>
        /// <summary>
        /// 宣告進度委託
        /// </summary>
        public delegate void ProgressEventHandler(object sender, FtpEventArgs e);
        /// <summary>
        /// 宣告進度事件
        /// </summary>
        public event ProgressEventHandler ProgressChanged;
        /// <summary>
        /// 進度設定
        /// </summary>
        /// <param name="n">代表進度百分比的整數</param>
        protected virtual void SetProgess(int n)
        {
            if (_iProgress != n)
            {
                _iProgress = n;
                if (ProgressChanged != null)
                {
                    ProgressChanged(this, new FtpEventArgs() { Progress = _iProgress, ResultStatus = _strResultStatus }); /* 事件被触发 */
                }
            }
        }

        // <自定義事件>
        /// <summary>
        /// 宣告進度事件
        /// </summary>
        public event ProgressEventHandler ProgressFinished;
        /// <summary>
        /// 進度修改事件觸法條件
        /// </summary>
        protected virtual void ProgressDone()
        {
            _bProcessing = false;
            if (ProgressFinished != null)
            {
                ProgressFinished(this, new FtpEventArgs() { Progress = _iProgress, ResultStatus = _strResultStatus }); /* 事件被触发 */
            }
        }
    }

    public class FtpUpload : FtpClient
    {
        /// <summary> 繼承建構子 </summary>
        /// <param name="host">ftp ip address</param>
        /// <param name="user">ftp user name</param>
        /// <param name="pass">ftp password</param>
        public FtpUpload(string host, string user, string pass) : base(host, user, pass) { }

        /// <summary> 上傳檔案(多載) </summary>
        /// <param name="file_uri">FTP上傳的目標完整地址,包含檔案名稱及副檔名</param>
        /// <param name="file_path">要被上傳的檔案絕對路徑</param>
        /// <param name="to_replace">是否取代原有的檔案, 默認為true</param>
        public void UploadFile(string file_uri, string file_path, bool to_replace = true)
        {
            string[] uri = new string[1] { file_uri };
            string[] path = new string[1] { file_path };

            UploadFile(uri, path, to_replace, true);
        }

        /// <summary> 上傳檔案(多載) </summary>
        /// <param name="file_uri">FTP上傳的目標完整地址陣列,包含檔案名稱及副檔名</param>
        /// <param name="file_path">要被上傳的檔案絕對路徑陣列</param>
        /// <param name="to_replace">是否取代原有的檔案, 默認為true</param>
        public void UploadFile(string[] file_uri, string[] file_path, bool to_replace = true)
        {
            _bProcessing = true;
            UploadFile(file_uri, file_path, to_replace, true);
        }

        /// <summary> 上傳檔案 內部呼叫方法 </summary>
        /// <param name="file_uri">FTP上傳的目標完整地址陣列,包含檔案名稱及副檔名</param>
        /// <param name="file_path">要被上傳的檔案絕對路徑陣列</param>
        /// <param name="to_replace">是否取代原有的檔案, 默認為true</param>
        /// <param name="col_size">是否計算檔案大小, 默認為true</param>
        /// <param name="bFlag">Flag標示是否為最外層呼叫</param>
        private void UploadFile(string[] file_uri, string[] file_path, bool to_replace = true, bool col_size = true, bool bFlag = true)
        {
            if (!to_replace)
            {
                var task = Task.Factory.StartNew(
                () =>
                {
                    foreach (string uri in file_uri)
                    {
                        if (IsFileExists(uri))
                        {
                            SetStatus("本地資料夾和遠端資料夾中存在重複檔案，參數設定為<不覆蓋重複檔案>，上傳失敗！\r\n請修改參數後重試...");
                            return;
                        }
                    }

                    if (col_size)
                    {
                        _dTotalSize = GetFileSize(file_path);
                    }
                    UploadWork(file_uri, file_path, bFlag);
                });
            }
            else
            {
                var task = Task.Factory.StartNew(
                () =>
                {
                    if (col_size)
                    {
                        _dTotalSize = GetFileSize(file_path);
                    }
                    UploadWork(file_uri, file_path, bFlag);
                });
            }
        }

        /// <summary> 上傳資料夾 </summary>
        /// <param name="folder_uri">FTP上傳的目標完整地址</param>
        /// <param name="folder_path">要被上傳的資料夾絕對路徑</param>
        /// <param name="to_replace">是否取代原有的檔案(保留原本多餘的檔案), 默認為true</param>
        /// <param name="brandnew_folder">是否儲存為全新資料夾(不會保留原本多餘的檔案), 默認為true</param>
        public void UploadFolder(string folder_uri, string folder_path, bool to_replace = true, bool brandnew_folder = true)
        {
            _bProcessing = true;
            if (brandnew_folder && IsDirectoryExist(folder_uri))
            {
                RemoveDirectory(folder_uri);
            }

            if (!to_replace)
            {
                var task = Task.Factory.StartNew(
                () =>
                {
                    if (FolderCheackRepeat(folder_uri, folder_path))
                    {
                        SetStatus("本地資料夾和遠端資料夾中存在重複檔案，參數設定為<不覆蓋重複檔案>，上傳失敗！\r\n請修改參數後重試...");
                        return;
                    }

                    _dTotalSize = GetFolderSize(folder_path);
                    pUploadFolder(folder_uri, folder_path);
                });
            }
            else
            {
                var task = Task.Factory.StartNew(
                () =>
                {
                    _dTotalSize = GetFolderSize(folder_path);
                    pUploadFolder(folder_uri, folder_path);
                });
            }
        }

        /// <summary> 上傳資料夾 內部呼叫方法 </summary>
        /// <param name="strTargetFolderUri">上傳目標資料夾</param>
        /// <param name="strSourceFolderPath">上傳來源資料夾</param>
        /// <param name="bFlag">Flag標示是否為最外層呼叫</param>
        private void pUploadFolder(string strTargetFolderUri, string strSourceFolderPath, bool bFlag = true)
        {
            /* Create directory if not Exists */
            if (!IsDirectoryExist(strTargetFolderUri))
            {
                CreateDirectory(strTargetFolderUri);
            }

            /* list all in local folder */
            DirectoryInfo clsDirInfo = new DirectoryInfo(strSourceFolderPath);
            /* 子目錄 遞迴  get all sub_folder in lcoal folder */
            DirectoryInfo[] Sub_Dirs = clsDirInfo.GetDirectories();
            if (Sub_Dirs.Length > 0)
            {
                foreach (DirectoryInfo dir in Sub_Dirs)
                {
                    string str_dir_name = dir.Name;
                    string str_dir_fullname = dir.FullName;

                    pUploadFolder(strTargetFolderUri + "/" + str_dir_name, str_dir_fullname, false);
                }
            }
            /* 檔案上傳 */
            FileInfo[] Files = clsDirInfo.GetFiles();
            if (Files.Length > 0)
            {
                string[] Files_uri = new string[Files.Length];
                string[] Files_path = new string[Files.Length];
                for (int index = 0; index < Files.Length; index++)
                {
                    Files_uri[index] = strTargetFolderUri + "/" + Files[index].Name;
                    Files_path[index] = Files[index].FullName;
                }
                UploadWork(Files_uri, Files_path, bFlag);
            }
        }

        /// <summary> 實際的檔案上傳方法 </summary>
        /// <param name="obj">上傳參數</param>
        private void UploadWork(string[] strFileUris, string[] strFilePaths, bool bFlag = true)
        {
            for (int index = 0; index < strFileUris.Length; index++)
            {
                string strFileUri = strFileUris[index];
                string strDir = strFileUri.Remove(strFileUri.LastIndexOf("/"));

                if (!IsDirectoryExist(strDir))
                {
                    CreateDirectory(strDir);
                }
                
                try
                {
                    /* Create an FTP Request */
                    FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(strFileUris[index]);
                    /* Log in to the FTP Server with the User Name and Password Provided */
                    ftpRequest.Credentials = new NetworkCredential(User, Pass);
                    /* Specify the Type of FTP Request */
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    /* Establish Return Communication with the FTP Server */
                    Stream ftpStream = ftpRequest.GetRequestStream();
                    /* Open a File Stream to Read the File for Upload */
                    FileStream localFileStream = new FileStream(strFilePaths[index], FileMode.Open);
                    /* Buffer for the Downloaded Data */
                    int bufferSize = 8192;
                    byte[] byteBuffer = new byte[bufferSize];
                    int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                    try
                    {
                        while (bytesSent != 0)
                        {
                            ftpStream.Write(byteBuffer, 0, bytesSent);
                            _dReadSize += bytesSent;
                            SetProgess((int)Math.Ceiling((Read * 100 / Total)));
                            bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetStatus(ex.ToString());
                        localFileStream.Close();
                        ftpStream.Close();
                        ftpRequest = null;
                        return;
                    }
                    /* Resource Cleanup */
                    localFileStream.Close();
                    ftpStream.Close();
                    ftpRequest = null;

                }
                catch (Exception ex)
                {
                    SetStatus(ex.ToString());
                    return;
                }
            }
            if (bFlag == true)
            {
                ProgressDone();
            }
            return;
        }

        /// <summary> 檢查上傳目標是否已經存在相同的資料夾以及其內部子檔案 </summary>
        /// <param name="strFolderUri">要被檢查的資料夾URI</param>
        /// <param name="strFolderPath">要被檢查的資料夾本地path</param>
        /// <returns> true:存在重複; false:不存在重複  </returns>
        private bool FolderCheackRepeat(string strFolderUri, string strFolderPath)
        {
            if (!IsDirectoryExist(strFolderUri)) return false;      // FTP資料夾不存在,不用比對

            /* 列出雲端目錄跟本地目錄中的所有檔案 */
            List<string> FilesUri = new List<string>();
            List<string> FilesPath = new List<string>();
            GetFileListUri(strFolderUri, ref FilesUri);
            GetFileListPath(strFolderPath, ref FilesPath);

            /* 將路徑改為相對路徑 */
            if (FilesPath.Count > 0)
            {
                for (int i = 0; i < FilesPath.Count; i++)
                {
                    FilesPath[i] = FilesPath[i].Replace(strFolderPath, "");
                    FilesPath[i] = FilesPath[i].Replace(@"\", "/");
                }
            }

            if (FilesUri.Count > 0)
            {
                for (int i = 0; i < FilesUri.Count; i++)
                {
                    /* 將路徑改為相對路徑 */
                    FilesUri[i] = FilesUri[i].Replace(strFolderUri, "");

                    /* 遠端檔案列表 跟 本地檔案列表 對比 */
                    for (int j = 0; j < FilesPath.Count; j++)
                    {
                        if (FilesUri[i] == FilesPath[j])
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary> 取得要上傳的檔案大小 </summary>
        /// <param name="obj">檔案參數物件</param>
        /// <returns>Double 檔案的大小</returns>
        private double GetFileSize(string[] strFilePath)
        {
            double dSize = 0;
            if (strFilePath.Count() > 0)
            {
                foreach (string path in strFilePath)
                {
                    FileInfo info = new FileInfo(path);
                    dSize += info.Length;
                }
            }
            return dSize;
        }

        /// <summary> 取得要上傳的資料夾大小 </summary>
        /// <param name="obj">資料夾參數物件</param>
        /// <returns>Double 資料夾的大小</returns>
        private double GetFolderSize(string strFolderPath)
        {
            double dSize = 0;
            DirectoryInfo Folderinfo = new DirectoryInfo(strFolderPath);
            FileInfo[] Files = Folderinfo.GetFiles();
            DirectoryInfo[] Folders = Folderinfo.GetDirectories();

            if (Files.Count() > 0)
            {
                foreach (FileInfo file in Files)
                {
                    dSize += file.Length;
                }
            }

            if (Folders.Count() > 0)
            {
                foreach (DirectoryInfo folder in Folders)
                {
                    dSize += GetFolderSize(folder.FullName);
                }
            }

            return dSize;
        }
    }

    public class FtpDownload : FtpClient
    {
        /// <summary> 繼承建構子 </summary>
        /// <param name="host">ftp ip address</param>
        /// <param name="user">ftp user name</param>
        /// <param name="pass">ftp password</param>
        public FtpDownload(string host, string user, string pass) : base(host, user, pass) { }

        /// <summary> 下載檔案(多載) </summary>
        /// <param name="file_uri">FTP檔案完整路徑,包含副檔名</param>
        /// <param name="file_path">檔案本地儲存絕對路徑,包含檔案名稱及附檔名</param>
        /// <param name="to_replace">是否覆蓋原本的檔案, 默認為true</param>
        public void DownloadFile(string file_uri, string file_path, bool to_replace = true)
        {
            string[] uri = new string[1] { file_uri };
            string[] path = new string[1] { file_path };

            DownloadFile(uri, path, to_replace, true);
        }

        /// <summary> 下載檔案(多載) </summary>
        /// <param name="file_uri">FTP檔案完整路徑陣列,包含副檔名</param>
        /// <param name="file_path">對應的檔案本地儲存絕對路徑陣列,包含檔案名稱及附檔名</param>
        /// <param name="to_replace">是否覆蓋原本的檔案, 默認為true</param>
        public void DownloadFile(string[] file_uri, string[] file_path, bool to_replace = true)
        {
            _bProcessing = true;
            DownloadFile(file_uri, file_path, to_replace, true);
        }

        /// <summary> 下載檔案 內部呼叫方法 </summary>
        /// <param name="file_uri">FTP檔案完整路徑陣列,包含副檔名</param>
        /// <param name="file_path">對應的檔案本地儲存絕對路徑陣列,包含檔案名稱及附檔名</param>
        /// <param name="to_replace">是否覆蓋原本的檔案, 默認為true</param>
        /// <param name="col_size">是否計算檔案大小, 默認為true</param>
        /// <param name="bFlag">是否為遞回的最外層, 默認為true</param>
        private void DownloadFile(string[] file_uri, string[] file_path, bool to_replace = true, bool col_size = true, bool bFlag = true)
        {
            /* check repeat before download work if user don't want to replace original file */
            if (!to_replace)
            {
                var task = Task.Factory.StartNew(
                () =>
                {
                    foreach (string path in file_path)
                    {
                        if (File.Exists(path))
                        {
                            SetStatus("本地資料夾和遠端資料夾中存在重複檔案，參數設定為<不覆蓋重複檔案>，下載失敗！\r\n請修改參數後重試...");
                            return;
                        }
                    }

                    if (col_size)
                    {
                        _dTotalSize = GetFileSize(file_uri);
                        if (_dTotalSize == -1) return;
                    }
                    DownloadWork(file_uri, file_path, bFlag);
                });
            }
            else
            {
                var task = Task.Factory.StartNew(
                () =>
                {
                    if (col_size)
                    {
                        _dTotalSize = GetFileSize(file_uri);
                        if (_dTotalSize == -1) return;
                    }
                    DownloadWork(file_uri, file_path, bFlag);
                });
            }
        }

        /// <summary>下載資料夾</summary>
        /// <param name="folder_uri">資料夾的FTP完整地址</param>
        /// <param name="folder_path">資料夾本地儲存的絕對路徑</param>
        /// <param name="to_replace">是否覆蓋重複檔案(會保留原本本地多餘的檔案), 默認為true</param>
        /// <param name="brandnew_folder">是否儲存為全新資料夾(不會保留原本本地多餘的檔案), 默認為true</param>
        public void DownloadFolder(string folder_uri, string folder_path, bool to_replace = true, bool brandnew_folder = true)
        {
            _bProcessing = true;
            if (brandnew_folder && Directory.Exists(folder_path))
            {
                Directory.Delete(folder_path, true);
            }

            if (!to_replace)
            {
                var task = Task.Factory.StartNew(
                () =>
                {
                    if (FolderCheackRepeat(folder_uri, folder_path))
                    {
                        SetStatus("本地資料夾和遠端資料夾中存在重複檔案，參數設定為<不覆蓋重複檔案>，下載失敗！\r\n請修改參數後重試...");
                        return;
                    }

                    _dTotalSize = GetFolderSize(folder_uri);
                    if (_dTotalSize == -1) return;
                    pDownloadFolder(folder_uri, folder_path);
                });
            }
            else
            {
                var task = Task.Factory.StartNew(
                () =>
                {
                    _dTotalSize = GetFolderSize(folder_uri);
                    if (_dTotalSize == -1) return;
                    pDownloadFolder(folder_uri, folder_path);
                });
            }
        }

        /// <summary>
        /// 下載資料夾 內部方法
        /// </summary>
        /// <param name="strFolderUri">下載資料夾的FTP Uri</param>
        /// <param name="strFolderPath">資料夾本地儲存路徑</param>
        /// <param name="bFlag">是否為遞回的最外層標籤 默認為True</param>
        private void pDownloadFolder(string strFolderUri, string strFolderPath, bool bFlag = true)
        {
            if (!Directory.Exists(strFolderPath))
            {
                Directory.CreateDirectory(strFolderPath);
            }

            string[] strFileName = FilesName_InDirectory(strFolderUri);
            string[] strFolderName = DirsName_InDirectory(strFolderUri);

            if (strFolderName.Length > 0)
            {
                for (int i = 0; i < strFolderName.Length; i++)
                {
                    string strDirUri = strFolderUri + "/" + strFolderName[i];
                    string strDirPath = strFolderPath + @"\" + strFolderName[i];

                    pDownloadFolder(strDirUri, strDirPath, bFlag: false);
                }
            }

            if (strFileName.Length > 0)
            {
                string[] strFileUri = new string[strFileName.Length];
                string[] strFilePath = new string[strFileName.Length];

                for (int i = 0; i < strFileName.Length; i++)
                {
                    strFileUri[i] = strFolderUri + "/" + strFileName[i];
                    strFilePath[i] = strFolderPath + @"\" + strFileName[i];
                }

                DownloadWork(strFileUri, strFilePath, bFlag);
            }
        }

        /// <summary> 實際下載檔案的執行方法 </summary>
        /// <param name="strFileUris">要下載檔案的FTP URI陣列</param>
        /// <param name="strFilePaths">檔案儲存路徑陣列</param>
        /// <param name="bFlag">是否為遞回的最外層標籤 默認為True</param>
        private void DownloadWork(string[] strFileUris, string[] strFilePaths, bool bFlag)
        {
            for (int index = 0; index < strFileUris.Length; index++)
            {
                FileInfo clsfi = new FileInfo(strFilePaths[index]);
                if (!clsfi.Directory.Exists)
                {
                    clsfi.Directory.Create();
                }

                try
                {
                    /* Create an FTP Request */
                    FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(strFileUris[index]);
                    /* Log in to the FTP Server with the User Name and Password Provided */
                    ftpRequest.Credentials = new NetworkCredential(User, Pass);
                    /* Specify the Type of FTP Request */
                    ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    /* Establish Return Communication with the FTP Server */
                    FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                    /* Get the FTP Server's Response Stream */
                    Stream ftpStream = ftpResponse.GetResponseStream();
                    /* Open a File Stream to Write the Downloaded File */
                    FileStream localFileStream = new FileStream(strFilePaths[index], FileMode.Create);
                    /* Buffer for the Downloaded Data */
                    int bufferSize = 8192;
                    byte[] byteBuffer = new byte[bufferSize];
                    int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                    try
                    {
                        while (bytesRead > 0)
                        {
                            localFileStream.Write(byteBuffer, 0, bytesRead);
                            _dReadSize += bytesRead;
                            SetProgess((int)Math.Ceiling((_dReadSize * 100 / Total)));
                            bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetStatus(ex.ToString());
                        localFileStream.Close();
                        ftpStream.Close();
                        ftpResponse.Close();
                        ftpRequest = null;
                        return;
                    }
                    /* Resource Cleanup */
                    localFileStream.Close();
                    ftpStream.Close();
                    ftpResponse.Close();
                    ftpRequest = null;
                }
                catch (Exception ex)
                {
                    SetStatus(ex.ToString());
                    return;
                }
            }
            if (bFlag)
            {
                ProgressDone();
            }
            return;
        }

        /// <summary> 取得下載檔案的資料大小, 結果將儲存於全域變數中 </summary>
        /// <param name="strFileUris"> 要取得大小的檔案 FTP URI</param>
        /// <returns>檔案總大小</returns>
        private double GetFileSize(string[] strFileUris)
        {
            double dSize = 0;
            foreach (string uri in strFileUris)
            {
                try
                {
                    /* Create an FTP Request */
                    FtpWebRequest size_ftpRequest = (FtpWebRequest)FtpWebRequest.Create(uri);
                    /* Log in to the FTP Server with the User Name and Password Provided */
                    size_ftpRequest.Credentials = new NetworkCredential(User, Pass);
                    /* Specify the Type of FTP Request */
                    size_ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                    dSize += size_ftpRequest.GetResponse().ContentLength;
                    size_ftpRequest = null;
                }
                catch (Exception ex)
                {
                    SetStatus(ex.ToString());
                    return -1;
                }
            }
            return dSize;
        }

        /// <summary> 取得下載資料夾的大小, 結果將儲存於全域變數中 </summary>
        /// <param name="strFolerUri">要下載的資料夾的FTP URI</param>
        /// <returns>資料夾總大小</returns>
        private double GetFolderSize(string strFolerUri)
        {
            double dSize = 0;
            string[] strList_FileName = FilesName_InDirectory(strFolerUri);
            string[] strList_DirName = DirsName_InDirectory(strFolerUri);

            if (strList_DirName.Length > 0)
            {
                for (int i = 0; i < strList_DirName.Length; i++)
                {
                    strList_DirName[i] = strFolerUri + "/" + strList_DirName[i];

                    double dfoldersize = GetFolderSize(strList_DirName[i]);
                    if (dfoldersize == -1) return -1;
                    else dSize += dfoldersize;
                }
            }

            if (strList_FileName.Length > 0)
            {
                for (int i = 0; i < strList_FileName.Length; i++)
                {
                    strList_FileName[i] = strFolerUri + "/" + strList_FileName[i];
                }

                double dfilesize = GetFileSize(strList_FileName);
                if (dfilesize == -1) return -1;
                else dSize += dfilesize;
            }
            return dSize;
        }

        /// <summary> 檢查要被下載的資料夾在目標儲存路徑是否已經存在,及資料夾內的子檔案和子資料夾是否重複 </summary>
        /// <param name="strFolderUri">做為比對的資料夾FTP URI</param>
        /// <param name="strFolderPath">要被檢查的資料夾本地path</param>
        /// <returns>true:存在重複檔案; false:不存在重複檔案</returns>
        private bool FolderCheackRepeat(string strFolderUri, string strFolderPath)
        {
            if (!Directory.Exists(strFolderPath)) return false;

            /* 列出雲端目錄跟本地目錄中的所有檔案 */
            List<string> FilesUri = new List<string>();
            List<string> FilesPath = new List<string>();
            GetFileListUri(strFolderUri, ref FilesUri);
            GetFileListPath(strFolderPath, ref FilesPath);

            /* 將路徑改為相對路徑 */
            if (FilesUri.Count > 0)
            {
                for (int i = 0; i < FilesUri.Count; i++)
                {
                    FilesUri[i] = FilesUri[i].Replace(strFolderUri, "");
                }
            }

            if (FilesPath.Count > 0)
            {
                for (int i = 0; i < FilesPath.Count; i++)
                {
                    FilesPath[i] = FilesPath[i].Replace(strFolderPath, "");
                    FilesPath[i] = FilesPath[i].Replace(@"\", "/");

                    /* 遠端檔案列表 跟 本地檔案列表 對比 */
                    for (int j = 0; j < FilesUri.Count; j++)
                    {
                        if (FilesPath[i] == FilesUri[j])
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
