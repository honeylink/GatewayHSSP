using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace GatewaySystem.log
{
    public static class Until
    {
        /// <summary>  
        /// 16进制字符串转字节数组  
        /// </summary>  
        /// <param name="hexString"></param>  
        /// <returns></returns>  
        public static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Contains("DOO"))
            {
                byte[] bTemp = System.Text.Encoding.Default.GetBytes(hexString);
                return bTemp;
            }

            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            return returnBytes;
        }
        /// <summary>
        /// Byte序列按照16进制转换成无符号整数
        /// </summary>
        /// <param name="tem">原始byte</param>
        /// <param name="startIndex">开始位置</param>
        /// <param name="endIndex">结束位置</param>
        /// <returns>结果Int</returns>
        public static int Byte16ToUInt(byte[] tem,int startIndex,int endIndex)//
        {
            uint I = 0;
            byte[] bte = new byte[4] { 0x00, 0x00, 0x00, 0x00};
            int length = endIndex - startIndex;
            Array.Copy(tem,startIndex,bte,0,length);
            I = BitConverter.ToUInt32(bte, 0);
            return (int)I; 
        }

        /// <summary>
        /// Byte序列按照16进制转换成有符号整数
        /// </summary>
        /// <param name="tem">原始byte</param>
        /// <param name="startIndex">开始位置</param>
        /// <param name="endIndex">结束位置</param>
        /// <returns>结果Int</returns>
        public static int Byte16ToInt(byte[] tem, int startIndex, int endIndex)
        {
            int I = 0;
            string vl = "";            

            //补全4字节，根据最高位确定是补0x00还是0xFF
            if (endIndex - startIndex < 4)
            {
                string prefix = "";
                if ((tem[endIndex-1] & 0x80) == 0)
                {
                    prefix = "00";
                }
                else
                {
                    prefix = "FF";
                }
                for (int i = 0; i < 4 - (endIndex - startIndex); i++)
                {
                    vl += prefix;
                }
            }

            for (int i = endIndex - 1; i >= startIndex; i--)
            {
                vl += tem[i].ToString("X2");
            }     

            I = Convert.ToInt32(vl, 16);
            return I;
        }

        /// <summary>
        /// Byte转string 
        /// </summary>
        /// <param name="tem">原始byte</param>
        /// <param name="startIndex">开始位置</param>
        /// <param name="endIndex">结束位置</param>
        /// <returns>结果Int</returns>
        public static string  ByteToString16(byte[] tem, int startIndex, int endIndex)
        {
            int I = 0;
            string vl = "";
            int length = endIndex - startIndex;
            for (int i = startIndex + length - 1; i >= startIndex; i--)
            {
                vl = vl + tem[i].ToString("X2");
            }
             
            return vl ;
        }

        /// <summary>
        /// int转byte
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte[] intTobyte(int val, int size)
        {
            byte[] relByte = new byte[size]; 
            byte[] aa = BitConverter.GetBytes(val);
            Array.Copy(aa, 0,  relByte,0, size);
            return relByte;
        }
    }
}
