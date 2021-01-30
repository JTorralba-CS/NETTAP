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

        public static String HEARTBEAT_REQUEST = "" + Convert.ToChar(02) + Convert.ToChar(72) + Convert.ToChar(03);
        public static String HEARTBEAT_ACKNOWLEDGEMENT = "" + Convert.ToChar(02) + Convert.ToChar(06) + Convert.ToChar(03);
        public static Byte[] HEARTBEAT_ACKNOWLEDGEMENT_Bytes = Encoding.ASCII.GetBytes(HEARTBEAT_ACKNOWLEDGEMENT);

        public static String CR = "" + Convert.ToChar(13);
        public static String CRLF = "" + Convert.ToChar(13) + Convert.ToChar(10);

        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Start(IPEndPoint local, IPEndPoint remote)
        {
            DateTime TimeStamp;

            _mainSocket.Bind(local);
            _mainSocket.Listen(10);

            while (true)
            {
                var source = _mainSocket.Accept();
                var destination = new TCPF();
                var state = new State(source, destination._mainSocket);

                try
                {
                    TimeStamp = DateTime.Now;
                    Log("Status", TimeStamp, "Start:Destination.Connect", null);

                    destination.Connect(remote, source);
                }
                catch (Exception E)
                {
                    TimeStamp = DateTime.Now;
                    Log("Exception", TimeStamp, "Start:Destination.Connect", E.ToString());
                }

                source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
            }
        }

        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            DateTime TimeStamp;

            var state = new State(_mainSocket, destination);

            TimeStamp = DateTime.Now;
            Log("Status", TimeStamp, "Connect:_mainSocket.Connect", null);

            _mainSocket.Connect(remoteEndpoint);
            _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            DateTime TimeStamp;

            Byte[] Packet_Bytes = null;

            String Packet_String = null;

            State state = (State)result.AsyncState;

            TimeStamp = DateTime.Now;

            try
            {
                IPEndPoint SLocalIPEndPoint = state.SourceSocket.LocalEndPoint as IPEndPoint;
                IPEndPoint SRemoteIPEndPoint = state.SourceSocket.RemoteEndPoint as IPEndPoint;

                IPEndPoint DLocalIPEndPoint = state.DestinationSocket.LocalEndPoint as IPEndPoint;
                IPEndPoint DRemoteIPEndPoint = state.DestinationSocket.RemoteEndPoint as IPEndPoint;

                var bytesRead = state.SourceSocket.EndReceive(result);

                if (bytesRead > 0)
                {
                    var bytesRaw = new byte[bytesRead];
                    var bytesCCC = new byte[0];

                    Buffer.BlockCopy(state.Buffer, 0, bytesRaw, 0, bytesRead);

                    Packet_Bytes = bytesRaw;
                    Capture("Raw", bytesRaw);

                    //Console.WriteLine("SourceSocket " + SLocalIPEndPoint.Address + ":" + SLocalIPEndPoint.Port + " <---> " + SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port);
                    //Console.WriteLine("DestinationSocket " + DLocalIPEndPoint.Address + ":" + DLocalIPEndPoint.Port + " <---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port);

                    if (CCC)
                    {
                        int pos = 0;
                        while (pos < (bytesRead))
                        {
                            if (bytesRaw[pos] != 10)
                            {
                                if (bytesRaw[pos] == 44)
                                {
                                    // Convert COMMA To SPACEBAR
                                    bytesCCC = addByteToArray(bytesCCC, 32);
                                }
                                else
                                {
                                    bytesCCC = addByteToArray(bytesCCC, bytesRaw[pos]);
                                }

                                try
                                {
                                    // ETX Check
                                    if (bytesRaw[pos] == 3)
                                    {
                                        bytesRaw[pos + 1] = 10;

                                        // Add END-OF-LINE Control Code
                                        bytesCCC = addByteToArray(bytesCCC, 13);
                                        bytesCCC = addByteToArray(bytesCCC, 10);
                                    }
                                }
                                catch (Exception E)
                                {
                                    TimeStamp = DateTime.Now;
                                    Log("Exception", TimeStamp, "OnDataReceive:CCC", E.ToString());
                                }
                            }

                            pos++;
                        }

                        Array.Reverse(bytesCCC, 0, bytesCCC.Length);

                        Packet_Bytes = bytesCCC;
                        Capture("CCC", bytesCCC);
                    }

                    Packet_String = System.Text.Encoding.ASCII.GetString(Packet_Bytes);

                    TimeStamp = DateTime.Now;

                    state.DestinationSocket.Send(Packet_Bytes, Packet_Bytes.Length, SocketFlags.None);

                    Log("Status", TimeStamp, SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port + " ---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port + " " + String.Format("{0:000000}", Packet_Bytes.Length) + " Byte(s)", Packet_String);

                    // HEARTBEAT_REQUEST Check
                    if (Packet_String.Contains(HEARTBEAT_REQUEST))
                    {
                        TimeStamp = DateTime.Now;

                        try
                        {
                            state.SourceSocket.Send(HEARTBEAT_ACKNOWLEDGEMENT_Bytes, HEARTBEAT_ACKNOWLEDGEMENT_Bytes.Length, SocketFlags.None);

                            Log("Status", TimeStamp, "TCPF" + " ---> " + SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port + " " + String.Format("{0:000000}", HEARTBEAT_ACKNOWLEDGEMENT_Bytes.Length) + " Byte(s)", HEARTBEAT_ACKNOWLEDGEMENT);
                        }
                        catch (Exception E)
                        {
                            Log("Exception", TimeStamp, "OnDataReceive", E.ToString());
                        }
                    }

                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch (Exception E)
            {

                Log("Exception", TimeStamp, "OnDataReceive", E.ToString());

                if (Packet_Bytes != null)
                {
                    Log("Exception", TimeStamp, "OnDataReceive: (Packet Loss)", Packet_String);
                }
                

                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }

        private class State
        {
            public Socket SourceSocket { get; private set; }
            public Socket DestinationSocket { get; private set; }
            public byte[] Buffer { get; private set; }
            public State(Socket source, Socket destination)
            {
                SourceSocket = source;
                DestinationSocket = destination;
                Buffer = new byte[16384];
            }
        }

        public static async Task AppendAllBytes(string path, byte[] bytes)
        {
            using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None,bufferSize:32768, useAsync:true))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.AbsolutePath).Directory.FullName;
        }

        static void Main(string[] args)
        {
            try
            {
                Console.Clear();
            }
            catch
            {
            }

            try
            {
                if (args[4] == "CCC")
                {
                    CCC = true;
                }
            }
            catch
            {
            }

            new TCPF().Start(
                new IPEndPoint(IPAddress.Parse(args[0]), int.Parse(args[1])),
                new IPEndPoint(IPAddress.Parse(args[2]), int.Parse(args[3])));
        }

        public static byte[] addByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
        }

        public static void Log(String File, DateTime TimeStamp, String Basic, String Verbose)
        {
            String Detail_String = null;
            Byte[] Detail_Bytes = null;

            if (Basic + Verbose != "")
            {
                Detail_String += TimeStamp.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + Basic;

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

                AppendAllBytes(Directory.GetCurrentDirectory() + "\\LOG_" + File + ".txt", Detail_Bytes).ConfigureAwait(false);

                // Make Readable In Terminal
                Detail_String = Regex.Replace(Detail_String, @"\r\n", CR);
                Detail_String = Regex.Replace(Detail_String, @"\n", CR);
                Detail_String = Regex.Replace(Detail_String, @"\r", CRLF);

                Console.Write(Detail_String);
            }
        }

        public static void Capture(String File, Byte[] Raw)
        {
            AppendAllBytes(Directory.GetCurrentDirectory() + "\\TCP_" + File + ".txt", Raw).ConfigureAwait(false);
        }
    }
}