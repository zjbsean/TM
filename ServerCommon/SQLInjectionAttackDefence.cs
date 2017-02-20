using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerCommon
{
    public class SQLInjectionAttackDefence
    {
        public static SQLInjectionAttackDefence Instance
        {
            get
            {
                return m_instance;
            }
        }

        public bool IsSQLInjectionAttack(string context)
        {
            string contextLow = context.ToLower();

            foreach (var keyList in m_sqlKeyWordList)
            {
                if(contextLow.Contains(keyList[0]))
                {
                    if(keyList.Count > 1)
                    {
                        for (int i = 1; i < keyList.Count; ++i)
                        {
                            if (contextLow.Contains(keyList[i]))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    else
                        return true;
                }
                else
                    return false;
            }

            return false;
        }

        private List<List<string>> m_sqlKeyWordList = new List<List<string>>();

        private static SQLInjectionAttackDefence m_instance = new SQLInjectionAttackDefence();
        private SQLInjectionAttackDefence() 
        {
            List<string> sqlKeys = new List<string>();
            sqlKeys.Add("select");
            m_sqlKeyWordList.Add(sqlKeys);

            sqlKeys = new List<string>();
            sqlKeys.Add("update");
            m_sqlKeyWordList.Add(sqlKeys);

            sqlKeys = new List<string>();
            sqlKeys.Add("replace");
            m_sqlKeyWordList.Add(sqlKeys);

            sqlKeys = new List<string>();
            sqlKeys.Add("delete");
            m_sqlKeyWordList.Add(sqlKeys);

            sqlKeys = new List<string>();
            sqlKeys.Add("insert");
            m_sqlKeyWordList.Add(sqlKeys);

            sqlKeys = new List<string>();
            sqlKeys.Add("create");
            sqlKeys.Add("table");
            sqlKeys.Add("procedure");
            sqlKeys.Add("event");
            m_sqlKeyWordList.Add(sqlKeys);

            sqlKeys = new List<string>();
            sqlKeys.Add("drop");
            sqlKeys.Add("table");
            sqlKeys.Add("procedure");
            sqlKeys.Add("event");
            m_sqlKeyWordList.Add(sqlKeys);

            sqlKeys = new List<string>();
            sqlKeys.Add("drop");
            sqlKeys.Add("procedure");
            m_sqlKeyWordList.Add(sqlKeys);
        }
    }
}
