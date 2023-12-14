using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using GeneralUtility.IO;

#region " Common Tool "

/// <summary>含有事件資料的物件。</summary>
public class XEventArgs : EventArgs
{

	#region " Properties "

	/// <summary>訊息資料。</summary>
	public string Message { get; set; }

	#endregion

	#region " Methods - New "

	/// <summary>建構訊息資料。</summary>
	public  XEventArgs()
	{
		this.Message = "";
	}

	/// <summary>建構訊息資料。</summary>
	/// <param name="strMessage">訊息。</param>
	public  XEventArgs(string strMessage)
	{
		this.Message = strMessage;
	}

	#endregion

}

/// <summary>事件委派。</summary>
/// <param name="sender">物件。</param>
/// <param name="e">事件物件。</param>
public delegate void XEventHandler(object sender, XEventArgs e);

/// <summary>鍵盤Key專用事件。</summary>
public class XKeysEventArgs : XEventArgs
{
	/// <summary>鍵盤Key資料。</summary>
	public Keys Key { get; set; }
}



/// <summary>事件委派。</summary>
/// <param name="sender">物件。</param>
/// <param name="e">XKeysEventHandler事件物件。</param>
public delegate void XKeysEventHandler(object sender, XKeysEventArgs e);

/// <summary>影像滑鼠物件。</summary>
public class XPictureMouseEventArgs : XEventArgs
{

    #region " Properties "

    /// <summary>滑鼠按下按鍵。</summary>
    public MouseButtons Button { get { return g_eButton; } }
    private MouseButtons g_eButton;

    /// <summary>滑鼠點擊次數。</summary>
    public int Clicks { get { return g_iClicks; } }
    private int g_iClicks;

    /// <summary>滑鼠旋轉次數。</summary>
    public int Delta { get { return g_iDelta; } }
    private int g_iDelta;

    /// <summary>圖片點位置。</summary>
    public Point PicturePoint { get { return g_tPicturePoint; } }
    private Point g_tPicturePoint;

    /// <summary>圖片X點位置。</summary>
    public int PictureX { get { return g_iPictureX; } }
    private int g_iPictureX;

    /// <summary>圖片Y點位置。</summary>
    public int PictureY { get { return g_iPictureY; } }
    private int g_iPictureY;

    /// <summary>真實點位置。</summary>
    public Point RealPoint { get { return g_tRealPoint; } }
    private Point g_tRealPoint;

    /// <summary>真實X點位置。</summary>
    public int RealX { get { return g_iRealX; } }
    private int g_iRealX;

    /// <summary>真實Y點位置。</summary>
    public int RealY { get { return g_iRealY; } }
    private int g_iRealY;

    #endregion

    #region " Methods - New "
    /// <summary>影像滑鼠物件。</summary>
    /// <param name="eButton">滑鼠按鍵。</param>
    /// <param name="iClick">點擊次數。</param>
    /// <param name="tPicturePoint">圖片的點位。</param>
    /// <param name="tRealPoint">真實的點位。</param>
    /// <param name="iDelta">滑鼠旋轉次數。</param>
    public XPictureMouseEventArgs(MouseButtons eButton,int iClick, Point tPicturePoint, Point tRealPoint, int iDelta)
    {
        g_eButton = eButton;
        g_iClicks = iClick;
        g_iDelta = iDelta;
        g_tPicturePoint = tPicturePoint;
        g_iPictureX = tPicturePoint.X;
        g_iPictureY = tPicturePoint.Y;
        g_tRealPoint = tRealPoint;
        g_iRealX = tRealPoint.X;
        g_iRealY = tRealPoint.Y;
    }

    #endregion

}

/// <summary>事件委派。</summary>
/// <param name="sender">物件。</param>
/// <param name="e">事件物件。</param>
public delegate void XPictureMouseEventHandler(object sender, XPictureMouseEventArgs e);


#endregion

namespace GeneralUtility
{
    /// <summary>系統狀態的回報中心，用來紀錄各種不同類型的資訊，並且利用內建功能進行存檔的動作，以利未來追蹤錯誤。</summary>
    public static class XStatus
    {

        #region " Definition "

        public const string TIME_FORMAT = "yyyy/MM/dd HH:mm:ss.fff";

