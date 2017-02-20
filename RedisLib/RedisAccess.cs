using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Redis;

namespace RedisLib
{
    public class RedisAccess
    {
        public RedisAccess(int redisID, string ip, int port, string password)
        {
            m_redisID = redisID;
            m_ip = ip;
            m_port = port;
            m_passworld = password;
        }

        public RedisAccess(int redisID, string ip, int port)
        {
            m_redisID = redisID;
            m_ip = ip;
            m_port = port;
        }

        public RedisAccess(int redisID, string ip)
        {
            m_redisID = redisID;
            m_ip = ip;
        }

        public int RedisID
        {
            get
            {
                return m_redisID;
            }
        }

        public string IP
        {
            get
            {
                return m_ip;
            }
        }

        public int Port
        {
            get
            {
                return m_port;
            }
        }

        public string Password
        {
            get
            {
                return m_passworld;
            }
        }

        public RedisClient RedisCli
        {
            get
            {
                return m_redisCli;
            }
        }

        public bool Connect()
        {
            try
            {
                if (m_ip == null || m_ip.Trim() == "")
                {
                    if (PrintFunc != null)
                        PrintFunc("Connect Redis Fail: IP==NULL");
                    return false;
                }

                if (PrintFunc != null)
                    PrintFunc("Start Connect Redis : IP={0}, Port={1}, Password={2}", m_ip, m_port, m_passworld);

                if (m_port == 0)
                    m_redisCli = new RedisClient(m_ip);
                else
                {
                    if (m_passworld.Trim() == "")
                        m_redisCli = new RedisClient(m_ip, m_port);
                    else
                        m_redisCli = new RedisClient(m_ip, m_port, m_passworld);
                }

                if (PrintFunc != null)
                    PrintFunc("Connect Redis Succ!");

                bool pingResult = m_redisCli.Ping();
                if (pingResult == false || m_redisCli.IsSocketConnected() == false)
                {
                    if (PrintFunc != null)
                        PrintFunc("Ping Fail Or Redis Socket Connect Fail");
                    return false;
                }

            }
            catch (Exception ex)
            {
                if (PrintFunc != null)
                    PrintFunc("FATAL : Message={0}", ex.Message);
                m_redisCli = null;
                return false;
            }

            return true;
        }

        public bool Reconnect()
        {
            if (PrintFunc != null)
                PrintFunc("Reconnect Redis Server ... ");
            if (m_redisCli != null && m_redisCli.Ping() == true)
            {
                if (PrintFunc != null)
                    PrintFunc("Reconnect Redis Server Succ! IP={0}, Port={1}", m_ip, m_port);
                return true;
            }

            if (PrintFunc != null)
                PrintFunc("Reconnect Redis Server Fail! IP={0}, Port={1}", m_ip, m_port);
            m_redisCli = null;

            return Connect();
        }

        public void Close()
        {
            if (m_redisCli != null)
                m_redisCli.Dispose();
        }

        private int m_redisID = 0;
        private string m_ip = "";
        private int m_port = 0;
        private string m_passworld = "";

        private RedisClient m_redisCli;
        public static PrintMsgFunc PrintFunc;
    }
}
