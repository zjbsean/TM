using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerCommon
{
    public class AwardItem
    {
        public AwardItem(int id, int count, bool isDropID)
        {
            m_id = id;
            m_count = count;
        }

        public AwardItem(int id, int count)
        {
            m_id = id;
            m_count = count;
        }

        public int ID
        {
            get
            {
                return m_id;
            }
        }

        public int Count
        {
            get
            {
                return m_count;
            }
        }

        private int m_id;
        private int m_count;
    }

    public class IntervalReward
    {
        public IntervalReward(int start, int end, List<AwardItem> awardList)
        {
            m_start = start;
            m_end = end;
            m_awardList = awardList;
        }

        public int Start
        {
            get
            {
                return m_start;
            }
        }

        public int End
        {
            get
            {
                return m_end;
            }
        }

        public List<AwardItem> AwardList
        {
            get
            {
                return m_awardList;
            }
        }

        private int m_start;
        private int m_end;
        private List<AwardItem> m_awardList;
    }

    public class IntervalRewardManager
    {
        public IntervalRewardManager(List<IntervalReward> rewardList)
        {
            int maxIndex = 0;
            foreach (IntervalReward reward in rewardList)
            {
                if (maxIndex < reward.End)
                    maxIndex = reward.End;
            }
            ++maxIndex;

            m_rewardArr = new IntervalReward[maxIndex];

            foreach (IntervalReward reward in rewardList)
            {
                for (int i = reward.Start; i <= reward.End; ++i)
                    m_rewardArr[i] = reward;
            }
        }

        public List<AwardItem> GetAwardList(int index)
        {
            if (index < 0 || index >= m_rewardArr.Length || m_rewardArr[index] == null)
                return new List<AwardItem>();

            return m_rewardArr[index].AwardList;
        }

        private IntervalReward[] m_rewardArr;
    }
}