        /// <summary>狀態類型。</summary>
        public enum Type
        {
            /// <summary>硬體裝置相關。</summary>
            Device,					// Device
            /// <summary></summary>
            Procedure,				// Abnormal procedure
            /// <summary>運動裝置卡相關。</summary>
            Motion,					// Motion relation
            /// <summary>影像演算法相關。</summary>
            Vision,					// Vision relation
            /// <summary>DIO 卡、A/D、D/A 卡相關。</summary>
            IO,                     // External IO relation such as A/D,D/A,DIO
            /// <summary>PLC通訊相關。</summary>
            PLC,
            /// <summary>通訊相關。</summary>
            Communication,			// Communication(RS-xxx, Tcp/IP)
            /// <summary>機器手臂相關。</summary>
            Robot,					// Robot relation
            /// <summary>Windows API 相關。</summary>
            Windows,				// Windows API or framework
            /// <summary>使用者介面 (GUI) 相關。</summary>
            Events,					// User interface(GUI)
            /// <summary>資訊相關。</summary>
            Information,			// This is not error message
            /// <summary>無。</summary>
            Null,

        }

        /// <summary>紀錄區間。</summary>
        public enum RecordInterval
        {
            /// <summary>無。</summary>
            None,
            /// <summary>以小時為區間單位。</summary>
            Hours,
            /// <summary>以日為區間單位。</summary>
            Days,
        }

        public enum SaveItems
        {
            Default,
            Immediate,
            NoImmediate
        }

        public enum ErrorLevel
        {
            Alarm,
            Warring,
            Nothing
        }

        #endregion

        #region " Properties "

        private static RecordInterval g_eRecordType = RecordInterval.None;
        public static RecordInterval RecordType
        {
            set { g_eRecordType = value; }
        }

        private static string g_strLogFolderPath = string.Empty;
        public static string LogFolderPath
        {
            get { return g_strLogFolderPath; }
            set { g_strLogFolderPath = value; }
        }

        public static List<string> LogMessage = new List<string>();
        private static List<string> g_strReserveMessages = new List<string>();
        private static uint g_iReserveMessageThreshold = 1;

        private static object g_oLockReport = new object();
        private static object g_oLockReserveMessage = new object();

        public static XLog LogRecorder;
        public static bool SendLogRecorder = false;

        #endregion

        #region " Events - Delegate "

        public delegate void XStatusEventHandler(string strMessage);

        /// <summary>當 有資訊回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_Changed;

        /// <summary>當 有Alarm等級的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_AlarmReport;

        /// <summary>當 有Device類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_DeviceReport;

        /// <summary>當 有Procedure類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_ProcedureReport;

        /// <summary>當 有Motion類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_MotionReport;

        /// <summary>當 有Vision類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_VisionReport;

        /// <summary>當 有IO類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_IOReport;

        /// <summary>當 有Communication類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_CommunicationReport;

        /// <summary>當 有Robot類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_RobotReport;

        /// <summary>當 有Windows類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_WindowsReport;

        /// <summary>當 有Events類型的錯誤訊息回報時，要進行的事件觸發項目。</summary>
        public static event XStatusEventHandler Status_EventsReport;

        #endregion

        #region " Methods - Log Process "

