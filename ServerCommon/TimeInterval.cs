using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GsTechLib;

namespace ServerCommon
{
    public class TimeInterval
    {
        public TimeInterval(int interval)
        {
            m_interval = interval * 1000;
        }

        public bool IsReach()
        {
            m_svrCheckEndTicket = Environment.TickCount;
            m_svrCheckSpendTicket = (m_svrCheckEndTicket >= m_svrCheckStartTicket ? m_svrCheckEndTicket - m_svrCheckStartTicket : (int.MaxValue + m_svrCheckEndTicket) + (int.MaxValue - m_svrCheckStartTicket));
            //SvLogger.Debug("##########m_svrCheckEndTicket={0}, m_svrCheckStartTicket={1}, m_svrCheckSpendTicket={2}, m_interval={3}", m_svrCheckEndTicket, m_svrCheckStartTicket, m_svrCheckSpendTicket, m_interval);
            if (m_svrCheckSpendTicket >= m_interval)
            {
                m_svrCheckStartTicket = m_svrCheckEndTicket;
                return true;
            }
            
            return false;
        }

        private int m_interval;
        private int m_svrCheckSpendTicket;
        private int m_svrCheckEndTicket;
        private int m_svrCheckStartTicket = Environment.TickCount;
    }
}
