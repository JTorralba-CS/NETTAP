using Core;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace HMM
{
    public class HMM : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static ExeConfigurationFileMap Settings_File;
        public static Configuration Settings_Data;

        public static Byte[] FindThis;

        public static SmtpClient SMTP_Client;
        public static String SMTP_Sender;
        public static String SMTP_Recipient;

        public HMM()
        {
            Name = "HMM";
            Description = "This is the HMM extension.";

            String XML = Directory.GetCurrentDirectory() + @"\" + "Extension" + @"\" + this.GetType().Namespace + ".xml";
            Settings_File = new ExeConfigurationFileMap { ExeConfigFilename = XML };
            Settings_Data = ConfigurationManager.OpenMappedExeConfiguration(Settings_File, ConfigurationUserLevel.None);

            Priority = Byte.Parse(Settings_Data.AppSettings.Settings["Priority"].Value);
            FindThis = Encoding.ASCII.GetBytes(Settings_Data.AppSettings.Settings["FindThis"].Value);

            if (Priority != 0)
            {
                SMTP_Client = new SmtpClient(Settings_Data.AppSettings.Settings["SMTP_Host"].Value)
                {
                    Port = int.Parse(Settings_Data.AppSettings.Settings["SMTP_Port"].Value),
                    Credentials = new NetworkCredential(Settings_Data.AppSettings.Settings["SMTP_Username"].Value, Settings_Data.AppSettings.Settings["SMTP_Password"].Value),
                    EnableSsl = true,
                };
                SMTP_Sender = Settings_Data.AppSettings.Settings["SMTP_Sender"].Value;
                SMTP_Recipient = Settings_Data.AppSettings.Settings["SMTP_Recipient"].Value;
            }

            //SMTP_Client.Send(SMTP_Sender, SMTP_Recipient, "HMM Test", "This is only a test.");
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\HMM";

            if (Find.Byte(ref Packet, ref FindThis) >= 0)
            {
                Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet);

                SMTP_Client.Send(SMTP_Sender, SMTP_Recipient, "Anomaly Detected", Log.Detail(Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Encoding.ASCII.GetString(Packet)).ToString());
            }

            return 0;
        }
    }
}