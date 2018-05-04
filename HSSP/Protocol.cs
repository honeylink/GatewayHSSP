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
    /// 参数结构
    /// </summary>
    public struct DataParameterType
    {
        public string deviceNumber;
        public int objectId;
        public int instance;
        public int resource;
        public int valueType;
        public int valueLen;
        public bool isOnline;
        public object value;        
    }
    
    public class Protocol
    {
        const byte version = 1;
        public enum SensorDataControler
        {
            ONLINE = 0x00,
            OFFLINE = 0x80,
        }

        /// <summary>
        /// 包类型
        /// </summary>
        public enum PacketType
        {
            CMD = 0x00,//命令包
            ACK = 0x01//ACK 
        }



        #region 数据打包和解包


        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        unsafe public static object Depackage(byte[] packet, out int packetServiceType, out int packetType, out int packetSequence)
        {
            packetServiceType = 0;
            packetType = (int)PacketType.CMD;
            packetSequence = 0;

            HSSP.HSSPPacket_t hssp_pkt;

            List<DataParameterType> returnX = new List<DataParameterType>();

            string deviceId = null;

            fixed (byte* pPacket = packet)
            {
                if (HSSP.HSSP.hssp_parse_message((void*)&hssp_pkt, pPacket, (ushort)packet.Length) != 0)
                {
                    LogHelper.WriteRight("Depackage HSSP.HSSP.hssp_parse_message error\r\n");
                    return null;
                }
                LogHelper.WriteRight("hssp_pkt.mid:" + hssp_pkt.mid);

                packetSequence = hssp_pkt.mid;
                packetServiceType = (int)hssp_pkt.code;

                if (hssp_pkt.type == (byte)MessageType.HSSP_TYPE_ACK)
                {               
                    packetType = (int)Protocol.PacketType.ACK;                   
                }

                if (hssp_pkt.payload_len == 0)
                {
                    LogHelper.WriteRight("Depackage hssp_pkt.payload_len == 0\r\n");
                    return null;
                }
                
                //将payload 复制到 byte[]中
                byte* payloadTmp = hssp_pkt.payload;

                uint payloadLen = hssp_pkt.payload_len;
                byte[] payload = new byte[payloadLen];
                for (uint i = 0; i < payloadLen; i++ )
                {
                    payload[i] = payloadTmp[i];
                }               
                

                //处理payload 业务
                int offset = 0;
                if (payloadLen >= 19)
                {
                    byte controlHeader = payload[0];
                    byte option = payload[1];
                    deviceId = System.Text.Encoding.ASCII.GetString(payload, 2, 15);
                    UInt16 objectDataLenght = BitConverter.ToUInt16(payload, 17);

                    if (objectDataLenght + 19 > payloadLen)
                    {
                        //error
                        LogHelper.WriteRight("Depackage objectDataLenght + 19 > packet.Length:" + objectDataLenght);
                        return null;
                    }
                }
                else
                {
                    LogHelper.WriteRight("Depackage packet.Length >= 19");
                    return null;
                }

                offset = 19;
                while (offset < payloadLen)
                {
                    if (payloadLen < offset + 6)
                    {
                        break;
                    }
                    UInt16 objectId = BitConverter.ToUInt16(payload, offset);
                    offset += 2;
                    byte instanceid = payload[offset];
                    offset += 1;
                    UInt16 resourceId = BitConverter.ToUInt16(payload, offset);
                    offset += 2;
                    byte valueType = payload[offset];
                    offset += 1;
                    byte valueLen = payload[offset];
                    offset += 1;

                    object value = null;

                    switch ((HSSPValueType)valueType)
                    {
                        case HSSPValueType.VALUE_BOOLEAN:
                            value = payload[offset];
                            break;
                        case HSSPValueType.VALUE_INTEGER:
                            value = BitConverter.ToInt32(payload, offset);
                            break;
                        case HSSPValueType.VALUE_FLOAT:
                            value = BitConverter.ToSingle(payload, offset);
                            break;
                        case HSSPValueType.VALUE_STRING:
                            value = System.Text.Encoding.ASCII.GetString(payload, offset, valueLen);
                            break;
                        case HSSPValueType.VALUE_BINARY:
                            byte[] tmp = new byte[valueLen];
                            Array.Copy(payload, offset, tmp, 0, valueLen);
                            value = tmp;
                            break;
                    }
                    offset += valueLen;

                    returnX.Add(new DataParameterType()
                        {
                            deviceNumber = deviceId,
                            objectId = objectId,
                            instance = instanceid,
                            resource = resourceId,
                            valueType = valueType,
                            valueLen = valueLen,
                            isOnline = true,
                            value = value,
                        }
                    );
                    
                }
            }                             
            
            return returnX;
        }
        #endregion

    }
}
