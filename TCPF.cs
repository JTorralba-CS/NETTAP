using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TCPF
{
    class TCPF
    {
        public static Boolean CCC = false;

        public static String CR = "" + Convert.ToChar(13);
        public static String CRLF = "" + Convert.ToChar(13) + Convert.ToChar(10);

        public static String HB_Request = "" + Convert.ToChar(02) + Convert.ToChar(72) + Convert.ToChar(03);
        public static String HB_Acknowledgement = "" + Convert.ToChar(02) + Convert.ToChar(06) + Convert.ToChar(03);
        public static Byte[] HB_Acknowledgement_Bytes = Encoding.ASCII.GetBytes(HB_Acknowledgement);

        private readonly Socket _Main_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Start(IPEndPoint Local, IPEndPoint Remote)
        {
            DateTime Time_Stamp;

            _Main_Socket.Bind(Local);
            _Main_Socket.Listen(10);

            while (true)
            {
                Time_Stamp = DateTime.Now;
                Log("Status", Time_Stamp, "Start: Source = _Main_Socket.Accept()", null);

                Socket Source = _Main_Socket.Accept();
                TCPF Destination = new TCPF();
                Socket_State State = new Socket_State(Source, Destination._Main_Socket);

                try
                {
                    Time_Stamp = DateTime.Now;
                    Log("Status", Time_Stamp, "Start: Destination.Connect", null);

                    Destination.Connect(Remote, Source);
                }
                catch (Exception E)
                {
                    Time_Stamp = DateTime.Now;
                    Log("Exception", Time_Stamp, "Start: Destination.Connect", E.ToString());
                }

                Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
            }
        }

        public static void Log(String File, DateTime Time_Stamp, String Basic, String Verbose)
        {
            String Detail_String = null;
            Byte[] Detail_Bytes = null;

            if (Basic + Verbose != "")
            {
                Detail_String += Time_Stamp.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + Basic;

                if (Verbose != null)
                {
                    Detail_String += CRLF;
                    Detail_String += "-----------------------------------------------------------------------------------";
                    Detail_String += CRLF;
                    Detail_String += Verbose;
                }

                Detail_String += CRLF;
                Detail_String += CRLF;

                Detail_Bytes = Encoding.ASCII.GetBytes(Detail_String);

                Write_To_File(Directory.GetCurrentDirectory() + "\\Log_" + File + ".txt", Detail_Bytes).ConfigureAwait(false);

                // Make Readable In Terminal
                Detail_String = Regex.Replace(Detail_String, @"\r\n", CR);
                Detail_String = Regex.Replace(Detail_String, @"\n", CR);
                Detail_String = Regex.Replace(Detail_String, @"\r", CRLF);

                Console.Write(Detail_String);
            }
        }

        public static async Task Write_To_File(String Path, Byte[] Bytes)
        {
            using (FileStream File = new FileStream(Path, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 32768, useAsync: true))
            {
                await File.WriteAsync(Bytes, 0, Bytes.Length);
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

            public Socket_State(Socket Source, Socket Destination)
            {
                Socket_Source = Source;
                Socket_Destination = Destination;
                Buffer = new Byte[16384];
            }
        }

        private void Connect(EndPoint Remote_Endpoint, Socket Destination)
        {
            DateTime Time_Stamp;

            Socket_State State = new Socket_State(_Main_Socket, Destination);

            Time_Stamp = DateTime.Now;
            Log("Status", Time_Stamp, "Connect: _Main_Socket.Connect", null);

            _Main_Socket.Connect(Remote_Endpoint);
            _Main_Socket.BeginReceive(State.Buffer, 0, State.Buffer.Length, SocketFlags.None, OnDataReceive, State);
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            DateTime Time_Stamp;

            Byte[] Packet_Bytes = null;

            String Packet_String = null;

            Socket_State State = (Socket_State)result.AsyncState;

            Time_Stamp = DateTime.Now;

            try
            {
                IPEndPoint Source_Local_IPEndPoint = State.Socket_Source.LocalEndPoint as IPEndPoint;
                IPEndPoint Source_Remote_IPEndPoint = State.Socket_Source.RemoteEndPoint as IPEndPoint;

                IPEndPoint Destination_Local_IPEndPoint = State.Socket_Destination.LocalEndPoint as IPEndPoint;
                IPEndPoint Destination_Remote_IPEndPoint = State.Socket_Destination.RemoteEndPoint as IPEndPoint;

                int Packet_Read = State.Socket_Source.EndReceive(result);

                if (Packet_Read > 0)
                {
                    Byte[] Packet_Raw = new byte[Packet_Read];
                    Byte[] Packet_CCC = new byte[0];

                    Buffer.BlockCopy(State.Buffer, 0, Packet_Raw, 0, Packet_Read);

                    Packet_Bytes = Packet_Raw;
                    Capture("Raw", Packet_Bytes);

                    if (CCC)
                    {
                        int Index = 0;

                        while (Index < (Packet_Read))
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
                                    Time_Stamp = DateTime.Now;
                                    Log("Exception", Time_Stamp, "OnDataReceive: CCC", E.ToString());
                                }
                            }

                            Index++;
                        }

                        Array.Reverse(Packet_CCC, 0, Packet_CCC.Length);

                        Packet_Bytes = Packet_CCC;
                        Capture("CCC", Packet_Bytes);
                    }

                    Packet_String = System.Text.Encoding.ASCII.GetString(Packet_Bytes);

                    Time_Stamp = DateTime.Now;

                    State.Socket_Destination.Send(Packet_Bytes, Packet_Bytes.Length, SocketFlags.None);

                    Log("Status", Time_Stamp, Source_Remote_IPEndPoint.Address + ":" + Source_Remote_IPEndPoint.Port + " ---> " + Destination_Remote_IPEndPoint.Address + ":" + Destination_Remote_IPEndPoint.Port + " " + String.Format("{0:000000}", Packet_Bytes.Length) + " Byte(s)", Packet_String);

                    // HB_Request Check
                    if (Packet_String.Contains(HB_Request))
                    {
                        Time_Stamp = DateTime.Now;

                        try
                        {
                            State.Socket_Source.Send(HB_Acknowledgement_Bytes, HB_Acknowledgement_Bytes.Length, SocketFlags.None);

                            Log("Status", Time_Stamp, "TCPF" + " ---> " + Source_Remote_IPEndPoint.Address + ":" + Source_Remote_IPEndPoint.Port + " " + String.Format("{0:000000}", HB_Acknowledgement_Bytes.Length) + " Byte(s)", HB_Acknowledgement);
                        }
                        catch (Exception E)
                        {
                            Log("Exception", Time_Stamp, "OnDataReceive", E.ToString());
                        }
                    }

                    State.Socket_Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
                }
            }
            catch (Exception E)
            {

                Log("Exception", Time_Stamp, "OnDataReceive", E.ToString());

                if (Packet_Bytes != null)
                {
                    Log("Exception", Time_Stamp, "OnDataReceive: (Packet Loss)", Packet_String);
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

        public static void Capture(string File_Name, byte[] Packet_Bytes)
        {
            Write_To_File(Directory.GetCurrentDirectory() + "\\Packet_" + File_Name + ".txt", Packet_Bytes).ConfigureAwait(false);
        }

        static void Main(String[] Arguments)
        {
            DateTime Time_Stamp;

            try
            {
                Console.Clear();
            }
            catch
            {
            }

            try
            {
                if (Arguments[4] == "CCC")
                {
                    CCC = true;
                }
            }
            catch
            {
            }

            try
            {
                Time_Stamp = DateTime.Now;
                Log("Status", Time_Stamp, "Main: TCPF().Start", null);

                new TCPF().Start(
                    new IPEndPoint(IPAddress.Parse(Arguments[0]), int.Parse(Arguments[1])),
                    new IPEndPoint(IPAddress.Parse(Arguments[2]), int.Parse(Arguments[3]))
                );
            }
            catch (Exception E)
            {
                Time_Stamp = DateTime.Now;
                Log("Exception", Time_Stamp, "Main: TCPF().Start", E.ToString());
            }
        }
    }
}
