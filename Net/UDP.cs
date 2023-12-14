using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GeneralUtility.Net
{
    public class XUDPTunnel : IDisposable
    {
        private EndPoint _Remote;
        private Socket _Server;
        private bool _bDisposed = false;
        public string RemoteIP
        {
            get
            {
                if (_Remote == null) return " : ";
                return _Remote.ToString();
            }
        }
        /// <summary>
        ///宣告 EndPoint, Socket
        /// </summary>
        /// <param name="clsRemote">EndPoint clsRemote</param>
        /// <param name="clsServer">Socket clsServer</param>
        public XUDPTunnel(EndPoint clsRemote, Socket clsServer)
        {
            _Remote = clsRemote;
            _Server = clsServer;
        }
        /// <summary>
        /// server Tunnel 傳送資料
        /// </summary>
        /// <param name="bSendDatas">sendData buffer</param>
        /// <param name="eEncode">編碼方式</param>
        public void Send(byte[] bSendDatas, Encoding eEncode)
        {
            if (eEncode == null) eEncode = Encoding.ASCII;
            _Server.SendTo(bSendDatas, bSendDatas.Length, SocketFlags.None, _Remote); //將原資料送回去   
        }

        ~XUDPTunnel()
        {
            Dispose(false);
        }

        // GC.SuppressFinalize(this) 目的 通知 GC，物件已完成釋放資源動作，GC 不需要再呼叫此物件 Finalize() 方法 
        // 因有宣告"解構式"，如果不宣告 GC.SuppressFinalize(this) 將會觸發"解構式"，使 GC 觸發兩次
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool IsDisposing)
        {
            if (_bDisposed)
            {
                return;
            }
            if (IsDisposing)
            {
                // 釋放具有實做 IDisposable 的物件(資源關閉或是 Dispose 等..)
                // ex: DataSet DS = new DataSet();
                // 可在這邊 使用 DS.Dispose(); 或是 DS = null;
                // 或是釋放 自訂的物件。
                // 若繼承這個類別，可覆寫這個函式。
                // Free any other managed objects here.
            }
            _bDisposed = true;
        }
    }

    public class UDPServer
    {
        private Socket _Server;
        private byte[] _bufServerGetData;
        private bool _isStarted = false;
        private Thread _thread;
        private bool _bDisposed = false;

        public delegate void ServerHandleData(byte[] eData, XUDPTunnel clsTunnel);
        public event ServerHandleData ServerReceiveData;

        /// <summary>
        /// 設定Server Port和陣列大小
        /// </summary>
        /// <param name="iPort">server port number</param>
        /// <param name="theBufSize">server receive data buffer size</param>
        public UDPServer(int iPort, int theBufSize)
        {
            IPEndPoint IPEnd = new IPEndPoint(IPAddress.Any, iPort); //定義好伺服器port
            _Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //傳輸模式為UDP
            _Server.Bind(IPEnd); //綁定ip和port

            _bufServerGetData = new byte[theBufSize];

            _thread = new Thread(new ThreadStart(ServerListenData));
            _thread.IsBackground = true;
            _isStarted = true;
            _thread.Start();
        }

        ~UDPServer()
        {
            _bufServerGetData = null;
            _isStarted = false;
            _Server.Close();
            Dispose(false);
        }

        /// <summary>
        /// Server等待Client送資料過來　
        /// </summary>
        private void ServerListenData()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0); //定義空位址 給接收的存放(唯讀);
            EndPoint Remote = (EndPoint)sender; //宣告可以存放IP位址的用 EndPoint;        

            while (_isStarted)
            {
                try
                {
                    int recv = _Server.ReceiveFrom(_bufServerGetData, ref Remote); //把接收的封包放進getdata且傳回大小存入recv , ReceiveFrom(收到的資料,來自哪個IP放進Remote) 不能放IPEndPoint 因為唯獨
                    byte[] bufDataOut = new byte[recv];
                    if (ServerReceiveData != null)
                    {
                        Array.Copy(_bufServerGetData, bufDataOut, recv);
                        Array.Clear(_bufServerGetData, 0, recv);
                        XUDPTunnel clsTunnel = new XUDPTunnel(Remote, _Server);
                        ServerReceiveData.Invoke(bufDataOut, clsTunnel);
                    }
                }
                catch (Exception ex)
                { 
                
                }
                Thread.Sleep(5);
            }
        }

        //public void ServerSendData(byte[] bSendDatas, Encoding eEncode)
        //{            
        //    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0); //定義空位址 給接收的存放(唯讀);
        //    EndPoint Remote = (EndPoint)sender; //宣告可以存放IP位址的用 EndPoint;        
        //    if (eEncode == null) eEncode = Encoding.ASCII;
        //    _Server.SendTo(bSendDatas, bSendDatas.Length, SocketFlags.None, Remote); //將原資料送回去
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool IsDisposing)
        {
            if (_bDisposed)
            {
                return;
            }
            if (IsDisposing)
            {

            }
            _bDisposed = true;
        }
    }

    public static class UDPClient
    {
        /// <summary>
        /// Client送出資料
        /// </summary>
        /// <param name="strIPaddr">ServerIP</param>
        /// <param name="portNum">Server Port Number</param>
        /// <param name="strTX">要送出的字串</param>
        /// <param name="eEncode">送出字串的編碼</param>
        /// <returns>Server return string</returns>
        public static string UDPClientSendData(string strIPaddr, int portNum, string strTX, Encoding eEncode, int iTimeout)
        {
            if (eEncode == null) eEncode = Encoding.ASCII;

            IPEndPoint remoteIP = new IPEndPoint(IPAddress.Parse(strIPaddr), portNum); //定義一個位址 (伺服器位址)
            Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //傳輸模式為UDP          

            try
            {
                Client.SendTo(eEncode.GetBytes(strTX), remoteIP); //(傳送資料,對象的IP位址)傳送前先編碼成byte資料格式
            }
            catch (Exception ex)
            {
                return null;
            }

            byte[] data = new byte[1024]; //存放接收的資料
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0); //定一個空端點(唯讀)
            EndPoint Remote = (EndPoint)sender; //宣告可以存放IP位址的用 EndPoint(??)

            try
            {
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, iTimeout); //設定的時間內解除阻塞模式 

                int recv = Client.ReceiveFrom(data, ref Remote); //(收到的資料,來自哪個IP放進Remote) 不能放IPEndPoint 好像是它唯獨的關係 這時候sender已經變成跟remoteIP一樣
                string ClientRX = eEncode.GetString(data, 0, recv);//顯示資料前也要編碼一次 轉換回string資料格式
                return ClientRX;
            }
            catch (Exception ex)
            {
                Client.Shutdown(SocketShutdown.Receive);
                return null;
            }
        }

        /// <summary>
        /// Client送出資料
        /// </summary>
        /// <param name="strIPaddr">ServerIP</param>
        /// <param name="portNum">Server Port</param>
        /// <param name="strTX">要送出的字串</param>
        /// <param name="eEncode">送出字串的編碼</param>
        public static void UDPClientSendData(string strIPaddr, int portNum, string strTX, Encoding eEncode)
        {
            if (eEncode == null) eEncode = Encoding.ASCII;

            IPEndPoint remoteIP = new IPEndPoint(IPAddress.Parse(strIPaddr), portNum); //定義一個位址 (伺服器位址)
            Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //傳輸模式為UDP          
            try
            {
                Client.SendTo(eEncode.GetBytes(strTX), remoteIP); //(傳送資料,對象的IP位址)傳送前先編碼成byte資料格式
            }
            catch (Exception ex) { }
        }
    }

}
