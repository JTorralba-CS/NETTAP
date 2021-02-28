using System;
using System.Collections.Generic;
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
        public static StringBuilder Detail(String General, String Specific)
        {
            StringBuilder Detail_String = new StringBuilder();
            int Detail_Length = 0;

            if (General.Length + Specific.Length != 0)
            {
                Detail_String.Append(DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + General);

                switch (Specific.Length)
                {
                    case 0:
                        break;
                    case 1:
                        Detail_String.Append(" " + Specific.Length + " Byte");
                        break;
                    default:
                        Detail_String.Append(" " + Specific.Length + " Bytes");
                        break;
                }

                Detail_Length = Detail_String.Length;

                if (Specific.Length != 0)
                {
                    Detail_String.Append(Constant.CRLF);
                    Detail_String.Append("".PadLeft(Detail_Length, '-'));
                    Detail_String.Append(Constant.CRLF);
                    Detail_String.Append(Specific);
                }

                Detail_String = Detail_String.Replace(Constant.CRLF, Constant.CR);
                Detail_String = Detail_String.Replace(Constant.LF, Constant.CR);
                Detail_String = Detail_String.Replace(Constant.CR, Constant.CRLF);
            }
            return Detail_String;
        }

        public static void Terminal(String General, String Specific)
        {
            Console.WriteLine(Detail(General, Specific));
            Console.WriteLine();
        }

        public static void Terminal(String General)
        {
            Terminal(General, "");
        }

        public static void Terminal(String General, Byte[] Specific)
        {
            Terminal(General, System.Text.Encoding.ASCII.GetString(Specific));
        }
    }

    public static class Network
    {
        public static String[] IP(Byte Version)
        {
            IPAddress[] IPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            List<String> IP = new List<String>();

            for (int I = IPAddress.Length; --I >= 0;)
            {
                switch (Version)
                {
                    case 4:
                        if (IPAddress[I].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            IP.Add(IPAddress[I].ToString());
                        }
                        break;
                    case 6:
                        if (IPAddress[I].AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            IP.Add(IPAddress[I].ToString());
                        }
                        break;
                    default:
                        IP.Add(IPAddress[I].ToString());
                        break;
                }
            }

            return IP.ToArray();
        }
    }
}
