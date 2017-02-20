//source signature: [D1-0E-0B-79-C1-4C-AF-B6-23-78-B3-B1-51-E6-FF-7B]
// Autogenerated at 2017/2/3 21:09:16
// Created by [CodeGenerator Library:1.7.3.2014.04.20.15.19] FOR Client Protocol.
// Don't manully change this code unless you have to, all changes will be lost next time code is generated.
// Copyright ideadynamo.com 2008-2012.
namespace com.tieao.mmo.protocol.client
{
    using System;
    using System.Text;
    using System.Xml;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using com.ideadynamo.foundation;
    using com.ideadynamo.foundation.buffer;
    using com.tieao.mmo.protocol;
    
      using com.tieao.mmo.CustomTypeInProtocol;
    
    // set com.tieao.mmo.protocol
    /*Reference Codes
    
    End Reference Codes*/
    #region Service And Interfaces
    
    internal class EnumOperatorRequestMethods
    {
        
        public const int SERVER_LOGINBACK_HASH = 89059997;
        public const int CLIENT_LOGIN_HASH = -1982264496;
        public const int CLIENT_REQUESTERROR_HASH = 304135916;
    }
    
    public class  OperatorRequestClientHelper
    {
        public const string SOURCE_HASH = "D1-0E-0B-79-C1-4C-AF-B6-23-78-B3-B1-51-E6-FF-7B";

        
        public static ByteArray LoginBack(string account,string errMsg)
        {
        #if NO_BUFFER
            ByteArray byteArray = new ByteArray();
        #else
            ByteArray byteArray = BufferQueue.ClientBuffer.GetSendBuffer(0);
        #endif
            
            byteArray.writeInt( EnumOperatorRequestMethods.SERVER_LOGINBACK_HASH );
            byteArray.EncryptKey =  EnumOperatorRequestMethods.SERVER_LOGINBACK_HASH ;
            byteArray.CRC = 0;
            byteArray.writeDynamicsInt(ByteArray.globalSeq);

            byteArray.writeUTF(account);byteArray.writeUTF(errMsg);
          
            ++ByteArray.globalSeq;
            byteArray.writeInt(byteArray.CRC);
            byteArray.EncryptKey =  0;
            return byteArray;
        }
        
        public static void LoginBack(string account,string errMsg, ref ByteArray byteArray)
        {
            byteArray.Reset();
            byteArray.WriteHeader();
                        
            
            byteArray.writeInt( EnumOperatorRequestMethods.SERVER_LOGINBACK_HASH );
            byteArray.EncryptKey =  EnumOperatorRequestMethods.SERVER_LOGINBACK_HASH ;
            byteArray.CRC = 0;
            byteArray.writeDynamicsInt(ByteArray.globalSeq);

            byteArray.writeUTF(account);byteArray.writeUTF(errMsg);
          
            ++ByteArray.globalSeq;
            byteArray.writeIntNCRC(byteArray.CRC);
            byteArray.EncryptKey =  0;
        }
        
        
        public static ByteArray ClientRequestFailed__(int reason)
        {
            ByteArray byteArray = new ByteArray();
            byteArray.writeInt( EnumOperatorRequestMethods.CLIENT_REQUESTERROR_HASH);
            byteArray.EncryptKey =  EnumOperatorRequestMethods.CLIENT_REQUESTERROR_HASH ;
            byteArray.CRC = 0;
            byteArray.writeDynamicsInt(ByteArray.globalSeq);
            byteArray.writeDynamicsInt(reason);
            ++ByteArray.globalSeq;
            byteArray.writeIntNCRC(byteArray.CRC);
            byteArray.EncryptKey =  0;
            return byteArray;
        }
        
        public static bool IntepretMessage(ByteArray byteArray,IOperatorRequestClientService clientService)
        {
            byteArray.BypassHeader();
            int methodID = byteArray.readInt();
            bool mtdrst__ = false;
            switch(methodID)
            {
            
            case EnumOperatorRequestMethods.CLIENT_LOGIN_HASH: 
                byteArray.EncryptKey = EnumOperatorRequestMethods.CLIENT_LOGIN_HASH;
                byteArray.CRC = 0;
                byteArray.readDynamicsInt();
                mtdrst__ = OnLogin(byteArray , clientService);
                byteArray.EncryptKey =  0;
                return mtdrst__;
        
            case EnumOperatorRequestMethods.CLIENT_REQUESTERROR_HASH:
                byteArray.EncryptKey =  EnumOperatorRequestMethods.CLIENT_REQUESTERROR_HASH ;
                byteArray.readDynamicsInt();
                OnClientRequestError__(byteArray , clientService);
                byteArray.EncryptKey = 0;
                return true;
            }
            
            if(methodID == EnumOperatorRequestMethods.CLIENT_REQUESTERROR_HASH)
            {
                return OnClientRequestError__(byteArray , clientService);
            }
            // recover the header
            byteArray.Rewind();
            return false;
        }
        
        public static String ParseMessage(ByteArray byteArray , ParamedDelegate<String> dumper)
        {
            byteArray.BypassHeader();
            int methodID = byteArray.readInt();
            String result = String.Empty;
            switch(methodID)
            {
            
            case EnumOperatorRequestMethods.CLIENT_LOGIN_HASH: 
                result = ParseLogin(byteArray); 
                break;               
            }
            if(result != String.Empty) dumper(result);
            // recover the header
            byteArray.Rewind();
            return result;
        }

        
        private static bool OnLogin(ByteArray byteArray ,IOperatorRequestClientService clientService)
        {
            String account = byteArray.readUTF();
            String token = byteArray.readUTF();
            
            int crc = byteArray.readIntNCRC();
            if(crc == byteArray.CRC)
            {
                clientService.OnLogin( account, token);
            }
            
            byteArray.Recycle();
            return true;
        }
        
        private static bool OnClientRequestError__(ByteArray byteArray , IOperatorRequestClientService clientService)
        {
            int errorCode = byteArray.readDynamicsInt();
            if(ClientRequestErrorHandler__ != null)
            {
                ClientRequestErrorHandler__(errorCode);
            }
            return true;
        }
        
        private static string ParseLogin(ByteArray byteArray)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Login(");
            
            sb.Append("account : string,");
            
            sb.Append("token : string");
            
            sb.Append(")\r\n{\r\n");
            int crc = byteArray.readIntNCRC(); // parse global seq
            String account = byteArray.readUTF();
            String token = byteArray.readUTF();
            sb.AppendFormat(@"account = ""{0}""",  account);sb.Append(",\r\n");sb.AppendFormat(@"token = ""{0}""",  token);
            sb.Append("}");
            return sb.ToString();
        }
        
        
        public delegate void ClientRequestErrorDelegate__(int errorCode);
        public static ClientRequestErrorDelegate__ ClientRequestErrorHandler__;
   }

    public interface IOperatorRequestClientService
    {
        void OnLogin( string account, string token);
        
    }
  
    #endregion    //SECTION_TRANSACTIONS
    }
    // END OF GENERATED CODE
  