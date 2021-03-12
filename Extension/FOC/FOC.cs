using Core;
using System;
using System.Net;
using System.Text;

namespace FOC
{
    public class FOC : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static Byte[] FindThis;

        public FOC()
        {
            Name = "FOC";
            Description = "This is the FOC extension.";
            Priority = 23;

            FindThis = Encoding.ASCII.GetBytes("VOIP");
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\FOC";

            if (Find.Byte(ref Packet, ref FindThis) >= 0)
            {
                Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet);
            }

            return 0;
        }
    }
}
