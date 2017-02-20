using System;
using System.Collections.Generic;
using System.Text;
using ServerCommon;
using GsTechLib;

namespace Server
{
    public class ServiceMain : BasicService
    {
        #region Basic Override Code.

        public override void InitInstance(string[] args)
        {
            Console.WriteLine("InitInstance ...");
            if (ServerMain.Instance.Init(args) == false)
                throw new Exception("Server Init Fail!");
            base.InitInstance(args);
        }

        public override void ExitInstance()
        {
            Console.WriteLine("ExitInstance ...");

            ServerCommon.Network.XDNetworkManager.Instance.StopRun();

            //资源释放
            DbAccess.Instance.DestroyAllDbConn();
        }

        public override int Update()
        {
            ServerMain.Instance.Update();
            return base.Update();
        }

        #endregion

        public static string AssemblyName()
        {
            string name = System.Reflection.Assembly.GetEntryAssembly().FullName;
            return name.Split(',')[0];
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            if (SvrCommCfg.Instance.CShutDownMode)
            {
                SvLogger.Warn("本服务器当前为C-ShutDown模式，但将被强制关闭。");
                StopRunnerService();
                //SvLogger.Warn("本服务器已关闭。");
            }

            return true;
        }

        [STAThread]
        static void Main(string[] args)
        {
            HandlerRoutine handlerRoutine = new HandlerRoutine(ConsoleCtrlCheck);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                PInvoker.SetConsoleCtrlHandler(handlerRoutine, true);

            ServiceMain instance = new ServiceMain();
            _Runner = new BasicServiceRunner(instance, args);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            _Runner.RunService(true);

            GC.KeepAlive(handlerRoutine);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SvLogger.Fatal((Exception)e.ExceptionObject, "UnhandledException");
        }
    }
}
