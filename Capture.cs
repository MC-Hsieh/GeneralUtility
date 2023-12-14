using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace GeneralUtility
{
    public class Capture
    {
        /// <summary> BitBlt 接圖內容成圖片 (被覆蓋或縮小無法截圖) </summary>
        /// <param name="panel">要被解讀的控件</param>
        /// <returns>Bitmap:截圖畫面</returns>
        public static Bitmap GetControlCapture(Control ctl)
        {
            //Get a Graphics Object from the Panel
            Graphics g1 = ctl.CreateGraphics();
            //Create a EMPTY bitmap from that graphics
            Bitmap ImageBuff = new Bitmap(ctl.Width, ctl.Height, g1);
            //Create a Graphics object in memory from that bitmap
            Graphics g2 = Graphics.FromImage(ImageBuff);
            //get the IntPtr's of the graphics
            IntPtr dc1 = g1.GetHdc();
            IntPtr dc2 = g2.GetHdc();
            //get the picture 
            BitBlt(dc2, 0, 0, ctl.Width, ctl.Height, dc1, 0, 0, 13369376);
            //clear all
            g1.ReleaseHdc(dc1);
            g2.ReleaseHdc(dc2);
            g1.Dispose();
            g2.Dispose();
            return ImageBuff;
        }

        /// <summary> PrintWindow 取得視窗截圖的二位元物件Bimap (可以被擋住/但不可以縮小) </summary>
        /// <param name="hWnd">視窗的handle(IntPtr PicWindow = this.panel.Handle;)</param>
        /// <returns>Bitmap:截圖畫面</returns>
        public static Bitmap GetWindowCapture(IntPtr hWnd)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            var windowRect = new RECT();
            GetWindowRect(hWnd, ref windowRect);
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, width, height);
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = Bitmap.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);//删除用过的对象
            DeleteDC(hmemdc);//删除用过的对象
            return bmp;
        }

        /// <summary> 屏幕截圖 </summary>
        /// <returns>Bitmap:截圖畫面</returns>
        public static Bitmap ScreenShot()
        {
            Bitmap myImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);
            g.Dispose();
            return myImage;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left; //最左坐标
            public int Top; //最上坐标
            public int Right; //最右坐标
            public int Bottom; //最下坐标
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(
         string lpszDriver,         // driver name驱动名
         string lpszDevice,         // device name设备名
         string lpszOutput,         // not used; should be NULL
         IntPtr lpInitData   // optional printer data
         );

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(
         IntPtr hdcDest, // handle to destination DC目标设备的句柄
         int nXDest,   // x-coord of destination upper-left corner目标对象的左上角的X坐标
         int nYDest,   // y-coord of destination upper-left corner目标对象的左上角的Y坐标
         int nWidth,   // width of destination rectangle目标对象的矩形宽度
         int nHeight, // height of destination rectangle目标对象的矩形长度
         IntPtr hdcSrc,   // handle to source DC源设备的句柄
         int nXSrc,    // x-coordinate of source upper-left corner源对象的左上角的X坐标
         int nYSrc,    // y-coordinate of source upper-left corner源对象的左上角的Y坐标
         UInt32 dwRop   // raster operation code光栅的操作值
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(
         IntPtr hdc // handle to DC
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(
         IntPtr hdc,         // handle to DC
         int nWidth,      // width of bitmap, in pixels
         int nHeight      // height of bitmap, in pixels
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(
         IntPtr hdc,           // handle to DC
         IntPtr hgdiobj    // handle to object
         );

        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(
         IntPtr hdc           // handle to DC
         );

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(
         IntPtr hwnd,                // Window to copy,Handle to the window that will be copied.
         IntPtr hdcBlt,              // HDC to print into,Handle to the device context.
         UInt32 nFlags               // Optional flags,Specifies the drawing options. It can be one of the following values.
         );

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(
         IntPtr hwnd
         );
    }
}
