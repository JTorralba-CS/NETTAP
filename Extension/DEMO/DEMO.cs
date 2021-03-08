using Core;
using System;
using System.Net;

namespace DEMO
{
    public class DEMO : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }
        public Byte[] Mutant { get; set; }
        public int Mutant_Size { get; set; }

        public DEMO()
        {
            Name = "DEMO";
            Description = "This is the DEMO extension.";
            Priority = 20;
        }

        public int Execute(IPEndPoint Source, IPEndPoint Destination, Byte[] Packet, int Packet_Size)
        {
            Log.Terminal(Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet, Packet_Size);

            return 0;
        }
    }
}
