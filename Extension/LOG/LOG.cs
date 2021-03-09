using Core;
using System;
using System.IO;
using System.Net;

namespace LOG
{
    public class LOG : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public LOG()
        {
            Name = "LOG";
            Description = "This is the LOG extension.";
            Priority = 20;
        }

        public int Execute(IPEndPoint Source, IPEndPoint Destination, Byte[] Packet, int Packet_Size)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\";
            Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet, Packet_Size);

            Log.Terminal(Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet, Packet_Size);

            return 0;
        }
    }
}
