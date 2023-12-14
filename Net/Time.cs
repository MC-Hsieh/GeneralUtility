using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;

namespace GeneralUtility.Net
{
    public class XTime
    {
        private struct SYSTEMTIME
        {
            public ushort wYear;

            public ushort wMonth;

            public ushort wDayOfWeek;

            public ushort wDay;

            public ushort wHour;

            public ushort wMinute;

            public ushort wSecond;

            public ushort wMilliseconds;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern void GetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        private bool GetWebTime(string strURL, ref DateTime sDateTime)
        {
            bool flag;
            try
            {
                WebRequest request = WebRequest.Create(strURL);
                request.Method = "GET";
                WebResponse response = request.GetResponse();
                string strData = response.Headers["Date"].Replace("GMT", "");
                sDateTime = DateTime.Parse(strData);
                response.Close();
                response = null;
                request = null;
            }
            catch (Exception Exception)
            {
                flag = false;
                return flag;
            }
            flag = true;
            return flag;
        }

        public bool WebTimeCorrection(string URL)
        {
            bool flag;
            DateTime sTime = DateTime.Now;
            if (this.GetWebTime("http://" + URL, ref sTime))
            {
                flag = (SetSystemTime(sTime) ? true : false);
            }
            else
            {
                flag = false;
            }
            return flag;
        }

        private bool SetSystemTime(DateTime sDateTime)
        {
            bool flag;
            try
            {
                SYSTEMTIME systime = new SYSTEMTIME();
                GetSystemTime(ref systime);
                systime.wYear = (ushort)sDateTime.Year;
                systime.wMonth = (ushort)sDateTime.Month;
                systime.wDay = (ushort)sDateTime.Day;
                systime.wHour = (ushort)sDateTime.Hour;
                systime.wMinute = (ushort)sDateTime.Minute;
                systime.wSecond = (ushort)sDateTime.Second;
                systime.wMilliseconds = (ushort)sDateTime.Millisecond;
                SetSystemTime(ref systime);
            }
            catch (Exception exception)
            {
                flag = false;
                return flag;
            }
            flag = true;
            return flag;
        }
    }
}
