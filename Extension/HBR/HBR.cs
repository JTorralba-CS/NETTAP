using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using Core;

namespace HBR
{
    public class HBR : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static ExeConfigurationFileMap Settings_File;
        public static Configuration Settings_Data;

        public static Byte[] FindThis;
        public static Byte[] HBA;

        public HBR()
        {
            Name = "HBR";
            Description = "This is the HBR extension.";

            String XML = Directory.GetCurrentDirectory() + @"\" + "Extension" + @"\" + this.GetType().Namespace + ".xml";
            Settings_File = new ExeConfigurationFileMap { ExeConfigFilename = XML };
            Settings_Data = ConfigurationManager.OpenMappedExeConfiguration(Settings_File, ConfigurationUserLevel.None);

            Priority = Byte.Parse(Settings_Data.AppSettings.Settings["Priority"].Value);
            FindThis = Encoding.ASCII.GetBytes(Settings_Data.AppSettings.Settings["FindThis"].Value);
            HBA = Encoding.ASCII.GetBytes(Settings_Data.AppSettings.Settings["HBA"].Value);
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\HBR";

            if (Find.Byte(ref Packet, ref FindThis) >= 0)
            {
                Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString() + " " + Encoding.ASCII.GetString(Packet));
                Packet = HBA;
                return 1;
            }

            return 0;
        }
    }
}
