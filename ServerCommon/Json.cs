using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;
using System.Runtime.Serialization.Json;

namespace ServerCommon
{
    public interface IJson
    {
        void ReadObject(string name, JsonReader reader, JsonSerialReader jsReader);
        void ReadString(string name, object value);
        void ReadBool(string name, object value);
        void ReadInt(string name, object value);
    }

    public class JsonSerialReader
    {
        string _currentProperty;

        public void Read<T>(string input, T obj) where T : IJson
        {
            JsonReader reader = new JsonTextReader(new StringReader(input));

            if (reader.Read())
            {
                Read<T>(reader, obj);
            }
        }

        public bool Read<T>(JsonReader reader, T t) where T : IJson
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndObject)
                    {
                        return true;
                    }

                    else if (reader.TokenType == JsonToken.PropertyName)
                    {
                        _currentProperty = reader.Value.ToString();
                    }

                    else if (reader.TokenType == JsonToken.String)
                    {
                        t.ReadString(_currentProperty, reader.Value);
                    }

                    else if (reader.TokenType == JsonToken.Boolean)
                    {
                        t.ReadBool(_currentProperty, reader.Value);
                    }

                    else if (reader.TokenType == JsonToken.Integer)
                    {
                        t.ReadInt(_currentProperty, reader.Value);
                    }

