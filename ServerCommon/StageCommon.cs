using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class StageCommon
    {

        /// <summary>
        /// 生成关卡ID
        /// </summary>
        /// <param name="bigID"></param>
        /// <param name="smallID"></param>
        /// <param name="diffiectID"></param>
        /// <returns></returns>
        public static int MakeStageID(int bigID, int smallID, int diffiectID)
        {
            return (bigID << 16) | (smallID << 8) | diffiectID;
        }

        /// <summary>
        /// 分解关卡ID
        /// </summary>
        /// <param name="stageID"></param>
        /// <param name="bigID"></param>
        /// <param name="smallID"></param>
        /// <param name="diffiectID"></param>
        public static void DecomposeStageID(int stageID, out int bigID, out int smallID, out int diffiectID)
        {
            int flag = 0xFF;
            diffiectID = stageID & flag;
            smallID = (stageID >> 8) & flag;
            bigID = (stageID >> 16) & flag;
        }

        public static int GetBigID(int stageID)
        {
            int flag = 0xFF;
            return (stageID >> 16) & flag;
        }
    }
}
