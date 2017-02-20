using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon
{
    public enum eWeaponBoxJingLianResc
    {
        YuRenGang = 0,
        TieJingKuai = 1,
        GangMu = 2,
        RenSi = 3,
        QingTongPian = 4,
        HanTie = 5,

        MaxCount = 6,
    }

    public enum ECharacterType
    {
        HeiTaiSui = 1,  //黑太岁
        NanFeiYan = 2,  //南飞雁
        XiaobaWang = 3, //小霸王
    }


    //邮件类型
    public enum eMailType
    {
        None = 0,           //无
        SysPublic = 1,      //系统公共邮件
        SysPrivate = 2,     //系统私人邮件
        Private = 3,        //私人邮件
    }

}
