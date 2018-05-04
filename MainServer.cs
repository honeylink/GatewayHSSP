using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Collections;

using GatewaySystem.log;
using GatewaySystem.socket;
using GatewayHSSP;

namespace GatewayHSSP
{
    public partial class MainServer : Form
    {
        public MainServer()
        {
            InitializeComponent();
        }
      
        SelectLanguage Language = new SelectLanguage();
        private bool IsStart = false;
        private SocketServer WinSocketServer = null;
        private LogCallback _LogCallback = null;//回写日志 
        Thread thSendPackets;


        /// <summary>
        /// 开关socket服务
        /// </summary>
        /// <param name="IsStart">true 开 false关</param>
        private void StartStopSocket()
        {
            try
            {
                if (IsStart)//关闭
                {
                    if (WinSocketServer != null)
                    { 
                        WinSocketServer.Deactive();
                        WinSocketServer = null; 
                        LogHelper.WriteRight(string.Format("  {0} {1}\r\n", DateTime.Now, "断开监听，端口："+ Global.ListenPort));
                    }
                    IsStart = false;
                    thSendPackets.Abort();
                }
                else //启动
                {
                 
                    WinSocketServer = new SocketServer(Global.ListenIP,int.Parse( Global.ListenPort)); 
                    //连接事件
                    WinSocketServer.OnConnect += new SocketServer.ConnectionEventHanlder(WinSocketServer_OnConnect);
                    //断开事件
                    WinSocketServer.OnDisconnect += new SocketServer.ConnectionEventHanlder(WinSocketServer_OnDisconnect);
                    //错误事件
                    WinSocketServer.OnError += new SocketServer.ErrorEventHandler(WinSocketServer_OnError);
                    //监听事件
                    WinSocketServer.OnListen += new SocketServer.ListenEventHandler(WinSocketServer_OnListen);
                    //读数据事件
                    WinSocketServer.OnRead += new SocketServer.ConnectionEventHanlder(WinSocketServer_OnRead); 
                    //启动监听
                    WinSocketServer.Active();
                    thSendPackets = new Thread(ThreadSendPackets);
                    thSendPackets.IsBackground = true;
                    thSendPackets.Start();
                    LogHelper.WriteRight("启动监听，端口："+Global.ListenPort);
                 
                    IsStart = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteException(ex);              
            }
        }
        /// <summary>
        /// 连接事件
        /// </summary>
        /// <param name="soc"></param>
        private void WinSocketServer_OnConnect(Socket soc)
        {
            try
            {
                IPEndPoint endPoint = (IPEndPoint)soc.RemoteEndPoint;
                string content = " 收到连接 | IP: " + endPoint.Address.ToString() + " | Port: " + endPoint.Port;
                LogHelper.WriteRight(string.Format("  {0} {1}\r\n", DateTime.Now, content));
            }
            catch (Exception ex)
            {
                LogHelper.WriteException(ex);
            }
        }
        /// <summary>
        /// socket断开连接事件
        /// </summary>
        /// <param name="soc"></param>
        private void WinSocketServer_OnDisconnect(Socket soc)
        {
            try
            {                
                var item = WinSocketServer.ActiveClient.Where(m => m.Value.Equals(soc)).FirstOrDefault();
                if (item.Key != null)
                {                  
                    //释放数据包管理单元
                    string GateWayNumber = WinSocketServer.GetCacheIdentity(soc);
                    //找到网关对应的管理单元
                    AnalyseService.SingleGateWayPacketMgr mgr = null;
                    if (!string.IsNullOrEmpty(GateWayNumber))
                    {
                        mgr = AnalyseService.PacketMgrSet.Find(
                                     delegate(AnalyseService.SingleGateWayPacketMgr tmp)
                                     {
                                         return tmp.gateWayNumber == GateWayNumber;
                                     });
                    }    
                    if (mgr !=null)
                    {
                        //清理命令池
                        List<AnalyseService.SendPacketStruct> listSendPackets = mgr.listSendPackets;
                        foreach (AnalyseService.SendPacketStruct xitem in listSendPackets)
                        {
                            //对于失败并删除的，如果是下发的命令，一定要根据失败的情况调用指定的回调函数以便更新数据库中的数据
                            if (xitem.sendCallBack != null)
                            {
                                xitem.sendCallBack(xitem, AnalyseService.SendPacketState.Fail);
                            }
                            listSendPackets.Remove(xitem);
                        }
                        //!@todo 需要确认是否需要将此管理单元彻底删除
                        AnalyseService.PacketMgrSet.Remove(mgr);
                    }


                    LogHelper.WriteRight("WinSocketServer_OnDisconnect for:" + GateWayNumber);

                     

                    //清理设备的网关
                    //DeviceModel dmodel = new DeviceModel();
                    //smodel.ConnetDeviceNo = item.Key;
                    //AnalyseService.deviceBLL.ClearGatway(dmodel);                   
                    LogHelper.WriteRight(string.Format("  {0} {1}\r\n", DateTime.Now, "断开服务"));
                } 
            }
            catch (Exception ex)
            {
                LogHelper.WriteException(ex);                
            }
        }
 
        /// <summary>
        /// socket发生异常
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="soc"></param>
        /// <param name="ErroCode"></param>
        private void WinSocketServer_OnError(Exception exception, Socket soc, int ErroCode)
        {
            try
            {
                string content = string.Empty;
                if (soc != null)
                {
                    IPEndPoint endPoint = (IPEndPoint)soc.RemoteEndPoint;
                }
                content = exception.Message;
                LogHelper.WriteRight(string.Format("  {0} {1}\r\n", DateTime.Now, "异常断开服务:" + exception.ToString()));          
            }
            catch (Exception ex)
            {
                LogHelper.WriteException(ex);
            }
        }
        /// <summary>
        /// 监听事件
        /// </summary>
        private void WinSocketServer_OnListen()
        {
            try
            {
                LogHelper.WriteRight(string.Format("  {0} {1}\r\n", DateTime.Now, "Win监听服务启动，端口： " + Global.ListenPort));
            }
            catch (Exception ex)
            {
                LogHelper.WriteException(ex);
            }
        }
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="soc"></param>
        private void WinSocketServer_OnRead(Socket soc)
        {
            try
            {
                AnalyseService.WinAnalyser(WinSocketServer, AnalyserCallback, _LogCallback);
            }
            catch (Exception ex)
            {
                LogHelper.WriteException(ex);
            }
        }
        /// <summary>
        /// 解析数据后的回调接口
        /// </summary>
        /// <param name="server"></param>
        private void AnalyserCallback(List<object> list)
        {
           //nothing to do 
        }


#region UI process

        private void MainServer_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;

            //开启监听服务 
            Thread th = new Thread(StartStopSocket);
            th.Name = " socket监控";
            th.Start();
            //语言包
            Language.SetLanguage(this);

            ShowHidden();
        }
       

        private void MainServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                WindowState = FormWindowState.Minimized; 
                e.Cancel = true;
            }
            catch
            {
                e.Cancel = false;
                return;
            }
        }
        
        private void MainServer_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            notifyIcon1 .Visible = false;
        }
#endregion

#region business process

        /// <summary>
        /// 发送包线程
        /// </summary>
        private void ThreadSendPackets()
        {
            #region 发送命令
            while (true)
            {
                Thread.Sleep(1000); 
                if (WinSocketServer == null)
                {
                    continue;
                }
            }
           
             #endregion
        }


        /// <summary>
        /// 发送用户设置命令
        /// </summary>
        private void SendUserSettingCommand()
        {

        }

        /// <summary>
        /// 用户数据命令发送回调接口
        /// </summary>
        /// <param name="param"></param>
        /// <param name="state"></param>
        private void UserCommandPacketCallback(AnalyseService.SendPacketStruct param, AnalyseService.SendPacketState state)
        {
        }


        public void ShowHidden()
        {
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Minimized;
            this.Location = new System.Drawing.Point(-1000, -1000);
            this.Size = new System.Drawing.Size(0, 0);
            this.Visible = false;
        }
    }
#endregion
}
