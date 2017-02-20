using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;

namespace ServerCommon
{
    public delegate bool HandlerRoutine(CtrlTypes CtrlType);
    public enum CtrlTypes
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT,
        CTRL_CLOSE_EVENT,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT
    }

    public sealed class PInvoker
    {
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        [DllImport("Kernel32")]
        public static extern bool SetConsoleTitle(string title);

        [DllImport("Kernel32")]
        public static extern int GetConsoleTitle(StringBuilder title, int size);

        [DllImport("User32")]
        public static extern int GetSystemMenu(int hwnd, bool bRevert);

        [DllImport("User32")]
        public static extern bool RemoveMenu(int hMenu, int uPosition, int uFlags);

        public static string GetConsoleTitle()
        {
            StringBuilder title = new StringBuilder(1024);
            GetConsoleTitle(title, 1024);
            return title.ToString();
        }
    }
}
