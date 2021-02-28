﻿using Standard;
using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public static class Listen
    {
        public static void Initialize(String Destination_IP, int Destination_Port, int Listen_Port)
        {
            try
            {
                for (int I = Network.IP(4).Length; --I >= 0;)
                {
                    new Tap().Start(Network.IP(4)[I], Listen_Port, new IPEndPoint(IPAddress.Parse(Destination_IP), Destination_Port));
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
                Tap Destination = new Tap();
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

        private static void OnDataReceive(IAsyncResult Result)
        {
            Socket_State State = (Socket_State)Result.AsyncState;

            try
            {
                IPEndPoint Source_Remote_IPEndPoint = State.Socket_Source.RemoteEndPoint as IPEndPoint;
                IPEndPoint Destination_Remote_IPEndPoint = State.Socket_Destination.RemoteEndPoint as IPEndPoint;

                if (State.Socket_Source.EndReceive(Result) != 0)
                {
                    State.Socket_Destination.Send(State.Buffer, State.Buffer.Length, SocketFlags.None);

                    Log.Terminal(Source_Remote_IPEndPoint.Address + ":" + Source_Remote_IPEndPoint.Port.ToString() + " ---> " + Destination_Remote_IPEndPoint.Address + ":" + Destination_Remote_IPEndPoint.Port.ToString(), State.Buffer);

                    State.Socket_Source.BeginReceive(State.Buffer, 0, State.Buffer.Length, 0, OnDataReceive, State);
                }
            }
            catch (Exception E)
            {
                Log.Terminal("OnDataReceive", E.Message);

                if (State.Buffer.Length != 0)
                {
                    Log.Terminal("OnDataReceive: (Packet Loss)", State.Buffer);
                }

                State.Socket_Destination.Close();
                State.Socket_Source.Close();
            }
        }
    }
}