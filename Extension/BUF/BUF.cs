using Core;
using System;
using System.Net;
using System.Text;

namespace BUF
{
    public class BUF : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static int Max;

        public BUF()
        {
            Name = "BUF";
            Description = "This is the BUF extension.";
            Priority = 22;

            Max = 0;
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\BUF";

            if (Packet.Length > Max)
            {
                Max = Packet.Length;
                Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet);
            }

            return 0;
        }
    }
}
