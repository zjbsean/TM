using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisLib
{
    public delegate void PrintMsgFunc(string msgFormate, params object[] avgr);

    public enum eRedisAccessError
    {
        None = 0,
        Not_Implement_The_DoCommand_Method = 1,
        Command_Exec_Fail = 2,
        Connection_Is_Disconnect = 3,
        Access_Thread_Not_Run = 4,
        Command_Exec_Exception = 5,
        Not_Find_The_Script = 6,
    };
}
