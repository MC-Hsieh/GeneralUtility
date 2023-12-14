using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneralUtility.IO;
using System.Threading;
using System.IO;

namespace GeneralUtility
{
    public class XXLog
    {
        public enum Type
        {
            Debug = 0,
            Info = 1,
            Warn = 2,
            Error = 3,
            Fatal = 4
        }

        public enum LogRecordType
        {
            Minute,
            Hour,
            Day,
        }

        /// <summary>分割方式</summary>
        public LogRecordType eLevel
        {
            get { return _RecordType; }
            set { _RecordType = value; }
        }
        private LogRecordType _RecordType = LogRecordType.Day;

        /// <summary>Log儲存路徑</summary>
        public  string LogPath
        {
            get { return _LogPath; }
        }
        private string _LogPath;

        /// <summary>Log檔前帶字串</summary>
        public string HeareName
        {
            get { return _HeareName; }
            set { _HeareName = value; }
        }
        private string _HeareName;

        /// <summary>Log時間儲存格式</summary>
        public string TimeFormat
        {
            get { return _TimeFormat; }
        }
        private string _TimeFormat = "yyyy/MM/dd HH:mm:ss.fff";

        /// <summary>存儲等級</summary>
        public Type Level
        {
            get { return _Level; }
            set { _Level = value; }
        }
        private Type _Level = Type.Info;

        private Queue<string> _LogTmp = new Queue<string>();

        private Thread _ThrScan;
        private bool _ThreadLeave = false;
        public XXLog(string LogPath)
        {
            _LogPath = LogPath;
            if (!XFile.IsExist_Folder(_LogPath))
            {
                if(XFile.CreateFolder(_LogPath))
                {
                    _ThrScan = new Thread(ScanTmp);
                    _ThrScan.Name = "ScanTmp";
                    _ThrScan.Start();
                }
            }
            else
            {
                _ThrScan = new Thread(ScanTmp);
                _ThrScan.Name = "ScanTmp";
                _ThrScan.Start();
            }
        }
        public void Close()
        {
            _ThreadLeave = true;
        }
        ~XXLog()
        {
            _ThreadLeave = true;
        }
        private void ScanTmp()
        {
            while (!_ThreadLeave)
            {
                lock (_LogTmp)
                {
                    if (_LogTmp.Count > 0)
                    {
                        string strSavePath = "";
                        switch (_RecordType)
                        {
                            case LogRecordType.Minute:
                                strSavePath = _LogPath + "\\" + _HeareName + "_" + DateTime.Now.ToString("yyyyMMddmm") + ".log";
                                break;
                            case LogRecordType.Hour:
                                strSavePath = _LogPath + "\\" + _HeareName + "_" + DateTime.Now.ToString("yyyyMMddHH") + ".log";
                                break;
                            case LogRecordType.Day:
                                strSavePath = _LogPath + "\\" + _HeareName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                                break;
                        }
                        File.AppendAllText(strSavePath, _LogTmp.Dequeue() + Environment.NewLine, Encoding.UTF8);
                    }
                }
                Thread.Sleep(10);
            }
        }

        public void Debug(string strLogData)
        {
            lock (_LogTmp)
            {
                if (_Level <= Type.Debug) _LogTmp.Enqueue(DateTime.Now.ToString(_TimeFormat) + ",Debug," + strLogData);
            }
        }
        public void Info(string strLogData)
        {
            lock (_LogTmp)
            {
                if (_Level <= Type.Info) _LogTmp.Enqueue(DateTime.Now.ToString(_TimeFormat) + ",Info," + strLogData);
            }
        }
        public void Warn(string strLogData)
        {
            lock (_LogTmp)
            {
                if (_Level <= Type.Warn) _LogTmp.Enqueue(DateTime.Now.ToString(_TimeFormat) + ",Warn," + strLogData);
            }
        }
        public void Error(string strLogData)
        {
            lock (_LogTmp)
            {
                if (_Level <= Type.Error) _LogTmp.Enqueue(DateTime.Now.ToString(_TimeFormat) + ",Error," + strLogData);
            }
        }
        public void Fatal(string strLogData)
        {
            lock (_LogTmp)
            {
                if (_Level <= Type.Fatal) _LogTmp.Enqueue(DateTime.Now.ToString(_TimeFormat) + ",Fatal," + strLogData);
            }
        }
        public void Other(string strLogData)
        {
            lock (_LogTmp)
            {
                _LogTmp.Enqueue(DateTime.Now.ToString(_TimeFormat) + ",Other," + strLogData);
            }
        }
        public bool Report(string strPath,string strHeareName, LogRecordType eRecord,string strReportData)
        {
            bool bReturn = true;
            try
            {
                if (!XFile.IsExist_Folder(strPath)) if (!XFile.CreateFolder(strPath)) return false;

                switch (eRecord)
                {
                    case LogRecordType.Minute:
                        strPath = strPath + "\\" + strHeareName + "_" + DateTime.Now.ToString("yyyyMMddmm") + ".log";
                        break;
                    case LogRecordType.Hour:
                        strPath = strPath + "\\" + strHeareName + "_" + DateTime.Now.ToString("yyyyMMddHH") + ".log";
                        break;
                    case LogRecordType.Day:
                        strPath = strPath + "\\" + strHeareName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                        break;
                }
                File.AppendAllText(strPath, strReportData + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                bReturn = false;
            }
            return bReturn;
        }
    } 
}
