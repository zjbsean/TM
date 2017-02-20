using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Redis;
using System.Diagnostics;
using GsTechLib;
using com.ideadynamo.foundation.buffer;
using ServiceStack.Text;

namespace RedisLib
{
    #region Redis Command Call Back Delegate

    /// <summary>
    /// redis命令无返回回调代理
    /// </summary>
    /// <param name="argv"></param>
    public delegate void RedisCommandCallBackNullFunc(eRedisAccessError backID, params object[] argv);

    /// <summary>
    /// redis命令无返回回调代理
    /// </summary>
    /// <param name="backID"></param>
    /// <param name="argv"></param>
    public delegate void RedisCommandCallBackLongFunc(eRedisAccessError backID, long theReturn, params object[] argv);

    /// <summary>
    /// redis命令返回Byte数组代理
    /// </summary>
    /// <param name="datas"></param>
    /// <param name="argv"></param>
    public delegate void RedisCommandCallBackByteArrFunc(eRedisAccessError backID, byte[] datas, params object[] argv);

    /// <summary>
    /// redis命令返回string代理
    /// </summary>
    /// <param name="data"></param>
    /// <param name="argv"></param>
    public delegate void RedisCommandCallBackStringFunc(eRedisAccessError backID, string datas, params object[] argv);

    /// <summary>
    /// redis命令返回List<string>代理
    /// </summary>
    /// <param name="datas"></param>
    /// <param name="argv"></param>
    public delegate void RedisCommandCallBackStringListFunc(eRedisAccessError backID, List<string> datas, params object[] argv);

    /// <summary>
    /// redis命令返回byte[][]代理
    /// </summary>
    /// <param name="backID"></param>
    /// <param name="datasList"></param>
    /// <param name="argv"></param>
    public delegate void RedisCommandCallBackObjBytesListFunc(eRedisAccessError backID, ref byte[][] datasList, params object[] argv);

    /// <summary>
    /// redis命令返回Dictionary<string, string>代理
    /// </summary>
    /// <param name="backID"></param>
    /// <param name="datas"></param>
    /// <param name="argv"></param>
    public delegate void RedisCommandCallBackStringDicFunc(eRedisAccessError backID, Dictionary<string, string> datas, params object[] argv);

    /// <summary>
    /// redis命令返回int代理
    /// </summary>
    /// <param name="backID"></param>
    /// <param name="theReturn"></param>
    /// <param name="argv"></param>
    public delegate void ReidsCommandCallBackIntFunc(eRedisAccessError backID, int theReturn, params object[] argv);

    /// <summary>
    /// redis命令返回bool代理
    /// </summary>
    /// <param name="backid"></param>
    /// <param name="theReturn"></param>
    /// <param name="argv"></param>
    public delegate void RedisCommandCallBackBoolFunc(eRedisAccessError backid, bool theReturn, params object[] argv);

    /// <summary>
    /// redis命令返回而且处理完代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="errID"></param>
    /// <param name="obj"></param>
    /// <param name="argv"></param>
    public delegate void RedisGetDataCallBackAndConverToGameOjbectFunc<T>(int errID, T obj, params object[] argv);

    #endregion

    public class RedisCommandBase
    {
        public RedisCommandBase(bool needDealCallBack)
        {
            NeedDealCallBack = needDealCallBack;
        }

        public bool NeedDealCallBack{ get; set; }

        public virtual eRedisAccessError DoCommand(RedisClient redisCli)
        {
            return eRedisAccessError.Not_Implement_The_DoCommand_Method;
        }

        public virtual void DoCommandCallBack()
        {
#if RedisCommandSpendTime
                m_sw.Stop();
                m_spendMillisecArr[2] = m_sw.ElapsedMilliseconds;
#endif
        }

        public object[] m_commandCallBackArgv = null;

        public eRedisAccessError m_Err = eRedisAccessError.None;

#if RedisCommandSpendTime
        protected long[] m_spendMillisecArr = new long[3];      //0-执行等待；1-执行；2-回调等待
        protected Stopwatch m_sw = new Stopwatch();
#endif
    }

