using Core;
using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace CCC
{
    public class CCC : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static ExeConfigurationFileMap Settings_File;
        public static Configuration Settings_Data;

        public CCC()
        {
            Name = "CCC";
            Description = "This is the CCC extension.";

            String XML = Directory.GetCurrentDirectory() + @"\" + "Extension" + @"\" + this.GetType().Namespace + ".xml";
            Settings_File = new ExeConfigurationFileMap { ExeConfigFilename = XML };
            Settings_Data = ConfigurationManager.OpenMappedExeConfiguration(Settings_File, ConfigurationUserLevel.None);

            Priority = Byte.Parse(Settings_Data.AppSettings.Settings["Priority"].Value);
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            Byte[] Mutant = new Byte[0];

            int Index = 0;

            for (int Counter = Packet.Length; --Counter >= 0;)
            {

                if (Packet[Index] != 10)
                {
                    switch (Packet[Index])
                    {
                        case 44:
                            Mutant = Append.Byte(Mutant, 32);
                            break;
                        case 3:
                            Mutant = Append.Byte(Mutant, 3);
                            Mutant = Append.Byte(Mutant, 13);
                            Mutant = Append.Byte(Mutant, 10);
                            try
                            {
                                if (Packet[Index + 1] != 2)
                                    Packet[Index + 1] = 10;
                            }
                            catch (Exception E)
                            {
                            }
                            break;
                        default:
                            Mutant = Append.Byte(Mutant, Packet[Index]);
                            break;
                    }
                }

                Index = Packet.Length - Counter;
            }

            Array.Reverse(Mutant, 0, Mutant.Length);
            Packet = Mutant;

            return Mutant.Length;
        }
    }
}
