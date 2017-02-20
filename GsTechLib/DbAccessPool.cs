using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.Diagnostics;

namespace GsTechLib
{
    //数据库连接枚举
    public enum eDbConnFlag
    {
        None = 0x0000,
        Game = 0x0100,      //游戏逻辑处理
        Secd = 0x0200,      //报告、邮件、公告等处理
        Log  = 0x0300,      //日志记录

        SqlLog = 0x0400,    //Sql日志记录，记录数据库访问出错脚本，方便维护

        Platform = 0x0500,  //平台数据库
        GM = 0x0600,        //GM数据库
        Moniteor = 0x0700,  //监控服务器
        Entry = 0x0800,     //入口服务器
        GiftCode = 0x0900,  //激活码服务器
    }

    /// <summary>
    /// 数据库访问池
    /// </summary>
    public class DbAccessPool
    {
        #region Private Variables

        private static Dictionary<int, DbAccessPool> _poolList = new Dictionary<int,DbAccessPool>();

        /// <summary>
        /// 添加一个数据库访问池
        /// </summary>
        /// <param name="dbp">数据库访问池对象</param>
        public static void AddPool(DbAccessPool dbp)
        {
            if (_poolList.ContainsKey(dbp.Flag))
                _poolList[dbp.Flag] = dbp;
            else
                _poolList.Add(dbp.Flag, dbp);
        }

        /// <summary>
        /// 移除一个数据库访问池
        /// </summary>
        /// <param name="flag">数据库访问池标识</param>
        public static void RemovePool(int flag)
        {
            if (_poolList.ContainsKey(flag))
            {
                _poolList[flag].DisconnectDatabase();
                _poolList.Remove(flag);
            }
        }

        /// <summary>
        /// 获取一个数据库访问池
        /// </summary>
        /// <param name="flag">数据库访问池标识</param>
        /// <returns>返回数据库访问池对象</returns>
        public static DbAccessPool GetPool(int flag)
        {
            if (_poolList.ContainsKey(flag))
                return _poolList[flag];
            else
                return null;
        }

        /// <summary>
        /// 获取数据库访问池列表
        /// </summary>
        /// <returns>返回数据库访问池列表</returns>
        public static Dictionary<int, DbAccessPool> GetPoolList()
        {
            return _poolList;
        }

