using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Standard
{
    public static class Constant
    {
        public static readonly String CR = Convert.ToChar(13).ToString();
        public static readonly String CRLF = Convert.ToChar(13).ToString() + Convert.ToChar(10).ToString();
        public static readonly String LF = Convert.ToChar(10).ToString();
    }

    public static class Log
    {
        public static void Detail(String General, String Specific)
        {
            StringBuilder Detail_String = new StringBuilder();
            int Detail_Length = 0;

            if (General.Length + Specific.Length != 0)
            {
                Detail_String.Append(DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + General);
                Detail_Length = Detail_String.Length;

                if (Specific.Length != 0)
                {
                    Detail_String.Append(Constant.CRLF);
                    Detail_String.Append("".PadLeft(Detail_Length, '-'));
                    Detail_String.Append(Constant.CRLF);
                    Detail_String.Append(Specific);
                }

                Detail_String.Append(Constant.CRLF);
                Detail_String.Append(Constant.CRLF);

                Detail_String = Detail_String.Replace(Constant.CRLF, Constant.CR);
                Detail_String = Detail_String.Replace(Constant.LF, Constant.CR);
                Detail_String = Detail_String.Replace(Constant.CR, Constant.CRLF);

                Console.Write(Detail_String);
            }
        }
    }

    public static class Network
    {
        public static String[] IP4_List()
        {
            String HostName = Dns.GetHostName();
            List<String> IP4_List = new List<String>();

            IPAddress[] IP_List = Dns.GetHostAddresses(HostName);

            foreach (IPAddress IP4 in IP_List.Where(IP => IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
            {
                IP4_List.Add(IP4.ToString());
            }

            return IP4_List.ToArray();
        }
    }
}
