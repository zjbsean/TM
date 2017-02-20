using System;
using System.Collections.Generic;
using System.Text;
using com.tieao.mmo.CustomTypeInProtocol;

namespace ServerCommon
{
    public class AccountGateway
    {
        public AccountGateway(string account, string sysPlatform, int gameDataChannelID, int shortConnXDGatewayChannelID, int shortConnXDGatewayClientSessionID)
        {
            m_account = account;
            m_sysPlatform = sysPlatform;
            m_gameDataChannelID = gameDataChannelID;
            m_shortConnXDGatewayChannelID = shortConnXDGatewayChannelID;
            m_shortConnXDGatewayClientSessionID = shortConnXDGatewayClientSessionID;
            m_firstLoginTimeInSec = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond;
            m_sign = Guid.NewGuid();
            m_tSign = ServerCommon.Network.NetworkManager.GuidConvertToPtGuid(m_sign);
        }

        #region Public Functions

        /// <summary>
        /// 更新签名
        /// </summary>
        /// <returns></returns>
        public PtGuid ChangeSign()
        {
            Guid oldSign = m_sign;

            m_sign = Guid.NewGuid();
            m_tSign = ServerCommon.Network.NetworkManager.GuidConvertToPtGuid(m_sign);

            AccountGatewayManager.Instance.ChangeSign(ref oldSign, ref m_sign);
            return m_tSign;
        }

        /// <summary>
        /// 签名认证
        /// </summary>
        /// <param name="sign"></param>
        /// <returns></returns>
        public bool CheckSign(PtGuid sign)
        {
            if (m_sign == ServerCommon.Network.NetworkManager.PtGuidConvertToGuid(sign))
                return true;
            return false;
        }

        /// <summary>
        /// 更新长连接
        /// </summary>
        /// <param name="xdgatewayChannelID"></param>
        /// <param name="clientSessionID"></param>
        public void ChangeLongConnection(int xdgatewayChannelID, int clientSessionID)
        {
            m_longConnXDGatewayChannelID = xdgatewayChannelID;
            m_longConnClientSessionID = clientSessionID;
        }

        /// <summary>
        /// 更新短连接
        /// </summary>
        /// <param name="xdgatewayChannelID"></param>
        /// <param name="clientSessionID"></param>
        public void ChangeShortConnection(int xdgatewayChannelID, int clientSessionID)
        {
            m_shortConnXDGatewayChannelID = xdgatewayChannelID;
            m_shortConnXDGatewayClientSessionID = clientSessionID;
        }

        /// <summary>
        /// 重置长连接
        /// </summary>
        public void ResetLongConnection()
        {
            m_longConnXDGatewayChannelID = 0;
            m_longConnClientSessionID = 0;
        }

        /// <summary>
        /// 改变当前玩家ID
        /// </summary>
        /// <param name="PlayerFlagiD"></param>
        public void ChangeCurPlayerFlagID(long PlayerFlagiD)
        {
            long oldCurPlayerFlagID = m_curPlayerFlagID;
            m_curPlayerFlagID = PlayerFlagiD;

            AccountGatewayManager.Instance.ChangeCurPlayerFlagID(m_account, m_curPlayerFlagID);
        }

        /// <summary>
        /// 改变平台号
        /// </summary>
        /// <param name="systemPlatform"></param>
        public void ChangeSysPlatform(string systemPlatform)
        {
            m_sysPlatform = systemPlatform;
        }

        /// <summary>
        /// 改变加密串
        /// </summary>
        /// <param name="encryptKey"></param>
        /// <param name="encryptValue"></param>
        public void ChangeEncrypt(string encryptKey, TByteList encryptValue)
        {
            m_encryptKey = encryptKey;
            m_encryptValue = encryptValue;
        }

        public void SetChatData(int chatSvrChannelID, int chatChannelID)
        {
            m_chatSvrChannelID = chatSvrChannelID;
            m_chatChannelID = chatChannelID;
        }

        public void ClearChatData()
        {
            m_chatChannelID = 0;
            m_chatSvrChannelID = 0;
        }

        #endregion

        #region Attribute

        public string Account
        {
            get
            {
                return m_account;
            }
        }

        public int FirstLoginTimeInSec
        {
            get
            {
                return m_firstLoginTimeInSec;
            }
        }

        public Guid Sign
        {
            get
            {
                return m_sign;
            }
        }

        public PtGuid TSign
        {
            get
            {
                return m_tSign;
            }
        }

        public int GameDataChannelID
        {
            get
            {
                return m_gameDataChannelID;
            }
        }

