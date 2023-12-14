using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Management;
using IWshRuntimeLibrary;
using System.Threading;
using System.Windows;
using GeneralUtility.IO;
using System.Drawing;
using System.Threading.Tasks;

namespace GeneralUtility
{
    /// <summary>提供系統內 Windows API function 常用工具。</summary>
    public partial class XWinApi
    {

        #region " Definition "

        /// <summary>硬碟資訊。</summary>
        public class XDiskInfo
        {
            #region " Properties "

            /// <summary>取得，硬碟模組名稱。</summary>
            public string Model
            {
                get { return g_strModel; }
            }
            private string g_strModel;

            /// <summary>取得，SerialNumber。</summary>
            public string SerialNumber
            {
                get { return g_strSerialNumber; }
            }
            private string g_strSerialNumber;

            #endregion

            #region " Method - New "

            /// <summary>建立一個硬碟資訊。</summary>
            /// <param name="strModel"></param>
            /// <param name="strSerialNumber"></param>
            public XDiskInfo(string strModel, string strSerialNumber)
            {
                g_strModel = strModel;
                g_strSerialNumber = strSerialNumber;
            }

            #endregion
        }

        #endregion

        #region " Propeties "

        /// <summary>取得，設定這個 XWinApi 的啟動路徑。</summary>
        public static string StartupPath { get { return g_strStartupPath; } }
        private static string g_strStartupPath = System.Windows.Forms.Application.StartupPath;


        //監控 CPU 傾聽器，觀看程序 CPU 的值，每一個想觀看的程序便產生一個 XCPU 去監看清單上的程序。
        private static Dictionary<string, XCPU> g_clsCPULists = new Dictionary<string, XCPU>();
        /// <summary> 網路磁碟機連線類別 。 </summary>
        private static WshNetwork g_NetworkShell = new WshNetwork();

        /// <summary>取得，設定這個 XWinApi 的螢幕寬度。</summary>
        public static int GetScanerWidth
        {
            get { return Screen.PrimaryScreen.Bounds.Width; }
        }

        /// <summary>取得，設定這個 XWinApi 的螢幕高度。</summary>
        public static int GetScanerHeight
        {
            get { return Screen.PrimaryScreen.Bounds.Height; }
        }

        #endregion

        #region " Methods "

