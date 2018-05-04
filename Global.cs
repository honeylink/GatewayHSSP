using System.Collections.Generic;
using System.Configuration;
using System.Net;

namespace GatewayHSSP
{
    /// <summary>
    /// 全局变量
    /// </summary>
    public static class Global
    {

        /// <summary>
        /// 监控ip
        /// </summary>
        private static string listenIP;
        public static string ListenIP
        {
            get
            {
                foreach (IPAddress add in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (!add.IsIPv6LinkLocal)
                        return add.ToString();
                }
                return listenIP;
            }
           
        }  
        /// <summary>
        /// 监听端口
        /// </summary>
        public static string ListenPort = ConfigurationManager.AppSettings["ListenPort"].ToString();
        /// <summary>
        /// 语言
        /// </summary>
        public static string Language = ConfigurationManager.AppSettings["Language"].ToString();
          /// <summary>
        /// 密码
        /// </summary>
        public static string pwd = ConfigurationManager.AppSettings["pwd"].ToString();            
    }
}