        public int LongConnXDGatewayChannelID
        {
            get
            {
                return m_longConnXDGatewayChannelID;
            }
        }

        public int LongConnClientSessionID
        {
            get
            {
                return m_longConnClientSessionID;
            }
        }

        public int ShortConnXDGatewayChannelID
        {
            get
            {
                return m_shortConnXDGatewayChannelID;
            }
        }

        public int ShortConnClientSessionID
        {
            get
            {
                return m_shortConnXDGatewayClientSessionID;
            }
        }

        public string EncryptKey
        {
            get
            {
                return m_encryptKey;
            }
        }

        public TByteList EncryptValue
        {
            get
            {
                return m_encryptValue;
            }
        }

        public long CurPlayerFlagID
        {
            get
            {
                return m_curPlayerFlagID;
            }
        }

        public string SysPlatform
        {
            get
            {
                return m_sysPlatform;
            }
        }

        public int ChatSvrChannelID
        {
            get
            {
                return m_chatSvrChannelID;
            }
        }

        public int ChatChannelID
        {
            get
            {
                return m_chatChannelID;
            }
        }

        #endregion

        private string m_account;
        private Guid m_sign;
        private PtGuid m_tSign;
        private long m_curPlayerFlagID;
        private int m_gameDataChannelID;
        private int m_longConnXDGatewayChannelID;
        private int m_longConnClientSessionID;
        private int m_shortConnXDGatewayChannelID;
        private int m_shortConnXDGatewayClientSessionID;
        private string m_encryptKey;
        private TByteList m_encryptValue;
        private int m_firstLoginTimeInSec;
        private string m_sysPlatform;
        private int m_chatSvrChannelID;
        private int m_chatChannelID;
    }

    //账号路由管理
    public class AccountGatewayManager
    {
        public static AccountGatewayManager Instance
        {
            get
            {
                return m_instance;
            }
        }
        private static AccountGatewayManager m_instance = new AccountGatewayManager();
        private AccountGatewayManager() { }

        public void AddAccountGateway(AccountGateway accountGateway)
        {
            m_accountGatewayByAccountDic[accountGateway.Account] = accountGateway;
            m_accountGatewayBySignDic[accountGateway.Sign] = accountGateway;
            m_accountGatewayByPlayerFlagIDDic[accountGateway.CurPlayerFlagID] = accountGateway;
        }

        public void ChangeSign(ref Guid oldSign, ref Guid newSign)
        {
            AccountGateway accGateway;
            if (m_accountGatewayBySignDic.TryGetValue(oldSign, out accGateway) == true)
            {
                m_accountGatewayBySignDic.Remove(oldSign);
                m_accountGatewayBySignDic.Add(newSign, accGateway);
            }
        }

        public void ChangeCurPlayerFlagID(string accountName, long newPlayerFlagID)
        {
            AccountGateway accGateway;
            if(m_accountGatewayByAccountDic.TryGetValue(accountName, out accGateway) == true)
            {
                if(accGateway.CurPlayerFlagID != 0)
                    m_accountGatewayByPlayerFlagIDDic.Remove(accGateway.CurPlayerFlagID);
                m_accountGatewayByPlayerFlagIDDic[newPlayerFlagID] = accGateway;
            }
        }

        public AccountGateway GetAccountGateway(string account)
        {
            AccountGateway accGateway;
            m_accountGatewayByAccountDic.TryGetValue(account, out accGateway);
            return accGateway;
        }

        public AccountGateway GetAccountGateway(PtGuid sign)
        {
            AccountGateway accGateway;
            m_accountGatewayBySignDic.TryGetValue(ServerCommon.Network.NetworkManager.PtGuidConvertToGuid(sign), out accGateway);
            return accGateway;
        }

        public AccountGateway GetAccountGateway(ref Guid sign)
        {
            AccountGateway accGateway;
            m_accountGatewayBySignDic.TryGetValue(sign, out accGateway);
            return accGateway;
        }

        public AccountGateway GetAccountGateway(long curPlayerFlagID)
        {
            AccountGateway accGateway;
            m_accountGatewayByPlayerFlagIDDic.TryGetValue(curPlayerFlagID, out accGateway);
            return accGateway;
        }

        private Dictionary<string, AccountGateway> m_accountGatewayByAccountDic = new Dictionary<string, AccountGateway>();
        private Dictionary<Guid, AccountGateway> m_accountGatewayBySignDic = new Dictionary<Guid, AccountGateway>();
        private Dictionary<long, AccountGateway> m_accountGatewayByPlayerFlagIDDic = new Dictionary<long, AccountGateway>();
    }
}
