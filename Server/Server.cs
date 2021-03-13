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
                Log.File("System\\Exception", "Initialize()", E.Message);
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

            Log.File("System\\Status", Local.Address.ToString() + ":" + Local.Port.ToString());

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
                    Log.File("System\\Exception", "Start: Destination.Connect()", E.Message);
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

            Byte[] Packet = null;
            int Packet_Size = 0;

            try
            {
                Packet_Size = State.Socket_Source.EndReceive(Result);

                if (Packet_Size != 0)
                {
                    Packet = new byte[Packet_Size];
                    Buffer.BlockCopy(State.Buffer, 0, Packet, 0, Packet_Size);

                    IPEndPoint Source_Remote_IPEndPoint = State.Socket_Source.RemoteEndPoint as IPEndPoint;
                    IPEndPoint Source_Local_IPEndPoint = State.Socket_Source.LocalEndPoint as IPEndPoint;

                    IPEndPoint Destination_Local_IPEndPoint = State.Socket_Destination.LocalEndPoint as IPEndPoint;
                    IPEndPoint Destination_Remote_IPEndPoint = State.Socket_Destination.RemoteEndPoint as IPEndPoint;

                    //Interface.Extension Extension = Extensions.FirstOrDefault(Extension => Extension.Name == "DEBUG");

                    foreach (Interface.Extension Extension in Extensions.Where(Extension => Extension.Priority >= 10 && Extension.Priority < 20))
                    {
                        Extension.Execute(ref Source_Remote_IPEndPoint, ref Destination_Remote_IPEndPoint, ref Packet);
                    }

                    State.Socket_Destination.Send(Packet, Packet.Length, SocketFlags.None);

                    foreach (Interface.Extension Extension in Extensions.Where(Extension => Extension.Priority >= 20 && Extension.Priority < 30))
                    {
                        Extension.Execute(ref Source_Remote_IPEndPoint, ref Destination_Remote_IPEndPoint, ref Packet);
                    }

                    foreach (Interface.Extension Extension in Extensions.Where(Extension => Extension.Priority >= 30 && Extension.Priority < 40))
                    {
                        if (Extension.Execute(ref Source_Remote_IPEndPoint, ref Destination_Remote_IPEndPoint, ref Packet) == 1)
                        {
                            try
                            {
                                State.Socket_Source.Send(Packet, Packet.Length, SocketFlags.None);
                                Log.File("System\\" + Extension.Name, Source_Local_IPEndPoint.Address + ":" + Source_Local_IPEndPoint.Port.ToString() + " ---> " + Source_Remote_IPEndPoint.Address + ":" + Source_Remote_IPEndPoint.Port.ToString(), Packet);
                            }
                            catch (Exception E)
                            {
                                Log.File("System\\Exception", "Extension Code Non-Zero", E.Message);
                            }
                        }
                    }

                    State.Socket_Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
                }
            }
            catch (Exception E)
            {
                Log.File("System\\Exception", "OnDataReceive", E.Message);

                if (Packet_Size != 0)
                {
                    Log.File("System\\Exception", "OnDataReceive: (Packet Loss)", Packet);
                }

                State.Socket_Destination.Close();
                State.Socket_Source.Close();
            }
        }
    }

}
