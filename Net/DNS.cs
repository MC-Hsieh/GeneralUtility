using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeneralUtility.Net
{
    public class DNS
    {
        public static string GetHostNameByIP(string strIP)
        {
            try
            {
                IPHostEntry clsHostEntry = Dns.GetHostEntry(strIP);
                return clsHostEntry.HostName;
            } 
            catch 
            {
                return "None";
            }   
        }

        public static string GetIPByHostName(string strHostName)
        {
            IPAddress[] clsIPAddresss = Dns.GetHostAddresses(strHostName);
            foreach (IPAddress clsAddr in clsIPAddresss) 
            {
                if (clsAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return clsAddr.ToString();
            }
            return "127.0.0.1";
        }
    }
}
