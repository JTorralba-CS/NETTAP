using System.Net;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Text;
using Core;

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
            String Path = Source.Address.ToString() + "_" + Source.Port + @"\";

            Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString() + " <" + this.Name + " Bytes>", Packet);

            String Output_String = "";

            Byte[] Output_Bytes = new byte[0];

            String Input_String = Encoding.Default.GetString(Packet).ToUpper();
            //Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString() + " <" + this.Name + " String>", Input_String);

            //Input_String = Regex.Replace(Input_String, @"\r|\n", " ");
            Input_String = Regex.Replace(Input_String, @"\r", " ");
            Input_String = Regex.Replace(Input_String, @"\n", " ");
            //Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString() + " <" + this.Name + " Replace>", Input_String);

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
                            String Fixed_String = "";

                            String RecordX = Record + String.Concat(Enumerable.Repeat(" ", 1024));

                            String Link_Status = RecordX.Substring(0, 1);

                            if (Link_Status == "1" || Link_Status == "2")
                            {
                                Fixed_String = "";

                                String Position = RecordX.Substring(1, 3);

                                //if (Position.Substring(0, 3) != "000")
                                //{
                                    Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString() + " <" + this.Name + " Record>", Record);

                                    String Class_Of_Service = RecordX.Substring(20, 4);

                                    String ALI_Provider_ID = RecordX.Substring(231, 5);

                                    String ALI_Date = RecordX.Substring(25, 11);

                                    String Callback_Number = RecordX.Substring(5, 14);

                                    String Customer_Name = RecordX.Substring(37, 28);

                                    String House_Number = RecordX.Substring(72, 6);
                                    String Direction = RecordX.Substring(98, 2);
                                    String Street = RecordX.Substring(101, 22);
                                    String Street_Line2 = RecordX.Substring(124, 20);

                                    String Address = String.Concat(House_Number.Trim(), " ", Direction.Trim(), " ", Street, Street_Line2.Trim()).Trim().Replace("  ", " ") + String.Concat(Enumerable.Repeat(" ", 50));
                                    Address = Address.Substring(0, 50);
                                    Street_Line2 = String.Concat(Enumerable.Repeat(" ", 20)); ;

                                    String City = RecordX.Substring(180, 28);
                                    String State = RecordX.Substring(177, 2);

                                    String Latitude = RecordX.Substring(237, 10);
                                    String Longitude = RecordX.Substring(249, 11);

                                    String Confidence_Meters = RecordX.Substring(260, 7).Trim() + String.Concat(Enumerable.Repeat(" ", 7));
                                    Confidence_Meters = Confidence_Meters.Substring(0, 7);

                                    if (Class_Of_Service.Trim() == "VOIP")
                                    {
                                        Street_Line2 = RecordX.Substring(145, 22);
                                    }
                                    else
                                    {
                                    }

                                    if (String.Concat(ALI_Date, Class_Of_Service, ALI_Provider_ID, Callback_Number).Trim().Length != 0)
                                    {
                                        Fixed_String = String.Concat(Link_Status, Position, Class_Of_Service, ALI_Provider_ID, ALI_Date, Callback_Number, Customer_Name, Address, Street_Line2, City, State, Latitude, Longitude, Confidence_Meters);
                                    }

                                    //Console.WriteLine();
                                    Console.WriteLine("Link_Status          = " + Link_Status + " <" + Link_Status.Length.ToString() + ">");
                                    Console.WriteLine("Position             = " + Position + " <" + Position.Length.ToString() + ">");
                                    Console.WriteLine("Class_Of_Service     = " + Class_Of_Service + " <" + Class_Of_Service.Length.ToString() + ">");
                                    Console.WriteLine("ALI_Provider_ID      = " + ALI_Provider_ID + " <" + ALI_Provider_ID.Length.ToString() + ">");
                                    Console.WriteLine("ALI_Date             = " + ALI_Date + " <" + ALI_Date.Length.ToString() + ">");
                                    Console.WriteLine("Callback_Number      = " + Callback_Number + "<" + Callback_Number.Length.ToString() + ">");
                                    Console.WriteLine("Customer_Name        = " + Customer_Name + " <" + Customer_Name.Length.ToString() + ">");
                                    Console.WriteLine("Street_Line2         = " + Street_Line2 + " <" + Street_Line2.Length.ToString() + ">");
                                    Console.WriteLine("Address              = " + Address + " <" + Address.Length.ToString() + ">");
                                    Console.WriteLine("City                 = " + City + " <" + City.Length.ToString() + ">");
                                    Console.WriteLine("State                = " + State + " <" + State.Length.ToString() + ">");
                                    Console.WriteLine("Latitude             = " + Latitude + " <" + Latitude.Length.ToString() + ">");
                                    Console.WriteLine("Longitude            = " + Longitude + " <" + Longitude.Length.ToString() + ">");
                                    Console.WriteLine("Confidence_Meters    = " + Confidence_Meters + "<" + Confidence_Meters.Length.ToString() + ">");
                                    Console.WriteLine();
                                //}
                            }
                            else if (Link_Status == "H" || Link_Status == "E" || Link_Status == "\u0006")
                            {
                                Fixed_String = RecordX.Trim();
                            }

                            if (Fixed_String.Length != 0)
                            {
                                Fixed_String = String.Concat("\u0002", Fixed_String, "\u0003");
                                //Log.File(Path, Source.Address + ":" + Source.Port.ToString() + " ---> " + Destination.Address + ":" + Destination.Port.ToString() + " <" + this.Name + " Fixed_String>", Fixed_String);

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