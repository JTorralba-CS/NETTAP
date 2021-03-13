using Core;
using System;
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

        public static Byte[] FindThis;

        public static SmtpClient SMTPClient;

        public HMM()
        {
            Name = "HMM";
            Description = "This is the HMM extension.";
            Priority = 0; //24

            FindThis = Encoding.ASCII.GetBytes("719-123-1234");

            SMTPClient = new SmtpClient("Host")
            {
                Port = int.Parse("Port"),
                Credentials = new NetworkCredential("Username", "Password"),
                EnableSsl = true,
            };
        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\HMM";

            if (Find.Byte(ref Packet, ref FindThis) >= 0)
            {
                Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Packet);

                SMTPClient.Send("Sender@FROM.com", "Recipient@TO.com", "Anomaly Detected", Log.Detail(Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString(), Encoding.ASCII.GetString(Packet)).ToString());
            }

            return 0;
        }
    }
}