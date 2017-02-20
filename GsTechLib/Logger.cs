using System;
using System.Collections;

namespace GsTechLib
{
    /// <summary>
    /// 日志类
    /// </summary>
    public class Logger : IGsLogger
    {
        private log4net.ILog _logger = null;
        public Logger(string title)
        {
            _logger = log4net.LogManager.GetLogger(title);
        }

        /// <summary>
        /// 打印Info日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        public void Info(string format, params object[] param)
        {
            try
            {
                if (param == null || param.Length == 0)
                    _logger.Info(format);
                else
                    _logger.InfoFormat(format, param);
            }
            catch (Exception ex)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    Console.WriteLine("Logger Info Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace);
                try
                {
                    HTEventLog.InitFileLog(@"ServerEvents.log");
                    HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Logger Info Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace), System.Windows.Forms.Application.ProductName);
                }
                catch
                {
                }
            }
        }


        /// <summary>
        /// 打印Debug日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        public void Debug(string format, params object[] param)
        {
            try
            {
                if (param == null || param.Length == 0)
                    _logger.Debug(format);
                else
                    _logger.DebugFormat(format, param);
            }
            catch (Exception ex)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    Console.WriteLine("Logger Debug Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace);
                try
                {
                    HTEventLog.InitFileLog(@"ServerEvents.log");
                    HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Logger Debug Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace), System.Windows.Forms.Application.ProductName);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 打印Warn日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        public void Warn(string format, params object[] param)
        {
            try
            {
                if (param == null || param.Length == 0)
                    _logger.Warn(format);
                else
                    _logger.WarnFormat(format, param);
            }
            catch (Exception ex)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    Console.WriteLine("Logger Warn Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace);
                try
                {
                    HTEventLog.InitFileLog(@"ServerEvents.log");
                    HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Logger Warn Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace), System.Windows.Forms.Application.ProductName);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 打印Error日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        public void Error(string format, params object[] param)
        {
            try
            {
                if (param == null || param.Length == 0)
                    _logger.Error(format);
                else
                    _logger.ErrorFormat(format, param);
            }
            catch (Exception ex)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    Console.WriteLine("Logger Info Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace);
                try
                {
                    HTEventLog.InitFileLog(@"ServerEvents.log");
                    HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Logger Info Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace), System.Windows.Forms.Application.ProductName);
                }
                catch
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        Console.WriteLine("Logger Error Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace);
                    try
                    {
                        HTEventLog.InitFileLog(@"ServerEvents.log");
                        HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Logger Error Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace), System.Windows.Forms.Application.ProductName);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 打印Fatal日志
        /// </summary>
        /// <param name="err">异常对象</param>
        /// <param name="description">错误描述</param>
        /// <param name="param">错误参数</param>
        public void Fatal(Exception err, string description, params object[] param)
        {
            try
            {
                _logger.Fatal(string.Format(description, param), err);
            }
            catch (Exception ex)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    Console.WriteLine("Logger Fatal Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace);
                try
                {
                    HTEventLog.InitFileLog(@"ServerEvents.log");
                    HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Logger Fatal Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace), System.Windows.Forms.Application.ProductName);
                }
                catch
                {
                }
            }
        }
    }

    /// <summary>
    /// 日志管理类
    /// </summary>
	public class LogManager
	{
		Hashtable _loggers = new Hashtable();

        private static LogManager _instance = new LogManager();
        public static LogManager Instance
		{
			get
			{
				return _instance;
			}
		}

		#region Initializers

        private log4net.Core.Level ConvertLog4NetLevel(string levelStr)
        {
            log4net.Core.Level level = log4net.Core.Level.All;
            levelStr = levelStr.Trim().ToLower();
            if (levelStr == "alert")
                level = log4net.Core.Level.Alert;
            else if (levelStr == "critical")
                level = log4net.Core.Level.Critical;
            else if (levelStr == "debug")
                level = log4net.Core.Level.Debug;
            else if (levelStr == "emergency")
                level = log4net.Core.Level.Emergency;
            else if (levelStr == "error")
                level = log4net.Core.Level.Error;
            else if (levelStr == "fatal")
                level = log4net.Core.Level.Fatal;
            else if (levelStr == "fine")
                level = log4net.Core.Level.Fine;
            else if (levelStr == "finer")
                level = log4net.Core.Level.Finer;
            else if (levelStr == "finest")
                level = log4net.Core.Level.Finest;
            else if (levelStr == "info")
                level = log4net.Core.Level.Info;
            else if (levelStr == "notice")
                level = log4net.Core.Level.Notice;
            else if (levelStr == "off")
                level = log4net.Core.Level.Off;
            else if (levelStr == "severe")
                level = log4net.Core.Level.Severe;
            else if (levelStr == "trace")
                level = log4net.Core.Level.Trace;
            else if (levelStr == "verbose")
                level = log4net.Core.Level.Verbose;
            else if (levelStr == "warn")
                level = log4net.Core.Level.Warn;
            else
                level = log4net.Core.Level.All;
            return level;
        }
        
        /// <summary>
        /// 初始化日志
        /// </summary>
        /// <param name="file">日志文件</param>
        /// <param name="datePattern">日期格式</param>
        /// <param name="fileLevelStr">文件日志等级，有debug、info、warn、error和fatal</param>
        /// <param name="consoleLevelStr">Console日志等级</param>
        /// <returns>返回是否成功</returns>
        public bool Initialize(string file, string datePattern, string fileLevelStr, string consoleLevelStr)
        {
            log4net.Core.Level fileLevel = ConvertLog4NetLevel(fileLevelStr);
            log4net.Core.Level consoleLevel = ConvertLog4NetLevel(consoleLevelStr);

            const string DefaultPattern = "%d [%t]%-5p %c - %m%n";
            
			try
			{
                log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout(DefaultPattern);
                if (file != string.Empty)
                {
                    log4net.Appender.RollingFileAppender rfileAppender = new log4net.Appender.RollingFileAppender();
                    rfileAppender.Layout = layout;
                    rfileAppender.File = file;

                    if (datePattern==string.Empty)
                    {
                        rfileAppender.DatePattern = "yyyyMMdd-HH";
                    }
                    else
                    {
                        rfileAppender.DatePattern = datePattern;
                    }
                    rfileAppender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Date; //.Composite;
                    rfileAppender.Threshold = fileLevel;
                    rfileAppender.ActivateOptions();
                    log4net.Config.BasicConfigurator.Configure(rfileAppender);
                }

                log4net.Appender.ConsoleAppender consoleAppender = new log4net.Appender.ConsoleAppender(layout);
                consoleAppender.Threshold = consoleLevel;
                consoleAppender.ActivateOptions();
                log4net.Config.BasicConfigurator.Configure(consoleAppender);
				return true;
			}
			catch(Exception err)
			{
                Console.WriteLine("Error:{0} StackTrace:{1}", err.Message, err.StackTrace);
                try
                {
                    HTEventLog.InitFileLog(@"ServerEvents.log");
                    HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Error : {0}\r\nStackTrace : \r\n{1}", err.Message, err.StackTrace), System.Windows.Forms.Application.ProductName);
                }
                catch
                {
                }
                return false;
			}
		}
		#endregion

        /// <summary>
        /// 获取指定标题的日志类
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
		public Logger GetLogger(string title)
		{
			string uTitle = title.ToUpper();
			if(_loggers.ContainsKey(uTitle))
			{
				return _loggers[uTitle] as Logger;
			}

            Logger nlogger = new Logger(uTitle);
            _loggers.Add(uTitle, nlogger);
            return nlogger;
		}
	}

    /// <summary>
    /// 日志访问接口
    /// </summary>
    public interface IGsLogger
    {
        /// <summary>
        /// 打印Info日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        void Info(string format, params object[] param);
        /// <summary>
        /// 打印Debug日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        void Debug(string format, params object[] param);
        /// <summary>
        /// 打印Warn日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        void Warn(string format, params object[] param);
        /// <summary>
        /// 打印Error日志
        /// </summary>
        /// <param name="format">日志格式串</param>
        /// <param name="param">日志参数</param>
        void Error(string format, params object[] param);
        /// <summary>
        /// 打印Fatal日志
        /// </summary>
        /// <param name="err">异常对象</param>
        /// <param name="description">错误描述</param>
        /// <param name="param">错误参数</param>
        void Fatal(Exception err, string description, params object[] param);
    }
}