        /// <summary>
        /// 获取当前数据库访问池
        /// </summary>
        /// <returns>返回数据库访问池对象</returns>
        public static DbAccessPool GetFirstPool()
        {
            if (_poolList.Count > 0)
            {
                foreach (var dap in _poolList.Values)
                {
                    return dap;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取Vistor列表
        /// </summary>
        /// <returns></returns>
        public static List<DbAccessPoolVistor> GetVistorList()
        {
            List<DbAccessPoolVistor> vistorlist = new List<DbAccessPoolVistor>();
            foreach (var dapool in _poolList.Values)
            {
                if (dapool.Vistor != null)
                    vistorlist.Add(dapool.Vistor);
            }
            return vistorlist;
        }

        //public delegate void PoolAction();

        private HTDBBaseFunc _htdbf = new HTDBBaseFunc();
        public HTDBBaseFunc DbComponent
        {
            get
            {
                return _htdbf;
            }
        }
        private int _poolFlag = 0x0000; //访问池标记
        private string _dbType = "", _dbServerName = "", _dbServerPort = "", _dbDatabaseName = "", _dbUserName = "", _dbUserPass = "";

        private ArrayList _dbaccessList = new ArrayList();//如果是DbAccessItem则执行一个；如果是ArrayList则用一个事务按顺序执行，一个出错全部回滚，执行运行到的最后一个DbAccessItem的回调
        private bool _threadRunning = false;
        private Thread _dbaccessThread = null;

        private IGsLogger _logger = null;
        private DbAccessPoolVistor _vistor = null;
        private static DbAccessPool _sqlLogPool = null;

        /// <summary>
        /// 数据库访问池名
        /// </summary>
        public int Flag
        {
            get
            {
                return _poolFlag;
            }
        }

        /// <summary>
        /// 数据库访问池是否有效
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return (_dbServerName != "" && _dbDatabaseName != "");
            }
        }

        /// <summary>
        /// 获取Vistor
        /// </summary>
        public DbAccessPoolVistor Vistor
        {
            get
            {
                return _vistor;
            }
        }

        /// <summary>
        /// 数据库访问池实例化
        /// </summary>
        /// <param name="poolFlag">数据库访问池标记</param>
        /// <param name="logger">日志接口</param>
        /// <param name="vistor">数据库访问池客户，用于进行处理结果和日志的操作</param>
        /// <param name="sqlLogPool">数据库日志池对象，这是个全局静态对象，用于向日志库记录对数据库的操作，如果不需要可以为null</param>
        public DbAccessPool(int poolFlag, IGsLogger logger, DbAccessPoolVistor vistor, DbAccessPool sqlLogPool)
        {
            _poolFlag = poolFlag;
            _logger = logger;
            _vistor = vistor;
            _sqlLogPool = sqlLogPool;
        }

        #endregion

        #region Private Functions

        public static HTDBBaseFunc.ConnectionTypeEnum GetConnectType(string dbType)
        {
            if (dbType.ToLower().Trim() == "mysql")
                return HTDBBaseFunc.ConnectionTypeEnum.MySql;
            else if (dbType.ToLower().Trim() == "sqloledb")
                return HTDBBaseFunc.ConnectionTypeEnum.SqlOleDB;
            else if (dbType.ToLower().Trim() == "oracle")
                return HTDBBaseFunc.ConnectionTypeEnum.Oracle;
            else
                return HTDBBaseFunc.ConnectionTypeEnum.Sql;
        }

        //连接数据库
        private void ConnectDatabase(string dbType, string dbServerName, string dbServerPort, string dbDatabaseName, string dbUserName, string dbUserPass)
        {
            _htdbf.ProcessErrorMode = HTDBBaseFunc.ProcessErrorModeEnum.None;
            if (!_htdbf.ConnectDatabase(GetConnectType(dbType), dbServerName, dbServerPort, dbDatabaseName, dbUserName, dbUserPass))
            {
                if (_logger != null)
                    _logger.Error("DbAccessPool " + _poolFlag + " Connect Fail : DbType = {5}, DbServerName={0}, DbServerPort={1}, DbDatabaseName={2}, DbUserName={3}, ErrMsg={4}.", dbServerName, dbServerPort, dbDatabaseName, dbUserName, _htdbf.GetLastError(), dbType);
                throw new Exception(_htdbf.GetLastError());
            }
            else
            {
                if (_logger != null)
                    _logger.Info("DbAccessPool " + _poolFlag + " Connect Succ : DbType = {4}, DbServerName={0}, DbServerPort={1}, DbDatabaseName={2}, DbUserName={3}", dbServerName, dbServerPort, dbDatabaseName, dbUserName, dbType);
            }
        }

        //断开与数据库的连接
        private void DisconnectDatabase()
        {
            if (_htdbf.DataBaseConnected)
            {
                _htdbf.DisConnectDatabase();
                if (_logger != null)
                    _logger.Info("Disconnect " + _poolFlag + " Database.");
            }
        }

        private bool CommitTransaction(string errMsg)
        {
            return _htdbf.CommitTransaction(errMsg);
        }

        private void RollbackTransaction()
        {
            _htdbf.RollbackTransaction();
        }
        
        #endregion

        #region SqlLog Functions

        private string GenSql(DbAccessItem dbi)
        {
            if (dbi.IsSqlRun)
                return dbi.SpName;
            else
            {
                string sql = "", outsql = "";
                try
                {
                    HTDBBaseFunc.ConnectionTypeEnum dbtype = GetConnectType(_dbType);
                    if (dbtype == HTDBBaseFunc.ConnectionTypeEnum.MySql || dbtype == HTDBBaseFunc.ConnectionTypeEnum.MySqlUseTran)
                    {
                        for (int i = 0; i < dbi.SpParams.Count; i++)
                        {
                            if (sql != "")
                                sql += ", ";

                            HTDBBaseFunc.StoredProcedureParam sparm = (HTDBBaseFunc.StoredProcedureParam)dbi.SpParams[i];
                            if (sparm.Type.Trim().ToLower() == "image" || sparm.Type.Trim().ToLower() == "blob")
                            {
                                if (outsql != "")
                                    outsql += "\n";
                                string pname = "@p" + i.ToString();
                                outsql += "set " + pname + "=" + HTBaseFunc.GetSqledValue(sparm.Value.GetType(), sparm.Value);
                                sql += pname;
                            }
                            else
                            {
                                if (sparm.Direction == ParameterDirection.Output || sparm.Direction == ParameterDirection.InputOutput)
                                {
                                    if (outsql != "")
                                        outsql += "\n";
                                    string pname = "@p" + i.ToString();
                                    outsql += "set " + pname + "=" + HTBaseFunc.GetSqledValue(sparm.Value.GetType(), sparm.Value);
                                    if (sparm.Direction == ParameterDirection.Output)
                                        sql += "out " + pname;
                                    else
                                        sql += "inout " + pname;
                                }
                                else
                                    sql += HTBaseFunc.GetSqledValue(sparm.Value.GetType(), sparm.Value);
                            }
                        }
                        sql = "call " + dbi.SpName + "(" + sql + ");";
                    }
                    else if (dbtype == HTDBBaseFunc.ConnectionTypeEnum.Sql || dbtype == HTDBBaseFunc.ConnectionTypeEnum.SqlOleDB || dbtype == HTDBBaseFunc.ConnectionTypeEnum.SybaseOleDB
                        || dbtype == HTDBBaseFunc.ConnectionTypeEnum.Oracle)
                    {
                        for (int i = 0; i < dbi.SpParams.Count; i++)
                        {
                            if (sql != "")
                                sql += ", ";

                            HTDBBaseFunc.StoredProcedureParam sparm = (HTDBBaseFunc.StoredProcedureParam)dbi.SpParams[i];
                            sql += sparm.Name + "=" + HTBaseFunc.GetSqledValue(sparm.Value.GetType(), sparm.Value);
                            if (sparm.Direction == ParameterDirection.Output || sparm.Direction == ParameterDirection.InputOutput)
                                sql += " output";
                        }
                        sql = "exec " + dbi.SpName + " " + sql + ";";
                    }
                    if (outsql != "")
                        sql = outsql + "\n" + sql;
                }
                catch(Exception ex)
                {
                    sql = dbi.SpName + string.Format("(Param generate error : {0})", ex.Message);
                }
                return sql;
            }
        }

        /// <summary>
        /// 处理SqlLog记录
        /// </summary>
        /// <param name="dbi">要记录</param>
        /// <param name="pos"></param>
        /// <param name="translist"></param>
        /// <returns></returns>
        private void DoSqlLog(DbAccessItem dbi, int pos, ArrayList translist)
        {
            try
            {
                if (_sqlLogPool != null)
                {
                    DateTime dtNow = HTBaseFunc.GetTime(0);
                    string occurgroup = ((pos == 0) ? "" : Environment.TickCount.ToString() + "_" + pos.ToString());
                    ArrayList newtranslist = new ArrayList();

                    ArrayList oParams = new ArrayList();
                    HTDBBaseFunc.StoredProcedureParam lSPparam;
                    lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_poolFlag", "int", _poolFlag, false);
                    oParams.Add(lSPparam);
                    lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_recTime", "datetime", dtNow, false);
                    oParams.Add(lSPparam);
                    lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_recSql", "text", GenSql(dbi), false);
                    oParams.Add(lSPparam);
                    lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_execResult", "int", ((dbi.Result.ErrCode == 0) ? 0 : 1), false);//执行结果：0-成功，1-错误，2-未执行
                    oParams.Add(lSPparam);
                    lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_errMsg", "text", ((dbi.Result.ErrCode == 0) ? "" : dbi.Result.ErrMsg), false);
                    oParams.Add(lSPparam);
                    lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_occurGroup", "string", occurgroup, false);
                    oParams.Add(lSPparam);
                    lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_occurPos", "int", pos, false);
                    oParams.Add(lSPparam);
                    newtranslist.Add(new DbAccessItem("xsp_AddSqlLog", oParams, null, null, true));

                    if (translist != null && translist.Count > 0)
                    {
                        for (int i = pos + 1; i < translist.Count; i++)
                        {
                            DbAccessItem ldbi = (DbAccessItem)translist[i];
                            oParams = new ArrayList();
                            lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_poolFlag", "int", _poolFlag, false);
                            oParams.Add(lSPparam);
                            lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_recTime", "datetime", dtNow, false);
                            oParams.Add(lSPparam);
                            lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_recSql", "blob", GenSql(ldbi), false);
                            oParams.Add(lSPparam);
                            lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_execResult", "int", 2, false);
                            oParams.Add(lSPparam);
                            lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_errMsg", "text", "", false);
                            oParams.Add(lSPparam);
                            lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_occurGroup", "string", occurgroup, false);
                            oParams.Add(lSPparam);
                            lSPparam = DbAccessPool._sqlLogPool.DbComponent.GenStoredProcedureParam("p_occurPos", "int", i, false);
                            oParams.Add(lSPparam);
                            newtranslist.Add(new DbAccessItem("xsp_AddSqlLog", oParams, null, null, true));
                        }
                    }

                    _sqlLogPool.Push(newtranslist);
                }
            }
            catch(Exception ex)
            {
                _logger.Fatal(ex, "DoSqlLog error : {0}.", ex.Message);
            }
        }

        #endregion

        #region Public Functions

        private object _runitem = null;
        private DateTime _runtime = DateTime.Now;
        private long _runednum = 0;
        private ArrayList _lastrunitemlist = new ArrayList();

        private long _lastDbaccessCount = 0;
        private long _lastRunednum = 0;
        private DateTime _lastRunedChangeTime = DateTime.Now;
        private TimeSpan _maxSleepTS = new TimeSpan(10000 * 50);
        private int _doWorkTimes = 100;

        private Stopwatch sw = new Stopwatch();

        /// <summary>
        /// 启动数据库访问池，包括连接数据库
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="dbServerName">数据库服务器名</param>
        /// <param name="dbServerPort">数据库服务器端口</param>
        /// <param name="dbDatabaseName">数据库名</param>
        /// <param name="dbUserName">数据库用户名</param>
        /// <param name="dbUserPass">数据库用户密码</param>
        public void Start(string dbType, string dbServerName, string dbServerPort, string dbDatabaseName, string dbUserName, string dbUserPass)
        {
            _dbType = dbType;
            _dbServerName = dbServerName;
            _dbServerPort = dbServerPort;
            _dbDatabaseName = dbDatabaseName;
            _dbUserName = dbUserName;
            _dbUserPass = dbUserPass;
            ConnectDatabase(dbType, dbServerName, dbServerPort, dbDatabaseName, dbUserName, dbUserPass);

            _dbaccessThread = new Thread(new ThreadStart(Run));
            _dbaccessThread.Start();
        }

        /// <summary>
        /// 压入一个数据库访问单元
        /// </summary>
        /// <param name="item">数据库访问单元</param>
        public void Push(object item)
        {
            int startTick = Environment.TickCount;

            lock (_dbaccessList)
            {
                if (item != null)
                {
                    _dbaccessList.Add(item);
                }

                if (_runednum == _lastRunednum)
                {
                    if (_dbaccessList.Count > _lastDbaccessCount && _lastRunedChangeTime.AddMinutes(10) < DateTime.Now)
                    {
                        if (IsAvailable && _htdbf.DataBaseConnected)
                        {
                            _htdbf.DisConnectDatabase();
                            _lastRunedChangeTime = DateTime.Now;
                        }
                    }
                }
                else
                {
                    _lastRunedChangeTime = DateTime.Now;
                    _lastRunednum = _runednum;
                }
                _lastDbaccessCount = _dbaccessList.Count;
            }

            int endTick = Environment.TickCount;
            int spendTick = (startTick == endTick) ? 0 : ((startTick < endTick) ? endTick - startTick : int.MaxValue - startTick + endTick);
            if (spendTick >= 1000)
            {
                if (_logger != null)
                    _logger.Warn("DbAccessPool {0} Push Delay : {1}.", _poolFlag, spendTick);
            }
        }

        /// <summary>
        /// 获取当前等待处理的数据库访问单元数
        /// </summary>
        /// <returns></returns>
        public int WaitedDbAccessNum()
        {
            lock (_dbaccessList)
            {
                return _dbaccessList.Count;
            }
        }

        /// <summary>
        /// 获取数据库访问池运行信息
        /// </summary>
        /// <returns>返回字符串格式的运行信息</returns>
        public string GetRunInfo()
        {
            lock (_dbaccessList)
            {
                string runifo = string.Format("RunedNum = {0}, NowTime = {1}, RunTime = {2}, RunItem = ", _runednum, DateTime.Now.ToString("HH:mm:ss"), _runtime.ToString("HH:mm:ss"));

                string runsql = "";
                if (_runitem != null)
                {
                    if (_runitem is DbAccessItem)
                    {
                        DbAccessItem dbAccessItem = (DbAccessItem)_runitem;
                        runsql = dbAccessItem.SpName;
                    }
                    else if (_runitem is ArrayList)
                    {
                        ArrayList translist = (ArrayList)_runitem;
                        for (int i = 0; i < translist.Count; i++)
                        {
                            DbAccessItem dbAccessItem = (DbAccessItem)translist[i];
                            if (runsql != "")
                                runsql += ";";
                            runsql += dbAccessItem.SpName;
                        }
                    }
                    //else if (_runitem is PoolAction)
                    //{
                    //    runsql = ((PoolAction)_runitem).Method.Name;
                    //}
                }

                runifo += runsql;
                return runifo;
            }
        }

        /// <summary>
        /// 获取最后几个数据库访问池的运行信息
        /// </summary>
        /// <param name="num">返回的数目</param>
        /// <returns>返回字符串格式的运行信息</returns>
        public string GetLastRunList(int num)
        {
            lock (_dbaccessList)
            {
                string lastrun = "";
                int pos = 0;
                for (int i = _lastrunitemlist.Count - 1; i >= 0; i--)
                {
                    object item = _lastrunitemlist[i];
                    if (item != null)
                    {
                        if (item is DbAccessItem)
                        {
                            DbAccessItem dbAccessItem = (DbAccessItem)item;
                            pos++;
                            lastrun += string.Format("【{0}】{1}", pos, dbAccessItem.SpName);
                        }
                        else if (item is ArrayList)
                        {
                            ArrayList translist = (ArrayList)item;
                            string runsql = "";
                            for (int j = 0; j < translist.Count; j++)
                            {
                                DbAccessItem dbAccessItem = (DbAccessItem)translist[j];
                                if (runsql != "")
                                    runsql += ";";
                                runsql += dbAccessItem.SpName;
                            }
                            pos++;
                            lastrun += string.Format("【{0}】{1}", pos, runsql);
                        }
                        //else if (item is PoolAction)
                        //{
                        //    pos++;
                        //    lastrun += string.Format("【{0}】{1}", pos, ((PoolAction)item).Method.Name);
                        //}
                    }
                    if (pos >= num)
                        break;
                }
                return lastrun;
            }
        }

        /// <summary>
        /// 停止数据库访问池的运行
        /// </summary>
        /// <param name="wait"></param>
        public void Stop(bool wait)
        {
            if (_dbaccessThread != null)
            {
                if (wait)
                {
                    int waitNum = WaitedDbAccessNum();
                    if (waitNum > 0)
                    {
                        int lastWaitNum = waitNum;
                        DateTime dtStopTime = DateTime.Now;
                        while (waitNum > 0)
                        {
                            if (_logger != null)
                                _logger.Warn("DbAccessPool " + _poolFlag + " StopWaiting : DbType = {0}, DbServerName={1}, DbDatabaseName={2}, WaitedDbAccessNum = {3}.", _dbType, _dbServerName, _dbDatabaseName, waitNum);
                            Thread.Sleep(3000);
                            waitNum = WaitedDbAccessNum();
                            if (waitNum == lastWaitNum && dtStopTime.AddMinutes(5) < DateTime.Now)
                            {
                                if (_logger != null)
                                    _logger.Warn("DbAccessPool " + _poolFlag + " StopTimeOut : DbType = {0}, DbServerName={1}, DbDatabaseName={2}, WaitedDbAccessNum = {3}.", _dbType, _dbServerName, _dbDatabaseName, waitNum);
                                break;
                            }
                            else
                            {
                                lastWaitNum = waitNum;
                                dtStopTime = DateTime.Now;
                            }
                        }
                    }
                }

                _threadRunning = false;
                try
                {
                    _dbaccessThread.Abort();
                }
                catch
                {
                }
                _dbaccessThread = null;

                DisconnectDatabase();
            }
        }

        //取出一个等待运行的数据库访问单元来执行
        private object Popup()
        {
            lock (_dbaccessList)
            {
                if (_dbaccessList.Count > 0)
                {
                    object item = _dbaccessList[0];
                    _dbaccessList.RemoveAt(0);

                    _runitem = item;
                    _runtime = DateTime.Now;
                    _lastrunitemlist.Add(item);
                    if (_lastrunitemlist.Count > 100)
                        _lastrunitemlist.RemoveAt(0);

                    return item;
                }
                else
                    return null;
            }
        }

        //一个数据库访问单元执行完
        private void OneRunOver(int addrunednum)
        {
            lock (_dbaccessList)
            {
                _runitem = null;
                _runtime = DateTime.Now;
                _runednum += addrunednum;
            }
        }

        //数据库访问池执行逻辑
        private void Run()
        {
            _threadRunning = true;
            ArrayList logList = new ArrayList();
            int lastruntick = Environment.TickCount;
            int needDealNum = 0;
            bool isGameThread = (_poolFlag & Convert.ToInt32(eDbConnFlag.Game)) != 0x0000 ? true : false;
            while (_threadRunning)
            {
                try
                {
                    sw.Reset();

                    logList.Clear();
                    int startTick = Environment.TickCount;
                    object item = Popup();
                    int stepTick1 = Environment.TickCount;
                    int spendTick1 = (startTick == stepTick1) ? 0 : ((startTick < stepTick1) ? stepTick1 - startTick : int.MaxValue - startTick + stepTick1);
                    if (spendTick1 >= 1000)
                        logList.Add(string.Format("DbAccessPool {0} Popup Delay : {1}.", _poolFlag, spendTick1));
                    
                    if (item != null)
                    {
                        if (item is DbAccessItem)
                        {
                            int stepTick2 = Environment.TickCount;

                            DbAccessItem dbAccessItem = (DbAccessItem)item;
                            int rc = 0;
                            if (dbAccessItem.IsSqlRun)
                                rc = _htdbf.OpenDataSet(ref dbAccessItem.OutDs, dbAccessItem.SpName, "") ? 0 : -9999;
                            else
                                rc = _htdbf.ExecStoredProcedure(dbAccessItem.SpName, ref dbAccessItem.SpParams, ref dbAccessItem.OutDs, "");
                            if (rc == 0)
                                dbAccessItem.Result.ErrMsg = "";
                            else
                                dbAccessItem.Result.ErrMsg = _htdbf.GetLastError();
                            dbAccessItem.Result.ErrCode = rc;

                            int stepTick3 = Environment.TickCount;
                            int spendTick3 = (stepTick2 == stepTick3) ? 0 : ((stepTick2 < stepTick3) ? stepTick3 - stepTick2 : int.MaxValue - stepTick2 + stepTick3);
                            if (spendTick3 >= 1000)
                                logList.Add(string.Format("DbAccessPool {0} Run1 Delay : {1}, {2}.", _poolFlag, spendTick3, dbAccessItem.SpName));

                            if (    rc != 0 && 
                                    (_poolFlag & Convert.ToInt32(eDbConnFlag.SqlLog)) == 0x0000 && 
                                    dbAccessItem.Result.ErrMsg != "" && 
                                    dbAccessItem.Result.ErrMsg != _htdbf.GetErrPerfix())
                                DoSqlLog(dbAccessItem, 0, null);

                            if (dbAccessItem.OnFinish != null)
                            {
                                int stepTick4 = Environment.TickCount;
                                if (_vistor != null)
                                    _vistor.PushCallback(dbAccessItem);
                                int stepTick5 = Environment.TickCount;
                                int spendTick5 = (stepTick4 == stepTick5) ? 0 : ((stepTick4 < stepTick5) ? stepTick5 - stepTick4 : int.MaxValue - stepTick4 + stepTick5);
                                if (spendTick5 >= 1000)
                                    logList.Add(string.Format("DbAccessPool {0} Run1PushBack Delay : {1}.", _poolFlag, spendTick5));
                            }
                        }
                        else if (item is ArrayList)
                        {
                            int stepTick2 = Environment.TickCount;
                            string logStr = "";

                            ArrayList translist = (ArrayList)item;
                            DbAccessItem dbAccessItem = null;
                            int errpos = -1;
                            for (int i = 0; i < translist.Count; i++)
                            {
                                int stepTick3 = Environment.TickCount;

                                dbAccessItem = (DbAccessItem)translist[i];
                                _htdbf.HoldDbTransNoCommit = true;

                                int rc = 0;
                                if (dbAccessItem.IsSqlRun)
                                    rc = _htdbf.OpenDataSet(ref dbAccessItem.OutDs, dbAccessItem.SpName, "") ? 0 : -9999;
                                else
                                    rc = _htdbf.ExecStoredProcedure(dbAccessItem.SpName, ref dbAccessItem.SpParams, ref dbAccessItem.OutDs, "");
                                logStr += string.Format("{1}({0})", i + 1, dbAccessItem.SpName);

                                int stepTick4 = Environment.TickCount;
                                int spendTick4 = (stepTick3 == stepTick4) ? 0 : ((stepTick3 < stepTick4) ? stepTick4 - stepTick3 : int.MaxValue - stepTick3 + stepTick4);
                                if (spendTick4 >= 1000)
                                    logList.Add(string.Format("DbAccessPool {0} Run2Sub Delay : {1}, {2}.", _poolFlag, spendTick4, dbAccessItem.SpName));
                                
                                if (rc != 0)
                                {
                                    errpos = i;
                                    dbAccessItem.Result.ErrCode = rc;
                                    dbAccessItem.Result.ErrMsg = _htdbf.GetLastError();

                                    if ((_poolFlag & Convert.ToInt32(eDbConnFlag.SqlLog)) == 0x0000 && dbAccessItem.Result.ErrMsg != "" && dbAccessItem.Result.ErrMsg != _htdbf.GetErrPerfix())
                                        DoSqlLog(dbAccessItem, errpos, translist);

                                    break;
                                }
                            }

                            int stepTick5 = Environment.TickCount;
                            int spendTick5 = (stepTick2 == stepTick5) ? 0 : ((stepTick2 < stepTick5) ? stepTick5 - stepTick2 : int.MaxValue - stepTick2 + stepTick5);
                            if (spendTick5 >= 1000)
                                logList.Add(string.Format("DbAccessPool {0} Run2 Delay : {1}, {2}.", _poolFlag, spendTick5, logStr));

                            if (dbAccessItem != null)
                            {
                                int stepTick6 = Environment.TickCount;
                                if (dbAccessItem.Result.ErrCode == 0)
                                    _htdbf.CommitTransaction("");
                                else
                                    _htdbf.RollbackTransaction();
                                int stepTick7 = Environment.TickCount;
                                int spendTick7 = (stepTick6 == stepTick7) ? 0 : ((stepTick6 < stepTick7) ? stepTick7 - stepTick6 : int.MaxValue - stepTick6 + stepTick7);
                                if (spendTick7 >= 1000)
                                    logList.Add(string.Format("DbAccessPool {0} Run2Tran Delay : {1}, {2}.", _poolFlag, spendTick7, dbAccessItem.Result.ErrCode));

                                if (dbAccessItem.OnFinish != null)
                                {
                                    int stepTick8 = Environment.TickCount;
                                    if (_vistor != null)
                                        _vistor.PushCallback(dbAccessItem);
                                    int stepTick9 = Environment.TickCount;
                                    int spendTick9 = (stepTick8 == stepTick9) ? 0 : ((stepTick8 < stepTick9) ? stepTick9 - stepTick8 : int.MaxValue - stepTick8 + stepTick9);
                                    if (spendTick9 >= 1000)
                                        logList.Add(string.Format("DbAccessPool {0} Run2PushBack Delay : {1}.", _poolFlag, spendTick9));
                                }
                            }
                        }

                        //每次有数据库操作就更新下上次执行时间
                        lastruntick = Environment.TickCount;
                    }

                    OneRunOver((item != null ? 1 : 0));

                    if (logList.Count > 0 && _vistor != null)
                        _vistor.PushLog(_poolFlag, logList);

                    sw.Stop();
                    needDealNum = WaitedDbAccessNum();
                    if (isGameThread == true && needDealNum > 0)
                    {
                        ++_doWorkTimes;   
                    }
                    if (needDealNum == 0 || _doWorkTimes > 100)
                    {
                        if (sw.Elapsed < _maxSleepTS)
                            Thread.Sleep(_maxSleepTS - sw.Elapsed);
                        else
                            Thread.Sleep(1);

                        _doWorkTimes = 0;
                    }
                }
                catch (ThreadAbortException)
                {
                    
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                        _logger.Fatal(ex, ex.Message);
                }
            }

        }

        /// <summary>
        /// 获取数据库缓存中数量
        /// </summary>
        /// <returns></returns>
        public int GetDBCountInPool()
        {
            lock (_dbaccessList)
            {
                return _dbaccessList.Count;
            }
        }

        #endregion

    }

