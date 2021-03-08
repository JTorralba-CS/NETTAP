using System;
using System.Net;

namespace Interface
{
    public interface Extension
    {
        String Name { get; }
        String Description { get; }
        Byte Priority { get; set; }
        Byte[] Mutant { get; set; }
        int Mutant_Size { get; set; }

        int Execute(IPEndPoint Source, IPEndPoint Destination, Byte[] Packet, int Packet_Size);
    }
}
