using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class RandomStr
    {
        static RandomStr()
        {
            s_randomCharArray.Add('1');
            s_randomCharArray.Add('2');
            s_randomCharArray.Add('3');
            s_randomCharArray.Add('4');
            s_randomCharArray.Add('5');
            s_randomCharArray.Add('6');
            s_randomCharArray.Add('7');
            s_randomCharArray.Add('8');
            s_randomCharArray.Add('9');
            s_randomCharArray.Add('0');

            for (int i = 97; i <= 122; i++)
            {
                s_randomCharArray.Add(Convert.ToChar(i));
            }
        }

        public static string GenRandomStr(int length)
        {
            StringBuilder sbStr = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int index = s_random.Next(0,s_randomCharArray.Count);
                sbStr.Append(s_randomCharArray[index]);
            }
            return sbStr.ToString();
        }

        static Random s_random = new Random();
        static List<char> s_randomCharArray = new List<char>();
    }
}