        /// <summary>提供一組的字串資料，並將此資料複製貼到剪貼簿中。</summary>
        /// <param name="strMessage">輸入值為字串資料。</param>
        public static void CopyToClipboard(string strMessage)
        {
            try
            {
                Clipboard.SetDataObject(strMessage);
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
        }

        /// <summary>
        /// 將剪貼簿資料貼上，就是Ctrl+V功能。
        /// </summary>
        public static void PasteFromClipboard()
        {
            try
            {
                SendKeys.Send("^{v}");
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
        }

        /// <summary>提供指定的資料夾或檔案路徑，利用預設的檔案總管方式來開啟資料夾。</summary>
        /// <param name="strPath">輸入值為資料夾或檔案路徑。</param>
        /// <param name="bSelectedMode">輸入值為選擇開啟路徑位置，預設為 true 。</param>
        public static void OpenFolder(string strPath, bool bSelectedMode = true)
        {
            try
            {
                if (bSelectedMode)
                {
                    System.Diagnostics.Process.Start("explorer", "/select, " + strPath);
                }
                else
                {
                    System.Diagnostics.Process.Start("explorer", strPath);
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
        }

        /// <summary>提供一個指定的執行檔名稱，並從 Windows 系統中搜尋符合名稱的檔案已被執行的數量。</summary>
        /// <param name="strProcessorName">輸入值為執行檔名稱。</param>
        /// <returns>回傳執行檔已被執行的數量。</returns>
        public static int ExecutedNumber(string strProcessorName = "")
        {
            try
            {
                if (strProcessorName == "")
                {
                    Process clsCurrent = Process.GetCurrentProcess();
                    strProcessorName = clsCurrent.ProcessName;
                }

                return Process.GetProcessesByName(strProcessorName).Length;
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return -1;
            }
        }

        /// <summary>提供取得當前的系統資訊。</summary>
        /// <returns>回傳當前的系統資訊。</returns>
        public static string GetSystemInformation()
        {
            try
            {
                string strOs = Environment.OSVersion.ToString();
                string strUser = "By " + Environment.UserName;
                return strOs + ", " + strUser;
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return string.Empty;
            }
        }

        /// <summary>提供取得當前使用系統的的使用者名稱。</summary>   
        /// <returns>回傳使用者名稱。</returns>  
        public static string GetPcUser()
        {
            return Environment.UserName;
        }

        /// <summary>提供取得當前的螢幕尺寸。</summary>   
        /// <returns>回傳螢幕尺寸。</returns>   
        public static System.Windows.Size GetScaner()
        {
            return new System.Windows.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        /// <summary>提供一個釋放系統記憶體的方式，當程式執行Dispose後，會提醒系統執行記憶體釋放，但是系統不會立即釋放，即使釋放也會從層級 0 的記憶體開始清除，所以當下如果需要立即釋放記憶體的動作，可以執行此功能。(建議可以在一個完整的功能循環結束時機點執行一次此功能。) </summary>
        /// <param name="bWaitReleaseDone">輸入值為是否等待記憶體釋放完成，預設為 false 。</param>
        public static void ReleaseAllMemory(bool bWaitReleaseDone = false)
        {
            // 目前預設是所有層級都一率釋放。
            GC.Collect();
            if (bWaitReleaseDone) GC.WaitForPendingFinalizers();
        }

        /// <summary>提供系統離開時強制關閉當前正在運行的的執行緒。</summary>
        public static void ExitAllThread()
        {
            XStatus.Report(XStatus.Type.Information, MethodInfo.GetCurrentMethod(), "Exit all thread.");
            Environment.Exit(Environment.ExitCode);
        }

        /// <summary>提供查詢當前程式所使用的記憶體大小。</summary>
        /// <returns>回傳當前程式的記憶體大小。</returns>
        public static long GetMemory()
        {
            return Environment.WorkingSet;
        }

        /// <summary>提供查詢當前程式所使用的記憶體大小，單位為：MB 。</summary>
        /// <returns>回傳記憶體大小，單位為：MB 。</returns>
        public static long GetMemoryMB()
        {
            return (long)(GetMemory() >> 20);
        }

        /// <summary>提供查詢當前程式所使用的記憶體大小，單位為：KB 。</summary>
        /// <returns>回傳記憶體大小，單位為：KB 。</returns>
        public static long GetMemoryKB()
        {
            return (long)(GetMemory() >> 10);
        }

        /// <summary>提供系統重啟功能。</summary>
        public static void Restart()
        {
            try
            {
                XStatus.Report(XStatus.Type.Information, MethodInfo.GetCurrentMethod(), "System reset.");
                Process clsCurrent = Process.GetCurrentProcess();
                Process.Start(clsCurrent.ProcessName);
                ExitAllThread();
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
        }


        /// <summary>監視程序 CPU 使用狀況。</summary>
        public class XCPU
        {

            #region " Propeties "

            private string g_strProcessName;
            /// <summary>取得，設定這個 XCPU 的程序名稱。</summary>
            public string ProcessName
            {
                get { return g_strProcessName; }
            }
            private float g_fProcessValue;
            /// <summary>取得，設定這個 XCPU 的程序數值。</summary>
            public float ProcessValue
            {
                get { return g_fProcessValue; }
            }

            //CPU 的 Buffer
            private List<float> g_fCpuBuffers;
            /// <summary>取得，這個 XCPU 的 CPU Buffer 的單精倍浮點數的 Array 數值。</summary>
            public List<float> CpuBuffer
            {
                get { return g_fCpuBuffers; }
            }

            //CPU 的 Buffer
            private int g_fCpuBuffersize = 300;
            /// <summary>取得或設定，這個 XCPU 的 CPU Buffer 大小。</summary>
            public int CpuBuffersize
            {
                get { return g_fCpuBuffersize; }
                set { g_fCpuBuffersize = value; }
            }
            private int g_iErrorCode;
            /// <summary>取得，這個 XCPU 的 錯誤訊息。</summary>
            public int ErrorCode
            {
                get { return g_iErrorCode; }
            }

            /// <summary>取得，這個 XCPU 的 CPU 核心數。</summary>
            public int ProcessorCount
            {
                get { return Environment.ProcessorCount; }
            }

            private List<double> g_fCpuTemperatures = new List<double>();
            /// <summary>取得，這個 XCPU 的 CPU 的溫度。</summary>
            public List<double> CpuTemperature
            {
                get { return g_fCpuTemperatures; }
            }

            private PerformanceCounter g_clsPerformanceCounter;
            private Thread g_clsThread;

            #endregion

            /// <summary>使用一組指定的程序名稱，來建構一個初始化 XCPU 的新執行個體。</summary>
            /// <param name="strProcessName">輸入值為程序名稱。</param>
            public XCPU(string strProcessName)
            {
                g_fCpuBuffers = new List<float>();
                g_strProcessName = strProcessName;
                g_fProcessValue = 0.0f;
                g_iErrorCode = 0;
                //判斷程序是否存在,這裡可能會有同名的Process,暫時先以#數字方法作為指定
                //以後可以改成float[]回傳,監視所有同名的Process
                Process[] clsProcess = Process.GetProcessesByName(g_strProcessName);
                if (clsProcess.Length <= 0 && g_strProcessName != "_Total")
                {
                    g_iErrorCode = -1;
                    XStatus.Report(XStatus.Type.Information, MethodInfo.GetCurrentMethod(), "程序不存在,或是程序名稱錯誤");
                    return;
                }
                if (g_strProcessName != "_Total")
                    g_clsPerformanceCounter = new PerformanceCounter("Process", "% Processor Time", g_strProcessName);
                else
                    g_clsPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                g_clsThread = new Thread(CPUListener);
                g_clsThread.IsBackground = true;
                g_clsThread.Start();
            }

            /// <summary>提供一個取得 CPU 輸出 Buffer 的接聽項的集合。</summary>
            private void CPUListener()
            {
                try
                {
                    double dTemperature = 0;
                    int iCount = 0;
                    System.Management.ManagementObjectSearcher clsMOS = new System.Management.ManagementObjectSearcher(@"root\WMI", "Select * From MSAcpi_ThermalZoneTemperature");
                    while (true)
                    {
                        g_fProcessValue = 0.0f;

                        //CPU Temperature
                        iCount = 0;
                        foreach (System.Management.ManagementObject MOS in clsMOS.Get())
                        {
                            iCount++;
                            if (g_fCpuTemperatures.Count < iCount)
                                g_fCpuTemperatures.Add(Convert.ToDouble(Convert.ToDouble(MOS.GetPropertyValue("CurrentTemperature").ToString()) - 2732) / 10);
                            else
                                g_fCpuTemperatures[iCount - 1] = Convert.ToDouble(Convert.ToDouble(MOS.GetPropertyValue("CurrentTemperature").ToString()) - 2732) / 10;
                        }

                        //CPU Value
                        if (g_strProcessName != "_Total")
                            g_fProcessValue = g_clsPerformanceCounter.NextValue() / Environment.ProcessorCount;
                        else
                            g_fProcessValue = g_clsPerformanceCounter.NextValue();

                        //CPU Buffer
                        if (g_fCpuBuffers.Count >= g_fCpuBuffersize)
                            g_fCpuBuffers.RemoveAt(0);
                        g_fCpuBuffers.Add(g_fProcessValue);

                        Thread.Sleep(300);

                    }
                }
                catch (Exception ex)
                {
                    XStatus.Report(XStatus.Type.Procedure, MethodInfo.GetCurrentMethod(), ex.ToString());
                }
            }

            /// <summary>提供一個釋放 Windows NT 效能計數器元件。</summary>
            public void Dispose()
            {
                if (g_clsThread != null)
                    g_clsThread.Abort();
                if (g_clsPerformanceCounter != null)
                    g_clsPerformanceCounter.Dispose();
            }
        }

        /// <summary>
        /// 查詢當前程式名所執行的Cpu使用量，當前可能分.vshost.exe或.exe此函式會自動判斷
        /// 如果程式想要多開會造成判別錯誤，請用指定名稱多載解決。
        /// </summary>
        /// <param name="fCPUProcessValue">輸入值為程序CPU使用量。</param>
        /// <returns>執行結果狀態碼 XErrorCode 。</returns>
        public static int GetCPU(out float fCPUProcessValue)
        {
            int iStt = 0;
            string strProcessName = Application.ProductName;
            iStt = GetCPU(strProcessName, out fCPUProcessValue);
            if (iStt < 0)
            {
                strProcessName = strProcessName + ".vshost";
                iStt = GetCPU(strProcessName, out fCPUProcessValue);
            }
            return iStt;
        }

        /// <summary>提供指定的程式名稱，並進行搜尋此名稱中已被執行的 CPU 使用量，但可能會有多個相同名稱的程式，後面可加#1、#2以此類推。<para></para>
        /// 想要cpu全部程式cpu執行量可輸入"_Total"。
        /// </summary>
        /// <param name="strSearchProcessName">輸入值為查詢的程式名稱，後面可加#1、#2...查詢同名程式的cpu執行量或輸入"_Total"看全部執行量。</param>
        /// <param name="fCPUProcessValue">輸入值為程序CPU使用量。</param>
        /// <returns>執行結果狀態碼 XErrorCode 。</returns>
        public static int GetCPU(string strSearchProcessName, out float fCPUProcessValue)
        {
            fCPUProcessValue = 0.0f;
            int iStt = 0;
            string strProcessName = strSearchProcessName;

            iStt = CreateCpuListener(strSearchProcessName);
            if (iStt >= 0)
                fCPUProcessValue = g_clsCPULists[strSearchProcessName].ProcessValue;
            else
                iStt = -1;
            XStatus.Report(XStatus.Type.Procedure, MethodInfo.GetCurrentMethod(), "程序不存在,或是程序名稱錯誤。");
            return iStt;
        }

        /// <summary>提供指定的程序名稱，來建立一個 CPU 監控方法。</summary>
        /// <param name="strSearchProcessName">輸入值為程序名稱。</param>
        /// <returns>執行結果狀態碼 XErrorCode 。</returns>
        private static int CreateCpuListener(string strSearchProcessName)
        {
            //判斷是否有安裝CPUListener
            if (!g_clsCPULists.ContainsKey(strSearchProcessName))
            {
                g_clsCPULists.Add(strSearchProcessName, new XCPU(strSearchProcessName));
            }
            //刪除 錯誤的CPUListener
            if (g_clsCPULists[strSearchProcessName].ErrorCode < 0)
            {
                g_clsCPULists[strSearchProcessName].Dispose();
                g_clsCPULists.Remove(strSearchProcessName);
                XStatus.Report(XStatus.Type.Procedure, MethodInfo.GetCurrentMethod(), "CPUListener錯誤。");
                return -1;
            }
            return 0;
        }

        /// <summary>取得前300筆CPU數值，此 Buffer 會一直不斷更新。</summary>
        /// <param name="strSearchProcessName"></param>
        /// <param name="fCPUCPUBuffer">輸入值為 CPU 使用率的單精倍浮數點數的 Array 數值。 </param>
        /// <returns>執行結果狀態碼 XErrorCode 。</returns>
        public static int GetCpuBuffer(string strSearchProcessName, out List<float> fCPUCPUBuffer)
        {
            fCPUCPUBuffer = null;
            int iStt = 0;
            string strProcessName = strSearchProcessName;

            iStt = CreateCpuListener(strSearchProcessName);
            if (iStt >= 0)
                fCPUCPUBuffer = g_clsCPULists[strSearchProcessName].CpuBuffer;
            else
                iStt = -1;
            XStatus.Report(XStatus.Type.Procedure, MethodInfo.GetCurrentMethod(), "創造CPU Listenerg失敗。");
            return iStt;
        }

        /// <summary>提供一個指定的檔案路徑，來取得此程式的版本資料，如果沒有輸入，則會輸入自己程式的版本號碼。</summary>
        /// <param name="strPath">輸入值為檔案路徑，如果要找自己的話，可以鍵入 Process.GetCurrentProcess().ProcessName ，並且請注意副檔名有沒有輸入。</param>
        /// <returns>回傳程式的版本號碼。</returns>
        public static string GetVersionName(string strPath)
        {
            if (!XFile.IsExist_File(ref strPath)) return "";
            return AssemblyName.GetAssemblyName(strPath).Version.ToString();
        }

        /// <summary>提供指定的 Process 格式，來取得此程式的版本資料，如果沒有輸入，則會輸入自己程式的版本號碼。</summary>
        /// <param name="clsSystemProcess">輸入值為檔案路徑，如果要找自己的話，可以鍵入 Process.GetCurrentProcess() ，並且請注意副檔名有沒有輸入。</param>
        /// <returns>傳回程式的版本號碼。</returns>
        public static string GetVersionName(Process clsSystemProcess)
        {
            string strPath = clsSystemProcess.ProcessName + ".exe";
            if (!XFile.IsExist_File(ref strPath)) return "";
            return AssemblyName.GetAssemblyName(strPath).Version.ToString();
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        /// <summary>提供一個指定的驅動器名稱，來取得此名稱中磁片剩餘的空間，例:D:\ 。</summary>
        /// <param name="strDriveDirectoryName">輸入值為驅動器名稱。</param>
        /// <returns>傳回磁片剩餘的空間。</returns>
        public static ulong GetFreeSpace(string strDriveDirectoryName)
        {
            ulong iFreeBytesAvailable, iTotalNumberOfBytes, iTotalNumberOfFreeBytes;
            if (!strDriveDirectoryName.EndsWith(":\\"))
            {
                strDriveDirectoryName += ":\\";
            }
            GetDiskFreeSpaceEx(strDriveDirectoryName, out iFreeBytesAvailable, out iTotalNumberOfBytes, out iTotalNumberOfFreeBytes);
            return iFreeBytesAvailable;
        }

        public static string ExecuteCMD(string cmd, Func<string, string> filterFunc)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.StandardInput.WriteLine(cmd + " &exit");
            process.StandardInput.AutoFlush = true;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            return filterFunc(output);
        }

        private static string GetTextAfterSpecialText(string fullText, string specialText)
        {
            if (string.IsNullOrWhiteSpace(fullText) || string.IsNullOrWhiteSpace(specialText))
            {
                return null;
            }
            string lastText = null;
            var idx = fullText.LastIndexOf(specialText);
            if (idx > 0)
            {
                lastText = fullText.Substring(idx + specialText.Length).Trim();
            }
            return lastText;
        }

        /// <summary>取得smBIOS UUID</summary>
        /// <returns>回傳smBIOS UUID</returns>
        public static string GetSmBIOSUUID()
        {
            var cmd = "wmic csproduct get UUID";
            return ExecuteCMD(cmd, output =>
            {
                string uuid = GetTextAfterSpecialText(output, "UUID");
                if (uuid == "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                {
                    uuid = null;
                }
                return uuid;
            });
        }

        /// <summary> 取得主機版序號。</summary>
        /// <returns>回傳主機版序號。</returns>
        public static string GetMotherBoardSerialNumber()
        {
            string strMotherBoardSerialNumber = "";
            try
            {
                ManagementClass clsManagementClass = new ManagementClass("WIN32_BaseBoard");
                ManagementObjectCollection clsManagementObjectCollection = clsManagementClass.GetInstances();

                foreach (ManagementObject clsManagementObject in clsManagementObjectCollection)
                {
                    strMotherBoardSerialNumber = clsManagementObject["SerialNumber"].ToString();
                    break;
                }
                clsManagementClass.Dispose();
                clsManagementObjectCollection.Dispose();
            }
            catch
            {
                strMotherBoardSerialNumber = "unknow";
            }
            return strMotherBoardSerialNumber;
        }

        /// <summary>取得作業系統名稱與版本。</summary>
        /// <returns>回傳作業系統名稱與版本。</returns>
        public static string GetOperatingSystem()
        {
            string strOS;
            try
            {
                OperatingSystem clsOperatingSystem = System.Environment.OSVersion;
                strOS = clsOperatingSystem.ToString();
            }
            catch
            {
                strOS = "unknow";
            }
            return strOS;
        }

        /// <summary>取得整體系統的記憶體大小。</summary>
        /// <returns>回傳系統記憶體大小。</returns>
        public static string GetTotalPhysicalMemory()
        {
            string strTotalPysicalMemory = "unknow";
            try
            {
                ManagementClass clsManagementClass = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection clsManagementObjectCollection = clsManagementClass.GetInstances();
                foreach (ManagementObject clsManagementObject in clsManagementObjectCollection)
                {
                    strTotalPysicalMemory = clsManagementObject["TotalPhysicalMemory"].ToString();
                }
                clsManagementObjectCollection = null;
                clsManagementClass = null;
            }
            catch
            {
                strTotalPysicalMemory = "unknow";
            }
            return strTotalPysicalMemory;
        }

        /// <summary>取得作業系統類別。</summary>
        /// <returns>回傳作業系統類別。</returns>
        public static string GetSystemType()
        {
            string strSystemType = "unknow";
            try
            {
                ManagementClass clsManagementClass = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection clsManagementObjectCollection = clsManagementClass.GetInstances();
                foreach (ManagementObject clsManagementObject in clsManagementObjectCollection)
                {
                    strSystemType = clsManagementObject["SystemType"].ToString();
                }
                clsManagementObjectCollection.Dispose();
                clsManagementClass.Dispose();
            }
            catch
            {
                strSystemType = "unknow";
            }
            return strSystemType;
        }

        /// <summary>取得電腦名稱。</summary>
        /// <returns>回傳電腦名稱。</returns>
        public static string GetComputerName()
        {
            string strComputerName = "unknow";
            try
            {
                strComputerName = System.Environment.MachineName; ;
            }
            catch
            {
                strComputerName = "unknow";
            }
            return strComputerName;
        }

        /// <summary>取得 IP 位址。</summary>
        /// <returns>回傳 IP 位址。</returns>
        public static string GetIPAddress()
        {
            string strIpAddress = "unknow";
            try
            {
                ManagementClass clsManagementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection clsManagementObjectCollection = clsManagementClass.GetInstances();
                foreach (ManagementObject clsManagementObject in clsManagementObjectCollection)
                {
                    if ((bool)clsManagementObject["IPEnabled"] == true)
                    {
                        Array clsArray;
                        clsArray = (Array)(clsManagementObject.Properties["IpAddress"].Value);
                        strIpAddress = clsArray.GetValue(0).ToString();
                        break;
                    }
                }
                clsManagementObjectCollection.Dispose();
                clsManagementClass.Dispose();
            }
            catch
            {
                strIpAddress = "unknow";
            }
            return strIpAddress;
        }

        /// <summary>取得本機 IP 。 </summary>
        /// <param name="eAddressFamily">IPv4 選 InterNetwork;
        /// <para>IPv6 選 InterNetworkV6。 </para></param>
        /// <returns>IP List。</returns>
        public static List<IPAddress> GetLocalIPAddress(AddressFamily eAddressFamily = AddressFamily.InterNetwork)
        {
            List<IPAddress> clsIPAddress = new List<IPAddress>();
            try
            {
                IPAddress[] clsLocalIps = Dns.GetHostAddresses(Dns.GetHostName());   // 查詢本機端的IP,
                foreach (IPAddress clsIpAddress in clsLocalIps)
                {
                    //判斷是否為IPV4
                    if (clsIpAddress.AddressFamily == AddressFamily.InterNetwork &&
                        eAddressFamily == AddressFamily.InterNetwork)
                    {
                        clsIPAddress.Add(clsIpAddress);
                    }
                    else if (clsIpAddress.AddressFamily == AddressFamily.InterNetworkV6 &&
                             eAddressFamily == AddressFamily.InterNetworkV6)
                    {
                        clsIPAddress.Add(clsIpAddress);
                    }
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return clsIPAddress;
        }

        /// <summary>取得所有硬碟資訊。</summary>
        /// <returns>所有硬碟資訊。</returns>
        public static List<XDiskInfo> GetAllDiskInfo()
        {
            List<XDiskInfo> clsDiskInfos = new List<XDiskInfo>();
            try
            {
                ManagementObjectSearcher clsHDD = new ManagementObjectSearcher("select * from Win32_DiskDrive");
                List<string> strHdInfo = new List<string>();
                string strModel;
                string strSerialNumber;
                foreach (ManagementObject clsDisk in clsHDD.Get())
                {
                    strModel = "Unknow";
                    strSerialNumber = "Unknow";
                    try
                    {
                        strModel = clsDisk["Model"].ToString();
                    }
                    catch
                    {
                        strModel = "Unknow";
                    }

                    try
                    {
                        strSerialNumber = clsDisk["SerialNumber"].ToString();
                    }
                    catch
                    {
                        strSerialNumber = "Unknow";
                    }

                    XDiskInfo clsDiskInfo = new XDiskInfo(strModel, strSerialNumber);
                    clsDiskInfos.Add(clsDiskInfo);
                }
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
            return clsDiskInfos;
        }

        /// <summary> 提供一個網路磁碟機連線的功能。 </summary>
        /// <param name="DiskCode"> 磁碟機代號。 </param>
        /// <param name="UNC"> 連線位址。 </param>
        /// <param name="Account"> 帳戶帳號。 </param>
        /// <param name="Password"> 帳戶密碼。 </param>
        /// <returns> XErrorCode 辨識碼。 </returns>
        public static int ReConnection(string DiskCode, string UNC, string Account, string Password)
        {
            int iStt = 0;
            try
            {
                // 先進行斷線 。
                if (DiskCode != "")
                {
                    DisconnectDrive(DiskCode, true, true);
                }
                else
                {
                    DisconnectDrive(UNC, true, true);
                }

                // 設定各項資訊 。
                object persistent = true;
                object user = Account;
                object pwd = Password;
                //重新連線
                g_NetworkShell.MapNetworkDrive(DiskCode, UNC, ref persistent, ref user, ref pwd);
                Thread.Sleep(1000);
                return iStt;
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return -1;
            }
        }

        [DllImport("mpr.dll", EntryPoint = "WNetAddConnectionA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern long WNetAddConnection(string lpszNetPath, string lpszPassword, string lpszLocalName);

        /// <summary> 提供一個網路磁碟機連線的功能。 </summary>
        /// <param name="UNC"> 連線位址。 </param>
        /// <param name="Password"> 帳戶密碼。 </param>
        /// <param name="DiskCode"> 磁碟機代號。 </param>
        /// <returns> XErrorCode 辨識碼。 </returns>
        public static int ReConnection(string UNC, string Password, string DiskCode)
        {
            try
            {
                int iStt = 0;
                long iSuccess = -1;
                iSuccess = WNetAddConnection(UNC, Password, DiskCode);
                Thread.Sleep(1000);
                if (iSuccess == 0)
                {
                    return iStt;
                }
                return -1;
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodBase.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return -1;
            }
        }

        /// <summary> 將網路磁碟機斷線。 </summary>
        /// <param name="UNC_or_DriveName">UNC 或 DiskCode 擇一。</param>
        /// <param name="willForce"> 預設都設 true。 </param>
        /// <param name="isPersistent"> 預設都設 true。 </param>
        public static void DisconnectDrive(string UNC_or_DriveName, bool willForce, bool isPersistent)
        {
            try
            {
                object force = willForce;
                object updateProfile = isPersistent;
                g_NetworkShell.RemoveNetworkDrive(UNC_or_DriveName, ref force, ref updateProfile);
            }
            catch
            {
            }
        }

        /// <summary>取得所有網卡資訊</summary>
        /// <returns>網卡資訊陣列</returns>
        public static List<NetworkInfo> GetAllNetworkInfo()
        {
            List<NetworkInfo> clsNetworkInfos = new List<NetworkInfo>();
            NetworkInterface[] Networks = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in Networks)
            {
                if (adapter.NetworkInterfaceType.ToString().Equals("Ethernet"))
                {
                    IPInterfaceProperties ipProperties = adapter.GetIPProperties();
                    if (ipProperties.UnicastAddresses.Count > 0)
                    {
                        NetworkInfo clsNetworkInfo = new NetworkInfo();

                        clsNetworkInfo.strMAC = adapter.GetPhysicalAddress().ToString();
                        clsNetworkInfo.strName = adapter.Name; 
                        clsNetworkInfo.strDescription = adapter.Description;
                        clsNetworkInfo.strIP = ipProperties.UnicastAddresses[1].Address.ToString();
                        clsNetworkInfo.strNetmask = ipProperties.UnicastAddresses[1].IPv4Mask.ToString();
                        clsNetworkInfos.Add(clsNetworkInfo);
                    }
                }
            }
            return clsNetworkInfos;
        }
        /// <summary>取得所有 MAC 位址。</summary>
        /// <returns>回傳所有 MAC 位址。</returns>
        public static List<string> GetAllMacAddress()
        {
            List<string> strMacAddress = new List<string>();
            try
            {
                NetworkInterface[] clsNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface clsNetworkInterface in clsNetworkInterfaces)
                {
                    string strMac = clsNetworkInterface.GetPhysicalAddress().ToString();
                    if (strMac.Length == 12)
                    {
                        strMac = strMac.Insert(10, ":");
                        strMac = strMac.Insert(8, ":");
                        strMac = strMac.Insert(6, ":");
                        strMac = strMac.Insert(4, ":");
                        strMac = strMac.Insert(2, ":");
                        strMacAddress.Add(strMac);
                    }
                }
            }
            catch
            {
                strMacAddress.Clear();
            }
            return strMacAddress;
        }

        /// <summary>取得當前使用的 MAC 位址。</summary>
        /// <returns>回傳當前使用的 MAC 位址。</returns>
        public static string GetUsedMacAddress()
        {
            string strMacAddres = "";
            try
            {
                ManagementClass clsManagementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection clsManagementObjectCollection = clsManagementClass.GetInstances();
                foreach (ManagementObject clsManagementObject in clsManagementObjectCollection)
                {
                    if ((bool)clsManagementObject["IPEnabled"] == true)
                    {
                        strMacAddres = clsManagementObject["MacAddress"].ToString();
                        break;
                    }
                }
                clsManagementObjectCollection.Dispose();
                clsManagementClass.Dispose();
            }
            catch
            {
                strMacAddres = "unknow";
            }
            return strMacAddres;
        }

        /// <summary>取得 CPU ID 。</summary>
        /// <returns>回傳 CPU ID 。</returns>
        public static string GetCpuID()
        {
            string strCpuID = "unknow";
            try
            {
                ManagementClass clsManagementClass = new ManagementClass("Win32_Processor");
                ManagementObjectCollection clsManagementObjectCollection = clsManagementClass.GetInstances();
                foreach (ManagementObject clsManagementObject in clsManagementObjectCollection)
                {
                    strCpuID = clsManagementObject.Properties["ProcessorId"].Value.ToString();
                }
                clsManagementObjectCollection.Dispose();
                clsManagementClass.Dispose();
            }
            catch
            {
                strCpuID = "unknow";
            }
            return strCpuID;
        }

        /// <summary>使用 DOS 下 Command的功能。</summary>
        /// <param name="strCommand">命令。（這邊一次只會執行一行，如需多行指令請用 && 隔開，如 [AAA] && [BBB]）</param>
        /// <param name="strReturnMessage">回傳的訊息。</param>
        /// <param name="iTimeout">逾時（預設為 300 秒）。</param>
        /// <returns>回傳錯誤碼。</returns>
        public static int DosCommand(string strCommand, out string strReturnMessage, int iTimeout = 300)
        {
            int iStt = 0;
            strReturnMessage = ""; //输出字符串
            if (strCommand != null && !strCommand.Equals(""))
            {
                Process clsProcess = new Process(); // 建立程序
                ProcessStartInfo clsStartInfo = new ProcessStartInfo();
                clsStartInfo.FileName = "cmd.exe"; //設定要執行的命令
                clsStartInfo.Arguments = "/C" + strCommand; // "/C" 執行完馬上結束
                clsStartInfo.UseShellExecute = false;
                clsStartInfo.RedirectStandardInput = false;
                clsStartInfo.RedirectStandardOutput = true;
                clsStartInfo.CreateNoWindow = true;
                clsProcess.StartInfo = clsStartInfo;
                try
                {
                    if (clsProcess.Start()) // 開始執行
                    {
                        if (iTimeout == 0)
                        {
                            clsProcess.WaitForExit(); // 等待程序結束
                        }
                        else
                        {
                            clsProcess.WaitForExit(iTimeout); //指定時間內結束程序
                        }
                        strReturnMessage = clsProcess.StandardOutput.ReadToEnd(); // 取得回傳字串
                    }
                }
                catch (Exception ex)
                {
                    XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                    iStt = -1;
                }
                finally
                {
                    if (clsProcess != null)
                        clsProcess.Close();
                }
            }
            return iStt;
        }


        /// <summary>將視窗最上層</summary>
        /// <param name="hWnd">視窗Handle</param>
        /// <param name="fAltTab">是否使用Alt+Tab</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        //設定視窗風格
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32")]
        public static extern int ShowWindow(int hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern Boolean GetWindowRect(IntPtr hWnd, ref Rectangle bound);

        public static bool CaptureWindowByProcessName(string ProcessName,string strSavePath)
        {
            //取得ProcessName
            Process[] process = Process.GetProcessesByName(ProcessName);

            //是否有相符ProcessName 否則跳出
            if (process.Length == 0) return false;

            /* 取得該視窗的大小與位置 */
            Rectangle bound = new Rectangle();

            SwitchToThisWindow(process[0].MainWindowHandle, true);
            Thread.Sleep(200);
            GetWindowRect(process[0].MainWindowHandle, ref bound);
            bound = new Rectangle(bound.X, bound.Y, bound.Width - bound.X, bound.Height - bound.Y);
            /* 抓取截圖 */
            Bitmap screenshot = new Bitmap(bound.Width, bound.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics gfx = Graphics.FromImage(screenshot);
            gfx.CopyFromScreen(bound.X, bound.Y, 0, 0, bound.Size, CopyPixelOperation.SourceCopy);
            screenshot.Save(strSavePath,System.Drawing.Imaging.ImageFormat.Jpeg);

            return true;                   
        }
        #endregion


        /// <summary>系統工具列</summary>
        public class AppBar
        {
            #region "AppBarTest"

            [StructLayout(LayoutKind.Sequential)]
            struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct APPBARDATA
            {
                public int cbSize;
                public IntPtr hWnd;
                public int uCallbackMessage;
                public int uEdge;
                public RECT rc;
                public IntPtr lParam;
            }

            enum ABMsg : int
            {
                ABM_NEW = 0,
                ABM_REMOVE,
                ABM_QUERYPOS,
                ABM_SETPOS,
                ABM_GETSTATE,
                ABM_GETTASKBARPOS,
                ABM_ACTIVATE,
                ABM_GETAUTOHIDEBAR,
                ABM_SETAUTOHIDEBAR,
                ABM_WINDOWPOSCHANGED,
                ABM_SETSTATE
            }

            enum ABNotify : int
            {
                ABN_STATECHANGE = 0,
                ABN_POSCHANGED,
                ABN_FULLSCREENAPP,
                ABN_WINDOWARRANGE
            }

            public enum ABEdge : int
            {
                ABE_LEFT = 0,
                ABE_TOP,
                ABE_RIGHT,
                ABE_BOTTOM
            }

            public enum AppBarStates
            {
                AlwaysOnTop = 0x00,
                AutoHide = 0x01
            }

            [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
            static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
            [DllImport("USER32")]
            static extern int GetSystemMetrics(int Index);
            [DllImport("User32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);
            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            static extern int RegisterWindowMessage(string msg);

            public static bool Registered
            {
                get { return fBarRegistered; }
            }
            private static bool fBarRegistered = false;
            private static int uCallBack;

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetWindowPos(IntPtr hWnd,
                int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

            private const int HWND_TOPMOST = -1;
            private const int HWND_NOTOPMOST = -2;
            private const int SWP_NOMOVE = 0x0002;
            private const int SWP_NOSIZE = 0x0001;
            private const int SWP_NOACTIVATE = 0x0010;

            #endregion

            /// <summary>向系統註冊系統工具列</summary>
            /// <param name="frmForm">工具列 form</param>
            /// <param name="BarStates">工具列狀態</param>
            public static void RegisterBar(Form frmForm, AppBarStates BarStates, ABEdge eABEdge = ABEdge.ABE_TOP)
            {
                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = frmForm.Handle;
                abd.lParam = (IntPtr)BarStates;

                if (!fBarRegistered)
                {
                    uCallBack = RegisterWindowMessage("AppBarMessage");
                    abd.uCallbackMessage = uCallBack;

                    uint ret = SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
                    fBarRegistered = true;
                    ABSetPos(frmForm, eABEdge);
                }
                else
                {
                    SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
                    fBarRegistered = false;
                }
            }

            /// <summary>更新Form位置</summary>
            /// <param name="frmForm">工具列 form</param>
            public static void ABSetPos(Form frmForm, ABEdge eABEdge = ABEdge.ABE_TOP)
            {
                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = frmForm.Handle;
                abd.uEdge = (int)eABEdge;

                if (abd.uEdge == (int)ABEdge.ABE_LEFT || abd.uEdge == (int)ABEdge.ABE_RIGHT)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = SystemInformation.PrimaryMonitorSize.Height;
                    if (abd.uEdge == (int)ABEdge.ABE_LEFT)
                    {
                        abd.rc.left = 0;
                        abd.rc.right = frmForm.Size.Width;
                    }
                    else
                    {
                        abd.rc.right = SystemInformation.PrimaryMonitorSize.Width;
                        abd.rc.left = abd.rc.right - frmForm.Size.Width;
                    }

                }
                else
                {
                    abd.rc.left = 0;
                    abd.rc.right = SystemInformation.PrimaryMonitorSize.Width;
                    if (abd.uEdge == (int)ABEdge.ABE_TOP)
                    {
                        abd.rc.top = 0;
                        abd.rc.bottom = frmForm.Size.Height;
                    }
                    else
                    {
                        abd.rc.bottom = SystemInformation.PrimaryMonitorSize.Height;
                        abd.rc.top = abd.rc.bottom - frmForm.Size.Height;
                    }
                }

                // Query the system for an approved size and position.
                SHAppBarMessage((int)ABMsg.ABM_QUERYPOS, ref abd);

                // Adjust the rectangle, depending on the edge to which the
                // appbar is anchored.
                switch (abd.uEdge)
                {
                    case (int)ABEdge.ABE_LEFT:
                        abd.rc.right = abd.rc.left + frmForm.Size.Width;
                        break;
                    case (int)ABEdge.ABE_RIGHT:
                        abd.rc.left = abd.rc.right - frmForm.Size.Width;
                        break;
                    case (int)ABEdge.ABE_TOP:
                        abd.rc.bottom = abd.rc.top + frmForm.Size.Height;
                        break;
                    case (int)ABEdge.ABE_BOTTOM:
                        abd.rc.top = abd.rc.bottom - frmForm.Size.Height;
                        break;
                }
                // Pass the final bounding rectangle to the system.
                SHAppBarMessage((int)ABMsg.ABM_SETPOS, ref abd);

                // Move and size the appbar so that it conforms to the
                // bounding rectangle passed to the system.
                MoveWindow(frmForm.Handle, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
            }

            public static void MoveWin(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint)
            {
                MoveWindow(hWnd, x, y, cx, cy, repaint);
            }
        }

        public class XWinApiMessage
        {

            #region " Properties "

            /// <summary> 取得或設定，這個 XWinApiMessage 的目標名稱。</summary>
            public string TargetID { get { return g_tTargetID.ToString(); } set { g_tTargetID = new IntPtr(int.Parse(value)); } }
            private IntPtr g_tTargetID;

            #endregion

            #region " Definition - Dll Improt "

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport("user32.dll")]
            private static extern long SendMessage(IntPtr hWnd, uint uiMessageType, uint wParam, ref COPYDATASTRUCT lParam);

             [DllImport("User32.dll", EntryPoint = "PostMessage")]
            private static extern long PostMessage(IntPtr hWnd, uint uiMessageType, uint wParam, ref COPYDATASTRUCT lParam);

            //PInvoke declarations
            [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                          string lpszClassName,
                                                          string lpszWindowName,
                                                          int style,
                                                          int x, int y,
                                                          int width, int height,
                                                          IntPtr hwndParent,
                                                          IntPtr hMenu,
                                                          IntPtr hInst,
                                                          [MarshalAs(UnmanagedType.AsAny)] object pvParam);


            /// <summary>視窗傳遞訊息的基礎結構。</summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct COPYDATASTRUCT
            {
                public IntPtr Header { get; set; }
                public int Length { get; set; }
                public IntPtr String { get; set; }
            }

            /// <summary>訊息類別(預設為 0x004A )。</summary>
            public uint MessageType
            {
                get { return g_uiMessageType; }
                set { g_uiMessageType = value; }
            }
            private uint g_uiMessageType = 0x004A;

            #endregion

            #region " Method - New "

            /// <summary>直接建構一個初始化 XWinApiMessage 類別的新執行個體，並設定目標名稱的初始位置。</summary>
            public XWinApiMessage()
            {
                g_tTargetID = IntPtr.Zero;
            }

            /// <summary>直接建構一個初始化 XWinApiMessage 類別的新執行個體，並設定目標名稱的初始位置。</summary>
            /// <param name="tTargetID">目標視窗 ID 。</param>
            public XWinApiMessage(IntPtr tTargetID)
            {
                g_tTargetID = tTargetID;
            }

            #endregion

            #region " Method - Send "

            /// <summary>發送訊息。</summary>
            /// <param name="iWindowUID">對方的視窗 ID 。</param>
            /// <param name="strMessage">訊息內容。</param>
            private void Send(int iWindowUID, string strMessage)
            {
                IntPtr tWindowUID = new IntPtr(iWindowUID);
                if (tWindowUID != IntPtr.Zero && strMessage.Trim() != "")
                {
                    try
                    {
                        IntPtr tString = Marshal.StringToHGlobalAnsi(strMessage);
                        COPYDATASTRUCT tData = new COPYDATASTRUCT();
                        tData.Header = IntPtr.Zero;
                        tData.Length = strMessage.Length;
                        tData.String = tString;

                        long result = SendMessage(tWindowUID, g_uiMessageType, 0, ref tData);
                        Marshal.FreeHGlobal(tString);
                    }
                    catch (Exception ex)
                    {
                        string strEx = ex.Message;
                    }
                }
            }

            /// <summary>發送訊息。</summary>
            /// <param name="iWindowUID">對方的視窗 ID 。</param>
            /// <param name="strMessage">訊息內容。</param>
            public void SendP(int iWindowUID, string strMessage)
            {
                IntPtr tWindowUID = new IntPtr(iWindowUID);
                if (tWindowUID != IntPtr.Zero && strMessage.Trim() != "")
                {
                    try
                    {
                        IntPtr tString = Marshal.StringToHGlobalAnsi(strMessage);
                        COPYDATASTRUCT tData = new COPYDATASTRUCT();
                        tData.Header = IntPtr.Zero;
                        tData.Length = strMessage.Length;
                        tData.String = tString;

                        long result = PostMessage(tWindowUID, g_uiMessageType, 0, ref tData);
                        Marshal.FreeHGlobal(tString);
                    }
                    catch (Exception ex)
                    {
                        string strEx = ex.Message;
                    }
                }
            }

            /// <summary>發送訊息。</summary>
            /// <param name="strWindowTittle">對方的視窗標題。</param>
            /// <param name="strMessage">訊息內容。</param>
            private void Send(string strWindowTittle, string strMessage)
            {
                IntPtr tWindowUID = FindWindow(null, strWindowTittle);

                if (tWindowUID == IntPtr.Zero)
                {
                    tWindowUID = XWinApiMessage.FindWindowID(strWindowTittle);
                }

                if (tWindowUID != IntPtr.Zero && strMessage.Trim() != "")
                {
                    try
                    {
                        IntPtr tString = Marshal.StringToHGlobalAnsi(strMessage);
                        COPYDATASTRUCT tData = new COPYDATASTRUCT();
                        tData.Header = IntPtr.Zero;
                        tData.Length = strMessage.Length;
                        tData.String = tString;

                        long result = SendMessage(tWindowUID, g_uiMessageType, 0, ref tData);
                        Marshal.FreeHGlobal(tString);
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

            /// <summary>發送訊息。</summary>
            /// <param name="strMessage">訊息內容。</param>
            /// <param name="uiParam">額外參數。</param>
            public void Send(string strMessage, uint uiParam = 0)
            {

                IntPtr tWindowUID = g_tTargetID;//找TestB的IntPtr 用來代表指標或控制代碼
                if (tWindowUID != IntPtr.Zero && strMessage.Trim() != "")
                {
                    try
                    {
                        IntPtr tString = Marshal.StringToHGlobalAnsi(strMessage);
                        COPYDATASTRUCT tData = new COPYDATASTRUCT();

                        tData.Header = IntPtr.Zero;
                        tData.Length = strMessage.Length;
                        tData.String = tString;

                        long result = SendMessage(tWindowUID, g_uiMessageType, uiParam, ref tData);
                        Marshal.FreeHGlobal(tString);

                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            /// <summary>發送訊息。</summary>
            /// <param name="strMessage">訊息內容。</param>
            /// <param name="uiParam">額外參數。</param>
            public static void Send(IntPtr tWindowUID, string strMessage, uint uiParam = 0)
            {
                if (tWindowUID != IntPtr.Zero && strMessage.Trim() != "")
                {
                    try
                    {
                        IntPtr tString = Marshal.StringToHGlobalAnsi(strMessage);
                        COPYDATASTRUCT tData = new COPYDATASTRUCT();

                        byte[] byteStr = Encoding.GetEncoding("big5").GetBytes(strMessage);

                        tData.Header = IntPtr.Zero;
                        tData.Length = byteStr.Length;
                        tData.String = tString;

                        long result = SendMessage(tWindowUID, 0x004A, uiParam, ref tData);
                        Marshal.FreeHGlobal(tString);

                    }
                    catch (Exception ex)
                    {
                    }
                }
                //Task.Factory.StartNew(() =>
                //{
                //    if (tWindowUID != IntPtr.Zero && strMessage.Trim() != "")
                //    {
                //        try
                //        {
                //            IntPtr tString = Marshal.StringToHGlobalAnsi(strMessage);
                //            COPYDATASTRUCT tData = new COPYDATASTRUCT();

                //            byte[] byteStr = Encoding.GetEncoding("big5").GetBytes(strMessage);

                //            tData.Header = IntPtr.Zero;
                //            tData.Length = byteStr.Length;
                //            tData.String = tString;

                //            long result = SendMessage(tWindowUID, 0x004A, uiParam, ref tData);
                //            Marshal.FreeHGlobal(tString);

                //        }
                //        catch (Exception ex)
                //        {
                //        }
                //    }
                //});
            }

            /// <summary>發送訊息。</summary>
            /// <param name="strMessage">訊息內容。</param>
            /// <param name="uiParam">額外參數。</param>
            public static void SendP(IntPtr tWindowUID, string strMessage, uint uiParam = 0)
            {

                if (tWindowUID != IntPtr.Zero && strMessage.Trim() != "")
                {
                    try
                    {
                        IntPtr tString = Marshal.StringToHGlobalAnsi(strMessage);
                        COPYDATASTRUCT tData = new COPYDATASTRUCT();

                        byte[] byteStr = Encoding.GetEncoding("big5").GetBytes(strMessage);

                        tData.Header = IntPtr.Zero;
                        tData.Length = byteStr.Length;
                        tData.String = tString;

                        long result = PostMessage(tWindowUID, 0x004A, uiParam, ref tData);
                        Marshal.FreeHGlobal(tString);

                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            #endregion

            #region " Method - Receive "

            /// <summary>接收訊息。</summary>
            /// <param name="iMessageType">視窗訊息類別。</param>
            /// <param name="tMessage">Windows 視窗訊息。</param>
            /// <returns>接收到的訊息。</returns>
            public static string Receive(int iMessageType, ref Message tMessage)
            {
                string strMessage = "";

                try
                {
                    if (tMessage.Msg == iMessageType)
                    {
                        string strCmdLine = string.Empty;
                        COPYDATASTRUCT tData = new COPYDATASTRUCT();
                        tData = (COPYDATASTRUCT)Marshal.PtrToStructure(tMessage.LParam, typeof(COPYDATASTRUCT));
                        if (tData.Length > 0)
                        {
                            strMessage = Marshal.PtrToStringAnsi(tData.String, tData.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    strMessage = null;
                }

                return strMessage;
            }

            #endregion

            #region " Methods "

            /// <summary>找尋視窗的 ID 。</summary>
            /// <param name="strTitle">視窗名稱。</param>
            /// <returns>視窗的 ID 指標</returns>
            public static IntPtr FindWindowID(string strTitle)
            {
                IntPtr tThis = IntPtr.Zero;

                Process[] clsProcessList = Process.GetProcesses();

                foreach (Process clsThisProcess in clsProcessList)
                {
                    if (clsThisProcess.MainWindowTitle.Contains(strTitle))
                    {
                        tThis = FindWindow(null, clsThisProcess.MainWindowTitle);
                        break;
                    }
                }

                return tThis;
            }

            /// <summary>判斷某一個視窗是否存在。</summary>
            /// <param name="strWindowsName">視窗名稱，必須完全一模一樣。</param>
            /// <returns>回傳錯誤碼。</returns>
            public static bool IsWindowsExist(string strWindowsName)
            {
                bool bIsExist = false;
                try
                {
                    IntPtr tThis = IntPtr.Zero;
                    tThis = FindWindow(null, strWindowsName);
                    bIsExist = (tThis != IntPtr.Zero) ? true : false;
                }
                catch (Exception ex)
                {
                }
                return bIsExist;
            }

            #endregion

        }

        public class WindowProc
        {
            public const int GWL_WNDPROC = -4;          //供GetWindowLong 和SetWindowLong 使用
            public static int oldWindow = 0;

            public delegate int CallWindowProcDelegate(int Wnd, int Msg, int WParam, int LParam);
            public CallWindowProcDelegate MyCallWindowProc;

            public delegate void CallWndProc(string strData);
            public event CallWndProc MyCallWndProc;

            [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                          string lpszClassName,
                                                          string lpszWindowName,
                                                          int style,
                                                          int x, int y,
                                                          int width, int height,
                                                          IntPtr hwndParent,
                                                          IntPtr hMenu,
                                                          IntPtr hInst,
                                                          [MarshalAs(UnmanagedType.AsAny)] object pvParam);

            [DllImport("user32.dll")]
            protected static extern int GetWindowLong(int hwindow, int unindex);
            [DllImport("user32.dll")]
            protected static extern int CallWindowProc(int lpPrevWndFunc, int hWnd, int Msg, int wParam, int lParam);
            [DllImport("user32.dll")]
            protected static extern int SetWindowLong(int hwindow, int unindex, CallWindowProcDelegate lnewvalue);


            public IntPtr WindowsHandle
            {
                get { return iWindowsHandle; }
            }
            private IntPtr iWindowsHandle;


            public WindowProc(string strClassName, string strWindowsName, IntPtr iHandle)
            {
                iWindowsHandle = CreateWindowEx(0, strClassName, strWindowsName, 0, 0, 0, 0, 0, (IntPtr)0, (IntPtr)0, iHandle, null);
                oldWindow = GetWindowLong((int)iWindowsHandle, GWL_WNDPROC);

                MyCallWindowProc = new CallWindowProcDelegate(WndProc);
                SetWindowLong((int)iWindowsHandle, GWL_WNDPROC, MyCallWindowProc);
            }

            private int WndProc(int Wnd, int Msg, int WParam, int LParam)
            {
                Message m = new System.Windows.Forms.Message();
                m.HWnd = (IntPtr)Wnd;
                m.Msg = Msg;
                m.WParam = (IntPtr)WParam;
                m.LParam = (IntPtr)LParam;

                if (MyCallWndProc != null)
                {
                    if (Msg == 0x4A)
                        MyCallWndProc(XWinApiMessage.Receive(Msg, ref m));
                }

                //拋回底層繼續做
                return CallWindowProc(oldWindow, (int)m.HWnd, (int)m.Msg, (int)m.WParam, (int)m.LParam);
            }
        }

        public class NetworkInfo
        {
            public string strIP;
            public string strMAC;
            public string strDescription;
            public string strName;
            public string strNetmask;
        }
    }
}
