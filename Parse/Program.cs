using System.Text;
using System.Text.RegularExpressions;

namespace Parse
{
    internal class Program
    {
        static void Main(string[] args)
        {
            String Output_String = "";

            Byte[] Output_Bytes = new byte[0];

            String Input_String = "\u00022517\r(719) 777-1234 RESD 04/10 14:48\rSMITH, JOHN A.              \r      4001       P#719-777-1234\r   WHITTIER DR           \r                    \r                      261 00220\rCO COLORADO SPRINGS            \r                  TEL=CTLQ \r                               \rPSAP=CSPD--COLO SPGS      \rCOLO SPGSPD      \r\nCOLO SPGS FD      \r\nCOLSPGS FD  \u0003\u0002";
            
            //String Input_String = "\u00022517\r(719) 888-1234 BUSN 04/10 14:47\rSCHOOLS-PUBLIC              \r      5250       P#719-888-1234\r   FARTHING DR           \r                    \r                      262 00220\rCO COLORADO SPRINGS            \r                  TEL=CTLQ \r                               \rPSAP=CSPD--COLO SPGS      \rCOLO SPGSPD      \r\nCOLO SPGS FD      \r\nCOLSPGS FD  \u0003\u007f";
            
            //String Input_String = "\u00022128\r(719) 999-1234 VOIP 04/10 15:15\rCO SPRINGS D11 711 E SAN RAF\r      5240       P#719-211-0083\r   GEIGER BLVD           \r                    \rFOTC-MAIN, LOWERLEVE  261 05025\rCO COLORADO SPRINGS            \r                  TEL=CTLQV\r+38.844566  -104.731185      0 \rPSAP=CO SPRINGS VOIP\r\rVOIP 911 CALL\r\nVERIFY CALLERS LOCATION\r\nVERIFY EMS     \u0003f";

            //String Input_String = "\u00022000\r(719) 111-1234 WPH2 04/10 15:36\rAT&T Mobility               \r      2886       P#719-555-9152\r   SOUTH CIRCLE DIRVE - S\rW Sector            \rCALLBK=(719)329-4319  261 00311\rCO COLORADO SPRINGS            \r                  TEL=ATTMO\r+38.794999  -104.807339      8 \rPSAP=CSPD--COLORADO SPRINGS WIRELESS\rVERIFY PD\r\nVERIFY FD\r\nVERIFY EMS         \u0003a";
            //String Input_String = "\u00022123\r(719) 222-1234 WPH2 04/10 15:35\rT-MOBILE                    \r      6760       P#719-555-9184\r   CORPORATE DRIVE - NE S\rector               \rCALLBK=(719)388-3178  261 00311\rCO COLORADO SPRINGS            \r                  TEL=ATTMO\r+38.935654  -104.812596      8 \rPSAP=CSPD--COLORADO SPRINGS WIRELESS\rVERIFY PD\r\nVERIFY FD\r\nVERIFY EMS         \u0003b";
            //String Input_String = "\u00022137\r(719) 333-1234 WPH1 04/10 15:44\rVERIZON                     \r      3155       P#719-555-9068\r   NORTH CASCADE AVENUE -\r W Sector           \rCALLBK=(719)352-4342  261 00311\rCO COLORADO SPRINGS            \r                  TEL=ATTMO\r+38.877342  -104.821869      0 \rPSAP=CSPD--COLORADO SPRINGS WIRELESS\rVERIFY PD\r\nVERIFY FD\r\nVERIFY EMS         \u0003c";

            //Input_String = Regex.Replace(Input_String, @"\r|\n", "*");
            //Console.WriteLine(Input_String);
            //Console.WriteLine();

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

                                if (Position.Substring(0, 2) != "000")
                                {
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

                                    Console.WriteLine();
                                    Console.WriteLine("Link_Status          = " + Link_Status + " <" + Link_Status.Length.ToString() + ">");
                                    Console.WriteLine("Position             = " + Position + " <" + Position.Length.ToString() + ">");
                                    Console.WriteLine("Class_Of_Service     = " + Class_Of_Service + " <" + Class_Of_Service.Length.ToString() + ">");
                                    Console.WriteLine("ALI_Provider_ID      = " + ALI_Provider_ID + " <" + ALI_Provider_ID.Length.ToString() + ">");
                                    Console.WriteLine("ALI_Date             = " + ALI_Date + " <" + ALI_Date.Length.ToString() + ">");
                                    Console.WriteLine("Callback_Number      = " + Callback_Number +  "<" + Callback_Number.Length.ToString() + ">");
                                    Console.WriteLine("Customer_Name        = " + Customer_Name + " <" + Customer_Name.Length.ToString() + ">");
                                    Console.WriteLine("Street_Line2         = " + Street_Line2 + " <" + Street_Line2.Length.ToString() + ">");
                                    Console.WriteLine("Address              = " + Address + " <" + Address.Length.ToString() + ">");
                                    Console.WriteLine("City                 = " + City + " <" + City.Length.ToString() + ">");
                                    Console.WriteLine("State                = " + State + " <" + State.Length.ToString() + ">");
                                    Console.WriteLine("Latitude             = " + Latitude + " <" + Latitude.Length.ToString() + ">");
                                    Console.WriteLine("Longitude            = " + Longitude + " <" + Longitude.Length.ToString() + ">");
                                    Console.WriteLine("Confidence_Meters    = " + Confidence_Meters +  "<" + Confidence_Meters.Length.ToString() + ">");
                                    Console.WriteLine();
                                }
                            }
                            else if (Link_Status == "H" || Link_Status == "E" || Link_Status == "\u0006")
                            {
                                Fixed_String = RecordX.Trim();
                            }

                            if (Fixed_String.Length != 0)
                            {
                                Fixed_String = String.Concat("\u0002", Fixed_String, "\u0003");
                                //Console.WriteLine(Fixed_String);
                                Console.WriteLine(Fixed_String + " <" + Fixed_String.Length.ToString() + ">");

                                Output_String = Output_String + Fixed_String;
                            }
                        }
                    }
                }
            }

            Output_Bytes = Encoding.Default.GetBytes(Output_String);

            //using (StreamWriter Writer = new StreamWriter("DEBUG.txt"))
            //{
            //    Writer.WriteLine(Output_String);
            //    //Writer.WriteLine(String.Join(" ", Output_Bytes));
            //};

            //String ReadText = File.ReadAllText("DEBUG.txt");
            //Console.WriteLine(ReadText);
        }
    }
}