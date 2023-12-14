using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace GeneralUtility
{
    public class BaseTool
    {

        /// <summary>檢查是否重複直行程式</summary>
        /// <param name="strRepeatTxt">重複執行提示字串</param>
        public static void CheckRepeat(string strRepeatTxt,bool IsShow = true)
        {
            //取得此process的名稱
            String SystemName = Process.GetCurrentProcess().ProcessName;
            //取得所有與目前process名稱相同的process
            Process[] ps = Process.GetProcessesByName(SystemName);
            //ps.Length > 1 表示此proces以重複執行
            if (ps.Length > 1)
            {
                if (IsShow)
                {
                    MessageBox.Show(strRepeatTxt, "Error");
                }
                System.Environment.Exit(2);
            }
        }

        /// <summary>產生捷徑</summary>
        /// <param name="strEXEPath">執行檔位置</param>
        /// <param name="strPathLnkName">產生捷徑檔位置</param>
        /// <param name="strDescription">捷徑檔說明</param>
        /// <returns>是否成功建立</returns>
        public static bool CreateLnk(string strEXEPath, string strPathLnkName, string strDescription)
        {
            try
            {
                if (strEXEPath != null && strEXEPath != "")
                {
                    int iLastIndex = strEXEPath.LastIndexOf('\\');
                    string strFileName = strEXEPath.Substring(iLastIndex + 1, strEXEPath.Length - iLastIndex - 1);
                    string strFileFolder = strEXEPath.Substring(0, iLastIndex);
                    WshShell shell = new WshShell();
                    string shortcutAddress = strPathLnkName;
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                    shortcut.Description = strDescription;//遊標停留在捷徑上顯示的文字
                    shortcut.Hotkey = "";//快速鍵
                    shortcut.WorkingDirectory = strFileFolder;//如果程式內檔案是使用相對路徑請設定這個
                    shortcut.TargetPath = strEXEPath;
                    shortcut.Save();
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>註冊開機執行</summary>
        /// <param name="strRegisterName">註冊名</param>
        /// <param name="strEXEPath">執行檔位置</param>
        /// <returns>是否成功</returns>
        public static bool RegisterAutoRun(string strRegisterName ,string strEXEPath)
        {
            bool bRrturn = true;
            try
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rkApp.SetValue(strRegisterName, strEXEPath);

                bRrturn = true;
            }
            catch (Exception ex)
            {
                bRrturn = false;
            }
            return bRrturn;
        }

        /// <summary>取消註冊開機執行</summary>
        /// <param name="strRegisterName">註冊名</param>
        /// <returns>是否成功</returns>
        public static bool UnRegisterAutoRun(string strRegisterName)
        {
            bool bRrturn = true;
            try
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue(strRegisterName, false);

                bRrturn = true;
            }
            catch (Exception ex)
            {
                bRrturn = false;
            }
            return bRrturn;
        }

        /// <summary>開啟自動執行</summary>
        public static void EnableAutoRun()
        {
            string strProductName = System.Windows.Forms.Application.ProductName;
            string strLnkPath = System.Windows.Forms.Application.StartupPath + "\\" + strProductName + ".lnk";

            BaseTool.CreateLnk(System.Windows.Forms.Application.ExecutablePath, strLnkPath, strProductName);
            BaseTool.RegisterAutoRun(strProductName, strLnkPath);
        }

        /// <summary>取消自動執行</summary>
        public static void DisableAutoRun()
        {
            string strProductName = System.Windows.Forms.Application.ProductName;

            BaseTool.UnRegisterAutoRun(strProductName);
        }

        public static bool ProcessStart(string strEXEPath)
        {
            bool bRrturn = true;
            try
            {
                int iLastIndex = strEXEPath.LastIndexOf('\\');
                string strFileName = strEXEPath.Substring(iLastIndex + 1, strEXEPath.Length - iLastIndex - 1);
                string strFileFolder = strEXEPath.Substring(0, iLastIndex);
                ProcessStartInfo open = new ProcessStartInfo();
                open.FileName = strFileName; // 檔案名稱
                open.WorkingDirectory = strFileFolder; // 資料夾路徑
                Process.Start(open);
                bRrturn = true;
            }
            catch (Exception ex)
            {
                bRrturn = false;
            }
            return bRrturn;
        }

        public static bool ProcessKill(string strPrecessName)
        {
            bool bRrturn = true;
            try
            {
                Process[] clsAllProcess = Process.GetProcesses();
                foreach (Process clsProcess in clsAllProcess)
                {
                    if (strPrecessName == clsProcess.ProcessName)
                    {
                        clsProcess.Kill();
                    }
                }
                bRrturn = true;
            }
            catch (Exception ex)
            {
                string strEx = ex.Message;
                bRrturn = false;
            }
            return bRrturn;
        }

        /// <summary>比較時間是否在A與B之間</summary>
        /// <param name="ATime">A時間</param>
        /// <param name="Time">比較時間</param>
        /// <param name="BTime">B時間</param>
        /// <returns></returns>
        public static bool DateTimeBetween(DateTime ATime ,DateTime Time ,DateTime BTime)
        {
            int iCopmare = DateTime.Compare(ATime, BTime);

            if (iCopmare < 0)
            {
                int iCA = DateTime.Compare(ATime, Time);
                int iCB = DateTime.Compare(BTime, Time);

                return (iCA < 0 && iCB > 0);
            }
            else
            {
                int iCA = DateTime.Compare(ATime, Time);
                int iCB = DateTime.Compare(BTime, Time);

                return (iCB < 0 && iCA > 0);
            }
        }

        /// <summary>比較時間是否在A與B之前</summary>
        /// <param name="ATime">A時間</param>
        /// <param name="Time">比較時間</param>
        /// <param name="BTime">B時間</param>
        /// <returns></returns>
        public static bool DateTimeBrfore(DateTime ATime, DateTime BTime, DateTime Time)
        {
                int iCA = DateTime.Compare(ATime, Time);
                int iCB = DateTime.Compare(BTime, Time);

                return (iCA < 0 && iCB < 0);
        }

        /// <summary>儲存當前螢幕畫面</summary>
        /// <param name="strPath">影像儲存位置</param>
        public static void SaveScreenImage(string strPath)
        {
            Bitmap clsBmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Graphics clsGraphics = Graphics.FromImage(clsBmp);

            clsGraphics.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));

            IntPtr dc1 = clsGraphics.GetHdc();

            clsGraphics.ReleaseHdc(dc1);

            clsBmp.Save(strPath);
        }

        /// <summary>取得執行檔ICON</summary>
        /// <param name="strExePath">執行檔路徑</param>
        /// <returns></returns>
        public static Icon GetExeIcon(string strExePath)
        {
            return Icon.ExtractAssociatedIcon(strExePath);
        }

        /// <summary>ICON轉為Byte</summary>
        /// <param name="clsIcon"></param>
        /// <returns></returns>
        public static byte[] IconToBytes(Icon clsIcon)
        {
            byte[] bBytes;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    clsIcon.Save(ms);
            //    bBytes = ms.ToArray();
            //}
            bBytes = ImageToBytes(clsIcon.ToBitmap());
            return bBytes;
        }
        public static byte[] ImageToBytes(Image clsIcon)
        {
            byte[] bBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                clsIcon.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bBytes = ms.ToArray();
            }
            return bBytes;
        }

        /// <summary>Byte轉為ICON</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Icon BytesToIcon(byte[] bytes)
        {
            if (bytes == null) return null;
            if (bytes.Length < 8) return null;

            if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 1 && bytes[3] == 0)
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return new Icon(ms);
                }
            }
            else if (bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71)
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    Image image = Image.FromStream(ms);
                    return Icon.FromHandle(new Bitmap(image).GetHicon());
                }
            }
            return null;

        }

        public static List<DateTime> SortAscending(List<DateTime> list)
        {
            list.Sort((a, b) => a.CompareTo(b));
            return list;
        }

        public static List<DateTime> SortDescending(List<DateTime> list)
        {
            list.Sort((a, b) => b.CompareTo(a));
            return list;
        }

        public static List<DateTime> SortMonthAscending(List<DateTime> list)
        {
            list.Sort((a, b) => a.Month.CompareTo(b.Month));
            return list;
        }

        public static List<DateTime> SortMonthDescending(List<DateTime> list)
        {
            list.Sort((a, b) => b.Month.CompareTo(a.Month));
            return list;
        }

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr window, int index, int value);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr window, int index);

        public static void setTaskmanager_Disable(IntPtr Handle)
        {
            /* GWL EXSTYLE  = -20 得到或設置擴展窗口的STYLE */
            const int GWL_EXSTYLE = -20;
            const int WS_EX_TOOLWINDOW = 0x00000080;
            const int WS_EX_APPWINDOW = 0x00040000;
            int windowStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE, windowStyle | WS_EX_TOOLWINDOW);
        }

        #region "Control Method - set window region"

        public static void SetControlRegion(Control control, int radius)
        {
            System.Drawing.Drawing2D.GraphicsPath formpath;
            formpath = new System.Drawing.Drawing2D.GraphicsPath();
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, control.Width, control.Height);//this.left-10,this.top-10,this.width-10,this.height-10);
            formpath = getroundedrectpath(rect, radius);
            control.Region = new System.Drawing.Region(formpath);
        }

        private static System.Drawing.Drawing2D.GraphicsPath getroundedrectpath(System.Drawing.Rectangle rect, int radius)
        {
            System.Drawing.Rectangle arcrect = new System.Drawing.Rectangle(rect.Location, new System.Drawing.Size(radius, radius));
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            // 左上角
            path.AddArc(arcrect, 180, 90);
            // 右上角
            arcrect.X = rect.Right - radius;
            path.AddArc(arcrect, 270, 90);
            // 右下角
            arcrect.Y = rect.Bottom - radius;
            path.AddArc(arcrect, 0, 90);
            // 左下角
            arcrect.X = rect.Left;
            path.AddArc(arcrect, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        #region " Events - Move Object With Mouse "

        public class UiTool
        {
            private int g_iFormIniPositionX, g_iFormIniPositionY;			// 視窗初始位置
            private int g_iMouseIniPostionX = 0, g_iMouseIniPostionY = 0;	// 滑鼠拖曳初始位置
            private bool g_bMouseMoveState = false;							// 移動狀態
            private Control ctrlTarget;											// 當前的表單

            // 當滑鼠點擊時
            private void Object_MouseDown(object sender, MouseEventArgs e)
            {
                if (!g_bMouseMoveState) // 假如非移動狀態
                {
                    g_iFormIniPositionX = ctrlTarget.Left;					// 紀錄視窗初始 X
                    g_iFormIniPositionY = ctrlTarget.Top;					// 紀錄視窗初始 Y
                    g_iMouseIniPostionX = Cursor.Position.X;				// 紀錄滑鼠初始 X
                    g_iMouseIniPostionY = Cursor.Position.Y;				// 紀錄滑鼠初始 Y
                    g_bMouseMoveState = true;								// 設定為移動狀態
                }
            }

            // 當滑鼠右鍵點擊時
            private void Object_MouseRightButtonDown(object sender, MouseEventArgs e)
            {
                if (!g_bMouseMoveState && e.Button == MouseButtons.Right) // 假如非移動狀態 且 為右鍵
                {
                    g_iFormIniPositionX = ctrlTarget.Left;					// 紀錄視窗初始 X
                    g_iFormIniPositionY = ctrlTarget.Top;					// 紀錄視窗初始 Y
                    g_iMouseIniPostionX = Cursor.Position.X;				// 紀錄滑鼠初始 X
                    g_iMouseIniPostionY = Cursor.Position.Y;				// 紀錄滑鼠初始 Y
                    g_bMouseMoveState = true;								// 設定為移動狀態
                }
            }

            // 當滑鼠左鍵點擊時
            private void Object_MouseLeftButtonDown(object sender, MouseEventArgs e)
            {
                if (!g_bMouseMoveState && e.Button == MouseButtons.Left) // 假如非移動狀態 且 為右鍵
                {
                    g_iFormIniPositionX = ctrlTarget.Left;					// 紀錄視窗初始 X
                    g_iFormIniPositionY = ctrlTarget.Top;					// 紀錄視窗初始 Y
                    g_iMouseIniPostionX = Cursor.Position.X;				// 紀錄滑鼠初始 X
                    g_iMouseIniPostionY = Cursor.Position.Y;				// 紀錄滑鼠初始 Y
                    g_bMouseMoveState = true;								// 設定為移動狀態
                }
            }

            // 當滑鼠移動時
            private void Object_MouseMove(object sender, MouseEventArgs e)
            {
                if (g_bMouseMoveState) // 若為移動狀態
                {
                    int tMouseX = Cursor.Position.X;						// 暫存目前滑鼠位置 X
                    int tMouseY = Cursor.Position.Y;						// 暫存目前滑鼠位置 Y

                    // 舊視窗位置 + 滑鼠移動距離 = 新位置
                    ctrlTarget.Left = g_iFormIniPositionX + (tMouseX - g_iMouseIniPostionX);	// 設定視窗新位置 X 
                    ctrlTarget.Top = g_iFormIniPositionY + (tMouseY - g_iMouseIniPostionY);	// 設定視窗新位置 Y
                }
            }

            // 當滑鼠放開時
            private void Object_MouseUp(object sender, MouseEventArgs e)
            {
                // 設定非拖曳狀態
                g_bMouseMoveState = false;
            }

            // 設定表單
            private void SetControl(Control CtrlSource)
            {
                ctrlTarget = CtrlSource;
            }

            /// <summary>提供一個指定的 Form 元件，來建立一個直接賦予拖曳該物件即可拖曳整個視窗事件的動作。</summary>
            /// <param name="clsObj">輸入值為 Form 元件。</param>
            public static void MoveFormWithMouse(Form clsObj)
            {
                try
                {
                    UiTool tool = new UiTool();
                    tool.SetControl(clsObj);
                    clsObj.MouseDown += new System.Windows.Forms.MouseEventHandler(tool.Object_MouseDown);
                    clsObj.MouseMove += new System.Windows.Forms.MouseEventHandler(tool.Object_MouseMove);
                    clsObj.MouseUp += new System.Windows.Forms.MouseEventHandler(tool.Object_MouseUp);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            /// <summary>提供指定的 Form 和 Control 元件，來建立一個直接賦予拖曳該物件即可拖曳整個視窗事件的動作。</summary>
            /// <param name="clsForm">輸入值為 Form 元件。</param>
            /// <param name="clsObj">輸入值為 Label 元件。</param>
            public static void MoveFormWithMouse(Form clsForm, Control clsObj)
            {
                try
                {
                    UiTool tool = new UiTool();
                    tool.SetControl(clsForm);
                    clsObj.MouseDown += new System.Windows.Forms.MouseEventHandler(tool.Object_MouseDown);
                    clsObj.MouseMove += new System.Windows.Forms.MouseEventHandler(tool.Object_MouseMove);
                    clsObj.MouseUp += new System.Windows.Forms.MouseEventHandler(tool.Object_MouseUp);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            /// <summary>提供一個指定的 Form 元件，來建立一個直接賦予拖曳該物件即可拖曳整個視窗事件的動作。</summary>
            /// <param name="clsObj">輸入值為 Form 元件。</param>
            public static void MoveControlWithMouse(UiTool clsTool, Control clsObj, MouseButtons emunMousebutton = MouseButtons.None)
            {
                try
                {
                    clsTool.SetControl(clsObj);
                    if (emunMousebutton == MouseButtons.Left)
                    {
                        clsObj.MouseDown += new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseLeftButtonDown);
                    }
                    else if (emunMousebutton == MouseButtons.Right)
                    {
                        clsObj.MouseDown += new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseRightButtonDown);
                    }
                    else
                    {
                        clsObj.MouseDown += new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseDown);
                    }
                    clsObj.MouseMove += new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseMove);
                    clsObj.MouseUp += new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseUp);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            /// <summary>提供一個指定的 Form 元件，來建立一個直接賦予拖曳該物件即可拖曳整個視窗事件的動作。</summary>
            /// <param name="clsObj">輸入值為 Form 元件。</param>
            public static void DisableMoveObjectWithMouse(UiTool clsTool, Control clsObj, MouseButtons emunMousebutton = MouseButtons.None)
            {
                try
                {
                    clsTool.SetControl(clsObj);
                    if (emunMousebutton == MouseButtons.Left)
                    {
                        clsObj.MouseDown -= new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseLeftButtonDown);
                    }
                    else if (emunMousebutton == MouseButtons.Right)
                    {
                        clsObj.MouseDown -= new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseRightButtonDown);
                    }
                    else
                    {
                        clsObj.MouseDown -= new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseDown);
                    }
                    clsObj.MouseMove -= new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseMove);
                    clsObj.MouseUp -= new System.Windows.Forms.MouseEventHandler(clsTool.Object_MouseUp);
                }

                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        #endregion
    }
}
