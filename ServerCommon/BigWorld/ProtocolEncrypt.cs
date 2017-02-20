using System;
using System.Collections.Generic;
using System.Text;
using com.ideadynamo.foundation.buffer;
using com.tieao.mmo.CustomTypeInProtocol;

namespace ServerCommon
{
    /// <summary>
    /// 协议秘钥
    /// </summary>
    public class ProtocolDecrypt
    {
        public static bool Decrypt(TByteList encryptValue, ByteArray protocolData)
        {
            if (encryptValue != null && encryptValue.GetElements().Count > 0)
            {
                for (var i = ByteArray.HEADERLENGTH; i < protocolData.Length; ++i)
                {
                    var v = encryptValue.GetElements()[i % encryptValue.GetElements().Count];
                    protocolData.Buffer[i] = (byte)(protocolData.Buffer[i] ^ 0xaa);
                    protocolData.Buffer[i] = (byte)((protocolData.Buffer[i] - v + 256) % 256);
                }

                return true;
            }
            return false;
        }
    }
}
