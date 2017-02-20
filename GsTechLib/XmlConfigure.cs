using System;
using System.Xml;
using System.Collections;

namespace GsTechLib
{
    public class XmlConfigure
    {
        private static XmlConfigure _instance = new XmlConfigure();
        public static XmlConfigure Instance
        {
            get
            {
                return _instance;
            }
        }

        public Hashtable _AppConfigureTable = new Hashtable();

        public string XmlParseAppConfigure(string szXmlFile)
        {
            _AppConfigureTable.Clear();

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(szXmlFile);
                XmlNodeList roots = doc.SelectNodes("XmlAppConfig");
                if (roots.Count == 1)
                {
                    XmlNodeList nodes = roots[0].SelectNodes("config");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        _AppConfigureTable.Add(nodes[i].Attributes["key"].Value, nodes[i].Attributes["value"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "";
        }

        public string GetAppConfigString(string key)
        {
            try
            {
                string result = (string)_AppConfigureTable[key];

                if (result == null)
                {
                    return string.Empty;
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public bool GetAppConfigBool(string key)
        {
            try
            {
                string s = GetAppConfigString(key);
                if (s.Equals(string.Empty)) return false;
                return Convert.ToBoolean(s);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetAppConfigInt(string key)
        {
            try
            {
                string s = GetAppConfigString(key);
                if (s.Equals(string.Empty))
                    return 0;
                return Convert.ToInt32(s);
            }
            catch (Exception)
            {
                return 0;
            }
        }

    }
}
