using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public static class Syntax
    {
        public static void Check(String[] Arguments, Type Program)
        {
            String Destination_IP = null;
            int Destination_Port = 0;
            int Listen_Port = 0;

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
                    Listen_Port = Destination_Port;
                }
                else if (Arguments.Length == 1)
                {
                    Destination_IP = Arguments[0];
                    Destination_Port = 80;
                    Listen_Port = Destination_Port;
                }
                else
                {
                    Console.WriteLine("NetTap [Destination_IP] [Destination_Port] [Listen_Port]");
                    Console.WriteLine();

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

                Listen.Initialize(Destination_IP, Destination_Port, Listen_Port, Program);
            }
            catch
            {
                Console.WriteLine();
                Console.WriteLine("NetTap [Destination_IP] [Destination_Port] [Listen_Port]");
                Console.WriteLine();
            }
        }
    }

    public static class Listen
    {
        public static void Initialize(String Destination_IP, int Destination_Port, int Listen_Port, Type Program)
        {
            try
            {
                IEnumerable<Interface.Extension> DLLs = DLLLoadContext.Initialize("Extension", Program);
                IEnumerable<Interface.Extension> Extensions = DLLs.OrderBy(DLL => DLL.Priority);

                for (int I = Network.IP(4).Length; --I >= 0;)
                {
                    new Tap(Extensions).Start(Network.IP(4)[I], Listen_Port, new IPEndPoint(IPAddress.Parse(Destination_IP), Destination_Port));
                }
            }
            catch (Exception E)
            {
                Log.Terminal("Initialize()", E.Message);
            }
        }
    }

    public class Tap
    {
        private readonly Socket _Main_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public IEnumerable<Interface.Extension> Extensions;

        public Tap(IEnumerable<Interface.Extension> Extensions)
        {
            this.Extensions = Extensions;
        }


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
                Tap Destination = new Tap(Extensions);
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

        private struct Socket_State
        {
            public Socket Socket_Source;
            public Socket Socket_Destination;
            public Byte[] Buffer;

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

        private void OnDataReceive(IAsyncResult Result)
        {
            Socket_State State = (Socket_State)Result.AsyncState;

            int Packet_Size = 0;

            try
            {
                Packet_Size = State.Socket_Source.EndReceive(Result);

                if (Packet_Size != 0)
                {
                    IPEndPoint Source_Remote_IPEndPoint = State.Socket_Source.RemoteEndPoint as IPEndPoint;
                    IPEndPoint Source_Local_IPEndPoint = State.Socket_Source.LocalEndPoint as IPEndPoint;

                    IPEndPoint Destination_Local_IPEndPoint = State.Socket_Destination.LocalEndPoint as IPEndPoint;
                    IPEndPoint Destination_Remote_IPEndPoint = State.Socket_Destination.RemoteEndPoint as IPEndPoint;

                    //Interface.Extension Extension = Extensions.FirstOrDefault(Extension => Extension.Name == "DEBUG");

                    foreach (Interface.Extension Extension in Extensions.Where(Extension => Extension.Priority >= 10 && Extension.Priority < 20))
                    {
                        Extension.Execute(Source_Remote_IPEndPoint, Destination_Remote_IPEndPoint, State.Buffer, Packet_Size);
                    }

                    State.Socket_Destination.Send(State.Buffer, Packet_Size, SocketFlags.None);

                    foreach (Interface.Extension Extension in Extensions.Where(Extension => Extension.Priority >= 20 && Extension.Priority < 30))
                    {
                        Extension.Execute(Source_Remote_IPEndPoint, Destination_Remote_IPEndPoint, State.Buffer, Packet_Size);
                    }

                    State.Socket_Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
                }
            }
            catch (Exception E)
            {
                Log.Terminal("OnDataReceive", E.Message);

                if (Packet_Size != 0)
                {
                    Log.Terminal("OnDataReceive: (Packet Loss)", State.Buffer, Packet_Size);
                }

                State.Socket_Destination.Close();
                State.Socket_Source.Close();
            }
        }
    }

}
