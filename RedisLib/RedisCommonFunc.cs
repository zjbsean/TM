using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Text;
using com.ideadynamo.foundation.buffer;

namespace RedisLib
{
    public class RedisCommonFunc
    {
        /// <summary>
        /// 从redis中的对象数据流转化为对象ByteArray
        /// </summary>
        /// <param name="objStr"></param>
        /// <returns></returns>
        public static ByteArray RedisObjBytesToObjByteArray(ref byte[] datas)
        {
            ByteArray dataArr = new ByteArray(datas);
            dataArr.AdjustWriter(1);
            dataArr.BypassHeader();
            return dataArr;
        }

        /// <summary>
        /// 从对象ByteArray转化为redis中的对象字符串
        /// </summary>
        /// <param name="objByteArray"></param>
        /// <returns></returns>
        //public static string ObjByteArrayToRedisObjString(ByteArray objByteArray)
        //{
        //    byte[] sendData = new byte[objByteArray.Length];
        //    Array.Copy(objByteArray.Buffer, sendData, objByteArray.Length);
        //    return System.Text.Encoding.ASCII.GetString(sendData);
        //}

        /// <summary>
        /// 从对象ByteArray转化为redis中的对象字节流
        /// </summary>
        /// <param name="objByteArray"></param>
        /// <returns></returns>
        public static byte[] ObjByteArrayToRedisObjBytes(ByteArray objByteArray)
        {
            byte[] sendData = new byte[objByteArray.Length];
            Array.Copy(objByteArray.Buffer, sendData, objByteArray.Length);
            return sendData;
        }

        /// <summary>
        /// 从对象string转化为Redis中的对象字节流
        /// </summary>
        /// <param name="objStr"></param>
        /// <returns></returns>
        public static byte[] ObjStringToReidsObjBytes(string objStr)
        {
            return objStr.ToUtf8Bytes();
        }
    }
}
