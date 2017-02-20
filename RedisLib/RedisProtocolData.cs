using System;
using System.Collections.Generic;
using System.Text;
using com.ideadynamo.foundation.buffer;

namespace RedisLib
{
    public class RedisProtocolData
    {
        public RedisProtocolData(int sourceChannelID, ByteArray gameProtocol, byte isFromClient = 0, byte isLongConnect = 0)
        {
            m_SourceChannelID = sourceChannelID;
            m_IsFromClient = isFromClient;
            m_IsLongConnect = isLongConnect;
            m_GameProtocolData = gameProtocol;
        }

        public RedisProtocolData(ref byte[] redisProtocol)
        {
            ByteArray redisProtocolDataArr = RedisCommonFunc.RedisObjBytesToObjByteArray(ref redisProtocol);
            m_SourceChannelID = redisProtocolDataArr.readDynamicsInt();
            m_IsFromClient = redisProtocolDataArr.readByte();
            m_IsLongConnect = redisProtocolDataArr.readByte();
            m_GameProtocolData = ByteArray.SReadFromByteArray(redisProtocolDataArr);
        }

        public ByteArray WriteToByteArray()
        {
            ByteArray byteArray = new ByteArray();
            byteArray.writeDynamicsInt(m_SourceChannelID);
            byteArray.writeByte(m_IsFromClient);
            byteArray.writeByte(m_IsLongConnect);
            m_GameProtocolData.WriteToByteArray(byteArray);
            return byteArray;
        }

        public int m_SourceChannelID;
        public byte m_IsFromClient;
        public byte m_IsLongConnect;
        public ByteArray m_GameProtocolData = null;
    }
}
