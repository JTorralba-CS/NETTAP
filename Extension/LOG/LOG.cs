using Core;
using System;
using System.Net;

namespace LOG
{
    public class LOG : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }
        public Byte[] Mutant { get; set; }
        public int Mutant_Size { get; set; }

        public LOG()
        {
            Name = "LOG";
            Description = "This is the LOG extension.";
            Priority = 20;
            Mutant = new Byte[0];
            Mutant_Size = 0;
        }

        public int Execute(IPEndPoint Source, IPEndPoint Destination, Byte[] Packet, int Packet_Size)
        {
            Log.Terminal(Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet, Packet_Size);

            return 0;
        }
    }
}
