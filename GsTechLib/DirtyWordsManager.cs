using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;

namespace GsTechLib
{
    //脏话名字枚举
    public enum EnumDirtyWordsName
    {
        Name,//取名
        Chat,//聊天
    }

    /// <summary>
    /// 脏话管理器
    /// </summary>
    public class DirtyWordsManager
    {
        private static Dictionary<string, DirtyWordsManager> _instance = new Dictionary<string, DirtyWordsManager>();

        /// <summary>
        /// 添加一个脏话管理器
        /// </summary>
        /// <param name="dwm"></param>
        public static void AddManager(DirtyWordsManager dwm)
        {
            if (_instance.ContainsKey(dwm.Name))
                _instance[dwm.Name] = dwm;
            else
                _instance.Add(dwm.Name, dwm);
        }

        /// <summary>
        /// 获取脏话管理器
        /// </summary>
        /// <param name="name">脏话管理器名</param>
        /// <returns>返回脏话管理器对象</returns>
        public static DirtyWordsManager GetManager(string name)
        {
            if (_instance.ContainsKey(name))
                return _instance[name];
            else
                return null;
        }

        private string _name = "";
        /// <summary>
        /// 脏话管理器名
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }
        private string _dirtyWordsFile = "";

        private static bool _invaild = false;
        /// <summary>
        /// 标志本类是否可用
        /// </summary>
        public static bool Invaild
        {
            get
            {
                return _invaild;
            }
        }

        /// <summary>
        /// 缓存脏字对象
        /// </summary>
        private ArrayList _dirtyWordsList = new ArrayList();

        #region Private Methods

        /// <summary>
        /// 脏话管理器实例化
        /// </summary>
        /// <param name="name">脏话管理器名</param>
        /// <param name="dirtyWordsFile">脏话管理器配置文件</param>
        public DirtyWordsManager(string name, string dirtyWordsFile)
        {
            _name = name;
            _dirtyWordsFile = dirtyWordsFile;

            ReloadDirtyWords();
        }

        /// <summary>
        /// 重新载入脏话管理器内容
        /// </summary>
        public void ReloadDirtyWords()
        {
            try
            {
                LoadXml(_dirtyWordsFile);
            }
            catch (Exception)
            {
                //标志此类不能用
                _invaild = true;
            }
        }

        /// <summary>
        /// 加载配置文件，获取脏字集。
        /// </summary>
        /// <param name="xmlFile">配置文件路径名</param>
        private void LoadXml(string xmlFile)
        {
            _dirtyWordsList = new ArrayList();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);

            XmlNodeList xmlNodes = xmlDoc.SelectNodes("data/dirtywords");
            if (xmlNodes != null)
            {
                foreach (XmlNode xmlNode in xmlNodes)
                {
                    _dirtyWordsList.Add(xmlNode.Attributes["keywords"].Value);
                }
            }
            SvLogger.Info("Load DirtyWords [{0}] Succ : File = {1}, Count = {2}.", _name, xmlFile, _dirtyWordsList.Count);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 检查字符串中是否有脏字
        /// </summary>
        /// <param name="data">要检查的字符串</param>
        /// <param name="checksymbol">是否检查符号</param>
        /// <returns>返回值 true有脏字，false没有脏字</returns>
        public bool CheckDirtyWord(string data, bool checksymbol)
        {
            if (checksymbol)
            {
                //这里认为这个函数只有取名的时候调用，那么下面这些字符都是不能包含的
                if (data.Contains("=") || data.Contains(",") || data.Contains(";") || data.Contains(":") || data.Contains("/") || data.Contains(@"\") || data.Contains("'") || data.Contains("\"") || data.Contains("　") || data.Contains(" "))
                    return true;
            }

            foreach (string restrictStr in _dirtyWordsList)
            {
                if (restrictStr != "")
                    if (data.ToUpper().IndexOf(restrictStr.ToUpper()) >= 0)
                        return true;
            }
            return false;
        }

        public bool CheckDirtyWord(string data)
        {
            return CheckDirtyWord(data, true);
        }

        /// <summary>
        /// 用指定名称的脏话管理器来检查字符串中是否有脏字
        /// </summary>
        /// <param name="name">脏话管理器名</param>
        /// <param name="data">要检查的字符串</param>
        /// <returns>返回值 true有脏字，false没有脏字</returns>
        public static bool CheckDirtyWord(string name, string data, bool checksymbol)
        {
            DirtyWordsManager dwman = DirtyWordsManager.GetManager(name);
            if (dwman != null)
                return dwman.CheckDirtyWord(data, checksymbol);
            else
                return false;
        }

        public static bool CheckDirtyWord(string name, string data)
        {
            return CheckDirtyWord(name, data, true);
        }

        /// <summary>
        /// 用指定名称的脏话管理器来检查字符串中是否有脏字
        /// </summary>
        /// <param name="name">脏话管理器名</param>
        /// <param name="data">要检查的字符串</param>
        /// <returns>返回值 true有脏字，false没有脏字</returns>
        public static bool CheckDirtyWord(EnumDirtyWordsName name, string data, bool checksymbol)
        {
            return CheckDirtyWord(name.ToString(), data, checksymbol);
        }

        public static bool CheckDirtyWord(EnumDirtyWordsName name, string data)
        {
            return CheckDirtyWord(name, data, true);
        }

        /// <summary>
        /// 过滤脏字
        /// </summary>
        /// <param name="data">要过滤的字符串</param>
        /// <returns>返回过滤后的字符串</returns>
        public string FilterDirtyWord(string data)
        {
            string newdata = data.ToUpper();
            foreach (string restrictStr in _dirtyWordsList)
            {
                if (restrictStr != "")
                {
                    string str = restrictStr.ToUpper();
                    int pos = 0;
                    while (pos >= 0)
                    {
                        pos = newdata.IndexOf(str);
                        if (pos >= 0)
                        {
                            newdata = newdata.Substring(0, pos) + "***" + newdata.Substring(pos + str.Length);
                            data = data.Substring(0, pos) + "***" + data.Substring(pos + str.Length);
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// 用指定名称的脏话管理器来过滤脏字
        /// </summary>
        /// <param name="name">脏话管理器名</param>
        /// <param name="data">要过滤的字符串</param>
        /// <returns>返回过滤后的字符串</returns>
        public static string FilterDirtyWord(string name, string data)
        {
            DirtyWordsManager dwman = DirtyWordsManager.GetManager(name);
            if (dwman != null)
                return dwman.FilterDirtyWord(data);
            else
                return data;
        }

        /// <summary>
        /// 用指定名称的脏话管理器来过滤脏字
        /// </summary>
        /// <param name="name">脏话管理器名</param>
        /// <param name="data">要过滤的字符串</param>
        /// <returns>返回过滤后的字符串</returns>
        public static string FilterDirtyWord(EnumDirtyWordsName name, string data)
        {
            return FilterDirtyWord(name.ToString(), data);
        }

        #endregion

    }

}
