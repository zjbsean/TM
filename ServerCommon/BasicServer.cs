using System;
using System.Collections.Generic;
using com.ideadynamo.foundation;
using com.ideadynamo.foundation.game;
using com.ideadynamo.foundation.channels;
using com.ideadynamo.foundation.buffer;
using com.tieao.mmo.interval.server;
using GsTechLib;

namespace ServerCommon
{
    public class BasicServer
    {
        public virtual bool Init(string[] args)
        {
            System.Diagnostics.Process currProcess = System.Diagnostics.Process.GetCurrentProcess();

            SvrCommCfg.Instance.ServerInfo.m_ProcessID = currProcess.Id;

            SvrCommCfg.Instance.m_Envirment = string.Format("ID={0};", currProcess.Id)
                + string.Format("FileName={0};", currProcess.MainModule.FileName)
                + string.Format("Version={0};", currProcess.MainModule.FileVersionInfo.ProductVersion)
                + string.Format("StartTime={0};", currProcess.StartTime.ToString("yyyy-MM-dd HH:mm:ss"))
                + string.Format("MainWindowHandle={0};", currProcess.MainWindowHandle)
                + string.Format("MaxWorkingSet={0};", currProcess.MaxWorkingSet)
                + string.Format("WorkingSet64={0};", currProcess.WorkingSet64)
                + string.Format("PeakWorkingSet64={0};", currProcess.PeakWorkingSet64)
                + string.Format("PagedMemorySize={0};", currProcess.PagedMemorySize64)
                + string.Format("PeakPagedMemorySize={0};", currProcess.PeakPagedMemorySize64)
                ;

            //配置载入
            if (args.Length <= 1)
            {
                string xmlerr;
                if (args.Length == 0)
                {
                    string configName = @"Config/" + currProcess.ProcessName.Replace(".vshost", "") + ".exe.xml";
                    xmlerr = XmlConfigure.Instance.XmlParseAppConfigure(configName);
                }
                else
                    xmlerr = XmlConfigure.Instance.XmlParseAppConfigure(args[0]);

                if (xmlerr != "")
                {
                    throw new Exception(string.Format("Parse XmlConfigureFile Config Fail : {0}.", xmlerr));
                }
            }

            return true;
        }
    }
}
