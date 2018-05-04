using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Sockets; 
using System.IO;
using GatewaySystem.log;
using System.Collections.Concurrent;


namespace GatewaySystem.socket
{
    /// <summary>
    /// Socket服务器类
    /// </summary>
    public class SocketServer
    {      

        #region 委托
        public delegate void ConnectionEventHanlder(Socket socket);
        public delegate void ErrorEventHandler(Exception exception, Socket soc, int ErroCode);
        public delegate void ListenEventHandler();
        #endregion

        #region Events
        public event ConnectionEventHanlder OnConnect = null;
        public event ConnectionEventHanlder OnDisconnect = null;
        public event ConnectionEventHanlder OnRead = null;
        public event ConnectionEventHanlder OnWrite = null;
        public event ErrorEventHandler OnError = null;
        public event ListenEventHandler OnListen = null;
  
        #endregion

        #region Variables
        //private Dictionary<string, Socket> _ClientCollections =  new Dictionary<string, Socket>();
        private ConcurrentDictionary<string, Socket> _ClientCollections = new ConcurrentDictionary<string, Socket>();

        private const int MaxPacketList = 1000;


        /// <summary>
        /// 异步完成时调用的方法
        /// </summary>
        private Socket _MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPEndPoint _ServerEndPoint;
        private int _Port = 0;
        private byte[] _BytesReceived;
        private object _ObjectSent;
        public bool _Listening = false;  //Variable control     
        private Queue<SocketPacket> _ReceivedPackets = new Queue<SocketPacket>();
        #endregion

        #region Propetiers
        /// <summary>
        /// Port to connect with clients
        /// </summary>
        public int Port
        {
            get
            {
                return (_Port);
            }
        }

        /// <summary>
        /// Bytes received by the Socket
        /// </summary>
        public byte[] ReceivedBytes
        {
            get
            {
                byte[] temp = null;
                if (_BytesReceived != null)
                {
                    temp = _BytesReceived;
                    _BytesReceived = null;
                }
                return (temp);
            }
        }

        public object WriteObject
        {
            get
            {
                object temp = _ObjectSent;
                _ObjectSent = null;
                return temp;
            }
        }