    public class RedisSetUTF8String : RedisCommandBase
    {
        public RedisSetUTF8String(string key, string value, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_value = value;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.SetEntry(m_key, m_value);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_key;
        private string m_value;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisSetObjectBytes : RedisCommandBase
    {
        public RedisSetObjectBytes(string key, byte[] value, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_value = value;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.Set(m_key, m_value);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_key;
        private byte[] m_value;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisGetUTF8String : RedisCommandBase
    {
        public RedisGetUTF8String(string key, RedisCommandCallBackStringFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                m_value = redisCli.GetValue(m_key);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_value, m_commandCallBackArgv);
        }

        private string m_key;
        private string m_value;

        public RedisCommandCallBackStringFunc m_commandCallBack = null;
    }

    public class RedisGetObjectBytes : RedisCommandBase
    {
        public RedisGetObjectBytes(string key, RedisCommandCallBackByteArrFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                m_value = redisCli.Get(m_key);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_value, m_commandCallBackArgv);
        }

        private string m_key;
        private byte[] m_value;

        public RedisCommandCallBackByteArrFunc m_commandCallBack = null;
    }

    public class RedisRemoveItem : RedisCommandBase
    {
        public RedisRemoveItem(string key, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.Remove(m_key);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_key;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisLPush : RedisCommandBase
    {
        public RedisLPush(string key, ref byte[] value, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_value = value;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public RedisLPush(string key, ByteArray datas, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_value = RedisCommonFunc.ObjByteArrayToRedisObjBytes(datas);
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                m_listElementCount = redisCli.LPush(m_key, m_value);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_key;
        private byte[] m_value;
        private long m_listElementCount = 0;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisLuaExecAsNull : RedisCommandBase
    {
        public RedisLuaExecAsNull(LuaScriptData script, ref string[] luaArgv, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_script = script;
            m_luaArgv = luaArgv;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                if (m_script.m_SHA == null || redisCli.HasLuaScript(m_script.m_SHA) == false)
                    m_script.m_SHA = redisCli.LoadLuaScript(m_script.m_Body);
                redisCli.ExecLuaShaAsString(m_script.m_SHA, m_luaArgv);
            }
            catch (Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "ScriptName={2}\nScriptBody={3} -> {0}\n{1}.", ex.Message, ex.StackTrace, m_script.m_Name, m_script.m_Body);
            }

#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private LuaScriptData m_script;
        private string[] m_luaArgv;
        private RedisCommandCallBackNullFunc m_commandCallBack;
    }

    public class RedisLuaExecAsString : RedisCommandBase
    {
        public RedisLuaExecAsString(LuaScriptData script, ref string[] luaArgv, RedisCommandCallBackStringFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_script = script;
            m_luaArgv = luaArgv;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                if (m_script.m_SHA == null || redisCli.HasLuaScript(m_script.m_SHA) == false)
                    m_script.m_SHA = redisCli.LoadLuaScript(m_script.m_Body);
                m_theReturn = redisCli.ExecLuaShaAsString(m_script.m_SHA, m_luaArgv);
            }
            catch (Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "ScriptName={2}\nScriptBody={3} -> {0}\n{1}.", ex.Message, ex.StackTrace, m_script.m_Name, m_script.m_Body);
            }

#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_theReturn, m_commandCallBackArgv);
        }

        private LuaScriptData m_script;
        private string[] m_luaArgv;
        private string m_theReturn;

        private RedisCommandCallBackStringFunc m_commandCallBack = null;
    }

    public class RedisLuaExecAsStringList : RedisCommandBase
    {
        public RedisLuaExecAsStringList(LuaScriptData script, ref string[] luaArgv, RedisCommandCallBackStringListFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_script = script;
            m_luaArgv = luaArgv;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                if (m_script.m_SHA == null || redisCli.HasLuaScript(m_script.m_SHA) == false)
                    m_script.m_SHA = redisCli.LoadLuaScript(m_script.m_Body);
                m_theReturn = redisCli.ExecLuaShaAsList(m_script.m_SHA, m_luaArgv);
            }
            catch (Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "ScriptName={2}\nScriptBody={3} -> {0}\n{1}.", ex.Message, ex.StackTrace, m_script.m_Name, m_script.m_Body);
            }

#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_theReturn, m_commandCallBackArgv);
        }

        private LuaScriptData m_script;
        private string[] m_luaArgv;
        private List<string> m_theReturn;

        private RedisCommandCallBackStringListFunc m_commandCallBack = null;
    }

    public class RedisLuaExecAsObjBytesList : RedisCommandBase
    {
        public RedisLuaExecAsObjBytesList(LuaScriptData script, ref string[] luaArgv, RedisCommandCallBackObjBytesListFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_script = script;
            m_luaArgv = luaArgv;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                if (m_script.m_SHA == null || redisCli.HasLuaScript(m_script.m_SHA) == false)
                    m_script.m_SHA = redisCli.LoadLuaScript(m_script.m_Body);
                byte[][] luaArgvByteArr = new byte[m_luaArgv.Length][];
                for(int i = 0; i < m_luaArgv.Length; ++i)
                    luaArgvByteArr[i] = m_luaArgv[i].ToUtf8Bytes();
                m_theReturn = redisCli.EvalSha(m_script.m_SHA, 0, luaArgvByteArr);
            }
            catch (Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "ScriptName={2}\nScriptBody={3} -> {0}\n{1}.", ex.Message, ex.StackTrace, m_script.m_Name, m_script.m_Body);
            }

#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, ref m_theReturn, m_commandCallBackArgv);
        }

