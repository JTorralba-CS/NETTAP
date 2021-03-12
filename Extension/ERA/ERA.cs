using Core;
using System;
using System.Net;
using System.Text;

namespace ERA
{
    public class ERA : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static Byte[] FindThis;

        public ERA()
        {
            Name = "ERA";
            Description = "This is the ERA extension.";
            Priority = 21;

            FindThis = Encoding.ASCII.GetBytes(Convert.ToChar(2).ToString() + Convert.ToChar(69).ToString());
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\ERA";

            if (Find.Byte(ref Packet, ref FindThis) >= 0)
            {
                Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString() + " " + Encoding.ASCII.GetString(Packet));
            }

            return 0;
        }
    }
}
