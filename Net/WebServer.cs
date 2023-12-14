using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeneralUtility.Net
{
    public class WebServer
    {
        private static string _strErrorMsg4Rresponse; 
        public static string ResponseError
        {
            get { return _strErrorMsg4Rresponse; }
        }

        public static HttpWebResponse POST_WebResopsne_Response(string strWebAddress, IDictionary<string, string> parameters, Encoding charset)
        {
            _strErrorMsg4Rresponse = null; 
            HttpWebRequest clsRequest = (HttpWebRequest)WebRequest.Create(strWebAddress);
            clsRequest.Method = "POST";
            clsRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
            try
            {
                if (!(parameters == null || parameters.Count == 0))
                {
                    StringBuilder buffer = new StringBuilder();
                    int i = 0;
                    foreach (string key in parameters.Keys)
                    {
                        if (i > 0)
                        {
                            buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                        }
                        else
                        {
                            buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        }
                        i++;
                    }
                    byte[] data = charset.GetBytes(buffer.ToString());
                    using (Stream stream = clsRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                HttpWebResponse clsRespose = (HttpWebResponse)clsRequest.GetResponse();
                //return clsRequest.GetResponse() as HttpWebResponse;
                return clsRespose; 
            }
            catch (Exception ex)
            {
                _strErrorMsg4Rresponse = ex.Message;
                return null;
            }
        }

        public static JObject POST_WebResopsne_Json(string strWebAddress, IDictionary<string, string> parameters, Encoding charset)
        {
            _strErrorMsg4Rresponse = null;

            HttpWebResponse clsResponse = POST_WebResopsne_Response(strWebAddress, parameters, charset); 
            if (clsResponse == null)
                return null;

            try 
            {
                StreamReader streamReader = new StreamReader(clsResponse.GetResponseStream(), Encoding.UTF8);
                string strResponse = streamReader.ReadToEnd();
                streamReader.Close();
                clsResponse.Close();
                return (JObject)JsonConvert.DeserializeObject(strResponse); 
            }
            catch (Exception ex)
            {
                _strErrorMsg4Rresponse = ex.Message;
                clsResponse.Close();
                return null;
            }
        }

        public static string POST_WebResopsne_String(string strWebAddress, IDictionary<string, string> parameters, Encoding charset)
        {
            _strErrorMsg4Rresponse = null;

            HttpWebResponse clsResponse = POST_WebResopsne_Response(strWebAddress, parameters, charset);
            if (clsResponse == null)
                return null;

            try
            {
                StreamReader streamReader = new StreamReader(clsResponse.GetResponseStream(), Encoding.UTF8);
                string strResponse = streamReader.ReadToEnd();
                streamReader.Close();
                clsResponse.Close();
                return strResponse;
            }
            catch (Exception ex)
            {
                _strErrorMsg4Rresponse = ex.Message;
                clsResponse.Close();
                return null;
            }
        }

        public static string Get_WebResopsne_String(string strWebAddress, int iTimeout, Encoding charset)
        {
            _strErrorMsg4Rresponse = null;
            try
            {
                HttpWebRequest clsRequest = (HttpWebRequest)WebRequest.Create(strWebAddress);
                clsRequest.Timeout = iTimeout;
                clsRequest.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)clsRequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader clsSR = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    string strResponse = clsSR.ReadToEnd();
                    clsSR.Close();
                    response.Close();
                    return strResponse;
                }
                else
                {
                    _strErrorMsg4Rresponse = "Get Response StatusCode : " + response.StatusCode.ToString();
                    response.Close();
                    return null;
                }
            }
            catch (Exception ex)
            {
                _strErrorMsg4Rresponse = ex.Message;
                return null;
            }
        }

        public static JObject Get_WebResopsne_Json(string strWebAddress, int iTimeout, Encoding charset)
        {
            _strErrorMsg4Rresponse = null;
            try
            {
                HttpWebRequest clsRequest = (HttpWebRequest)WebRequest.Create(strWebAddress);
                clsRequest.Timeout = iTimeout;
                clsRequest.Method = "GET";
                HttpWebResponse clsResponse = (HttpWebResponse)clsRequest.GetResponse();

                if (clsResponse.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        StreamReader streamReader = new StreamReader(clsResponse.GetResponseStream(), Encoding.UTF8);
                        string strResponse = streamReader.ReadToEnd();
                        streamReader.Close();
                        clsResponse.Close();
                        return (JObject)JsonConvert.DeserializeObject(strResponse);
                    }
                    catch (Exception ex)
                    {
                        _strErrorMsg4Rresponse = ex.Message;
                        clsResponse.Close();
                        return null;
                    }
                }
                else
                {
                    _strErrorMsg4Rresponse = "Get Response StatusCode : " + clsResponse.StatusCode.ToString();
                    clsResponse.Close();
                    return null;
                }
            }
            catch (Exception ex)
            {
                _strErrorMsg4Rresponse = ex.Message;
                return null;
            }
        }
    }
}
