using System;
using System.Net;
using System.Text;
using Core;

namespace ACK
{
    public class HBR : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static Byte[] FindThis;
        public static Byte[] HBA;

        public HBR()
        {
            Name = "HBR";
            Description = "This is the HBR extension.";
            Priority = 30;

            FindThis = Encoding.ASCII.GetBytes(Convert.ToChar(2).ToString() + Convert.ToChar(72).ToString() + Convert.ToChar(3).ToString());
            HBA = Encoding.ASCII.GetBytes(Convert.ToChar(02).ToString() + Convert.ToChar(06).ToString() + Convert.ToChar(03).ToString());
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\HBR";

            if (Find.Byte(ref Packet, ref FindThis) >= 0)
            {
                Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet);
                Packet = HBA;
                return 1;
            }

            return 0;
        }
    }
}
