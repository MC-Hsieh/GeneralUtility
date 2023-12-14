using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace GeneralUtility.Net
{
    public class XServer : IDisposable
    {   
        public List<XTunnle> TunnleList
        {
            get { return _TunnleList; }
        }
        private List<XTunnle> _TunnleList = new List<XTunnle>();
        public IPEndPoint ServerEP
        {
            get { return _clsServerEP; }
        }
        private IPEndPoint _clsServerEP;

        public int iCount
        {
            get { return _iCount; }
        }
        private int _iCount;

        private TcpListener _clsTcpListener;
        private Thread _ListenPort;
        private Thread _ScanXTunnel;
        private bool _bThreadExit = false;
        public event XeClientIn ClientIn;
        

        #region "New - Init"

        /// <summary>建構一個Server</summary>
        /// <param name="clsIpPort">IP端口</param>
        public XServer(IPEndPoint clsIpPort, int iCount = 1)
        {
            _iCount = iCount;
            _clsServerEP = clsIpPort;
            _clsTcpListener = new TcpListener(_clsServerEP);

            _ListenPort = new Thread(ScanServerPort);
            _ListenPort.Start();

            _ScanXTunnel = new Thread(ScanXTunnle);
            _ScanXTunnel.Start();
        }

        /// <summary>建構一個Server</summary>
        /// <param name="strIP">IP端口</param>
        public XServer(string strIP,int iCount = 1 )
        {
            _iCount = iCount;
            string[] strIP_Port = strIP.Split(':');
            _clsServerEP = new IPEndPoint(IPAddress.Parse(strIP_Port[0]), int.Parse(strIP_Port[1]));
            _clsTcpListener = new TcpListener(_clsServerEP);

            _ListenPort = new Thread(ScanServerPort);
            _ListenPort.Start();

            _ScanXTunnel = new Thread(ScanXTunnle);
            _ScanXTunnel.Start();
        }

        /// <summary>Server解構子</summary>
        ~XServer()
        {
            _clsTcpListener = null;
        }

        /// <summary>關閉Server並釋放資源</summary>
        public void Dispose()
        {
            _bThreadExit = true;
            if (_clsTcpListener != null)    _clsTcpListener.Stop();  
            foreach (XTunnle clsTunnle in _TunnleList)
            {
                clsTunnle.Close();
            }
        }

        /// <summary>關閉Server</summary>
        public void Close()
        {
            Dispose();
        }

        #endregion

        #region "Thread Scan"

        private void ScanServerPort()
        {
            while (!_bThreadExit)
            {
                try
                {
                    if (_TunnleList.Count < _iCount)
                    {
                        _clsTcpListener.Start();
                        Socket mySocket = _clsTcpListener.AcceptSocket();
                        XTunnle clsTunnle = new XTunnle(mySocket);
                        _TunnleList.Add(clsTunnle);
                        if (ClientIn != null)
                        {
                            ThreadPool.QueueUserWorkItem(s =>
                            {
                                ClientIn(clsTunnle);
                            });
                        }
                    }
                    else
                    {
                        _clsTcpListener.Stop();
                    }
                    Thread.Sleep(50);
                }
                catch(Exception ex)
                {
                    Thread.Sleep(50);
                    break;
                }
            }
        }

        private void ScanXTunnle()
        {
            while (!_bThreadExit)
            {
                for (int i = 0; i < _TunnleList.Count; i++)
                {
                    if (!_TunnleList[i].IsConnected)
                    {
                        _TunnleList[i].Close();
                        _TunnleList[i] = null;
                        _TunnleList.RemoveAt(i);
                        break;
                    }
                }
                Thread.Sleep(100);
            }
        }

        #endregion

        /// <summary>事件委派。</summary>
        /// <param name="sender">物件。</param>
        /// <param name="e">XCommunicationEventArgs事件物件。</param>
        public delegate void XeClientIn(XTunnle sender);
    }

    public class XClient : IDisposable
    {
        public IPEndPoint ClientEP
        {
            get { return _ClientEP; }
        }
        private IPEndPoint _ClientEP;

        public bool IsConnected
        {
            get { return _IsConnected; }
        }

        private bool _bThreadExit = false;
        public BackgroundWorker _bgwScanBuffer;
        private TcpClient _TcpClient;
        private bool _IsAutoConnect = true;
        private bool _IsConnected = true;
        public event XeRecive Received;
        public event XeStatic Disconnect;

        #region "New - Init"

        /// <summary>建立Client</summary>
        /// <param name="clsIpPort">IP端口</param>
        /// <param name="IsAutoConnect">是否自動連線</param>
        public XClient(IPEndPoint clsIpPort,bool IsAutoConnect = true)
        {
            _ClientEP = clsIpPort;
            _IsAutoConnect = IsAutoConnect;
            _bThreadExit = false;

            _bgwScanBuffer = new BackgroundWorker();
            _bgwScanBuffer.DoWork += new DoWorkEventHandler(ScanBuffer);
            _bgwScanBuffer.ProgressChanged += new ProgressChangedEventHandler(_bgwScanBuffer_ProgressChanged);
            _bgwScanBuffer.WorkerReportsProgress = true;
            _bgwScanBuffer.WorkerSupportsCancellation = true;
            _bgwScanBuffer.RunWorkerAsync();
        }

        /// <summary>建立Client</summary>
        /// <param name="clsIpPort">IP端口</param>
        /// <param name="IsAutoConnect">是否自動連線</param>
        public XClient(string strIP,bool IsAutoConnect = true)
        {
            string[] strIP_Port = strIP.Split(':');
            _ClientEP = new IPEndPoint(IPAddress.Parse(strIP_Port[0]), int.Parse(strIP_Port[1]));       
            _IsAutoConnect = IsAutoConnect;
            _bThreadExit = false;

            _bgwScanBuffer = new BackgroundWorker();
            _bgwScanBuffer.DoWork += new DoWorkEventHandler(ScanBuffer);
            _bgwScanBuffer.ProgressChanged += new ProgressChangedEventHandler(_bgwScanBuffer_ProgressChanged);
            _bgwScanBuffer.WorkerReportsProgress = true;
            _bgwScanBuffer.WorkerSupportsCancellation = true;
            _bgwScanBuffer.RunWorkerAsync();  
        }

        /// <summary>Client解構子</summary>
        ~XClient()
        {
            if (_bgwScanBuffer != null)
                _bgwScanBuffer.CancelAsync();
            _TcpClient = null;
        }
        /// <summary>關閉Client並釋放資源</summary>
        public void Dispose()
        {
            _bThreadExit = true;
            if (_bgwScanBuffer != null)
                _bgwScanBuffer.CancelAsync();
            if (_TcpClient != null) _TcpClient.Close();
        }
        /// <summary>關閉Client</summary>
        public void Close()
        {
            Dispose();
        }

        #endregion

        #region "Thread Scan"

        /// <summary>掃描Socket Buffer 與 檢察是否斷線</summary>
        private void ScanBuffer(object sender, DoWorkEventArgs e)
        {
            while (!_bThreadExit)
            {
                try
                {
                    if (_IsAutoConnect)
                    {
                        if (_TcpClient == null)
                        {
                            _TcpClient = new TcpClient();
                            _TcpClient.Connect(_ClientEP);
                            _IsConnected = true;
                        }
                    }
                    //測試是否斷線
                    byte[] testByte = new byte[1];
                    if (_TcpClient.Client.Connected && _TcpClient.Client.Poll(0, SelectMode.SelectRead))
                        _IsConnected = _TcpClient.Client.Receive(testByte, SocketFlags.Peek) != 0;

                    int iLenget = _TcpClient.Client.Available;
                    if (iLenget != 0)
                    {
                        byte[] byteRealData = new byte[iLenget];
                        _TcpClient.Client.Receive(byteRealData);
                        if (byteRealData.Length > 0)
                        {
                            if (Received != null)
                            {
                                ((BackgroundWorker)sender).ReportProgress(0, byteRealData);
                            }
                        }
                    }
                    if (!_IsConnected)
                    {
                        _TcpClient.Close();
                        _TcpClient = null;
                        if (Disconnect != null)
                        {
                            ThreadPool.QueueUserWorkItem(s =>
                            {
                                Disconnect(false);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _IsConnected = false;
                }
                Thread.Sleep(1);
            }
        }

        void _bgwScanBuffer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (Received != null)
                Received(this, (byte[])e.UserState);
        }

        /// <summary>送出資料</summary>
        /// <param name="bDatas">資料</param>
        public void Send(byte[] bDatas)
        {
            if (_TcpClient != null)
                if (_TcpClient.Client != null)
                    if (_IsConnected)
                        _TcpClient.Client.Send(bDatas);
        }

        #endregion

        public delegate void XeRecive(XClient sender, byte[] eData);
        public delegate void XeStatic(bool bStatic);
    }

    public class XTunnle
    {
        public bool IsConnected
        {
            get { return _IsConnected; }
        }
        private bool _IsConnected = true;

        private Socket _clsSocket;
        private bool _bThreadExit = false;

        public BackgroundWorker _bgwScanBuffer;
        public event XeRecive Received;
        public event XeStatic Disconnect;

        #region "New - Init"

        public XTunnle(Socket clsSocket)
        {
            _clsSocket = clsSocket;

            _bgwScanBuffer = new BackgroundWorker();
            _bgwScanBuffer.DoWork += new DoWorkEventHandler(ScanBuffer);
            _bgwScanBuffer.ProgressChanged += new ProgressChangedEventHandler(_bgwScanBuffer_ProgressChanged);
            _bgwScanBuffer.WorkerReportsProgress = true;
            _bgwScanBuffer.WorkerSupportsCancellation = true;
            _bgwScanBuffer.RunWorkerAsync();  
        }
        ~XTunnle()
        {
        }
        public void Close()
        {
            _bThreadExit = true;
            _clsSocket.Close();
            _clsSocket.Dispose();
            _IsConnected = false;
        }

        #endregion

        private void ScanBuffer(object sender, DoWorkEventArgs e)
        {
            while (!_bThreadExit)
            {
                try
                {
                    //測試是否斷線
                    byte[] testByte = new byte[1];
                    if (_clsSocket.Connected && _clsSocket.Poll(0, SelectMode.SelectRead))
                        _IsConnected = _clsSocket.Receive(testByte, SocketFlags.Peek) != 0;

                    byte[] byteRealData = new byte[_clsSocket.Available];
                    _clsSocket.Receive(byteRealData);
                    if (byteRealData.Length > 0)
                        if (Received != null)
                        {
                            if (Received != null)
                            {
                                ((BackgroundWorker)sender).ReportProgress(0, byteRealData);
                            }
                        } 

                    if (!_IsConnected)
                    {
                        if (Disconnect != null)
                        {
                            ThreadPool.QueueUserWorkItem(s =>
                            {
                                Disconnect(false);
                            });
                        } 
                    }
                }
                catch (Exception ex)
                {
                    _IsConnected = false;
                    break;
                }
            }
        }

        void _bgwScanBuffer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (Received != null)
                Received(this, (byte[])e.UserState);
        }

        public void Send(byte[] bDatas)
        {
            if (_clsSocket != null)  _clsSocket.Send(bDatas);
        }
        public void Connect()
        {
        }

        public delegate void XeRecive(XTunnle sender, byte[] eData);
        public delegate void XeStatic(bool bStatic);
    }
}