    /// <summary>
    /// 数据库访问单元
    /// </summary>
    public class DbAccessItem
    {
        /// <summary>
        /// 数据库访问单元实例化
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="spParams">存储过程参数</param>
        /// <param name="onFinish">处理完成时的回调函数</param>
        /// <param name="finishParams">处理完成时的回调参数</param>
        /// <param name="onFinishOnlyDealError">完成时是否仅处理错误，这个仅做标识，具体由业务逻辑控制</param>
        public DbAccessItem(string spName, ArrayList spParams, OnFinishAsync onFinish, object[] finishParams, bool onFinishOnlyDealError)
        {
            SpName = spName;
            SpParams = spParams;
            IsSqlRun = false;
            OnFinish = onFinish;
            FinishParams = finishParams;
            OnFinishOnlyDealError = onFinishOnlyDealError;
        }

        /// <summary>
        /// 数据库访问单元实例化
        /// </summary>
        /// <param name="sql">数据库脚本</param>
        /// <param name="onFinish">处理完成时的回调函数</param>
        /// <param name="finishParams">处理完成时的回调参数</param>
        /// <param name="onFinishOnlyDealError">完成时是否仅处理错误，这个仅做标识，具体由业务逻辑控制</param>
        public DbAccessItem(string sql, OnFinishAsync onFinish, object[] finishParams, bool onFinishOnlyDealError)
        {
            SpName = sql;
            IsSqlRun = true;
            OnFinish = onFinish;
            FinishParams = finishParams;
            OnFinishOnlyDealError = onFinishOnlyDealError;
        }

