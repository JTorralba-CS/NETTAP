using System.Net;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Text;

namespace PAR
{
    public class PAR : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public static ExeConfigurationFileMap Settings_File;
        public static Configuration Settings_Data;

        public PAR()
        {
            Name = "PAR";
            Description = "This is the PAR extension.";

            String XML = Directory.GetCurrentDirectory() + @"\" + "Extension" + @"\" + this.GetType().Namespace + ".xml";
            Settings_File = new ExeConfigurationFileMap { ExeConfigFilename = XML };
            Settings_Data = ConfigurationManager.OpenMappedExeConfiguration(Settings_File, ConfigurationUserLevel.None);

            Priority = Byte.Parse(Settings_Data.AppSettings.Settings["Priority"].Value);

        }

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref byte[] Packet)
        {
            String Output_String = "";

            Byte[] Output_Bytes = new byte[0];

            String Input_String = Encoding.Default.GetString(Packet).ToUpper();

            //Console.WriteLine();
            //Console.WriteLine("Input: " + Input_String);
            //Console.WriteLine();

            //Input_String = Regex.Replace(Input_String, @"\r|\n", " ");

            String[] Strip_STX = Input_String.Split("\u0002");

            foreach (String Input_String2 in Strip_STX)
            {
                if (Input_String2.Trim().Length != 0)
                {
                    String[] Strip_ETX = Input_String2.Split("\u0003");

                    foreach (String Record in Strip_ETX)
                    {

                        if (Record.Trim().Length != 0)
                        {
                            //Console.WriteLine("Record: " + Record + " <" + Record.Length.ToString() + ">");

                            String Fixed_String = "";

                            String RecordX = Record + String.Concat(Enumerable.Repeat(" ", 1024));

                            String Type = RecordX.Substring(0, 1);

                            //Console.WriteLine("Type  : " + Type);

                            if (Type == "1" || Type == "2")
                            {
                                Fixed_String = "";

                                String Station = RecordX.Substring(1, 3);

                                if (Station.Substring(0, 2) != "00")
                                {
                                    String Date_Time = RecordX.Substring(26, 11);
                                    String Class_Of_Service = RecordX.Substring(21, 5);
                                    String Carrier = RecordX.Substring(239, 5);

                                    String CallBack_Wired = RecordX.Substring(6, 14);
                                    String CallBack_Wireless = RecordX.Substring(158, 14);

                                    String Phone = CallBack_Wired.Replace(" ", "") + String.Concat(Enumerable.Repeat(" ", 14));
                                    Phone = Phone.Substring(0, 14);

                                    String Caller = RecordX.Substring(39, 28);
                                    

                                    String Address_Number = RecordX.Substring(74, 12);
                                    String Address_Street_Prefix = RecordX.Substring(102, 3);
                                    String Address_Street = RecordX.Substring(105, 22);
                                    String Address_Misc = RecordX.Substring(129, 20);
                                    String Address = String.Concat(Address_Number.Trim(), " ", Address_Street_Prefix.Trim(), " ", Address_Street, Address_Misc.Trim()).Trim().Replace("  ", " ") + String.Concat(Enumerable.Repeat(" ", 60));
                                    Address = Address.Substring(0, 60);
                                    Address_Misc = String.Concat(Enumerable.Repeat(" ", 22)); ;

                                    String City = RecordX.Substring(187, 28);
                                    String State = RecordX.Substring(184, 2);

                                    String Latitude = RecordX.Substring(246, 12);
                                    String Longitude = RecordX.Substring(258, 12);

                                    String Confidence = RecordX.Substring(269, 7).Trim() + String.Concat(Enumerable.Repeat(" ", 7));
                                    Confidence = Confidence.Substring(0, 7);

                                    if (Class_Of_Service.Trim() == "WPH2")
                                    {
                                        //Caller = String.Concat("WIRELESS CALLER (", Carrier.Trim(), ")") + String.Concat(Enumerable.Repeat(" ", 50));
                                        //Caller = Caller.Substring(0, 28);

                                        Phone = CallBack_Wireless.Replace(" ", "") + String.Concat(Enumerable.Repeat(" ", 14));
                                        Phone = Phone.Substring(0, 14);
                                    }
                                    else
                                    {
                                        CallBack_Wireless = String.Concat(Enumerable.Repeat(" ", 14));
                                    }

                                    if (Class_Of_Service.Trim() == "VOIP")
                                    {
                                        Address_Misc = RecordX.Substring(151, 22);
                                    }
                                    else
                                    {
                                    }

                                    if (String.Concat(Date_Time, Class_Of_Service, Carrier, Phone).Trim().Length != 0)
                                    {
                                        Fixed_String = String.Concat(Station, Date_Time, Class_Of_Service, Carrier, Phone, Caller, Address_Misc, Address, City, State, Latitude, Longitude, Confidence);
                                    }

                                    Console.WriteLine();
                                    Console.WriteLine("Station          =" + Station + "<" + Station.Length.ToString() + ">");
                                    Console.WriteLine("Date_Time        =" + Date_Time + "<" + Date_Time.Length.ToString() + ">");
                                    Console.WriteLine("Class_Of_Service =" + Class_Of_Service + "<" + Class_Of_Service.Length.ToString() + ">");
                                    Console.WriteLine("Carrier          =" + Carrier + "<" + Carrier.Length.ToString() + ">");
                                    Console.WriteLine("Phone            =" + Phone + "<" + Phone.Length.ToString() + ">");
                                    Console.WriteLine("Caller           =" + Caller + "<" + Caller.Length.ToString() + ">");
                                    Console.WriteLine("Address_Misc     =" + Address_Misc + "<" + Address_Misc.Length.ToString() + ">");
                                    Console.WriteLine("Address          =" + Address + "<" + Address.Length.ToString() + ">");
                                    Console.WriteLine("City             =" + City + "<" + City.Length.ToString() + ">");
                                    Console.WriteLine("State            =" + State + "<" + State.Length.ToString() + ">");
                                    Console.WriteLine("Latitude         =" + Latitude + "<" + Latitude.Length.ToString() + ">");
                                    Console.WriteLine("Longitude        =" + Longitude + "<" + Longitude.Length.ToString() + ">");
                                    Console.WriteLine("Confidence       =" + Confidence + "<" + Confidence.Length.ToString() + ">");
                                    Console.WriteLine();
                                }
                            }
                            else if (Type == "H" || Type == "E" || Type == "\u0006")
                            {
                                Fixed_String = RecordX.Trim();
                            }

                            if (Fixed_String.Length != 0)
                            {
                                Fixed_String = String.Concat("\u0002", Fixed_String, "\u0003");
                                //Console.WriteLine(Fixed_String);
                                //Console.WriteLine(Fixed_String + " <" + Fixed_String.Length.ToString() + ">");

                                Output_String = Output_String + Fixed_String;
                            }
                        }
                    }
                }
            }

            Output_Bytes = Encoding.Default.GetBytes(Output_String);
            Packet = Output_Bytes;

            //using (StreamWriter Writer = new StreamWriter("DEBUG.txt"))
            //{
            //    Writer.WriteLine(Output_String);
            //    //Writer.WriteLine(String.Join(" ", Output_Bytes));
            //};

            //String ReadText = File.ReadAllText("DEBUG.txt");
            //Console.WriteLine(ReadText);

            return Output_Bytes.Length;
        }
    }
}