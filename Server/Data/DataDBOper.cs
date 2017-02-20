using GsTechLib;

namespace Server
{
    class DBItemBase<T>
    {
        public void Update(T data)
        {
            m_Data = data;
            updateSql();
        }

        public void Delete()
        {
            deleteSql();
        }

        public string OperSql()
        {
            return m_operSql;
        }

        protected virtual void updateSql()
        {

        }

        protected virtual void deleteSql()
        {
            
        }

        public T m_Data;
        public string m_operSql = null;

    }

    abstract class IDataDBOper
    {
        public abstract void LoadDataFromDB();

        public abstract void OnLoadDataFromDBFinish(bool isSucc);
    }
}
