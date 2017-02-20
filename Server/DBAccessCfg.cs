using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using GsTechLib;
using ServerCommon;

namespace Server
{
    //玩家数据库访问管理
    class DBAccessCfg
    {
        public static DBAccessCfg Instance
        {
            get
            {
                return _instance;
            }
        }

        #region Public Functions

        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <returns>是否成功</returns>
        public bool InitDBConnections()
        {
            //配置载入
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(@"Config\DatabaseCfg.xml");

                XmlNode root = doc.SelectSingleNode("XmlAppConfig");
                if (root == null)
                    return false;
                else
                {
                    XmlNodeList dbNodeList = root.SelectNodes("DBItem");
                    foreach (XmlNode dbNode in dbNodeList)
                    {
                        eDbConnFlag connFlag = eDbConnFlag.None;
                        string dbTypeName = dbNode.Attributes["name"].Value;
                        switch (dbTypeName)
                        {
                            case "GameDB":
                                connFlag = eDbConnFlag.Game;
                                break;
                            case "LogDB":
                                connFlag = eDbConnFlag.Log;
                                break;
                            case "SecdDB":
                                connFlag = eDbConnFlag.Secd;
                                break;
                            case "GMDB":
                                connFlag = eDbConnFlag.GM;
                                break;
                        }
                        if (connFlag != eDbConnFlag.None)
                        {
                            string dbType = "", serverIP = "", serverPort = "", databaseName = "", userName = "", UserPass = "", childConnCount = "";
                            XmlNodeList cfgNodeList = dbNode.SelectNodes("config");
                            foreach (XmlNode cfgNode in cfgNodeList)
                            {
                                switch (cfgNode.Attributes["key"].Value)
                                {
                                    case "DbType":
                                        dbType = cfgNode.Attributes["value"].Value;
                                        break;
                                    case "ServerIP":
                                        serverIP = cfgNode.Attributes["value"].Value;
                                        break;
                                    case "ServerPort":
                                        serverPort = cfgNode.Attributes["value"].Value;
                                        break;
                                    case "DatabaseName":
                                        databaseName = cfgNode.Attributes["value"].Value;
                                        break;
                                    case "UserName":
                                        userName = cfgNode.Attributes["value"].Value;
                                        break;
                                    case "UserPass":
                                        UserPass = cfgNode.Attributes["value"].Value;
                                        break;
                                    case "ChildConnCount":
                                        childConnCount = cfgNode.Attributes["value"].Value;
                                        break;
                                }
                            }
                            int errCode = DbAccess.Instance.InitilizeDbConn(connFlag, dbType, serverIP, serverPort, databaseName, userName, UserPass, Convert.ToInt32(childConnCount));
                            if (errCode != 0)
                                return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SvLogger.Fatal(ex, "Load Fail Fatal: ErrMsg={0}, Stack={1}.", ex.Message, ex.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 初始化DB连接
        /// </summary>
        /// <param name="dbConnFlag"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="name"></param>
        /// <param name="user"></param>
        /// <param name="passwrod"></param>
        /// <returns></returns>
        public bool InitDBConnection(eDbConnFlag dbConnFlag, string ip, int port, string name, string user, string passwrod)
        {
            //配置载入
            try
            {
                int errCode = DbAccess.Instance.InitilizeDbConn(dbConnFlag, "mysql", ip, port.ToString(), name, user, passwrod, 0);
                if (errCode != 0)
                    return false;
            }
            catch (Exception ex)
            {
                SvLogger.Fatal(ex, "Connect MySql DataBase Fail: ErrMsg={0}, Stack={1}.", ex.Message, ex.StackTrace);
                return false;
            }

            return true;
        }

        #endregion

        #region Private Functions

        private DBAccessCfg() { }

        #endregion

        #region Data Member

        private static DBAccessCfg _instance = new DBAccessCfg();

        #endregion

    }
}
