using System;
using System.Net;

namespace Interface
{
    public interface Extension
    {
        String Name { get; }
        String Description { get; }
        Byte Priority { get; set; }

        int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet);
    }
}
