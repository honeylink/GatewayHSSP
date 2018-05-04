using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetWaySystems433.protocol
{
    class ProtocolSeqCache
    {
        private static readonly ProtocolSeqCache instance = new ProtocolSeqCache();  //实例(线程安全)
        private HashtableSession deviceSeqMap = new HashtableSession();  //设备通信流水号缓存
        private Object thisLock = new Object();

        #region 单例

        private ProtocolSeqCache()
        {
        }

        ~ProtocolSeqCache()
        {
            deviceSeqMap.Clear();
            deviceSeqMap = null;
        }

        // 单例
        public static ProtocolSeqCache Instance
        {
            get
            {
                //if (instance == null)
                //    instance = new DeviceSeqCache();
                return instance;
            }
        }

        //供外部调用此方法加载终端信息
        public void Init()
        {
        }

        #endregion 单例


        /// <summary>
        /// 判断流水号是否存在，并进行相应的缓存维护
        /// </summary>
        /// <param name="id"></param>
        /// <param name="seq"></param>
        /// <returns></returns>
        public bool isExistSeq(string id, string seq)
        {       
            bool ret = false;
            lock (thisLock)     //StringBuilder 非线程安全，必须保护
            {
                try
                {
                    do
                    {
                        string seqMask = ",'" + seq + "'";
                        StringBuilder sbSeq = null;
                        if (!deviceSeqMap.Exist(id))
                        {
                            sbSeq = new StringBuilder(seqMask);
                            deviceSeqMap.Add(id, sbSeq);
                            break;
                        }

                        sbSeq = (StringBuilder)deviceSeqMap.Find(id);
                        if (sbSeq == null || sbSeq.Length < 0)  //if (sbSeq == null || sbSeq.Length <= 0)
                        {
                            break;
                        }
                        ret = sbSeq.ToString().Contains(seqMask);
                        if (ret == false)
                        {
                            sbSeq.Insert(0, seqMask);
                            if (sbSeq.Length > 500)
                            {
                                sbSeq = sbSeq.Remove(300, sbSeq.Length - 300);
                            }
                        }
                    } while (false);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return ret;
        }

        /// <summary>
        /// 重置设备的流水号缓存
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void resetDeviceSeq(string id)
        {
            if (deviceSeqMap.Exist(id))
            {
                StringBuilder sbSeq = (StringBuilder)deviceSeqMap.Find(id);
                sbSeq.Clear();
                sbSeq.Length = 0;
            }
        }

        /// <summary>
        /// 获取id的流水号缓存
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string getDeviceSeq(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return "";
            }
            if (deviceSeqMap.Exist(id))
            {
                return ((StringBuilder)deviceSeqMap.Find(id)).ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