        /// <summary>
        /// <para>如果要記錄Log檔案，那就要先設定Log檔案的專屬資料夾，否則Log不會被記錄到檔案中。	</para>
        /// <para>紀錄時可以設定要以天為一個檔案或者小時為一個檔案。								</para>
        /// <para>預設的資料夾是跟執行檔同一個資料夾，再去建構一個新的資料夾來做存放的動作。		</para>
        /// </summary>
        /// <param name="strLogFolderPath">Log檔案的資料夾路徑。</param>
        /// <param name="eRecoderType">紀錄類別。</param>
        /// <param name="iReserveMessageThreshold">紀錄多少筆資料後才進行寫成檔案的動作（預設：1）。</param>
        public static void LogFileSetup(string strLogFolderPath = @".\log\", RecordInterval eRecoderType = RecordInterval.Days, uint iReserveMessageThreshold = 1)
        {
            if (!XFile.IsExist_Folder(strLogFolderPath)) XFile.CreateFolder(strLogFolderPath);
            g_strLogFolderPath = strLogFolderPath;
            g_eRecordType = eRecoderType;
            g_iReserveMessageThreshold = iReserveMessageThreshold;
        }

        /// <summary>輸入一個例外處理訊息，可以直接取特定的資料來分析錯誤的行數。 </summary>
        /// <param name="ex">意外事件。</param>
        public static string GetExceptionLine(Exception ex)
        {
            return ex.StackTrace.Split(':')[ex.StackTrace.Split(':').Length - 1] + " : " + ex.Message;
        }

        /// <summary>
        /// 提供一個訊息回報的機制或者用以記錄一些資訊，取得特定的必要資料，讓未來取得更為方便的除錯工具。
        /// </summary>
        /// <param name="eType">這個回報訊息的類型。</param>
        /// <param name="clsFunction">請鍵入 MethodInfo.GetCurrentMethod()。</param>
        /// <param name="strMessage">要記錄的主要訊息。</param>
        /// <param name="eSaveItems">儲存寫檔的時機點。</param>
        /// <example>
        /// <code>
        /// private void Example()
        /// {
        ///		try
        ///		{
        ///			// ============
        ///			// 你要得動作
        ///			// ============
        ///			
        ///			// 可以當成一般訊息紀錄使用 (很重要才紀錄)
        ///			XStatus.Report(XStatus.Type.Infomation, MethodInfo.GetCurrentMethod(),"想要紀錄的資訊");
        ///		}
        ///		catch(Exception ex)
        ///		{
        ///			XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
        ///		}
        /// }
        /// </code>
        /// </example>
        public static void Report(Type eType, MethodBase clsFunction, string strMessage, ErrorLevel eErrorLevel = ErrorLevel.Alarm, SaveItems eSaveItems = SaveItems.Default)
        {
            string strClassName = (clsFunction != null) ? "{" + clsFunction.ReflectedType.Name + "}" : "{Unknow}";
            string functionName = (clsFunction != null) ? strClassName + "[" + clsFunction.Name + "]" : "Unknow[Unknow]";

            string strAllMessage = DateTime.Now.ToString(TIME_FORMAT) + ", " + eType.ToString() + ", " + functionName + " - " + strMessage;

            if (LogRecorder != null && SendLogRecorder)
            {
                LogRecorder.WriteLog(eErrorLevel.ToString() + "|" + strAllMessage);
            }
            else
            {
                if (g_strLogFolderPath.Length > 3)
                {
                    string strFilePath = "";
                    switch (g_eRecordType)
                    {
                        case RecordInterval.None:
                            strFilePath = g_strLogFolderPath;
                            break;
                        case RecordInterval.Hours:
                            strFilePath = g_strLogFolderPath + "Log_" + DateTime.Now.ToString("yyyyMMddHH") + ".log";
                            break;
                        case RecordInterval.Days:
                            strFilePath = g_strLogFolderPath + "Log_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                            break;
                    }

                    lock (g_oLockReserveMessage)
                    {
                        try
                        {
                            if (eType != Type.Null)
                            {
                                // 在下面這段程式碼不直接呼叫其他功能函數，以免造成無窮回圈。
                                // 因為只有單行，所以在此允許插隊讓硬碟做此工作。
                                g_strReserveMessages.Add(strAllMessage);
                            }
                            lock (XFile.DiskLock)
                            {
                                switch (eSaveItems)
                                {
                                    case SaveItems.Default:
                                    case SaveItems.NoImmediate:
                                        if (g_strReserveMessages.Count >= g_iReserveMessageThreshold)
                                        {
                                            File.AppendAllLines(strFilePath, g_strReserveMessages);
                                            g_strReserveMessages.Clear();
                                        }
                                        break;
                                    case SaveItems.Immediate:
                                        File.AppendAllLines(strFilePath, g_strReserveMessages);
                                        g_strReserveMessages.Clear();
                                        break;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            lock (g_oLockReport)
            {
                try
                {
                    LogMessage.Insert(0, strAllMessage);
                    while (LogMessage.Count > 500)
                    {
                        LogMessage.RemoveAt(500);
                    }
                }
                catch
                {
                }
            }

            // Events Process (Callback function)
            //-------------------------------------------------------------------
            try
            {
                // 此順序由 Frank on 2014.10.9 排出，以危險性高低來做排序。
                switch (eType)
                {
                    case Type.Motion:
                        if (Status_MotionReport != null) Status_MotionReport(strAllMessage);
                        break;
                    case Type.Robot:
                        if (Status_RobotReport != null) Status_RobotReport(strAllMessage);
                        break;
                    case Type.Device:
                        if (Status_DeviceReport != null) Status_DeviceReport(strAllMessage);
                        break;
                    case Type.IO:
                        if (Status_IOReport != null) Status_IOReport(strAllMessage);
                        break;
                    case Type.Communication:
                        if (Status_CommunicationReport != null) Status_CommunicationReport(strAllMessage);
                        break;
                    case Type.Vision:
                        if (Status_VisionReport != null) Status_VisionReport(strAllMessage);
                        break;
                    case Type.Procedure:
                        if (Status_ProcedureReport != null) Status_ProcedureReport(strAllMessage);
                        break;
                    case Type.Windows:
                        if (Status_WindowsReport != null) Status_WindowsReport(strAllMessage);
                        break;
                    case Type.Events:
                        if (Status_EventsReport != null) Status_EventsReport(strAllMessage);
                        break;
                    default:
                        if (Status_AlarmReport != null) Status_AlarmReport(strAllMessage);
                        break;
                }
                if (Status_Changed != null) Status_Changed(strAllMessage);
            }
            catch
            {
                strAllMessage = DateTime.Now.ToString() + ", Events, {XStatus}[Report] - Callback function error.";
                g_strReserveMessages.Add(strAllMessage);	// 放到暫存區等待存檔。
                LogMessage.Insert(0, strAllMessage);		// 插入歷史紀錄的暫存區域。
            }
        }

        public static void Report(string SavePath, string strType, MethodBase clsFunction, string strMessage, ErrorLevel eErrorLevel = ErrorLevel.Alarm, SaveItems eSaveItems = SaveItems.Default)
        {
            string strClassName = (clsFunction != null) ? "{" + clsFunction.ReflectedType.Name + "}" : "{Unknow}";
            string functionName = (clsFunction != null) ? strClassName + "[" + clsFunction.Name + "]" : "Unknow[Unknow]";

            string strAllMessage = DateTime.Now.ToString(TIME_FORMAT) + ", " + strType.ToString() + ", " + functionName + " - " + strMessage;

            if (LogRecorder != null && SendLogRecorder)
            {
                LogRecorder.WriteLog(eErrorLevel.ToString() + "|" + strAllMessage);
            }
            else
            {
                if (SavePath.Length > 3)
                {
                    string strFilePath = "";
                    switch (g_eRecordType)
                    {
                        case RecordInterval.None:
                            strFilePath = SavePath;
                            break;
                        case RecordInterval.Hours:
                            strFilePath = SavePath + "Log_" + DateTime.Now.ToString("yyyyMMddHH") + ".log";
                            break;
                        case RecordInterval.Days:
                            strFilePath = SavePath + "Log_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                            break;
                    }

                    lock (g_oLockReserveMessage)
                    {
                        try
                        {
                            if (strType != null && strType != "")
                            {
                                // 在下面這段程式碼不直接呼叫其他功能函數，以免造成無窮回圈。
                                // 因為只有單行，所以在此允許插隊讓硬碟做此工作。
                                g_strReserveMessages.Add(strAllMessage);
                            }
                            lock (XFile.DiskLock)
                            {
                                switch (eSaveItems)
                                {
                                    case SaveItems.Default:
                                    case SaveItems.NoImmediate:
                                        if (g_strReserveMessages.Count >= g_iReserveMessageThreshold)
                                        {
                                            File.AppendAllLines(strFilePath, g_strReserveMessages);
                                            g_strReserveMessages.Clear();
                                        }
                                        break;
                                    case SaveItems.Immediate:
                                        File.AppendAllLines(strFilePath, g_strReserveMessages);
                                        g_strReserveMessages.Clear();
                                        break;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            lock (g_oLockReport)
            {
                try
                {
                    LogMessage.Insert(0, strAllMessage);
                    while (LogMessage.Count > 500)
                    {
                        LogMessage.RemoveAt(500);
                    }
                }
                catch
                {
                }
            }

            // Events Process (Callback function)
            //-------------------------------------------------------------------
            try
            {
                if (Status_Changed != null) Status_Changed(strAllMessage);
            }
            catch
            {
                strAllMessage = DateTime.Now.ToString() + ", Events, {XStatus}[Report] - Callback function error.";
                g_strReserveMessages.Add(strAllMessage);	// 放到暫存區等待存檔。
                LogMessage.Insert(0, strAllMessage);		// 插入歷史紀錄的暫存區域。
            }
        }

        public static void Report(string strType, MethodBase clsFunction, string strMessage, ErrorLevel eErrorLevel = ErrorLevel.Alarm, SaveItems eSaveItems = SaveItems.Default)
        {
            Report(g_strLogFolderPath, strType, clsFunction, strMessage, eErrorLevel, eSaveItems);
        }
        /// <summary>直接顯示錯誤訊息視窗。</summary>
        public static void Show()
        {
            //InxGeneral.Dialog.XErrorHistoryViewer.ShowErrorHistory();
        }

        /// <summary>直接顯示錯誤訊息視窗。</summary>
        public static void ShowDialog()
        {
            //InxGeneral.Dialog.XErrorHistoryViewer.ShowDialogErrorHistory();
        }

        #endregion

    }
}