        /// <summary>
        /// 客户端数量
        /// </summary>
        public int ActiveConnections
        {
            get
            {
                return (_ClientCollections.Count);
            }
        }
        /// <summary>
        /// 客户端队列
        /// </summary>
        //public Dictionary<string, Socket> ActiveClient
        public ConcurrentDictionary<string, Socket> ActiveClient
        {
            get
            {
                return _ClientCollections;
            }
            set
            {
                _ClientCollections = value;
            }
        }
        public Queue<SocketPacket> ReceivedPackets
        {
            get { return _ReceivedPackets; }
            set { this._ReceivedPackets = value; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="port">Port to wait for call</param>
        public SocketServer(int port)
        {        
            try
            {
                _Port = port;
                _ServerEndPoint = new IPEndPoint(IPAddress.Any, _Port);
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public SocketServer(string ip, int port)
        {
            try
            {
                _Port = port;
                _ServerEndPoint = new IPEndPoint(IPAddress.Any, _Port);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region Functions and Events
        /// <summary>
        /// 启动监听服务
        /// </summary>
        /// <returns></returns>
        public bool Active()
        {
            try
            {
                _Listening = true;
                _MainSocket.Bind(_ServerEndPoint);
                _MainSocket.Listen(1024);
                _MainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null); //定义异步事件客户连接时触发
                if (OnListen != null)
                {
                    OnListen();
                }
                return true;
            }
            catch (SocketException ex)
            {
                _Listening = false;

                string exMsg = "Exception: " + ex.Message  ;
                if (OnError != null)
                {
                    OnError(new Exception(exMsg, ex.InnerException), _MainSocket, 0);
                }
                return false;
            }
        }

        /// <summary>
        /// 关闭socket服务
        /// </summary>
        public bool Deactive()
        {
            try
            {
                _Listening = false;
                bool err = true;
                if (_MainSocket != null)
                    _MainSocket.Close();
                if (_ClientCollections != null && _ClientCollections.Count > 0)
                {
                    foreach (KeyValuePair<string, Socket> item in _ClientCollections)
                    {
                        Socket workerSocket = item.Value;
                        if (workerSocket != null)
                        {
                            if (OnDisconnect != null)
                            {
                                OnDisconnect(workerSocket);
                            }
                            workerSocket.Close();
                            err = err && workerSocket.Connected;
                        }
                    }
                }
                _ClientCollections.Clear();
                return err;
            }
            catch (Exception ex)
            {
                _ClientCollections.Clear();
                if (OnError != null)
                {
                    OnError(ex, null, 0);
                }
                return false;
            }
        }

        /// <summary>
        /// 客户端连接时触发
        /// 将会导致的速度影响问题：每次accept都是等到wait
        /// 如果从开始接收到accept包到开始处理时间过长，理所当然将会导致accept 异常
        /// </summary>
        /// <param name="asyn"></param>
        private void OnClientConnect(IAsyncResult asyn)
        {
            //log("OnClientConnect Callback\r\n");   
            try
            {
                Socket workSocket = null;
                try
                {
                    if (_Listening)
                    {
                        workSocket = _MainSocket.EndAccept(asyn);
                        try
                        {
                            WaitForData(workSocket);
                            RaiseOnConnect(workSocket);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (workSocket != null)
                    {
                        RaiseOnDisconnect(workSocket);
                        RemoveClientSocketCacheBySocket(workSocket);
                        workSocket.Close();
                        workSocket = null;
                    }
                    LogHelper.WriteException(ex);
                }
                finally
                {
                    _MainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
                }
            }
            catch (System.Exception ex)
            {
            	
            }

        }
        /// <summary>
        /// 等待客户端数据
        /// </summary>
        /// <param name="soc"></param>
        private void WaitForData(Socket soc)
        { 
            try
            {
                SocketPacket theSocPkt = new SocketPacket(soc);
                soc.BeginReceive(theSocPkt.DataBuffer, 0, SocketPacket.BufferMax, SocketFlags.None, new AsyncCallback(OnDataReceived), theSocPkt);
            }
            catch (SocketException ex)
            {               
                if (OnError != null)
                {
                    OnError(ex, soc, 0);
                }
                throw ex;
            }
        }
        /// <summary>
        /// 接收客户端数据
        /// 
        /// </summary>
        /// <param name="asyn"></param>
        private void OnDataReceived(IAsyncResult asyn)
        {
            log("OnDataReceived\r\n");
            try
            {
                SocketPacket socketData = (SocketPacket)asyn.AsyncState;
                try
                {
                    int iRx = socketData.CurrentSocket.EndReceive(asyn);
                    socketData.ReceivedLength = iRx;
                    if (iRx < 1)
                    {
                        //socket 有问题
                        RaiseOnDisconnect(socketData.CurrentSocket);
                        RemoveClientSocketCacheBySocket(socketData.CurrentSocket);
                        socketData.CurrentSocket.Close();
                        socketData.CurrentSocket = null;
                    }
                    else
                    {

                        if (_ReceivedPackets.Count < MaxPacketList)
                        {
                            ///需要进行数量的限制，防止数量过多导致上层处理崩溃              
                            _ReceivedPackets.Enqueue(socketData);//入列                              
                            if (OnRead != null)
                            {
                                OnRead(socketData.CurrentSocket);
                            }
                        }

                        WaitForData(socketData.CurrentSocket);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    //socket 有问题
                    RaiseOnDisconnect(socketData.CurrentSocket);
                    RemoveClientSocketCacheBySocket(socketData.CurrentSocket);
                    socketData.CurrentSocket.Close();
                    socketData.CurrentSocket = null;
                    //RaiseOnError(ex, null, 0);                   
                }
                catch (SocketException ex)
                {
                    //socket 有问题
                    RaiseOnDisconnect(socketData.CurrentSocket);
                    RemoveClientSocketCacheBySocket(socketData.CurrentSocket);
                    socketData.CurrentSocket.Close();
                    socketData.CurrentSocket = null;
                    //RaiseOnError(ex, socketData.CurrentSocket, ex.ErrorCode);                            
                }
            }
            catch (System.Exception ex)
            {
            	
            }
            
        } 


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="socketCacheIdentity"></param>
        /// <returns></returns>
        public bool SendMsg(byte[] message, string socketCacheIdentity)
        {
            try
            {
                Socket sc = GetSocket(socketCacheIdentity);

                if (sc != null)
                {
                    return SendMsg(message, sc);
                }
                else
                {
                    string exMsg = "发送" + socketCacheIdentity + "失败:" + BitConverter.ToString(message);
                    RaiseOnError(new Exception(exMsg), null, 0);
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                return false;
            }

        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="socketCacheIdentity"></param>
        /// <returns></returns>
        public bool SendMsg(byte[] message, Socket sc)
        {
            try
            {
                try
                {
                    if (sc == null)
                    {
                        return false;
                    }
                    if (!sc.Connected)
                    {
                        throw new Exception("not connected");
                    }
                    int NumBytes = sc.Send(message);
                    if (NumBytes == message.Length)
                    {
                        if (OnWrite != null)
                        {
                            OnWrite(sc);
                        }
                        return true;
                    }
                    else
                        return false;
                }
                catch (System.Exception ex)
                {
                    RaiseOnDisconnect(sc);
                    RemoveClientSocketCacheBySocket(sc);
                    sc.Close();
                    sc = null;
                    return false;
                }   
            }
            catch (System.Exception ex)
            {
                return false;
            }      
        }

        
        private void RaiseOnConnect(Socket sc)
        {
            //log("RaiseOnConnect from:" + sc.RemoteEndPoint.ToString() +"\r\n");
            if (OnConnect != null)
            {
                OnConnect(sc);
            }  
        }

        private void RaiseOnDisconnect(Socket sc)
        {
            //log("RaiseOnDisconnect from:" + sc.RemoteEndPoint.ToString() + "\r\n");
            if (OnDisconnect != null)
            {
                OnDisconnect(sc);
            }
        }
        private void RaiseOnError(Exception exception, Socket soc, int ErroCode)
        {
            if (OnError != null)
            {
                OnError(exception, soc, ErroCode);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        ///
        ///被调用型，不能触发任何的事件
        ///
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 检查socket 是否存在
        /// </summary> 
        /// <param name="wineStockNumber"></param>
        /// <returns></returns>
        public bool CheckSocket(string socketCacheIdentity)
        {

            if (_ClientCollections.ContainsKey(socketCacheIdentity))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 返回匹配的socket
        /// 无匹配则返回null
        /// </summary> 
        /// <param name="socketCacheIdentity"></param>
        /// <returns></returns>
        public Socket GetSocket(string socketCacheIdentity)
        {
            if (_ClientCollections.ContainsKey(socketCacheIdentity))
            {
                return (Socket)_ClientCollections[socketCacheIdentity];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 返回匹配的cacheIdentity
        /// </summary> 
        /// <param name="Socket">连接对象</param>
        /// <returns></returns>
        public string GetCacheIdentity(Socket soc)
        {
            return _ClientCollections.Where(l => l.Value == soc).Select(l => l.Key).FirstOrDefault();

        }
        /// <summary>
        /// 关闭 socket
        /// </summary> 
        /// <param name="socketCacheIdentity"></param>
        /// <returns></returns>
        public void  closeSocket(string socketCacheIdentity)
        {
            if (_ClientCollections.ContainsKey(socketCacheIdentity))
            {
                Socket workerSocket = (Socket)_ClientCollections[socketCacheIdentity];
                _ClientCollections.TryRemove(socketCacheIdentity, out workerSocket);
                workerSocket.Close();
                workerSocket = null;
            }            
        }

        /// <summary>
        ///判断编号是否在已连接的socket队列中
        /// </summary>
        /// <param name="SocketIndex">Index of connection</param>
        public bool IsConnected(string socketCacheIdentity)
        {
            if (_ClientCollections.ContainsKey(socketCacheIdentity))
            {
                Socket soc = (Socket)_ClientCollections[socketCacheIdentity];
                return soc.Connected;
            }
            if (OnError != null)
            {
                string exMsg = "Exception: {" + socketCacheIdentity + "} is not exist";
                OnError(new Exception(exMsg), null, 0);
            }
            return false;
        }
        


        /// <summary>
        /// 添加cache
        /// </summary>
        /// <param name="socketCacheIdentity"></param>
        /// <param name="sc"></param>
        public void AddClientSocketCache(string socketCacheIdentity, Socket sc)
        {
            _ClientCollections.TryAdd(socketCacheIdentity, sc);

            //log("After add " + socketCacheIdentity + ",the client count:" + ActiveConnections + "\r\n");
        }

        /// <summary>
        /// 删除cache
        /// 由于同个socket 可能有多个项匹配，所以这里是删除多项
        /// </summary>
        /// <param name="sc"></param>
        private void RemoveClientSocketCacheBySocket(Socket sc)
        {    
            try
            {
                if (sc == null)
                {
                    return;
                }

                /*
                var item = _ClientCollections.Where(m => m.Value.Equals(sc)).FirstOrDefault();
                if (item.Key != null)
                {
                    _ClientCollections.TryRemove(item.Key, out sc);
                    log("After delete " + item.Key + ",the client count:" + ActiveConnections + "\r\n");   
                }
                 */
                

                //在由SendMsg调用本方法时此处会报异常，原因不明
                //原因可能是sc本身已经由其它的线程释放了
                Dictionary<string, Socket> tobedo = _ClientCollections.Where(m => m.Value.Equals(sc)).ToDictionary(o => o.Key, o => o.Value);

                foreach (var item in tobedo)
                {
                    _ClientCollections.TryRemove(item.Key, out sc);
                    //log("After delete " + item.Key + ",the client count:" + ActiveConnections + "\r\n");
                }
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        private void RemoveClientSocketCacheByIdentity(string id)
        {
            Socket sc;
            _ClientCollections.TryRemove(id, out sc);
        }


        private void log(string s)
        {
            LogHelper.Write(DateTime.Now.ToString() + " " + s, "socketServer.txt");   
        }

        #endregion

       
    }

    #region Class
    /// <summary>
    /// 数据对象  
    /// </summary>
    public class SocketPacket
    {
        public SocketPacket(Socket soc)
        {
            CurrentSocket = soc;
        }
        public Socket CurrentSocket;
        public const int BufferMax = 2048;
        public byte[] DataBuffer = new byte[BufferMax];
        public int ReceivedLength = 0;
    }
    #endregion

}
