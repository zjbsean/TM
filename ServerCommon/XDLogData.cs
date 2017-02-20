using System;
using System.Collections;
using System.Collections.Generic;
using com.tieao.mmo.interval;
using com.tieao.mmo.logserver;
using com.tieao.mmo.logserver.client;
using com.tieao.mmo.logserver.server;
using com.ideadynamo.foundation.buffer;
using com.tieao.mmo.CustomTypeInProtocol;
using System.Text;
using GsTechLib;

namespace ServerCommon
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum EXDLogType
    {
        None = 0,                   //无

        /*******签到*******/
        DailyBook = 1,                  //签到--
        DailyBookCountAward = 2,       //每日签到累计天数奖励
        PayForTrapBook = 3,             //支付每日签到重签--

        /*********商店*********/
        PayForStageMallBargain = 5,            //支付关卡商店议价--
        BuyMallItem = 6,                       //购买关卡商城道具--

        /*********关卡*********/
        StageChallengeCost = 8,      //挑战关卡元宝消耗
        LueZhenChests = 9,           //略阵宝箱
        StageFight = 10,             //关卡战斗 --
        StageQuickFight = 11,            //关卡扫荡--
        StageStarAward = 12,            //关卡星数奖励
        FirstPassStageAward = 13,      //首次通关奖励
        EliteStageFight = 14,           //精英关卡战斗
        EliteStageQuickFight = 15,      //精英关卡快速战斗

        /***********竞技场***********/
        ArenaFight = 16,               //竞技场战斗--
        BuyArenaFightTimes = 17,        //购买竞技场次数--
        ClearArenaFightFailCD = 18,     //清除竞技场失败CD--
        BuyArenaFlashOppTimes = 19,     //购买竞技场刷新对手次数--
        ArenaFristRankingAward = 20,   //竞技场首次最高名次奖励
        RoundWinAward2V2 = 21,          //2V2回合胜利奖励
        RankAwardFrom2V2 = 22,              //2V2排名奖励

        /*********药庄*********/
        ActiveYaoCaiGroup = 23,         //激活药材配方
        ExchageYaoCai = 24,             //兑换药材
        YaoZhuangFight = 25,            //药庄战斗--
        FightAgainInYaoZhuang = 26,     //药庄重新战斗--
        FlashYaoZhuangBattleField = 27, //药庄战场刷新--
        ReliveInYaoZhuangBattle = 28,   //药庄战场复活--
        YaoCaiBack = 29,                //药材回收

        /******联盟*****/
        CreateGuild = 30,                       //创建联盟--
        CreateGuildFail = 31,                   //创建联盟失败
        PayForGuildDonation = 32,               //帮会元宝捐献--
        GuildJuBaoPenAward = 33,                //帮会聚宝盆奖励
        GuildRadomMallBuy = 34,                 //帮会商城购买--
        PayForChangeGuildName = 35,             //帮会改名支出--
        PayForBuyGuildBossBuff = 36,            //购买联盟BOSS Buff--
        PayForBuyGuildBossFightTimes = 37,      //购买联盟BOSS挑战次数--
        PayForBuyGuildXiuLianPlunderTimes = 38, //购买帮会修炼掠夺次数--
        GetGuildBossFightAward = 39,            //领取帮会BOSS挑战奖励--
        PayGuildBossRevice = 40,                //联盟Boss复活
        GuildDonation = 41,                     //联盟普通捐献
        GuildTask = 42,                         //联盟任务
        GuildRankReward = 43,                   //联盟排名奖励
        GuildTaskBuy = 44,                      //购买联盟任务

        /********装备*******/
        PayForOpenWeaponGemHole = 45,           //开启武器宝石孔--
        PayForFlashWeaponGemHole = 46,          //刷新武器宝石孔--
        ReflashEquipmentAttachProperty = 47,    //刷新装备属性[废弃]
        EquipmentLevelUp = 48,                      //装备升级
        PayForCancelEquipmentAttachProperty = 49,   //取消装备附加属性
        PayForChongZhu = 50,                    //重铸支付
        GemUnload = 51,                         //宝石卸下
        EquipmentMix = 52,                      //装备合成
        WeaponResolve = 53,                     //武器分解
        CancelWuHunFlashResult = 54,            //取消武魂洗练结果--
        EquipmentGemSet = 55,                   //装备镶嵌--
        WeaponStrengthen = 56,                  //武器强化--
        ArmorJinJie = 57,                       //防具进阶--
        UseForWeaponJinJie = 58,                //武器进阶--
        FlashNewHunStone = 59,                  //武魂普通培养

        /**********音韵*********/
        FlashYinYunStar = 62,                   //音韵刷星--
        OpenYuYunPos = 63,                      //开启余韵--
        YinYunResolve = 64,                     //音韵分解--
        PayForBuyListenYinYun = 66,             //支付购买余音绕梁次数--
        StopFlashYinYun = 67,                   //停止刷新聆听音韵--
        YinYunTiaoLv = 68,                      //音韵调律
        YinYunHeYin = 69,                       //音韵和音

        /********世界Boss*******/
        PayForWorldBossBuff = 70,                       //世界BOSS Buff购买--
        PayForWorldBossRevive = 71,                     //世界BOSS复活--
        BuyWorldBossMissAward = 72,                     //世界BOSS回购--
        WorldBossAward = 73,                            //世界Boss奖励--
        PayForWorldBossChallengeTimes = 74,                   //世界BOSS购买次数
        PayForWorldBossBuffFailBack = 75,                       //支付世界BOSSBUFF 失败退钱
        PayForWorldBossChallengeTimesFailBack = 76,             //支付世界BOSS挑战次数失败退钱
        PayForWorldBossMissAwardFailBack = 77,                  //支付世界BOSS回购失败退钱

        /********摇钱树*******/
        PayForShakeMoneyTree = 79,              //摇钱树摇一摇--
        MontyTreeGrawAward = 80,                //摇钱树幸运奖--
        MontyTreeBigAward = 81,                 //摇钱树中大奖

        /********任务(即成就)*******/
        FinishTask = 82,                          //支付完成任务--
        TaskConsume = 83,                        //任务消耗
        TaskAward = 84,                         //任务奖励

        /********心法*******/
        AddXinFaExp = 85,                       //添加心法经验--
        PayForBuyXinFaExp = 86,                 //心法经验购买

        /********勇闯地宫(老的武器冢)*******/
        WeaponStageFight = 90,                  //勇闯地宫战斗--
        WeaponStageQuickFight = 91,             //支付勇闯地宫快速战斗--
        PayForReliveInWeaponStageFight = 92,    //支付勇闯地宫战斗复活--
        PayForEnterWeaponStageFloor = 93,       //支付进入勇闯地宫某层
        WeaponStageBossFight = 94,              //勇闯地宫Boss战斗

        /********江湖试炼(老的地下城)*******/
        PayForBuyJiangHuShiLianTimes = 96,      //支付江湖试炼战斗次数--
        JiangHuShiLianFight = 97,               //江湖试炼战斗--

        /********藏剑阁*******/
        CangJianGeFight = 100,                      //藏剑阁挑战--
        PayForOpenCangJianGeNewFloor = 101,         //支付开启藏剑阁新的层(废弃)--
        PayForCangJianGeAddRate = 102,              //支付藏剑阁添加成功率--
        CangJianGeWeaponLevelUp = 103,              //藏剑阁武器升级

        /********武诀*******/
        AddWuJueStar = 105,          //武诀升星--
        UseForWuJueLevelUp = 106,    //武诀升级--


        /**********杂项**********/
        GMCommond = 110,               //GM命令
        RankAward = 111,                //排行奖励
        ShenYingYouLi = 112,            //神鹰游历--
        MailAttachments = 113,          //领取邮件附件--
        DailyActivityAward = 114,       //领取每日活动活跃度奖励--  
        Recharge = 115,                 //充值
        InitPlayer = 116,               //玩家初始化
        Resurrection = 117,                //复活--
        PayForBuyTiLi = 118,               //支付购买体力--
        PayForBuyLuoPan = 119,             //支付购买罗盘
        PayForFlashNewHunStone = 120,      //元宝武魂培养
        PayForFlashJiGuan = 121,           //支付机关刷新--
        SystemGiveAway = 122,               //系统赠送
        FromTemporary = 123,                //从临时背包返回
        GemMix = 124,                       //宝石合成--
        AwardHall = 125,                    //奖励大厅
        QuickFightItemByTimeLapse = 126,    //扫荡道具随时间增长
        BuyVIPMall = 127,                      //VIP商城购买
        MeritAward = 128,                   //成就奖励
        LevelUp = 129,                      //升级
        ShanZhuangYongShan = 130,           //山庄用膳(领取体力)

        WelfareReceive = 133,               //福利领取
        VIPAward = 134,                     //VIP奖励
        FlashJiGuan = 135,                  //机关普通培养
        TakeFriendsGive = 136,              //领取好友赠送
        LikeVidio = 137,                    //视频点赞
        StageThreeStarPassRankingAward = 138,   //关卡三星活动奖励
        DailyLoginActivityAward = 139,          //每日登录活动奖励
        Player2V2OnceFightAward = 140,          //玩家2V2单次战斗奖励
        BuyVipCardReturnActivityAward = 141,    //购买VIP卡返还活动奖励
        FirstRechargeActivityAward = 142,       //首充活动奖励
        GiftCodeReceive = 143,                  //兑换码领取

        //玄兵匣
        XuanBingXiaStrengthen = 201,        //玄兵匣强化
        XuanBingXiaChange = 202,            //玄兵匣改造

        //PVP
        PVPWinOnceFightAward = 210,         //PVP战胜奖励
        PVPLoseOnceFightAward = 211,        //PVP战败奖励
        PVPDanAward = 212,                  //PVP段位奖励
        PVPBuyJiangHuLingCost = 213,        //PVP购买江湖令花费
        PVEWinOnceFightAward = 214,         //PVE战胜奖励
        PVELoseOnceFightAward = 215,        //PVE战败奖励
		NewArenaZhanJiClear = 216,          //擂台战绩清0
        NewArenaWinAward = 217,             //擂台战胜奖励
        NewArenaLoseAward = 218,            //擂台战败奖励

        //天机阁
        TianJiGeWeaponSkillActivate = 220,  //天机阁武器技能激活

        //神鹰游历
        ShenYingYouLiSpeed = 230,           //神鹰游历加速
        ShenYingYouLiBuyDrumstick = 231,    //神鹰游历购买鸡腿
        ShenYingYouLiTakeReward = 232,      //神鹰游历领取奖励

    }
    
}