        private LuaScriptData m_script;
        private string[] m_luaArgv;
        private byte[][] m_theReturn;

        private RedisCommandCallBackObjBytesListFunc m_commandCallBack = null;
    }

    public class RedisHashSetKeyValue : RedisCommandBase
    {
        public RedisHashSetKeyValue(string hashID, string key, string value, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_hashID = hashID;
            m_key = key;
            m_isStrValue = true;
            m_valueStr = value;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

//        public RedisHashSetKeyValue(string hashID, string key, ByteArray value, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
//            : base(commandCallBack != null)
//        {
//            m_hashID = hashID;
//            m_key = key;
//            m_isStrValue = false;
//            m_valueBtyeArr = value;
//            m_commandCallBack = commandCallBack;
//            m_commandCallBackArgv = argv;
//#if RedisCommandSpendTime
//            m_sw.Start();
//#endif
//        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                if (m_isStrValue == true)
                    redisCli.SetEntryInHash(m_hashID, m_key, m_valueStr);
                //else
                //{
                //    byte[] datas = RedisCommonFunc.ObjByteArrayToRedisObjBytes(m_valueBtyeArr);
                //    reGameDataAllocAccountCountdisCli.HSet(m_hashID, m_key.ToUtf8Bytes(), datas);
                //}
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_hashID;
        private string m_key;
        private string m_valueStr;
        private ByteArray m_valueBtyeArr;
        private bool m_isStrValue;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisHashMSetKeyValue : RedisCommandBase
    {
        public RedisHashMSetKeyValue(string hashID, Dictionary<string, string> dataDic, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_hashID = hashID;
            m_dataStrDic = dataDic;
            m_isStrValue = true;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

//        public RedisHashMSetKeyValue(string hashID, Dictionary<string, ByteArray> dataDic, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
//            : base(commandCallBack != null)
//        {
//            m_hashID = hashID;
//            m_dataByteArrDic = dataDic;
//            m_isStrValue = false;
//            m_commandCallBack = commandCallBack;
//            m_commandCallBackArgv = argv;
//#if RedisCommandSpendTime
//            m_sw.Start();
//#endif
//        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                if(m_isStrValue == true)
                    redisCli.SetRangeInHash(m_hashID, m_dataStrDic);
                //else
                //{
                //    byte[][] m_keysArr = new byte[m_dataByteArrDic.Count][];
                //    byte[][] m_valuesArr = new byte[m_dataByteArrDic.Count][];
                //    int index = 0;
                //    foreach(KeyValuePair<string, ByteArray> dataByteArrEle in m_dataByteArrDic)
                //    {
                //        m_keysArr[index] = dataByteArrEle.Key.ToUtf8Bytes();
                //        m_valuesArr[index] = RedisCommonFunc.ObjByteArrayToRedisObjBytes(dataByteArrEle.Value);
                //        ++index;
                //    }
                //    redisCli.HMSet(m_hashID, m_keysArr, m_valuesArr);
                //}
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_hashID;
        private Dictionary<string, string> m_dataStrDic;
        private Dictionary<string, ByteArray> m_dataByteArrDic;
        private bool m_isStrValue;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisHashSetSome : RedisCommandBase
    {
        public RedisHashSetSome(string hashID, Dictionary<string, string> keyValues, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_hashID = hashID;
            m_keyValuesStrDic = keyValues;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.SetRangeInHash(m_hashID, m_keyValuesStrDic);
            }
            catch (Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_hashID;
        private Dictionary<string, string> m_keyValuesStrDic;
        private bool m_isStrValue;
        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisHashGetKeyValue : RedisCommandBase
    {
        public RedisHashGetKeyValue(string hashID, string key, RedisCommandCallBackStringFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_hashID = hashID;
            m_key = key;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                m_value = redisCli.GetValueFromHash(m_hashID, m_key);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_value, m_commandCallBackArgv);
        }

        private string m_hashID;
        private string m_key;
        private string m_value;

        public RedisCommandCallBackStringFunc m_commandCallBack = null;
    }

    public class RedisHashGetAll : RedisCommandBase
    {
        public RedisHashGetAll(string hashID, RedisCommandCallBackStringDicFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_hashID = hashID;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                m_keyValueDic = redisCli.GetAllEntriesFromHash(m_hashID);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_keyValueDic, m_commandCallBackArgv);
        }

        private string m_hashID;
        private Dictionary<string, string> m_keyValueDic;

        public RedisCommandCallBackStringDicFunc m_commandCallBack = null;
    }

    public class RedisHashGetSome : RedisCommandBase
    {
        public RedisHashGetSome(string hashID, List<string> keys, RedisCommandCallBackStringDicFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_hashID = hashID;
            m_keys = keys;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                m_values = redisCli.GetValuesFromHash(m_hashID, m_keys.ToArray());
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
            {
                m_keyValueDic.Clear();
                for (int i = 0; i < m_keys.Count; ++i )
                    m_keyValueDic[m_keys[i]] = m_values[i];
                m_commandCallBack(m_Err, m_keyValueDic, m_commandCallBackArgv);
            }
        }

        private string m_hashID;
        private List<string> m_keys;
        private List<string> m_values;
        private Dictionary<string, string> m_keyValueDic = new Dictionary<string,string>();

        public RedisCommandCallBackStringDicFunc m_commandCallBack = null;
    }

    public class RedisHashDelKey : RedisCommandBase
    {
        public RedisHashDelKey(string hashID, string key, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_hashID = hashID;
            m_key = key;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.RemoveEntryFromHash(m_hashID, m_key);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_hashID;
        private string m_key;
        
        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisIncrNumber : RedisCommandBase
    {
        public RedisIncrNumber(string key, RedisCommandCallBackLongFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                m_return = redisCli.Incr(m_key);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_return, m_commandCallBackArgv);
        }

        private string m_key;
        private long m_return;

        public RedisCommandCallBackLongFunc m_commandCallBack = null;
    }

    public class RedisAddSetItem : RedisCommandBase
    {
        public RedisAddSetItem(string key, string item, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_item = item;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.AddItemToSet(m_key, m_item);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_key;
        private string m_item;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisIsSetMember : RedisCommandBase
    {
        public RedisIsSetMember(string key, string item, RedisCommandCallBackBoolFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_item = item;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                m_return = redisCli.SetContainsItem(m_key, m_item);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_return, m_commandCallBackArgv);
        }

        private string m_key;
        private string m_item;
        private bool m_return = false;

        public RedisCommandCallBackBoolFunc m_commandCallBack = null;
    }

    public class RedisGetAllSetItem : RedisCommandBase
    {
        public RedisGetAllSetItem(string key, RedisCommandCallBackStringListFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                HashSet<string> items = redisCli.GetAllItemsFromSet(m_key);
                if(items != null)
                {
                    m_returns = new List<string>();
                    m_returns.AddRange(items);
                }
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_returns, m_commandCallBackArgv);
        }

        private string m_key;
        private List<string> m_returns;

        public RedisCommandCallBackStringListFunc m_commandCallBack = null;
    }

    public class ReidsRemoveSetItem : RedisCommandBase
    {
        public ReidsRemoveSetItem(string key, string item, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_item = item;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.RemoveItemFromSet(m_key, m_item);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_key;
        private string m_item;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisAddSortedSetItem : RedisCommandBase
    {
        public RedisAddSortedSetItem(string key, long score, string item, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_item = item;
            m_score = score;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.AddItemToSortedSet(m_key, m_item, m_score);
            }
            catch(Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop(); 
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_key;
        private string m_item;
        private long m_score;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisRemoveSortedSetItem : RedisCommandBase
    {
        public RedisRemoveSortedSetItem(string key, string item, RedisCommandCallBackNullFunc commandCallBack, params object[] argv)
            : base(commandCallBack != null)
        {
            m_key = key;
            m_item = item;
            m_commandCallBack = commandCallBack;
            m_commandCallBackArgv = argv;
#if RedisCommandSpendTime
            m_sw.Start();
#endif
        }

        public override eRedisAccessError DoCommand(RedisClient redisCli)
        {
#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[0] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif
            try
            {
                redisCli.RemoveItemFromSortedSet(m_key, m_item);
            }
            catch (Exception ex)
            {
                m_Err = eRedisAccessError.Command_Exec_Fail;
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
            }

#if RedisCommandSpendTime
            m_sw.Stop();
            m_spendMillisecArr[1] = m_sw.ElapsedMilliseconds;
            m_sw.Restart();
#endif

            return m_Err;
        }

        public override void DoCommandCallBack()
        {
            base.DoCommandCallBack();

            if (m_commandCallBack != null)
                m_commandCallBack(m_Err, m_commandCallBackArgv);
        }

        private string m_key;
        private string m_item;

        public RedisCommandCallBackNullFunc m_commandCallBack = null;
    }

    public class RedisGetSortedSetItemScore
    {

    }
}
