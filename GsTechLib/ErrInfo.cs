using System;
using System.Collections.Generic;
using System.Text;

namespace GsTechLib
{
    /// <summary>
    /// 错误信息类
    /// </summary>
    public class ErrInfo
    {
        #region Constructor

        public ErrInfo()
        {
            ErrCode = 0;
            ErrMsg = "";
            Param = null;
        }
        public ErrInfo(string msg)
        {
            ErrMsg = msg;
            // 只要有错误则不为0
            ErrCode = 1;
        }

        public ErrInfo(int errCode, string errMsg, object param)
        {
            ErrCode = errCode;
            ErrMsg = errMsg;
            Param = param;
        }

        public ErrInfo(int errCode, string errMsg)
        {
            ErrCode = errCode;
            ErrMsg = errMsg;
            Param = null;
        }

        #endregion

        public int ErrCode = 0;
        public string ErrMsg = "";
        public object Param = null;

        /// <summary>
        /// 静态单例，描述无错误
        /// </summary>
        public static readonly ErrInfo EmptyErr = new ErrInfo();

    }
}