        public int ID = 0;
        public string SpName = "";//存储过程名
        public ArrayList SpParams = new ArrayList();//存储过程参数
        public bool IsSqlRun = false;

        public delegate void OnFinishAsync(DbAccessItem item);
        public OnFinishAsync OnFinish = null;//完成时的回调
        public object[] FinishParams = null;
        public bool OnFinishOnlyDealError = false;//结束时仅处理错误

        public DataSet OutDs = new DataSet();//返回的DataSet
        public ErrInfo Result = new ErrInfo();//执行结果
    }

    /// <summary>
    /// 数据库访问池客户
    /// </summary>
    public class DbAccessPoolVistor
    {
        private ArrayList _dbAccessPoolCallbackList = new ArrayList();//以DbAccessItem元素
        private Hashtable _dblogList = new Hashtable();//以数据库连接存储日志List

        private int _flag = 0x0000;
        public int Flag
        {
            get
            {
                return _flag;
            }
        }

        public DbAccessPoolVistor(int name)
        {
            _flag = name;
        }

        #region 数据库访问池处理结果

        /// <summary>
        /// 数据库访问池将处理结果压回
        /// </summary>
        /// <param name="dai">数据库访问单元</param>
        public void PushCallback(DbAccessItem dai)
        {
            lock (_dbAccessPoolCallbackList)
            {
                _dbAccessPoolCallbackList.Add(dai);
            }
        }

