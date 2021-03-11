using System;
using System.Net;

namespace Interface
{
    public interface Extension
    {
        String Name { get; }
        String Description { get; }
        Byte Priority { get; set; }

        int Execute(IPEndPoint Source, IPEndPoint Destination, ref Byte[] Packet, ref int Packet_Size);
    }
}
