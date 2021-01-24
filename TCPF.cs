using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TCPF
{
    class TCPF
    {
        public static Boolean CCC = false;
        public static Socket PreviousSocket = null;
        public static String MessageNotSent = null;

        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Start(IPEndPoint local, IPEndPoint remote)
        {
            DateTime TimeStamp = DateTime.Now;

            _mainSocket.Bind(local);
            _mainSocket.Listen(10);

            while (true)
            {
                var source = _mainSocket.Accept();
                var destination = new TCPF();
                var state = new State(source, destination._mainSocket);

                try
                {
                    if (PreviousSocket != null & CCC)
                    {
                        PreviousSocket.Close();
                    }

                    Log("Status", TimeStamp, "Start", "Destination.Connect");

                    destination.Connect(remote, source);

                    PreviousSocket = destination._mainSocket;
                }
                catch (Exception E)
                {
                    Log("Exception", TimeStamp, "Start", E.ToString());
                }

                source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
            }
        }
        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            DateTime TimeStamp = DateTime.Now;

            var state = new State(_mainSocket, destination);

            _mainSocket.Connect(remoteEndpoint);
            _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);

            Log("Status", TimeStamp, "Connect", "_mainSocket");
        }
        private static void OnDataReceive(IAsyncResult result)
        {
            DateTime TimeStamp = DateTime.Now;
            MessageNotSent = null;

            var state = (State)result.AsyncState;

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
                    var bytesTimeStamp = new byte[0];
                    var stringTimeStamp = "";

                    Buffer.BlockCopy(state.Buffer, 0, bytesRaw, 0, bytesRead);

                    if (CCC)
                    {
                        int pos = 0;
                        while (pos < (bytesRead))
                        {
                            if (bytesRaw[pos] != 10)
                            {
                                if (bytesRaw[pos] == 44)
                                {
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
                                catch
                                {
                                }
                            }

                            pos++;
                        }

                        Array.Reverse(bytesCCC, 0, bytesCCC.Length);
                        Capture("CCC", bytesCCC);
                    }

                    Capture("Raw", bytesRaw);

                    //Console.WriteLine("SourceSocket " + SLocalIPEndPoint.Address + ":" + SLocalIPEndPoint.Port + " <---> " + SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port);
                    //Console.WriteLine("DestinationSocket " + DLocalIPEndPoint.Address + ":" + DLocalIPEndPoint.Port + " <---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port);

                    if (!CCC)
                    {
                        Log("Status", TimeStamp, SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port + " ---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port +" " + String.Format("{0:000000}", bytesRead) + " Byte(s)", System.Text.Encoding.ASCII.GetString(bytesRaw));
                    }
                    else
                    {
                        Log("Status", TimeStamp, SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port + " ---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port + " (" + TimeStamp.ToString("yyyy-MM-dd_HH:mm:ss.fff") + ") " + String.Format("{0:000000}", bytesRead) + " Byte(s)", null);
                    }

                    stringTimeStamp = SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port + " ---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port + " (" + TimeStamp.ToString("yyyy-MM-dd_HH:mm:ss.fff") + ") " + String.Format("{0:000000}", bytesRead) + " Byte(s)" + Convert.ToChar(13) + Convert.ToChar(10) + "------------------------------------------------------------------------------------" + Convert.ToChar(13) + Convert.ToChar(10) + System.Text.Encoding.ASCII.GetString(bytesRaw) + Convert.ToChar(13) + Convert.ToChar(10) + Convert.ToChar(13) + Convert.ToChar(10) + Convert.ToChar(13) + Convert.ToChar(10);

                    MessageNotSent = stringTimeStamp;

                    if (CCC)
                    {
                        state.DestinationSocket.Send(bytesCCC, bytesCCC.Length, SocketFlags.None);
                    }
                    else
                    {
                        state.DestinationSocket.Send(bytesRaw, bytesRaw.Length, SocketFlags.None);
                    }

                    bytesTimeStamp = Encoding.ASCII.GetBytes(stringTimeStamp);
                    AppendAllBytes(Directory.GetCurrentDirectory() + "\\_TimeStamp.log", bytesTimeStamp).ConfigureAwait(false);

                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch (Exception E)
            {

                Log("Exception", TimeStamp, "OnDataReceive", E.ToString());

                if (MessageNotSent != null)
                {
                    Log("Exception", TimeStamp, "OnDataReceive:MessageNotSent:String", MessageNotSent);
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

        public static void Log(String File, DateTime TimeStamp, String Header, String Message)
        {
            String CR = "" + Convert.ToChar(13) + Convert.ToChar(10);
            String Detail_String = null;
            Byte[] Detail_Bytes = new Byte[0];

            if (Header + Message != "")
            {
                Detail_String += TimeStamp.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + Header;
                Detail_String += CR;

                if (Message != null)
                {
                    Detail_String = CR + Detail_String;
                    Detail_String += "----------------------------------------------------------------------------------";
                    Detail_String += CR;
                    Detail_String += Message;
                    Detail_String += CR;
                    Detail_String += CR;
                }

                Detail_Bytes = Encoding.ASCII.GetBytes(Detail_String);

                AppendAllBytes(Directory.GetCurrentDirectory() + "\\LOG_" + File + ".txt", Detail_Bytes).ConfigureAwait(false);

                Console.Write(Detail_String);
            }
        }

        public static void Capture(String File, Byte[] Raw)
        {
            AppendAllBytes(Directory.GetCurrentDirectory() + "\\TCP_" + File + ".txt", Raw).ConfigureAwait(false);
        }
    }
}