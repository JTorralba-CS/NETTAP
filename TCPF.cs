using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace TCPF
{
    class TCPF
    {
        public static Boolean CCC = false;

        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public void Start(IPEndPoint local, IPEndPoint remote)
        {
            _mainSocket.Bind(local);
            _mainSocket.Listen(10);

            while (true)
            {
                var source = _mainSocket.Accept();
                var destination = new TCPF();
                var state = new State(source, destination._mainSocket);
                destination.Connect(remote, source);
                source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
            }
        }
        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var state = new State(_mainSocket, destination);
            _mainSocket.Connect(remoteEndpoint);
            _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
        }
        private static void OnDataReceive(IAsyncResult result)
        {
            DateTime TimeStamp = DateTime.Now;

            var state = (State)result.AsyncState;

            IPEndPoint SLocalIPEndPoint = state.SourceSocket.LocalEndPoint as IPEndPoint;
            IPEndPoint SRemoteIPEndPoint = state.SourceSocket.RemoteEndPoint as IPEndPoint;

            IPEndPoint DLocalIPEndPoint = state.DestinationSocket.LocalEndPoint as IPEndPoint;
            IPEndPoint DRemoteIPEndPoint = state.DestinationSocket.RemoteEndPoint as IPEndPoint;
            
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);

                if (bytesRead > 0)
                {
                    var bytesData = new byte[bytesRead];
                    var bytesClean = new byte[0];

                    Buffer.BlockCopy(state.Buffer, 0, bytesData, 0, bytesRead);

                    if (CCC)
                    {
                        int pos = 0;
                        while (pos < (bytesRead))
                        {
                            if (bytesData[pos] != 10)
                            {
                                bytesClean = addByteToArray(bytesClean, bytesData[pos]);
                            }

                            try
                            {
                                if (bytesData[pos + 1] == 3)
                                {
                                    bytesData[pos + 1] = 10;
                                }
                            }
                            catch
                            {
                            }
                            pos++;
                        }

                        //Array.Reverse(bytesClean, 0, bytesClean.Length);
                        AppendAllBytes(GetExecutingDirectoryName() + "\\_TCP.log", bytesClean).ConfigureAwait(false);
                    }
                    else
                    {
                        AppendAllBytes(GetExecutingDirectoryName() + "\\_TCP.log", bytesData).ConfigureAwait(false);
                    }
                    

                    //Console.WriteLine("SourceSocket " + SLocalIPEndPoint.Address + ":" + SLocalIPEndPoint.Port + " <---> " + SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port);
                    //Console.WriteLine("DestinationSocket " + DLocalIPEndPoint.Address + ":" + DLocalIPEndPoint.Port + " <---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port);

                    Console.WriteLine(SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port + " ---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port + " (" + TimeStamp.ToString("yyyy-MM-dd_HH:mm:ss.fff") + ") " + bytesRead.ToString() + " Byte(s)");
                    Console.WriteLine("-----------------------------------------------------------------------------------------");

                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(bytesData));
                    Console.WriteLine("");

                    state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch
            {
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
                Buffer = new byte[8192];
            }
        }
        public static async Task AppendAllBytes(string path, byte[] bytes)
        {
            using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None,bufferSize:4096, useAsync:true))
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
                Console.WriteLine(args[4]);
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
    }
}