        /// <summary>
        /// 业务逻辑从队列中取出一个处理结果
        /// </summary>
        /// <returns>返回数据库访问单元</returns>
        public DbAccessItem PopupCallback()
        {
            lock (_dbAccessPoolCallbackList)
            {
                if (_dbAccessPoolCallbackList.Count > 0)
                {
                    DbAccessItem dai = (DbAccessItem)_dbAccessPoolCallbackList[0];
                    _dbAccessPoolCallbackList.RemoveAt(0);
                    return dai;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// 业务逻辑获取处理结果队列数
        /// </summary>
        /// <returns>返回处理结果队列数</returns>
        public int GetCallbackNum()
        {
            lock (_dbAccessPoolCallbackList)
            {
                return _dbAccessPoolCallbackList.Count;
            }
        }

        #endregion

        #region 数据库访问日志

        /// <summary>
        /// 数据库访问池将日志压回
        /// </summary>
        /// <param name="poolName">数据库访问池名</param>
        /// <param name="al">压入的日志列表</param>
        public void PushLog(int poolFlag, ArrayList al)
        {
            lock (_dblogList)
            {
                ArrayList inloglist;
                if (_dblogList.Contains(poolFlag))
                    inloglist = (ArrayList)_dblogList[poolFlag];
                else
                {
                    inloglist = new ArrayList();
                    _dblogList.Add(poolFlag, inloglist);
                }

                if (al != null && al.Count > 0)
                {
                    for (int i = 0; i < al.Count; i++)
                    {
                        inloglist.Add(al[i]);
                    }
                }
                if (inloglist.Count > 100)
                {
                    inloglist.RemoveRange(0, inloglist.Count - 100);
                }
            }
        }

        /// <summary>
        /// 业务逻辑取出日志，并清空
        /// </summary>
        /// <returns>返回取出的日志列表</returns>
        public ArrayList PopupLog()
        {
            ArrayList al = new ArrayList();
            lock (_dblogList)
            {
                foreach (DictionaryEntry de in _dblogList)
                {
                    ArrayList inloglist = (ArrayList)de.Value;
                    for (int i = 0; i < inloglist.Count; i++)
                    {
                        al.Add(inloglist[i]);
                    }
                    inloglist.Clear();
                }
            }
            return al;
        }

        #endregion
    }

}
