using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace GeneralUtility.IO
{
    public partial class Mouse
    {
        #region " Definition "

        [DllImport("user32.dll", SetLastError = true)]
        public static extern Int32 SendInput(Int32 cInputs, ref INPUT pInputs, Int32 cbSize);

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 28)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public INPUTTYPE dwType;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBOARDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MOUSEINPUT
        {
            public Int32 dx;
            public Int32 dy;
            public Int32 mouseData;
            public MOUSEFLAG dwFlags;
            public Int32 time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct KEYBOARDINPUT
        {
            public Int16 wVk;
            public Int16 wScan;
            public KEYBOARDFLAG dwFlags;
            public Int32 time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HARDWAREINPUT
        {
            public Int32 uMsg;
            public Int16 wParamL;
            public Int16 wParamH;
        }

        public enum INPUTTYPE : int
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags()]
        public enum MOUSEFLAG : int
        {
            MOVE = 0x1,
            LEFTDOWN = 0x2,
            LEFTUP = 0x4,
            RIGHTDOWN = 0x8,
            RIGHTUP = 0x10,
            MIDDLEDOWN = 0x20,
            MIDDLEUP = 0x40,
            XDOWN = 0x80,
            XUP = 0x100,
            VIRTUALDESK = 0x400,
            WHEEL = 0x800,
            ABSOLUTE = 0x8000
        }

        [Flags()]
        public enum KEYBOARDFLAG : int
        {
            EXTENDEDKEY = 1,
            KEYUP = 2,
            UNICODE = 4,
            SCANCODE = 8
        }

        #endregion

        #region " Properties "

        /// <summary>取得或設定，是否獨佔所有滑鼠事件。</summary>
        public static bool Monopolize { get; set; }

        /// <summary>取得或設定，是否開始接收全域滑鼠事件。</summary>
        public static bool Enabled
        {
            get { return g_bEnabled; }
            set
            {
                if (g_bEnabled != value)
                {
                    g_bEnabled = value;
                    if (value)
                        Install();
                    else
                        Uninstall();
                }
            }
        }
        private static bool g_bEnabled = false;

        private static int g_iHookHandle = 0;
        private static XNativeStructs.HookProc g_dlgHookProc;

        //記憶游標上一次的位置，避免MouseMove事件一直引發。
        private static int g_iOldX = 0;
        private static int g_iOldY = 0;

        //記憶上次MouseDonw的引發位置，如果與MouseUp的位置不同則不引發Click事件。
        private static int g_iLastBTDownX = 0;
        private static int g_iLastBTDownY = 0;

        private static MouseButtons g_eLastClickedButton;
        private static System.Windows.Forms.Timer g_tmrDoubleClickTimer;

        #endregion

        #region " Methods - KeyAction"

        /// <summary>左鍵按下</summary>
        static public void LeftDown()
        {
            INPUT leftdown = new INPUT();

            leftdown.dwType = 0;
            leftdown.mi = new MOUSEINPUT();
            leftdown.mi.dwExtraInfo = IntPtr.Zero;
            leftdown.mi.dx = 0;
            leftdown.mi.dy = 0;
            leftdown.mi.time = 0;
            leftdown.mi.mouseData = 0;
            leftdown.mi.dwFlags = MOUSEFLAG.LEFTDOWN;

            SendInput(1, ref leftdown, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>左鍵彈起</summary>
        static public void LeftUp()
        {
            INPUT leftup = new INPUT();

            leftup.dwType = 0;
            leftup.mi = new MOUSEINPUT();
            leftup.mi.dwExtraInfo = IntPtr.Zero;
            leftup.mi.dx = 0;
            leftup.mi.dy = 0;
            leftup.mi.time = 0;
            leftup.mi.mouseData = 0;
            leftup.mi.dwFlags = MOUSEFLAG.LEFTUP;

            SendInput(1, ref leftup, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>左鍵單擊</summary>
        static public void LeftClick()
        {
            LeftDown();
            Thread.Sleep(20);
            LeftUp();
        }

        /// <summary>左鍵雙擊</summary>
        static public void LeftDoubleClick()
        {
            LeftClick();
            Thread.Sleep(50);
            LeftClick();
        }

        /// <summary>右鍵按下</summary>
        static public void RightDown()
        {
            INPUT leftdown = new INPUT();

            leftdown.dwType = 0;
            leftdown.mi = new MOUSEINPUT();
            leftdown.mi.dwExtraInfo = IntPtr.Zero;
            leftdown.mi.dx = 0;
            leftdown.mi.dy = 0;
            leftdown.mi.time = 0;
            leftdown.mi.mouseData = 0;
            leftdown.mi.dwFlags = MOUSEFLAG.RIGHTDOWN;

            SendInput(1, ref leftdown, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>右鍵彈起</summary>
        static public void RightUp()
        {
            INPUT leftup = new INPUT();

            leftup.dwType = 0;
            leftup.mi = new MOUSEINPUT();
            leftup.mi.dwExtraInfo = IntPtr.Zero;
            leftup.mi.dx = 0;
            leftup.mi.dy = 0;
            leftup.mi.time = 0;
            leftup.mi.mouseData = 0;
            leftup.mi.dwFlags = MOUSEFLAG.RIGHTUP;

            SendInput(1, ref leftup, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>右鍵單擊</summary>
        static public void RightClick()
        {
            RightDown();
            Thread.Sleep(20);
            RightUp();
        }

        /// <summary>右鍵雙擊</summary>
        static public void RightDoubleClick()
        {
            RightClick();
            Thread.Sleep(50);
            RightClick();
        }

        /// <summary>提供取得當前的滑鼠游標的位置。</summary>
        /// <returns>回傳滑鼠游標位置。</returns>
        public static Point GetMousePoint()
        {
            return System.Windows.Forms.Cursor.Position;
        }

        /// <summary>提供設定當前的滑鼠游標的位置。</summary>
        /// <param name="clsPoint">滑鼠位置</param>
        public static void SetMousePoint(Point clsPoint)
        {
            try
            {
                System.Windows.Forms.Cursor.Position = clsPoint;
            }
            catch (Exception ex)
            {
                XStatus.Report(XStatus.Type.Windows, MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
            }
        }

        #endregion

        #region " Methods - Hook"

        /// <summary>
        /// 向Windows註冊Hook。
        /// </summary>
        private static void Install()
        {
            if (g_iHookHandle == 0)
            {
                Process curProcess = Process.GetCurrentProcess();
                ProcessModule curModule = curProcess.MainModule;

                g_dlgHookProc = new XNativeStructs.HookProc(HookProc);
                g_iHookHandle = XNativeMethods.SetWindowsHookEx(XNativeContansts.WH_MOUSE_LL, g_dlgHookProc, XNativeMethods.GetModuleHandle(curModule.ModuleName), 0);

                curModule.Dispose();
                curProcess.Dispose();

                g_tmrDoubleClickTimer = new System.Windows.Forms.Timer
                {
                    Interval = XNativeMethods.GetDoubleClickTime(),
                    Enabled = false
                };
                g_tmrDoubleClickTimer.Tick += DoubleClickTimeElapsed;
                GlobalMouseDown += OnMouseDown;

                if (g_iHookHandle == 0)
                    throw new Exception("Install Hook Faild.");
            }
        }

        private static void Uninstall()
        {
            if (g_iHookHandle != 0)
            {
                bool ret = XNativeMethods.UnhookWindowsHookEx(g_iHookHandle);

                if (ret)
                    g_iHookHandle = 0;
                else
                    throw new Exception("Uninstall Hook Faild.");
            }
        }

        /// <summary>註冊Windows Hook時用到的委派方法，當全域事件發生時會執行這個方法，並提供全域事件資料。</summary>
        private static int HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            XMouseEventArgs e = null;

            if (nCode >= 0)
            {
                int wParam_Int32 = wParam.ToInt32();
                XNativeStructs.MOUSELLHookStruct mouseHookStruct = (XNativeStructs.MOUSELLHookStruct)Marshal.PtrToStructure(lParam, typeof(XNativeStructs.MOUSELLHookStruct));

                short mouseDelta = 0;

                if (GlobalMouseWheel != null && wParam_Int32 == XNativeContansts.WM_MOUSEWHEEL)
                    mouseDelta = (short)((mouseHookStruct.MouseData >> 16) & 0xffff);

                e = new XMouseEventArgs(wParam_Int32, mouseHookStruct.Point.X, mouseHookStruct.Point.Y, mouseDelta);

                if (wParam_Int32 == XNativeContansts.WM_MOUSEWHEEL)
                {
                    if (GlobalMouseWheel != null)
                        GlobalMouseWheel.Invoke(null, e);
                }
                else if (wParam_Int32 == XNativeContansts.WM_LBUTTONUP || wParam_Int32 == XNativeContansts.WM_RBUTTONUP || wParam_Int32 == XNativeContansts.WM_MBUTTONUP)
                {
                    if (GlobalMouseUp != null)
                        GlobalMouseUp.Invoke(null, e);
                    if (mouseHookStruct.Point.X == g_iLastBTDownX && mouseHookStruct.Point.Y == g_iLastBTDownY)
                    {
                        if (GlobalMouseClick != null)
                            GlobalMouseClick.Invoke(null, e);
                    }
                }
                else if (wParam_Int32 == XNativeContansts.WM_LBUTTONDOWN || wParam_Int32 == XNativeContansts.WM_RBUTTONDOWN || wParam_Int32 == XNativeContansts.WM_MBUTTONDOWN)
                {
                    g_iLastBTDownX = mouseHookStruct.Point.X;
                    g_iLastBTDownY = mouseHookStruct.Point.Y;
                    if (GlobalMouseDown != null)
                        GlobalMouseDown.Invoke(null, e);
                }
                else if (g_iOldX != mouseHookStruct.Point.X || g_iOldY != mouseHookStruct.Point.Y)
                {
                    g_iOldX = mouseHookStruct.Point.X;
                    g_iOldY = mouseHookStruct.Point.Y;
                    if (GlobalMouseMove != null)
                        GlobalMouseMove.Invoke(null, e);
                }
            }

            if (Monopolize || (e != null && e.Handled))
                return -1;

            return XNativeMethods.CallNextHookEx(g_iHookHandle, nCode, wParam, lParam);
        }

        private static void OnMouseDown(object sender, XMouseEventArgs e)
        {
            if (e.Button.Equals(g_eLastClickedButton))
            {
                if (GlobalMouseDoubleClick != null)
                    GlobalMouseDoubleClick.Invoke(null, e);
            }
            else
            {
                g_tmrDoubleClickTimer.Enabled = true;
                g_eLastClickedButton = e.Button;
            }
        }

        private static void DoubleClickTimeElapsed(object sender, EventArgs e)
        {
            g_tmrDoubleClickTimer.Enabled = false;
            g_eLastClickedButton = MouseButtons.None;
        }

        #endregion

        #region " Events "

        /// <summary>當滑鼠按鍵壓下時引發此事件。</summary>
        public static event EventHandler<XMouseEventArgs> GlobalMouseDown;

        /// <summary>當滑鼠按鍵放開時引發此事件。</summary>
        public static event EventHandler<XMouseEventArgs> GlobalMouseUp;

        /// <summary>
        /// 當滑鼠按鍵點擊時引發此事件。
        /// </summary>
        public static event EventHandler<XMouseEventArgs> GlobalMouseClick;

        /// <summary>
        /// 當滑鼠按鍵連點兩次時引發此事件。
        /// </summary>
        public static event EventHandler<XMouseEventArgs> GlobalMouseDoubleClick;

        /// <summary>
        /// 當滑鼠滾輪滾動時引發此事件。
        /// </summary>
        public static event EventHandler<XMouseEventArgs> GlobalMouseWheel;

        /// <summary>
        /// 當滑鼠移動時引發此事件。
        /// </summary>
        public static event EventHandler<XMouseEventArgs> GlobalMouseMove;

        #endregion
    }

    /// <summary> 提供 GlobalMouseUp、GlobalMouseDown 和 GlobalMouseMove 事件的資料。</summary>
    public partial class XMouseEventArgs : EventArgs
    {

        #region " Propertis "

        /// <summary>取得，按下哪個滑鼠鍵的資訊。</summary>
        public MouseButtons Button { get; private set; }

        /// <summary>取得，滑鼠滾輪滾動時帶有正負號的刻度數乘以 WHEEL_DELTA 常數。 一個刻度是一個滑鼠滾輪的刻痕。</summary>
        public int Delta { get; private set; }

        /// <summary>取得，滑鼠在產生滑鼠事件期間的 X 座標。</summary>
        public int X { get; private set; }

        /// <summary>取得，滑鼠在產生滑鼠事件期間的 Y 座標。</summary>
        public int Y { get; private set; }

        /// <summary>取得或設定，指出是否處理事件。</summary>
        public bool Handled
        {
            get { return g_bHandled; }
            set { g_bHandled = value; }
        }
        private bool g_bHandled;

        #endregion

        #region " Methods "

        internal XMouseEventArgs(int iParam, int iX, int iY, int iDelta)
        {
            Button = MouseButtons.None;
            switch (iParam)
            {
                case (int)XNativeContansts.WM_LBUTTONDOWN:
                case (int)XNativeContansts.WM_LBUTTONUP:
                    Button = MouseButtons.Left;
                    break;
                case (int)XNativeContansts.WM_RBUTTONDOWN:
                case (int)XNativeContansts.WM_RBUTTONUP:
                    Button = MouseButtons.Right;
                    break;
                case (int)XNativeContansts.WM_MBUTTONDOWN:
                case (int)XNativeContansts.WM_MBUTTONUP:
                    Button = MouseButtons.Middle;
                    break;
            }
            this.X = iX;
            this.Y = iY;
            this.Delta = iDelta;
        }

        #endregion

    }
}
