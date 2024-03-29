﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TCPF
{
    class TCPF
    {
        public static String CR = Convert.ToChar(13).ToString();
        public static String CRLF = Convert.ToChar(13).ToString() + Convert.ToChar(10).ToString();

        public static String HB_Request = Convert.ToChar(02).ToString() + Convert.ToChar(72).ToString() + Convert.ToChar(03).ToString();
        public static String HB_Acknowledgement = Convert.ToChar(02).ToString() + Convert.ToChar(06).ToString() + Convert.ToChar(03).ToString();
        public static Byte[] HB_Acknowledgement_Bytes = Encoding.ASCII.GetBytes(HB_Acknowledgement);

        public static String Erase_Message = Convert.ToChar(02).ToString() + Convert.ToChar(69).ToString();

        public static ExeConfigurationFileMap Settings_File = null;
        public static Configuration Settings_Data = null;
        public static SmtpClient SMTP_Client = null;

        private readonly Socket _Main_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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

        public void Start(String IP, int Listen_Port, IPEndPoint Remote)
        {
            IPEndPoint Local = null;

            Settings_File = new ExeConfigurationFileMap { ExeConfigFilename = Directory.GetCurrentDirectory() + "\\" + "Settings" + ".xml" };
            Settings_Data = ConfigurationManager.OpenMappedExeConfiguration(Settings_File, ConfigurationUserLevel.None);

            SMTP_Client = new SmtpClient(Settings_Data.AppSettings.Settings["SMTP_Host"].Value)
            {
                Port = int.Parse(Settings_Data.AppSettings.Settings["SMTP_Port"].Value),
                Credentials = new NetworkCredential(Settings_Data.AppSettings.Settings["SMTP_Username"].Value, Settings_Data.AppSettings.Settings["SMTP_Password"].Value),
                EnableSsl = true,
            };

            if (Listen_Port == 0)
            {
                Listen_Port = int.Parse(Settings_Data.AppSettings.Settings["Listen_Port"].Value);
            }

            Local = new IPEndPoint(IPAddress.Parse(IP), Listen_Port);

            _Main_Socket.Bind(Local);
            _Main_Socket.Listen(0);

            Log("Status", "Start: Listening On " + Local.Address.ToString() + ":" + String.Format("{0:000000}", Local.Port.ToString()), null);

            while (true)
            {
                Log("Status", "Start: _Main_Socket.Accept()", null);

                Socket Source = _Main_Socket.Accept();
                TCPF Destination = new TCPF();
                Socket_State State = new Socket_State(Source, Destination._Main_Socket);

                try
                {
                    Log("Status", "Start: Destination.Connect()", null);

                    Destination.Connect(Remote, Source);
                }
                catch (Exception E)
                {
                    Log("Exception", "Start: Destination.Connect()", E.Message);
                }

                Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
            }
        }

        public static void Log(String File, String General, String Specific)
        {
            String Detail_String = null;
            Byte[] Detail_Bytes = null;

            if (General + Specific != "")
            {
                Detail_String += DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + General;

                if (Specific != null)
                {
                    Detail_String += CRLF;
                    Detail_String += "--------------------------------------------------------------------------------------";
                    Detail_String += CRLF;
                    Detail_String += Specific;
                }

                Detail_String += CRLF;
                Detail_String += CRLF;

                Detail_Bytes = Encoding.ASCII.GetBytes(Detail_String);

                Write_To_File(Directory.GetCurrentDirectory() + "\\" + File + ".txt", Detail_Bytes).ConfigureAwait(false);

                // Make Readable In Terminal
                Detail_String = Regex.Replace(Detail_String, @"\r\n", CR);
                Detail_String = Regex.Replace(Detail_String, @"\n", CR);
                Detail_String = Regex.Replace(Detail_String, @"\r", CRLF);

                Console.Write(Detail_String);
            }
        }

        public static async Task Write_To_File(String Path, Byte[] Bytes)
        {
            String Detail_String = null;

            try
            {
                using (FileStream File = new FileStream(Path, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 32768, useAsync: true))
                {
                    await File.WriteAsync(Bytes, 0, Bytes.Length);
                }
            }
            catch (Exception E)
            {
                Detail_String += DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " Exception: Write_To_File";
                Detail_String += CRLF;
                Detail_String += "--------------------------------------------------------------------------------------";
                Detail_String += CRLF;
                Detail_String += E.Message;
                Detail_String += CRLF;
                Detail_String += CRLF;
                Console.Write(Detail_String);
            }
        }

        private class Socket_State
        {
            public Socket Socket_Source
            {
                get;
                private set;
            }

            public Socket Socket_Destination
            {
                get;
                private set;
            }

            public Byte[] Buffer
            {
                get;
                private set;
            }

            public Boolean CCC
            {
                get;
                set;
            }

            public int Packet_Largest
            {
                get;
                set;
            }

            public Socket_State(Socket Source, Socket Destination)
            {
                Socket_Source = Source;
                Socket_Destination = Destination;
                Buffer = new Byte[16384];
                CCC = false;
                Packet_Largest = 0;
            }
        }

        private void Connect(EndPoint Remote_Endpoint, Socket Destination)
        {
            Socket_State State = new Socket_State(_Main_Socket, Destination);

            Log("Status", "Connect: _Main_Socket.Connect()", null);

            _Main_Socket.Connect(Remote_Endpoint);
            _Main_Socket.BeginReceive(State.Buffer, 0, State.Buffer.Length, SocketFlags.None, OnDataReceive, State);
        }

        private static void OnDataReceive(IAsyncResult Result)
        {
            Byte[] Packet_Bytes = null;

            String Packet_String = null;

            Socket_State State = (Socket_State)Result.AsyncState;

            String File = null;

            try
            {
                IPEndPoint Source_Local_IPEndPoint = State.Socket_Source.LocalEndPoint as IPEndPoint;
                IPEndPoint Source_Remote_IPEndPoint = State.Socket_Source.RemoteEndPoint as IPEndPoint;

                File = Source_Remote_IPEndPoint.Address.ToString() + "_" + Source_Remote_IPEndPoint.Port + "\\";
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\" + File))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + File);
                }

                IPEndPoint Destination_Local_IPEndPoint = State.Socket_Destination.LocalEndPoint as IPEndPoint;
                IPEndPoint Destination_Remote_IPEndPoint = State.Socket_Destination.RemoteEndPoint as IPEndPoint;

                int Packet_Size = State.Socket_Source.EndReceive(Result);

                if (Packet_Size > 0)
                {
                    if (Settings_Data.AppSettings.Settings["CCC"].Value == "Auto" && State.CCC != true && State.Buffer[0] == 02)
                    {
                        State.CCC = true;
                    }

                    Byte[] Packet_Raw = new byte[Packet_Size];
                    Byte[] Packet_CCC = new byte[0];

                    Buffer.BlockCopy(State.Buffer, 0, Packet_Raw, 0, Packet_Size);

                    Packet_Bytes = Packet_Raw;
                    Capture(File + "Raw", Packet_Bytes);

                    if (State.CCC)
                    {
                        int Index = 0;

                        while (Index < (Packet_Size))
                        {
                            if (Packet_Raw[Index] != 10)
                            {
                                if (Packet_Raw[Index] == 44)
                                {
                                    // Convert COMMA To SPACEBAR
                                    Packet_CCC = Append_Byte(Packet_CCC, 32);
                                }
                                else
                                {
                                    Packet_CCC = Append_Byte(Packet_CCC, Packet_Raw[Index]);
                                }

                                try
                                {
                                    // ETX Check
                                    if (Packet_Raw[Index] == 3)
                                    {
                                        Packet_Raw[Index + 1] = 10;

                                        // Add END-OF-LINE Control Code
                                        Packet_CCC = Append_Byte(Packet_CCC, 13);
                                        Packet_CCC = Append_Byte(Packet_CCC, 10);
                                    }
                                }
                                catch (Exception E)
                                {
                                    Log(File + "Exception", "OnDataReceive: ETX Check", E.Message);
                                }
                            }

                            Index++;
                        }

                        Array.Reverse(Packet_CCC, 0, Packet_CCC.Length);

                        Packet_Bytes = Packet_CCC;
                        Capture(File + "CCC", Packet_Bytes);
                    }

                    Packet_String = System.Text.Encoding.ASCII.GetString(Packet_Bytes);

                    State.Socket_Destination.Send(Packet_Bytes, Packet_Bytes.Length, SocketFlags.None);

                    Log(File + "Status", Source_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Source_Remote_IPEndPoint.Port) + " ---> " + Destination_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Destination_Remote_IPEndPoint.Port) + " " + String.Format("{0:000000}", Packet_Bytes.Length) + " Byte(s)", Packet_String);

                    if (Packet_Size > State.Packet_Largest)
                    {
                        State.Packet_Largest = Packet_Size;
                        Log(File + "Largest", Source_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Source_Remote_IPEndPoint.Port) + " ---> " + Destination_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Destination_Remote_IPEndPoint.Port) + " " + String.Format("{0:000000}", Packet_Bytes.Length) + " Byte(s)", Packet_String);
                    }

                    // HB_Request Check
                    if (Settings_Data.AppSettings.Settings["ACK"].Value == "Auto" && Packet_String.Contains(HB_Request))
                    {
                        try
                        {
                            State.Socket_Source.Send(HB_Acknowledgement_Bytes, HB_Acknowledgement_Bytes.Length, SocketFlags.None);

                            Log(File + "Status", "TCPF" + " ---> " + Source_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Source_Remote_IPEndPoint.Port) + " " + String.Format("{0:000000}", HB_Acknowledgement_Bytes.Length) + " Byte(s)", HB_Acknowledgement);
                        }
                        catch (Exception E)
                        {
                            Log(File + "Exception", "OnDataReceive: State.Socket_Source.Send()", E.Message);
                        }
                    }

                    // Anomaly Check
                    if (Packet_String.Contains("303-911-0000"))
                    {
                        try
                        {
                            EMail(Source_Remote_IPEndPoint.Address + ":" + Source_Remote_IPEndPoint.Port + " ---> " + Destination_Remote_IPEndPoint.Address + ":" + Destination_Remote_IPEndPoint.Port + " " + String.Format("{0:000000}", Packet_Bytes.Length) + " Byte(s)", Packet_String);
                        }
                        catch (Exception E)
                        {
                            Log(File + "Exception", "OnDataReceive: EMail()", E.Message);
                        }
                    }

                    // Erase_Message Check
                    if (Packet_String.Contains(Erase_Message))
                    {
                        try
                        {
                            String Detail_String = Packet_String;
                            Detail_String = Regex.Replace(Detail_String, @"\r\n", "");
                            Detail_String = Regex.Replace(Detail_String, @"\n", "");
                            Detail_String = Regex.Replace(Detail_String, @"\r", "");
                            Log(File + "Erase", Source_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Source_Remote_IPEndPoint.Port) + " ---> " + Destination_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Destination_Remote_IPEndPoint.Port) + " " + String.Format("{0:000000}", Packet_Bytes.Length) + " Byte(s) " + Detail_String, null);
                        }
                        catch (Exception E)
                        {
                            Log(File + "Exception", "OnDataReceive: Erase Message Check", E.Message);
                        }
                    }

                    // Filter Check
                    if (Settings_Data.AppSettings.Settings["Filter"].Value != "" && Packet_String.ToUpper().Contains(Settings_Data.AppSettings.Settings["Filter"].Value.ToUpper()))
                    {
                        State.Packet_Largest = Packet_Size;
                        Log(File + "Filter", Source_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Source_Remote_IPEndPoint.Port) + " ---> " + Destination_Remote_IPEndPoint.Address + ":" + String.Format("{0:000000}", Destination_Remote_IPEndPoint.Port) + " " + String.Format("{0:000000}", Packet_Bytes.Length) + " Byte(s)", Packet_String);
                    }

                    State.Socket_Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
                }
            }
            catch (Exception E)
            {
                Log(File + "Exception", "OnDataReceive", E.Message);

                if (Packet_Bytes != null)
                {
                    Log(File + "Exception", "OnDataReceive: (Packet Loss)", Packet_String);
                }

                State.Socket_Destination.Close();
                State.Socket_Source.Close();
            }
        }

        public static Byte[] Append_Byte(Byte[] Packet_Bytes, Byte Packet_Byte)
        {
            Byte[] Packet_Bytes_New = new Byte[Packet_Bytes.Length + 1];

            Packet_Bytes.CopyTo(Packet_Bytes_New, 1);
            Packet_Bytes_New[0] = Packet_Byte;

            return Packet_Bytes_New;
        }

        public static void Capture(string File_Name, byte[] Bytes)
        {
            Write_To_File(Directory.GetCurrentDirectory() + "\\" + File_Name + ".txt", Bytes).ConfigureAwait(false);
        }

        public static void EMail(String General, String Specific)
        {
            String Detail_String = null;

            if (General + Specific != "")
            {
                Detail_String += DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + General;

                if (Specific != null)
                {
                    Detail_String += CRLF;
                    Detail_String += "--------------------------------------------------------------------------------------";
                    Detail_String += CRLF;
                    Detail_String += Specific;
                }
            }

            SMTP_Client.Send(Settings_Data.AppSettings.Settings["SMTP_Sender"].Value, Settings_Data.AppSettings.Settings["SMTP_Recipient"].Value, "Anomaly Detected", Detail_String);
        }

        static void Main(String[] Arguments)
        {
            String Destination_IP = null;
            int Destination_Port = 0;
            int Listen_Port = 0;

            try
            {
                Console.Clear();
            }
            catch
            {
            }

            try
            {
                if (Arguments.Length > 2)
                {
                    Destination_IP = Arguments[0];
                    Destination_Port = int.Parse(Arguments[1]);
                    Listen_Port = int.Parse(Arguments[2]);
                }
                else if (Arguments.Length == 2)
                {
                    Destination_IP = Arguments[0];
                    Destination_Port = int.Parse(Arguments[1]);
                }
                else
                {
                    while (Destination_IP == null || Destination_Port == 0 || Listen_Port == 0)
                    {
                        Console.Write("Destination-IP:    ");
                        Destination_IP = Console.ReadLine();

                        Console.Write("Destination-Port:  ");
                        Destination_Port = int.Parse(Console.ReadLine());

                        Console.Write("Listen-Port:       ");
                        Listen_Port = int.Parse(Console.ReadLine());

                        Console.WriteLine();
                    }
                }
            }
            catch
            {
            }

            try
            {
                Log("Status", "Main: TCPF().Start()", null);

                foreach (String IP in IP4_List())
                {
                    new TCPF().Start(IP, Listen_Port, new IPEndPoint(IPAddress.Parse(Destination_IP), Destination_Port));
                }
            }
            catch (Exception E)
            {
                Log("Exception", "Main: TCPF().Start()", E.Message);
            }
        }
    }
}
