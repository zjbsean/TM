using System;

namespace ServerCommon
{
    public class IncrementIDMaker
    {
        public static int MakeID(byte type, int curID)
        {
            int id = curID % m_24bitMod;
            if (id + 1 == m_24bitMod)
                id = 1;
            else
                ++id;

            return (type << 24) + id;
        }

        public static int MakeID(int curID)
        {
            if (curID == int.MaxValue)
                curID = 1;
            else
                ++curID;
            return curID;
        }

        private static int m_24bitMod = Convert.ToInt32(Math.Pow(2, 24));
    }
}
