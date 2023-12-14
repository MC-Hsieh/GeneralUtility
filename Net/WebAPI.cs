using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace GeneralUtility.Net
{
    public class WebAPI
    {
        public static string PostData(string strURL, string strData)
        {
            string strResult = null;
            try
            {
                HttpWebRequest HttpWebRequest = (HttpWebRequest)WebRequest.Create(strURL);
                HttpWebRequest.ContentType = "application/json;charset=UTF-8";
                HttpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(HttpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(strData);
                }
                var httpResponse = (HttpWebResponse)HttpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    strResult = streamReader.ReadToEnd();
                }
                httpResponse.Close();
                HttpWebRequest = null;
            }
            catch (Exception ex)
            {
                strResult = ex.Message;
            }
            return strResult;
        }

        public static string GetData(string strURL, string strData)
        {
            string strResult = null;
            try
            {
                HttpWebRequest HttpWebRequest = (HttpWebRequest)WebRequest.Create(strURL + "?" + strData);
                HttpWebRequest.ContentType = "application/json";
                HttpWebRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)HttpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    strResult = streamReader.ReadToEnd();
                }
                httpResponse.Close();
                HttpWebRequest = null;
            }
            catch (Exception ex)
            {
                strResult = ex.Message;
            }
            return strResult;
        }
    }
}
