using GsTechLib;

namespace Server
{
    class DBVisiter
    {
        public static void Exec(string sql, DbAccessItem.OnFinishAsync onFinish, object[] finishParams)
        {
            DbAccessPool pool = ServerCommon.DbAccess.Instance.GetDBMainPool(eDbConnFlag.Game);
            if (pool != null && pool.IsAvailable)
            {
                DbAccessItem dbi = new DbAccessItem(sql, onFinish, finishParams, true);
                pool.Push(dbi);
            }
            else
            {
                DbAccessItem dbi = new DbAccessItem(sql, onFinish, finishParams, true);
                dbi.Result = new ErrInfo(101, "DB Pool=Null Or Pool Is Not Available!"); //游戏数据库池不可访问
                onFinish(dbi);
            }
        }
    }
}
