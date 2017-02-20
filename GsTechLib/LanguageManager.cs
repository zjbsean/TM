using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace GsTechLib
{
    /// <summary>
    /// 语言管理器
    /// </summary>
    public class LanguageManager
    {
        private string _defaultLang = "CN";
        private string _generalServerType = "General";
        private Hashtable _serverLangList = null;//以serverType为Key存储HashTable(以itemId为Key存储LanguageItem)

        private static LanguageManager _instance = new LanguageManager();
        public static LanguageManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public LanguageManager()
        {
            string langFile = @"GameConfigData\ServerLanguage.xml";
            if (File.Exists(langFile))
            {
                string errMsg = LoadLangList(langFile);
                if (errMsg != "")
                    throw new Exception(string.Format("Load language config file fail for {0}.", errMsg));
            }
            else
                throw new Exception("Language config file not exists!");
        }

        private string LoadLangList(string xmlFile)
        {
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(xmlFile);

                XmlNode xroot = xdoc.DocumentElement;
                _defaultLang = xroot.Attributes.GetNamedItem("default").Value;

                _serverLangList = new Hashtable();
                XmlNodeList xserverlist = xroot.SelectNodes("server");
                foreach (XmlNode xserver in xserverlist)
                {
                    string serverType = xserver.Attributes.GetNamedItem("type").Value;
                    Hashtable serverItemList = null;
                    if (_serverLangList.Contains(serverType))
                        serverItemList = _serverLangList[serverType] as Hashtable;
                    else
                    {
                        serverItemList = new Hashtable();
                        _serverLangList[serverType] = serverItemList;
                    }

                    XmlNodeList xitemlist = xserver.SelectNodes("item");
                    foreach (XmlNode xitem in xitemlist)
                    {
                        string itemId = xitem.Attributes.GetNamedItem("id").Value;
                        LanguageItem langItem = null;
                        if (serverItemList.Contains(itemId))
                            langItem = serverItemList[itemId] as LanguageItem;
                        else
                        {
                            langItem = new LanguageItem();
                            serverItemList.Add(itemId, langItem);
                        }

                        XmlNodeList xmessagelist = xitem.SelectNodes("message");
                        foreach (XmlNode xmessage in xmessagelist)
                        {
                            string lang = xmessage.Attributes.GetNamedItem("lang").Value;
                            string words = xmessage.Attributes.GetNamedItem("words").Value;
                            langItem.AddMessage(lang, words);
                        }
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取指定语言的内容
        /// </summary>
        /// <param name="serverType">服务类型</param>
        /// <param name="itemId">单元ID</param>
        /// <param name="targetLang">目标语言</param>
        /// <returns>返回目标语言指定的内容</returns>
        public string GetLangWords(string serverType, string itemId, string targetLang)
        {
            if (_serverLangList != null && itemId != "")
            {
                if (serverType != "")
                {
                    if (_serverLangList.Contains(serverType))
                    {
                        Hashtable serverItemList = _serverLangList[serverType] as Hashtable;
                        if (serverItemList.Contains(itemId))
                        {
                            LanguageItem langItem = serverItemList[itemId] as LanguageItem;
                            return langItem.GetMessage(targetLang, _defaultLang);
                        }
                    }

                    if (_serverLangList.Contains(_generalServerType))
                    {
                        Hashtable serverItemList = _serverLangList[_generalServerType] as Hashtable;
                        if (serverItemList.Contains(itemId))
                        {
                            LanguageItem langItem = serverItemList[itemId] as LanguageItem;
                            return langItem.GetMessage(targetLang, _defaultLang);
                        }
                    }
                }
                else
                {
                    foreach (Hashtable serverItemList in _serverLangList.Values)
                    {
                        if (serverItemList.Contains(itemId))
                        {
                            LanguageItem langItem = serverItemList[itemId] as LanguageItem;
                            return langItem.GetMessage(targetLang, _defaultLang);
                        }
                    }
                }
            }

            return "Error:" + itemId;
        }

        /// <summary>
        /// 获取缺省语言的内容
        /// </summary>
        /// <param name="serverType">服务类型</param>
        /// <param name="itemId">单元ID</param>
        /// <returns>返回缺省语言指定的内容</returns>
        public string GetLangWords(string serverType, string itemId)
        {
            return GetLangWords(serverType, itemId, _defaultLang);
        }

        /// <summary>
        /// 获取缺省语言的内容
        /// </summary>
        /// <param name="itemId">单元ID</param>
        /// <returns>返回缺省语言指定的内容</returns>
        public string GetLangWords(string itemId)
        {
            return GetLangWords("", itemId, _defaultLang);
        }

        public string GetOtherLangWordsReferToDefaultLangWords(string serverType, string defaultLangWords, string targetLang)
        {
            if (_serverLangList != null)
            {
                if (_serverLangList.Contains(serverType))
                {
                    Hashtable serverItemList = _serverLangList[serverType] as Hashtable;
                    foreach (LanguageItem langItem in serverItemList.Values)
                    {
                        if (langItem.GetMessage(_defaultLang, _defaultLang) == defaultLangWords)
                            return langItem.GetMessage(targetLang, _defaultLang);
                    }
                }

                if (_serverLangList.Contains(_generalServerType))
                {
                    Hashtable serverItemList = _serverLangList[_generalServerType] as Hashtable;
                    foreach (LanguageItem langItem in serverItemList.Values)
                    {
                        if (langItem.GetMessage(_defaultLang, _defaultLang) == defaultLangWords)
                            return langItem.GetMessage(targetLang, _defaultLang);
                    }
                }
            }

            return defaultLangWords;
        }

        class LanguageItem
        {
            private Hashtable _messageList = new Hashtable();//以lang为Key存储words

            public Hashtable MessageList
            {
                get
                {
                    return _messageList;
                }
            }

            public void AddMessage(string lang, string words)
            {
                _messageList[lang] = words;
            }

            public string GetMessage(string lang, string defaultLang)
            {
                string words = "";
                if (_messageList.Contains(lang))
                {
                    words = _messageList[lang].ToString();
                    if (words != "")
                        return words;
                }

                if (_messageList.Contains(defaultLang))
                {
                    words = _messageList[defaultLang].ToString();
                    return words;
                }

                return "";
            }

            public string GetMessageLang(string langWords)
            {
                foreach (string lang in _messageList.Keys)
                {
                    string words = _messageList[lang].ToString();
                    if (words == langWords)
                        return lang;
                }

                return "";
            }
        }

        public void SetDefaultLang(string newDefaultLang)
        {
            _defaultLang = newDefaultLang;
        }

        //添加到列表，返回itemId
        public string SetLangWords(string serverType, string itemId, string targetLang, string langWords)
        {
            if (serverType == "")
            {
                if (itemId == "")
                {
                    bool bFound = false;
                    foreach (string serverTypeKey in _serverLangList.Keys)
                    {
                        Hashtable itemList1 = _serverLangList[serverTypeKey] as Hashtable;
                        foreach (string itemId1 in itemList1.Keys)
                        {
                            LanguageItem langItem1 = itemList1[itemId1] as LanguageItem;
                            if (targetLang != "")
                            {
                                if (langItem1.GetMessage(targetLang, "") == langWords)
                                {
                                    serverType = serverTypeKey;
                                    itemId = itemId1;
                                    bFound = true;
                                    break;
                                }
                            }
                            else
                            {
                                targetLang = langItem1.GetMessageLang(langWords);
                                if (targetLang != "")
                                {
                                    serverType = serverTypeKey;
                                    itemId = itemId1;
                                    bFound = true;
                                    break;
                                }
                            }
                        }
                        if (bFound)
                            break;
                    }
                }
                else
                {
                    foreach (string serverTypeKey in _serverLangList.Keys)
                    {
                        Hashtable itemList1 = _serverLangList[serverTypeKey] as Hashtable;
                        if (itemList1.Contains(itemId))
                        {
                            LanguageItem langItem1 = itemList1[itemId] as LanguageItem;
                            serverType = serverTypeKey;
                            if (targetLang == "")
                                targetLang = langItem1.GetMessageLang(langWords);
                            break;
                        }
                    }
                }

                if (serverType == "")
                    serverType = _generalServerType;
            }

            if (targetLang == "")
                targetLang = _defaultLang;

            Hashtable itemList;
            if (_serverLangList.Contains(serverType))
                itemList = _serverLangList[serverType] as Hashtable;
            else
            {
                itemList = new Hashtable();
                _serverLangList.Add(serverType, itemList);
            }

            LanguageItem langItem;
            if (itemId != "")
            {
                if (itemList.Contains(itemId))
                    langItem = itemList[itemId] as LanguageItem;
                else
                {
                    langItem = new LanguageItem();
                    itemList.Add(itemId, langItem);
                }
            }
            else
            {
                int maxItemId = 0;
                foreach (Hashtable itemList2 in _serverLangList.Values)
                {
                    foreach (string itemId2 in itemList2.Keys)
                    {
                        int itemId3 = 0;
                        try
                        {
                            itemId3 = Convert.ToInt32(itemId2);
                        }
                        catch
                        {
                        }
                        if (itemId3 > maxItemId)
                            maxItemId = itemId3;
                    }
                }
                itemId = (maxItemId + 1).ToString();

                langItem = new LanguageItem();
                itemList.Add(itemId, langItem);
            }

            langItem.AddMessage(targetLang, langWords);
            return itemId;
        }

        public string SaveLangList(string xmlFile)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml("<language></language>");

            XmlNode xroot = xdoc.DocumentElement;
            XmlAttribute xAttr = xdoc.CreateAttribute("default");
            xAttr.Value = _defaultLang;
            xroot.Attributes.Append(xAttr);

            foreach (string serverType in _serverLangList.Keys)
            {
                XmlNode xserver = xdoc.CreateNode(XmlNodeType.Element, "server", "");
                xroot.AppendChild(xserver);
                xAttr = xdoc.CreateAttribute("type");
                xAttr.Value = serverType;
                xserver.Attributes.Append(xAttr);

                Hashtable itemList = _serverLangList[serverType] as Hashtable;
                foreach (string itemId in itemList.Keys)
                {
                    XmlNode xitem = xdoc.CreateNode(XmlNodeType.Element, "item", "");
                    xserver.AppendChild(xitem);
                    xAttr = xdoc.CreateAttribute("id");
                    xAttr.Value = itemId;
                    xitem.Attributes.Append(xAttr);

                    LanguageItem langItem = itemList[itemId] as LanguageItem;
                    foreach (string lang in langItem.MessageList.Keys)
                    {
                        string words = langItem.MessageList[lang].ToString();
                        XmlNode xmessage = xdoc.CreateNode(XmlNodeType.Element, "message", "");
                        xitem.AppendChild(xmessage);
                        xAttr = xdoc.CreateAttribute("lang");
                        xAttr.Value = lang;
                        xmessage.Attributes.Append(xAttr);
                        xAttr = xdoc.CreateAttribute("words");
                        xAttr.Value = words;
                        xmessage.Attributes.Append(xAttr);
                    }
                }
            }

            try
            {
                xdoc.Save(xmlFile);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

}
