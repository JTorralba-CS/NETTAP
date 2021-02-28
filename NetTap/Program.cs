using Standard;
using System;
using System.Net;
using System.Net.Sockets;

namespace NetTap
{
    class Program
    {
        private readonly Socket _Main_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Start(String Listen_IP, int Listen_Port, IPEndPoint Remote)
        {
            IPEndPoint Local = null;

            Local = new IPEndPoint(IPAddress.Parse(Listen_IP), Listen_Port);

            _Main_Socket.Bind(Local);
            _Main_Socket.Listen(0);

            Log.Terminal(Local.Address.ToString() + ":" + Local.Port.ToString());

            while (true)
            {
                Socket Source = _Main_Socket.Accept();
                Program Destination = new Program();
                Socket_State State = new Socket_State(Source, Destination._Main_Socket);

                try
                {
                    Destination.Connect(Remote, Source);
                }
                catch (Exception E)
                {
                    Log.Terminal("Start: Destination.Connect()", E.Message);
                }

                Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
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
                Buffer = new Byte[1024];
            }
        }

        private void Connect(EndPoint Remote_Endpoint, Socket Destination)
        {
            Socket_State State = new Socket_State(_Main_Socket, Destination);

            _Main_Socket.Connect(Remote_Endpoint);
            _Main_Socket.BeginReceive(State.Buffer, 0, State.Buffer.Length, SocketFlags.None, OnDataReceive, State);
        }

        private static void OnDataReceive(IAsyncResult Result)
        {
            Byte[] Packet_Bytes = null;

            Socket_State State = (Socket_State)Result.AsyncState;

            try
            {
                IPEndPoint Source_Local_IPEndPoint = State.Socket_Source.LocalEndPoint as IPEndPoint;
                IPEndPoint Source_Remote_IPEndPoint = State.Socket_Source.RemoteEndPoint as IPEndPoint;

                IPEndPoint Destination_Local_IPEndPoint = State.Socket_Destination.LocalEndPoint as IPEndPoint;
                IPEndPoint Destination_Remote_IPEndPoint = State.Socket_Destination.RemoteEndPoint as IPEndPoint;

                int Packet_Size = State.Socket_Source.EndReceive(Result);

                if (Packet_Size > 0)
                {
                    Byte[] Packet_Raw = new byte[Packet_Size];

                    Buffer.BlockCopy(State.Buffer, 0, Packet_Raw, 0, Packet_Size);

                    Packet_Bytes = Packet_Raw;

                    State.Socket_Destination.Send(Packet_Bytes, Packet_Bytes.Length, SocketFlags.None);

                    Log.Terminal(Source_Remote_IPEndPoint.Address + ":" + Source_Remote_IPEndPoint.Port.ToString() + " ---> " + Destination_Remote_IPEndPoint.Address + ":" + Destination_Remote_IPEndPoint.Port.ToString(), Packet_Bytes);

                    State.Socket_Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
                }
            }
            catch (Exception E)
            {
                Log.Terminal("OnDataReceive", E.Message);

                if (Packet_Bytes != null)
                {
                    Log.Terminal("OnDataReceive: (Packet Loss)", Packet_Bytes);
                }

                State.Socket_Destination.Close();
                State.Socket_Source.Close();
            }
        }

        static void Main(String[] Arguments)
        {
            String Destination_IP = null;
            int Destination_Port = 0;
            int Listen_Port = 35263;

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
                for (int I = Network.IP(4).Length; --I >= 0;)
                {
                    new Program().Start(Network.IP(4)[I], Listen_Port, new IPEndPoint(IPAddress.Parse(Destination_IP), Destination_Port));
                }
            }
            catch (Exception E)
            {
                Log.Terminal("Main: Program().Start()", E.Message);
            }
        }
    }
}
