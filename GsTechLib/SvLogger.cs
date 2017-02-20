using System;

namespace GsTechLib
{
    /// <summary>
    /// 服务器日志类
    /// </summary>
    public class SvLogger
    {
        private static Logger _logger;

        /// <summary>
        /// 初始化服务器日志
        /// </summary>
        /// <param name="logfile">服务器日志文件</param>
        /// <param name="logFileLevel">文件日志等级，有debug、info、warn、error和fatal</param>
        /// <param name="logConsoleLevel">Console日志等级</param>
        public static void Init(string logfile, string logFileLevel, string logConsoleLevel)
        {
            if (LogManager.Instance.Initialize(logfile, string.Empty, logFileLevel, logConsoleLevel))
                _logger = LogManager.Instance.GetLogger("");
        }

        /// <summary>
        /// 获取日志类
        /// </summary>
        /// <returns>返回日志类</returns>
        public static Logger GetLogger()
        {
            return _logger;
        }

        /// <summary>
        /// 打印Info日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        public static void Info(string format, params object[] param)
        {
            if (_logger != null)
                _logger.Info(format, param);
        }

        /// <summary>
        /// 打印Debug日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        public static void Debug(string format, params object[] param)
        {
            if (_logger != null)
                _logger.Debug(format, param);
        }

        /// <summary>
        /// 打印Warn日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        public static void Warn(string format, params object[] param)
        {
            if (_logger != null)
                _logger.Warn(format, param);
        }

        /// <summary>
        /// 打印Error日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        public static void Error(string format, params object[] param)
        {
            if (_logger != null)
                _logger.Error(format, param);
        }

        /// <summary>
        /// 打印Fatal日志
        /// </summary>
        /// <param name="err">异常对象</param>
        /// <param name="description">错误描述</param>
        /// <param name="param">错误参数</param>
        public static void Fatal(Exception err, string description, params object[] param)
        {
            if (_logger != null)
            {
                if (err.Message.Contains("OutOfMemoryException"))
                {
                    try
                    {
                        long usedMem = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
                        long availMem = HTBaseFunc.GetMemoryAvailable();
                        description += string.Format("(CurrUseMem = {0}MB, CurrAvailMem = {1}MB)", usedMem / 1024 / 1024, availMem / 1024 / 1024);
                    }
                    catch
                    {
                        description += "(GetMemFail)";
                    }
                }
                _logger.Fatal(err, description, param);
            }
        }
    }
}
