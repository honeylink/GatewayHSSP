using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Reflection;

namespace GetWaySystems433.protocol
{
    public class HashtableSession
    {
        public const int DefaultIniCount = 1000;  // 默认的初始大小 
        private Hashtable sessionList = Hashtable.Synchronized(new Hashtable());  // 保存所有项的哈希表 

        #region 属性

        public Hashtable SessionList
        {
            get { return sessionList; }
        }

        public int Count
        {
            get { return sessionList.Count; }
        }

        public ArrayList Keys
        {
            get { return new ArrayList(sessionList.Keys); }
        }

        public ArrayList Values
        {
            get { return new ArrayList(sessionList.Values); }
        }

        public string[] StrKeys
        {
            get
            {
                string[] keys = new string[sessionList.Keys.Count];
                sessionList.Keys.CopyTo(keys, 0);
                return keys;
            }
        }

        #endregion

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public HashtableSession()
            : this(DefaultIniCount)
        {
        }

        //构造函数
        public HashtableSession(int capacity)
        {
            sessionList = Hashtable.Synchronized(new Hashtable(capacity));
        }

        public bool Exist(object key)
        {
            return sessionList.ContainsKey(key);
        }

        public object Find(object key)
        {
            if (sessionList.ContainsKey(key))
                return sessionList[key];
            else
                return null;
        }

        public bool Add(object key, object value)
        {
            if (sessionList.ContainsKey(key))
            {
                sessionList[key] = value;
            }
            else
            {
                sessionList.Add(key, value);
            }

            return true;
        }

        public void Remove(object key)
        {
            if (sessionList.ContainsKey(key))
            {
                sessionList.Remove(key);
            }
        }

        public object RemoveEx(object key)
        {
            object ret = null;
            if (sessionList.ContainsKey(key))
            {
                ret = sessionList[key];
                sessionList.Remove(key);
            }
            return ret;
        }

        public void Clear()
        {
            sessionList.Clear();
        }
    }

}
