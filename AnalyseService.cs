using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GatewaySystem.socket;
using GatewaySystem.log;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using GatewayHSSP.HSSP;

namespace GatewayHSSP
{
    /// <summary>
    /// socket 委托
    /// </summary> 
    public delegate void AnalyseCallback(List<object> list);
    /// <summary>
    /// socket数据包
    /// </summary> 
    public delegate void LogCallback(object data);
    
    public class AnalyseService
    {        
        public delegate void SendPacketCallBack(SendPacketStruct param, SendPacketState state);
        public enum SendPacketState
        {
            Retransmit = 0x01,
            Success = 0x02,
            Fail = 0x03
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        public struct SendPacketStruct
        {
            public string socketEndIdentity;
            public int packetSequence;
            public byte[] btPacket;
            public int sendTryCount;
            public DateTime sendingTime;
            /*For callback*/
            public object otherParam;
            public SendPacketCallBack sendCallBack;
        }        

         /// <summary>
        /// 解析对象
        /// </summary>
        public struct Analysis
        {
            public SocketServer socketServer;
            public byte[] data;
            public Socket sk;
        }

        //对协议解析模块analyseService的优化：对于每个链路的发送数据单独组成一个列表，然后所有的列表组成一个大列表；方便查询和管理

        //单个网关的发送数据包管理
        public class SingleGateWayPacketMgr
        {
            public string gateWayNumber;
            public byte packetSequence;
            public List<SendPacketStruct> listSendPackets;
        }
        /// <summary>
        /// 待确认的已发送数据包管理单元集合
        /// </summary>
        public static List<SingleGateWayPacketMgr> PacketMgrSet = new List<SingleGateWayPacketMgr>();



        private static SocketServer winSocketServer;

       /// <summary>
       /// 分析数据
       /// </summary>
       /// <param name="winSocketServer">socket服务</param>
       /// <param name="callback"></param>
       /// <param name="logback"></param>
        public static void WinAnalyser(SocketServer winSocketServer, AnalyseCallback callback, LogCallback logback)
        {
            //严格注意，此处是否有内存泄露的风险?
            try
            {
                SocketPacket dataPacket = winSocketServer.ReceivedPackets.Dequeue();
                byte[] buffer = new byte[dataPacket.ReceivedLength];
                Array.Copy(dataPacket.DataBuffer, 0, buffer, 0, dataPacket.ReceivedLength);
                LogHelper.WriteRight("Receive: " + BitConverter.ToString(buffer));
                if (buffer == null || buffer.Length == 0)
                {
                    return;
                } 
                
                #region   解析数据
                Analysis newAnalyser = new Analysis();
                newAnalyser.socketServer = winSocketServer;
                newAnalyser.data = buffer;
                newAnalyser.sk = dataPacket.CurrentSocket;
                Thread thAnalysis = new Thread(new ParameterizedThreadStart(analysis));
                thAnalysis.IsBackground = true;
                thAnalysis.Start(newAnalyser); 
                //更新登录的客户端 
                #endregion  
            }
            catch (Exception ex)
            {
                LogHelper.WriteException(ex);
            }
        }
        
        /// <summary>
        /// 解析处理数据
        /// </summary>
        /// <param name="obj">参数</param>
        private static void analysis(object obj)
        {
            try
            {          
                Analysis analysisObj = (Analysis)obj;

                winSocketServer = analysisObj.socketServer; 
                byte[] tempAllBuffer = analysisObj.data;
                Socket sourceSocket = analysisObj.sk; 

                //!@todo 分解包

                ProccessSinglePacket(sourceSocket, tempAllBuffer);
            }
            catch (Exception ex)
            {
                LogHelper.WriteException(ex);
            } 
        }

        /// <summary>
        /// 处理单个包
        /// </summary>
        /// <param name="sourceSocket"></param>
        /// <param name="gatewayNumber"></param>
        /// <param name="gateWayPacketMgr"></param>
        /// <param name="packet"></param>
        private static void ProccessSinglePacket(Socket sourceSocket, byte[] packet)
        {
            int packetServiceType = 0;
            int packetType = 0;
            int packetSequence = 0;
            object depackageResult = Protocol.Depackage(packet, out packetServiceType, out  packetType, out packetSequence);

            if (packetType == (int)Protocol.PacketType.ACK)
            {
                //!@todo 去掉缓存池中缓存的待确认命令
            }
            else  
            {
                //判断是否是重复包
                if (depackageResult == null)
                {
                    LogHelper.WriteRight("depackageResult == null");
                    return;
                }
             
                bool isRepeatPacket = false; //!@todo 判断是否未重复包
 
                //处理                      
                string deviceId = null;
                List<DataParameterType> listIndication = (List<DataParameterType>)depackageResult;
                if (!isRepeatPacket)
                {
                    //非重复包，才进行业务处理 

                    foreach (DataParameterType dp in listIndication)
                    {
                        LogHelper.WriteRight(string.Format("received deviceNumber({0}),object:instance:resource-valuetype:value({1}:{2}:{3}-{4}:{5})", dp.deviceNumber, dp.objectId, dp.instance, dp.resource, dp.valueType, dp.value));
                        deviceId = dp.deviceNumber;
                    }
                }

                byte[] responsePacket = null;

                if (packetServiceType == (int)MethodCodes.HSSP_PUT)
                {
                    responsePacket = HSSP.HSSP.HSSP_packAck((byte)HSSP.ResponseCode.CONTENT_2_05, (UInt16)packetSequence);
                }
                else if (packetServiceType == (int)MethodCodes.HSSP_GET)
                {
                    unsafe
                    {
                        HSSPPacket_t coap_pkt;
                        List<HSSPObjectData_t> ObjectDataList = new  List<HSSPObjectData_t>();
                        foreach (DataParameterType dp in listIndication)
                        {
                            if (dp.resource == IPSO.IPSO_RESOURCE_CurrentTime)
                            {
                                //获取时间，执行时间的处理
                                HSSPObjectData_t Object = new HSSPObjectData_t()
                                {
                                    ObjectId = (UInt16)dp.objectId,
                                    InstanceId = (byte)dp.instance,
                                    ResourceId = (UInt16)dp.resource,
                                    ValueType = (byte)dp.valueType,
                                    valueLen = (byte)dp.valueLen,                                    
                                };
                                byte[] tmp = System.Text.Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                fixed (byte* tmpX = tmp)
                                {
                                    Object.pValue = tmpX;
                                }
                                Object.valueLen = (byte)tmp.Length;

                                ObjectDataList.Add(Object);
                            }
                        }
                        responsePacket = HSSP.HSSP.HSSP_pack(&coap_pkt, ObjectDataList.ToArray(), deviceId, (byte)MessageType.HSSP_TYPE_ACK, (byte)HSSP.ResponseCode.CONTENT_2_05, (UInt16)packetSequence);
                    }
                }

                if (responsePacket != null)
                {
                    winSocketServer.SendMsg(responsePacket, sourceSocket);
                    LogHelper.WriteRight(string.Format("send:{0}", BitConverter.ToString(responsePacket)));
                }                
            }
        }
    }
}

