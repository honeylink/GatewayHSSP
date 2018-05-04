using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using log4net;
 

namespace GatewaySystem.log
{
    public static class LogHelper {
        public static string formName;
        /// <summary>
        /// 调试日志
        /// </summary>
        private static ILog logger;
        /// <summary>
        /// 调试日志
        /// </summary>
        public static ILog Logger {
            get {
                if (logger == null) {
                    logger = LogManager.GetLogger(formName);
                }
                return logger;
            }
        }

        /// <summary>
        /// 记录运行日志
        /// 
        /// </summary>
        /// <param name="userval"></param>
        public static void Write(string userval, string filePath)
        {
            try
            {
                StreamWriter writer = File.AppendText(Application.StartupPath + @"\" + filePath);
                writer.Write(userval);
                writer.Flush();
                writer.Close();
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 记录运行日志
        /// 优化空间：对文件流进行保持
        /// </summary>
        /// <param name="userval"></param>
        public static void WriteRight(string userval)
        {
            try
            {
                string msg = string.Format("{0}:{1} \r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), userval);
                if (!Directory.Exists(Application.StartupPath + @"\运行日志"))
                    Directory.CreateDirectory(Application.StartupPath + @"\运行日志"); 
                StreamWriter writer = File.AppendText(Application.StartupPath + @"\运行日志\" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                writer.Write(msg);
                writer.Flush();
                writer.Close();
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteException(Exception ex)
        {
            WriteException(ex, string.Empty);
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteException(Exception ex, string backStr)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("************************************异常文本开始************************************");
                sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());
                if (ex != null)
                {
                    sb.AppendLine("【异常类型】：" + ex.GetType().Name);
                    sb.AppendLine("【异常信息】：" + ex.Message);
                    sb.AppendLine("【堆栈调用】：" + ex.StackTrace);
                }
                else
                {
                    sb.AppendLine("【未处理异常】：" + backStr);
                }
                sb.AppendLine("【程序名称】：" + formName);
                sb.AppendLine("************************************异常文本结束************************************");
                if (!Directory.Exists(Application.StartupPath + @"\异常日志"))
                    Directory.CreateDirectory(Application.StartupPath + @"\异常日志");
                StreamWriter writer = File.AppendText(Application.StartupPath + @"\异常日志\" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                writer.Write(sb.ToString());
                writer.Flush();
                writer.Close();
            }
            catch
            {
                return;
            }
        }
    }
}
