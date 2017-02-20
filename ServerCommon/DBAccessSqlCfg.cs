using System;
using System.Collections.Generic;
using System.Text;


namespace ServerCommon
{
    //脚本名枚举
    public enum eSqlName
    {
        Account,
    }

    /// <summary>
    /// 脚本参数生成者
    /// </summary>
    public class SqlParamsMaker
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="sqlName">脚本类型</param>
        public SqlParamsMaker(eSqlName sqlName)
        {
            m_SQL_Params = DBAccessSqlCfg.Instance.GetSqlParamContent(sqlName);
            m_sql_Name = sqlName;
        }

        /// <summary>
        /// 获取脚本类型
        /// </summary>
        /// <returns>脚本类型</returns>
        public eSqlName GetSqlName()
        {
            return m_sql_Name;
        }

        /// <summary>
        /// 设置在等待池中
        /// </summary>
        public void SetInWaitingPool()
        {
            m_isInWaitingPool = true;
        }

        /// <summary>
        /// 设置不在等待池中
        /// </summary>
        public void SetOutWaitingPool()
        {
            m_isInWaitingPool = false;
        }

        /// <summary>
        /// 获取是否在等待池中
        /// </summary>
        /// <returns></returns>
        public bool GetIsInWaitingPool()
        {
            return m_isInWaitingPool;
        }

        /// <summary>
        /// 获取脚本参数字符串
        /// </summary>
        /// <returns>脚本参数字符串</returns>
        public virtual string GetSqlParams() { return ""; }

        //脚本字符串
        public string m_SQL_Params;
        //脚本类型
        private eSqlName m_sql_Name;
        //是否在等待池中
        private bool m_isInWaitingPool = false;
    }

    //数据库访问脚本配置
    public class DBAccessSqlCfg
    {
        #region Public Functions

        /// <summary>
        /// 获取SQL脚步主体内容
        /// </summary>
        /// <param name="sqlName">SQL脚本类型</param>
        /// <returns>SQL脚本主体内容</returns>
        public string GetSqlMainContent(eSqlName sqlName)
        {
            string strTemp;
            if (m_sqlMainContentDic.TryGetValue(sqlName, out strTemp))
                return strTemp;
            return "";
        }

        /// <summary>
        /// 获取SQL脚本参数内容
        /// </summary>
        /// <param name="sqlName">SQL脚本类型</param>
        /// <returns>SQL脚本参数内容</returns>
        public string GetSqlParamContent(eSqlName sqlName)
        {
            string strTemp;
            if (m_sqlParamContentDic.TryGetValue(sqlName, out strTemp))
                return strTemp;
            return "";
        }

        /// <summary>
        /// 获取SQL处理间隔时间（毫秒）
        /// </summary>
        /// <param name="sqlName">SQL脚本类型</param>
        /// <returns>处理间隔时间</returns>
        public int GetSqlDealIntervalTime(eSqlName sqlName)
        {
            if (m_sqlDealIntervalDic.ContainsKey(sqlName))
                return m_sqlDealIntervalDic[sqlName];
            return 0;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// 初始化字典数据
        /// </summary>
        private void init()
        {
            
        }

        #endregion

      
        //sql脚本参数部分字典
        private Dictionary<eSqlName, string> m_sqlParamContentDic = new Dictionary<eSqlName,string>();
        //sql脚本主体部分字典
        private Dictionary<eSqlName, string> m_sqlMainContentDic = new Dictionary<eSqlName, string>();
        //sql处理间隔字典
        private Dictionary<eSqlName, int> m_sqlDealIntervalDic = new Dictionary<eSqlName, int>();

        private StringBuilder m_strBuilder = new StringBuilder();
        public static DBAccessSqlCfg Instance
        {
            get
            {
                return _instance;
            }
        }
        private static DBAccessSqlCfg _instance = new DBAccessSqlCfg();
        private DBAccessSqlCfg() { }
    }
}
