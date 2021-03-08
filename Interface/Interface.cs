using System;
using System.Net;

namespace Interface
{
    public interface Extension
    {
        String Name { get; }
        String Description { get; }
        int Priority { get; set; }
        Byte[] Mutant { get; set; }
        int Mutant_Size { get; set; }

        int Execute(IPEndPoint From, IPEndPoint To, Byte[] Packet, int Packet_Size);
    }
}
