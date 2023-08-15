using Core;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace ERA
{
    public class ERA : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static ExeConfigurationFileMap Settings_File;
        public static Configuration Settings_Data;

        public static Byte[] FindThis;

        public ERA()
        {
            Name = "ERA";
            Description = "This is the ERA extension.";

            String XML = Directory.GetCurrentDirectory() + @"\" + "Extension" + @"\" + this.GetType().Namespace + ".xml";
            Settings_File = new ExeConfigurationFileMap { ExeConfigFilename = XML };
            Settings_Data = ConfigurationManager.OpenMappedExeConfiguration(Settings_File, ConfigurationUserLevel.None);

            Priority = Byte.Parse(Settings_Data.AppSettings.Settings["Priority"].Value);
            FindThis = Encoding.ASCII.GetBytes(Settings_Data.AppSettings.Settings["FindThis"].Value);
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\ERA";

            if (FindThis.Length !> 0 && Find.Byte(ref Packet, ref FindThis) >= 0)
            {
                Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString() + " <" + this.Name + ">", Packet);
            }

            return 0;
        }
    }
}
