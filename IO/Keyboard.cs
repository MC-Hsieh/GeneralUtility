using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace GeneralUtility.IO
{
    public partial class Keyboard
    {
        #region " Definition "

        //取得按鈕狀態
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        //調用user32.dll內的keybd_event函式
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        const byte KEYEVENTF_EXTENDEDKEY = 0x01;
        const byte KEYEVENTF_KEYUP = 0x02;

        //調用user32.dll內的keybd_event函式
        [DllImport("sas.dll")]
        public static extern void SendSAS(bool AsUser);

        [DllImport("user32.dll")] 	        
        public static extern void BlockInput(bool Block); 


        #endregion

        #region " Propertis "

        /// <summary>取得或設定，是否獨佔所有鍵盤事件。</summary>
        public static bool Monopolize { get; set; }

        /// <summary>
        /// 不論是否擁有焦點，當鍵盤按下時引發此事件。
        /// </summary>
        public static event EventHandler<XKeyEventArgs> GlobalKeyDown;

        /// <summary>
        /// 不論是否擁有焦點，當鍵盤放開時引發此事件。
        /// </summary>
        public static event EventHandler<XKeyEventArgs> GlobalKeyUp;

        /// <summary>
        /// 取得或設定是否開始接收全域鍵盤事件。
        /// </summary>
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

        #endregion

        #region "Methods - InputJudge"

        /// <summary>判斷輸入是否為數字。</summary>
        /// <param name="e">KeysArg。</param>
        /// <param name="bExceptCopyPaste">複製貼上例外。</param>
        /// <returns>是否。</returns>
        public static bool IsNumber(KeyPressEventArgs e, bool bExceptCopyPaste = true)
        {
            bool bIsNumber = false;
            try
            {
                //判斷是否為英文
                if (!((int)e.KeyChar < 48 | (int)e.KeyChar > 57))
                {
                    bIsNumber = true;
                }

                if (bExceptCopyPaste)
                {
                    if (((int)e.KeyChar == 3 | (int)e.KeyChar == 22))
                        bIsNumber = true;
                }
            }
            catch (Exception ex)
            {
                XStatus.Report("Keyboard", MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return false;
            }
            return bIsNumber;
        }


        /// <summary>判斷輸入是否為英文。</summary>
        /// <param name="e">KeysArg。</param>
        /// <param name="bExceptCopyPaste">複製貼上例外。</param>
        /// <returns>是否。</returns>
        public static bool IsEnglish(KeyPressEventArgs e, bool bExceptCopyPaste = true)
        {
            bool bIsNumber = false;
            try
            {
                //判斷是否為英文
                if (!((int)e.KeyChar < 65 | (int)e.KeyChar > 90))
                {
                    bIsNumber = true;
                }

                if (bExceptCopyPaste)
                {
                    if (((int)e.KeyChar == 3 | (int)e.KeyChar == 22))
                        bIsNumber = true;
                }
            }
            catch (Exception ex)
            {
                XStatus.Report("Keyboard", MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return false;
            }
            return bIsNumber;
        }
        /// <summary>判斷輸入是否為Backspace。</summary>
        /// <param name="e">KeysArg。</param>
        /// <param name="bExceptCopyPaste">複製貼上例外。</param>
        /// <returns>是否。</returns>
        public static bool IsBackspace(KeyPressEventArgs e, bool bExceptCopyPaste = true)
        {
            bool bIsNumber = false;
            try
            {
                if (((int)e.KeyChar == 8))
                {
                    bIsNumber = true;
                }

                if (bExceptCopyPaste)
                {
                    if (((int)e.KeyChar == 3 | (int)e.KeyChar == 22))
                        bIsNumber = true;
                }
            }
            catch (Exception ex)
            {
                XStatus.Report("Keyboard", MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return false;
            }
            return bIsNumber;
        }

        /// <summary>判斷輸入是否為Dot。</summary>
        /// <param name="e">KeysArg。</param>
        /// <param name="bExceptCopyPaste">複製貼上例外。</param>
        /// <returns>是否。</returns>
        public static bool IsDot(KeyPressEventArgs e, bool bExceptCopyPaste = true)
        {
            bool bIsNumber = false;
            try
            {
                if (((int)e.KeyChar == 46))
                {
                    bIsNumber = true;
                }

                if (bExceptCopyPaste)
                {
                    if (((int)e.KeyChar == 3 | (int)e.KeyChar == 22))
                        bIsNumber = true;
                }
            }
            catch (Exception ex)
            {
                XStatus.Report("Keyboard", MethodInfo.GetCurrentMethod(), XStatus.GetExceptionLine(ex));
                return false;
            }
            return bIsNumber;
        }

        /// <summary>判斷NumLock是否開啟</summary>
        /// <returns>是否。</returns>
        public static bool IsNumLock()
        {
            if ((((ushort)GetKeyState(0x90)) & 0xffff) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region "Methods - KeyAction"

        public static void Sas(bool bbb)
        {
            SendSAS(bbb);
        }

        /// <summary>按下按鈕</summary>
        /// <param name="bKey">按鈕碼</param>
        public static void KeyDown(byte bKey)
        {
            keybd_event(bKey, 0, KEYEVENTF_EXTENDEDKEY, (IntPtr)0);
        }

        /// <summary>彈起按鈕</summary>
        /// <param name="bKey">按鈕碼</param>
        public static void KeyUp(byte bKey)
        {
            keybd_event(bKey, 0, KEYEVENTF_KEYUP, (IntPtr)0);
        }

        /// <summary>點擊鍵盤按鈕</summary>
        /// <param name="bKey">按鈕代碼</param>
        public static void KeyClick(byte bKey)
        {
            KeyDown(bKey);
            Thread.Sleep(20);
            KeyUp(bKey);
        }

        #endregion

        #region "Methods - KeyBind"

        public static void BindNumber(KeyPressEventArgs clsKeyPressEventArgs)
        {
            // 使用 Char.IsDigit 方法 : 指示指定的 Unicode 字元是否分類為十進位數字。
            // e.KeyChar == (Char)48 ~ 57 -----> 0~9

            // Char.IsControl 方法 : 指示指定的 Unicode 字元是否分類為控制字元。
            // e.KeyChar == (Char)8 -----------> Backpace
            // e.KeyChar == (Char)13-----------> Enter

            if (Char.IsDigit(clsKeyPressEventArgs.KeyChar) || Char.IsControl(clsKeyPressEventArgs.KeyChar))
            {
                clsKeyPressEventArgs.Handled = false;
            }
            else
            {
                clsKeyPressEventArgs.Handled = true;
            }
        }

        #endregion

        #region "Methods - Hook"

        /// <summary>
        /// 向 Windows 註冊 Hook。
        /// </summary>
        private static void Install()
        {
            if (g_iHookHandle == 0)
            {
                Process clsProcess = Process.GetCurrentProcess();
                ProcessModule clsModule = clsProcess.MainModule;

                g_dlgHookProc = new XNativeStructs.HookProc(HookProc);
                g_iHookHandle = XNativeMethods.SetWindowsHookEx(XNativeContansts.WH_KEYBOARD_LL, g_dlgHookProc, XNativeMethods.GetModuleHandle(clsModule.ModuleName), 0);

                clsModule.Dispose();
                clsProcess.Dispose();

                if (g_iHookHandle == 0)
                    throw new Exception("Install Hook Faild.");
            }
        }

        /// <summary>
        /// 向 Windows 取消註冊 Hook。
        /// </summary>
        private static void Uninstall()
        {
            if (g_iHookHandle != 0)
            {
                bool bReturne = XNativeMethods.UnhookWindowsHookEx(g_iHookHandle);

                if (bReturne)
                    g_iHookHandle = 0;
                else
                    throw new Exception("Uninstall Hook Faild.");
            }
        }

        /// <summary>
        /// 註冊Windows Hook時用到的委派方法，當全域事件發生時會執行這個方法，並提供全域事件資料。
        /// </summary>
        private static int HookProc(int iCode, IntPtr tWParam, IntPtr tLParam)
        {
            XKeyEventArgs clsEvent = null;
            int iParamInt32 = tWParam.ToInt32();
            if (iCode >= 0)
            {
                XNativeStructs.KEYBOARDLLHookStruct keyboardHookStruct = (XNativeStructs.KEYBOARDLLHookStruct)Marshal.PtrToStructure(tLParam, typeof(XNativeStructs.KEYBOARDLLHookStruct));
                if (GlobalKeyDown != null && (iParamInt32 == XNativeContansts.WM_KEYDOWN || iParamInt32 == XNativeContansts.WM_SYSKEYDOWN))
                {
                    clsEvent = new XKeyEventArgs(keyboardHookStruct.VirtualKeyCode);
                    GlobalKeyDown.Invoke(null, clsEvent);
                }
                else if (GlobalKeyUp != null && (iParamInt32 == XNativeContansts.WM_KEYUP || iParamInt32 == XNativeContansts.WM_SYSKEYUP))
                {
                    clsEvent = new XKeyEventArgs(keyboardHookStruct.VirtualKeyCode);
                    GlobalKeyUp.Invoke(null, clsEvent);
                }
            }

            if (Monopolize || (clsEvent != null && clsEvent.Handled))
                return -1;
            return XNativeMethods.CallNextHookEx(g_iHookHandle, iCode, tWParam, tLParam);
        }

        #endregion

    }

    internal static class XNativeContansts
    {
        public const int WH_MOUSE_LL = 14;
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE = 7;
        public const int WH_KEYBOARD = 2;

        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_MBUTTONUP = 0x208;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_RBUTTONDBLCLK = 0x206;
        public const int WM_MBUTTONDBLCLK = 0x209;
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;

        public const int MEF_LEFTDOWN = 0x00000002;
        public const int MEF_LEFTUP = 0x00000004;
        public const int MEF_MIDDLEDOWN = 0x00000020;
        public const int MEF_MIDDLEUP = 0x00000040;
        public const int MEF_RIGHTDOWN = 0x00000008;
        public const int MEF_RIGHTUP = 0x00000010;

        public const int KEF_EXTENDEDKEY = 0x1;
        public const int KEF_KEYUP = 0x2;

        public const byte VK_SHIFT = 0x10;
        public const byte VK_CAPITAL = 0x14;
        public const byte VK_NUMLOCK = 0x90;
    }

    internal static class XNativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, XNativeStructs.HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern int GetDoubleClickTime();
        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);
        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }

    internal static class XNativeStructs
    {
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSELLHookStruct
        {
            public Point Point;
            public int MouseData;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBOARDLLHookStruct
        {
            public int VirtualKeyCode;
            public int ScanCode;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }
    }

    /// <summary> 提供 GlobalKeyDown 或 GlobalKeyUp 事件的資料。</summary>
    public partial class XKeyEventArgs : EventArgs
    {

        #region " Properties "

        /// <summary>取得或設定，指出是否處理事件。</summary>
        public bool Handled { get; set; }

        /// <summary> 取得，虛擬鍵盤碼的System.Windows.Forms.Keys表示。</summary>
        public System.Windows.Forms.Keys Keys { get { return (System.Windows.Forms.Keys)VirtualKeyCode; } }

        /// <summary>取得，虛擬鍵盤碼的System.Windows.Input.Key表示。</summary>
        public System.Windows.Input.Key Key { get { return System.Windows.Input.KeyInterop.KeyFromVirtualKey(VirtualKeyCode); } }

        /// <summary>取得，指出是否按下 ALT 鍵。</summary>
        public bool Alt
        {
            get
            {
                return KeyIsDown((int)System.Windows.Forms.Keys.LMenu) || KeyIsDown((int)System.Windows.Forms.Keys.RMenu);
            }
        }

        /// <summary>取得，指出是否按下 CTRL 鍵。</summary>
        public bool Control
        {
            get
            {
                return KeyIsDown((int)System.Windows.Forms.Keys.LControlKey) || KeyIsDown((int)System.Windows.Forms.Keys.RControlKey);
            }
        }

        /// <summary>取得，指出是否按下 SHIFT 鍵。</summary>
        public bool Shift
        {
            get
            {
                return KeyIsDown((int)System.Windows.Forms.Keys.LShiftKey) || KeyIsDown((int)System.Windows.Forms.Keys.RShiftKey);
            }
        }

        /// <summary>取得，引發事件的虛擬鍵盤碼。</summary>
        public int VirtualKeyCode { get; private set; }

        #endregion

        #region " Methods "

        internal XKeyEventArgs(int virtualKey)
        {
            this.Handled = false;
            this.VirtualKeyCode = virtualKey;
        }

        private static bool KeyIsDown(int KeyCode)
        {
            if ((XNativeMethods.GetKeyState(KeyCode) & 0x80) == 0x80)
                return true;
            else
                return false;
        }

        #endregion

    }

    /// <summary> 進行熱鍵功能的建立，焦點不在主表單上依舊可以使用熱鍵功能 </summary>
    public partial class XGlobalHotKey : IMessageFilter, IDisposable
    {

        #region " Operation "

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern UInt32 GlobalAddAtom(String lpString);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern UInt32 GlobalDeleteAtom(UInt32 nAtom);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern UInt32 UnregisterHotKey(IntPtr hWnd, UInt32 id);

        #endregion

        #region " Property "

        private IntPtr g_tWnd = IntPtr.Zero;
        private UInt32 g_tHotKeyID;
        private Keys g_eHotKey = Keys.None;
        private Keys g_eComboKey = Keys.None;

        #endregion

        #region " Methods - New "

        /// <summary>此熱鍵功能用來建立，如果焦點不在主表單上依舊可以使用熱鍵 </summary>
        /// <param name="tformHandle">主表單的 HANDLE 可以直接使用 this.Handle</param>
        /// <param name="eHotKey">主要熱鍵</param>
        /// <param name="eComboKey">合併熱鍵，如果沒有則選擇 Keys.None</param>
        public XGlobalHotKey(IntPtr tformHandle, Keys eHotKey, Keys eComboKey = Keys.None)
        {
            g_tWnd = tformHandle;		//Form Handle, 註冊系統熱鍵需要用到這個    
            g_eHotKey = eHotKey;		//熱鍵    
            g_eComboKey = eComboKey;	//組合鍵, 必須設定Keys.Control, Keys.Alt, Keys.Shift, Keys.None以及Keys.LWin等值才有作用    
            UInt32 iComboKey;			//由於API對於組合鍵碼的定義不一樣, 所以我們這邊做個轉換    
            switch (eComboKey)
            {
                case Keys.Alt:
                    iComboKey = 0x1;
                    break;
                case Keys.Control:
                    iComboKey = 0x2;
                    break;
                case Keys.Shift:
                    iComboKey = 0x4;
                    break;
                case Keys.LWin:
                    iComboKey = 0x8;
                    break;
                default: //沒有組合鍵        
                    iComboKey = 0x0;
                    break;
            }

            g_tHotKeyID = GlobalAddAtom(Guid.NewGuid().ToString());						//向系統取得一組id    
            RegisterHotKey((IntPtr)g_tWnd, g_tHotKeyID, iComboKey, (UInt32)eHotKey);	//使用Form Handle與id註冊系統熱鍵    
            Application.AddMessageFilter(this);											//使用HotKey類別來監視訊息

        }

        #endregion

        #region " Methods - Prefilter Message "

        public delegate void GlobalHotkeyEventHandler(object sender, XGlobalHotKeyEventArgs e); //HotKeyEventArgs是自訂事件參數
        public event GlobalHotkeyEventHandler OnHotkey; //自訂事件 
        const int WM_GLOBALHOTKEYDOWN = 0x312; //當按下系統熱鍵時, 系統會發送的訊息 
        public bool PreFilterMessage(ref Message tMsg)
        {
            if (OnHotkey != null && tMsg.Msg == WM_GLOBALHOTKEYDOWN && (UInt32)tMsg.WParam == g_tHotKeyID) //如果接收到系統熱鍵訊息且id相符時    
            {
                OnHotkey(this, new XGlobalHotKeyEventArgs(g_eHotKey, g_eComboKey)); //呼叫自訂事件, 傳遞自訂參數        
                return true; //並攔截這個訊息, Form將不再接收到這個訊息    
            }
            return false;
        }

        #endregion

        #region " Methods - Dispose "

        private bool bIsDisposed = false;
        public void Dispose()
        {
            if (!bIsDisposed)
            {
                UnregisterHotKey(g_tWnd, g_tHotKeyID);	//取消熱鍵        
                GlobalDeleteAtom(g_tHotKeyID);			//刪除id        
                OnHotkey = null;						//取消所有關聯的事件       
                Application.RemoveMessageFilter(this);	//不再使用HotKey類別監視訊息        
                GC.SuppressFinalize(this);
                bIsDisposed = true;
            }
        }

        ~XGlobalHotKey()
        {
            Dispose();
        }

        #endregion

    }
    /// <summary>自訂的熱鍵觸發事件</summary>
    public partial class XGlobalHotKeyEventArgs : EventArgs
    {

        #region " Properties "

        private Keys g_eHotKey;
        public Keys HotKey //熱鍵    
        {
            get { return g_eHotKey; }
            private set { }
        }

        private Keys g_eComboKey;
        public Keys ComboKey //組合鍵    
        {
            get { return g_eComboKey; }
            private set { }
        }

        #endregion

        #region " Methods - New "

        public XGlobalHotKeyEventArgs(Keys eHotKey, Keys eComboKey)
        {
            g_eHotKey = eHotKey;
            g_eComboKey = eComboKey;
        }

        #endregion

    }

}
