using System.Collections.Generic;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;
using System.Data;

namespace Server
{
    class Account : DBItemBase<TAccount>
    {
        public Account(TAccount acc)
        {
            Update(m_Data);
        }

        protected override void updateSql()
        {
            m_operSql = string.Format(AccountManager.SQL_Replace, m_Data.account, m_Data.token, m_Data.showName, m_Data.power, m_Data.createTime);
        }

        protected override void deleteSql()
        {
            m_operSql = string.Format(AccountManager.SQL_Delete, m_Data.account);
        }
    }

    class AccountManager : IDataDBOper
    {
        public static AccountManager Instance
        {
            get { return m_instance; }
        }

        public override void LoadDataFromDB()
        {
            DBVisiter.Exec(SQL_Select, loadBack, null);
        }

        public override void OnLoadDataFromDBFinish(bool isSucc)
        {

        }

        /// <summary>
        /// 载入道具返回
        /// </summary>
        /// <param name="dbi"></param>
        private void loadBack(DbAccessItem dbi)
        {
            if (dbi.Result.ErrCode != 0)
                SvLogger.Error("DB Access Fail : Op=Load Account, ErrMsg={0}, Data={1}.", dbi.Result.ErrMsg, dbi.SpName);
            else
            {
                DataTable dt = dbi.OutDs.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    TAccount t = new TAccount();
                    t.account = HTBaseFunc.NullToStr(dr["LoginName"]);
                    t.token = HTBaseFunc.NullToStr(dr["Token"]);
                    t.showName = HTBaseFunc.NullToStr(dr["ShowName"]);
                    t.power = HTBaseFunc.NullToInt(dr["Power"]);
                    t.createTime = HTBaseFunc.NullToDateTime(dr["CreateTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                    Account acc = new Account(t);
                    m_dataDic[acc.m_Data.account] = acc;
                }

                DataLoadManager.Instance.AddLoadSuccTableCount();
            }

            OnLoadDataFromDBFinish(dbi.Result.ErrCode == 0);
        }

        public const string SQL_Select = "select * from info_account;";
        public const string SQL_Replace = "replace into info_account(LoginName,Token,ShowName,Power,CreateTime) values (\"{0}\",\"{1}\",\"{2}\",{3},\"{4}\");";
        public const string SQL_Delete = "delete from info_account where LoginName=\"{0}\";";

        private static Dictionary<string, Account> m_dataDic = new Dictionary<string, Account>();

        private static AccountManager m_instance = new AccountManager();
        private AccountManager() { }
    }
}