                    else if (reader.TokenType == JsonToken.StartObject)
                    {
                        t.ReadObject(_currentProperty, reader, this);
                    }
                }
                return true;
            }

            return false;
        }


        public static string GetString(object value, string defaultval)
        {
            if (value == null) return defaultval;
            return value.ToString();
        }

        public static int GetInt(object value, int defaultval)
        {
            if (value == null) return defaultval;
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception err)
            {
                return defaultval;
            }
        }

        public static bool GetBool(object value, bool defaultval)
        {
            if (value == null) return defaultval;
            try
            {
                return Convert.ToBoolean(value);
            }
            catch (Exception err)
            {
                return defaultval;
            }
        }
    }

    public class JsonTool
    {

        /// <summary>
        /// 将队列数据转换为json数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string List2JsonStr(List<int> list, bool appendHead = true)
        {

            if (list != null && list.Count > 0)
            {
                StringBuilder jsonStr = new StringBuilder();

                if (appendHead)
                    jsonStr.Append("[");

                for (int i = 0; i < list.Count; i++)
                {
                    jsonStr.Append(list[i]);

                    if (i != list.Count - 1)
                    {
                        jsonStr.Append(",");
                    }
                }

                if (appendHead)
                    jsonStr.Append("]");

                return jsonStr.ToString();
            }
            else
            {
                return "[]";
            }

        }

        /// <summary>
        /// 将队列数据转换为json数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string List2JsonStr(List<long> list, bool appendHead = true)
        {

            if (list != null && list.Count > 0)
            {
                StringBuilder jsonStr = new StringBuilder();

                if (appendHead)
                    jsonStr.Append("[");

                for (int i = 0; i < list.Count; i++)
                {
                    jsonStr.Append(list[i]);

                    if (i != list.Count - 1)
                    {
                        jsonStr.Append(",");
                    }
                }

                if (appendHead)
                    jsonStr.Append("]");

                return jsonStr.ToString();
            }
            else
            {
                return "[]";
            }

        }

        /// <summary>
        /// 将队列数据转换为json数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string List2JsonStr(List<string> list, bool appendHead = true)
        {

            if (list != null && list.Count > 0)
            {
                StringBuilder jsonStr = new StringBuilder();

                if (appendHead)
                    jsonStr.Append("[");

                for (int i = 0; i < list.Count; i++)
                {
                    jsonStr.Append("\"").Append(list[i]).Append("\"");

                    if (i != list.Count - 1)
                    {
                        jsonStr.Append(",");
                    }
                }

                if (appendHead)
                    jsonStr.Append("]");

                return jsonStr.ToString();
            }
            else
            {
                return "[]";
            }
        }

        public static TIntList Json2TIntList(string jsonStr)
        {
            TIntList result = null;

            if (jsonStr.Length < 3)
                return null;

            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2);
            string[] valArray = jsonStr.Split(',');
            for (int i = 0; i < valArray.Length; i++)
            {
                string valItem = valArray[i].Trim();
                if (valItem == string.Empty)
                    continue;

                if (result == null)
                    result = new TIntList();

                result.Add(Convert.ToInt32(valItem));
            }

            return result;
        }

        public static TLongList Json2TLongList(string jsonStr)
        {
            TLongList result = null;

            if (jsonStr.Length < 3)
                return null;

            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2);
            string[] valArray = jsonStr.Split(',');
            for (int i = 0; i < valArray.Length; i++)
            {
                string valItem = valArray[i].Trim();
                if (valItem == string.Empty)
                    continue;

                if (result == null)
                    result = new TLongList();

                result.Add(Convert.ToInt64(valItem));
            }

            return result;
        }

        public static TStrList Json2TStrList(string jsonStr)
        {
            TStrList result = null;

            if (jsonStr.Length < 3)
                return null;

            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2);
            string[] valArray = jsonStr.Split(',');
            for (int i = 0; i < valArray.Length; i++)
            {
                string valItem = valArray[i].Trim().Replace("\"", "");
                if (valItem == string.Empty)
                    continue;

                if (result == null)
                    result = new TStrList();

                result.Add(valItem);
            }

            return result;
        }

        /// <summary>
        /// 对象类型(即每一项都以{}包含)的数组拆分
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static string TryJsonObj2ArrayParse(string jsonStr, out TStrList strList)
        {
            string result = null;
            strList = null;

            jsonStr = jsonStr.Substring(1, jsonStr.Length - 2);

            while (true)
            {
                int beginIndex = jsonStr.IndexOf('{');
                if (beginIndex == -1)
                    break;

                int endIndex = jsonStr.IndexOf('}');
                if (endIndex == -1)
                {
                    result = string.Format("JsonObjArrayParse 字符串格式不正确,没有找到反括号 jsonStr = {0}", jsonStr);
                    SvLogger.Error(result);
                    return result;
                }

                if (beginIndex > endIndex)
                {
                    result = string.Format("JsonObjArrayParse 字符串格式不正确,正反括号位置异常  jsonStr = {0}", jsonStr);
                    SvLogger.Error(result);
                    return result;
                }

                if (strList == null)
                    strList = new TStrList();

                string strItem = jsonStr.Substring(beginIndex + 1, endIndex - beginIndex - 1);
                strList.Add(strItem);

                if (endIndex + 1 == jsonStr.Length)
                    break;

                jsonStr = jsonStr.Substring(endIndex + 1);
            }

            return result;
        }

        public static string GetJsonStr(object obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            var stream = new MemoryStream();
            serializer.WriteObject(stream, obj);

            byte[] dataBytes = new byte[stream.Length];

            stream.Position = 0;

            stream.Read(dataBytes, 0, (int)stream.Length);

            return Encoding.UTF8.GetString(dataBytes);
        }

        public static T JsonDeserialized<T>(string strJson)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            var mStream = new MemoryStream(Encoding.UTF8.GetBytes(strJson));
            return (T)serializer.ReadObject(mStream);
        }

        /*  class格式
         [DataContract]
    class Config
    {
        [DataMember(Order = 0)]
        public string encoding { get; set; }
        [DataMember(Order = 1)]
        public string[] plugins { get; set; }
        [DataMember(Order = 2)]
        public Indent indent { get; set; }
    }
    
    [DataContract]
    class Indent
    {
        [DataMember(Order = 0)]
        public int length { get; set; }
        [DataMember(Order = 1)]
        public bool use_space { get; set; }
    }
         */
    }
}
