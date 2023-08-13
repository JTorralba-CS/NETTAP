using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Parse
{
    internal class Program
    {
        static void Main(string[] args)
        {
            String Output_String = "";

            byte[] Output_Bytes;

            String Input = "\u00022133\r\n(111) 495-6004 VOIP 08/10 14:17\r\nCHILDREN'S HOSPITAL COLORADO\r\n      1111       P#111-211-0089\r\n   BRIARGATE PKWY        \r\n                    \r\n2ND FLOOR             261 05025\r\nCO COLORADO SPRINGS            \r\n                  TEL=LVL3 \r\n+38.965266  -104.752707      0 \r\nPSAP=CO SPRINGS VOIP\r\n\r\nVOIP 911 CALL\r\nVERIFY CALLERS LOCATION\r\nVERIFY EMS     \u0003#\r\n\u00022517\r\n(222) 632-4403 RESD 08/10 14:24\r\nBOBBITT  H L                \r\n      2222       P#222-632-4403\r\nE  SAN MIGUEL ST         \r\n                    \r\n                      261 00220\r\nCO COLORADO SPRINGS            \r\n                  TEL=CTLQ \r\n                               \r\nPSAP=CSPD--COLO SPGS      \r\nCOLO SPGSPD      \r\nCOLO SPGS FD      \r\nCOLSPGS FD  \u0003@\r\n\u00022125\r\n(333) 511-4008 WPH2 08/10 17:27\r\nT-MOBILE USA                \r\n       333       P#333-511-4008\r\n   NORTH GATE BLVD - E SE\r\nCTOR                \r\nCALLBK=(303)523-3333  261 00311\r\nCO COLORADO SPRINGS            \r\n                  TEL=TMOB \r\n+39.024017  -104.823282     14 \r\nPSAP=CSPD--COLORADO SPRINGS WIRELESS\r\nVERIFY PD\r\nVERIFY FD\r\nVERIFY EMS         \u0003\r\n\u00022209\r\n(444) 511-0463 WPH2 08/10 17:41\r\nVERIZON                     \r\n                 P#444-511-0463\r\n   4444 FOUNTAIN MESA RD \r\n- NE                \r\nCALLBK=(719)600-4444  261 02000\r\nCO COLORADO SPRINGS            \r\n                  TEL=VZW  \r\n+38.720455  -104.699299      5 \r\nPSAP=EPSO--EL PASO SHERIFF\r\nWIRELESS\r\nVERIFY PD\r\nVERIFY FD\r\nVERIFY EMS         \u0003\r\n\u00022125\r\n(555) 511-4005 WPH1 08/10 17:49\r\nT-MOBILE USA                \r\n                 P#555-511-4005\r\n   55555 TEMPLETON GAP RO\r\nAD - SECTOR NW      \r\nCALLBK=(911)103-5555  261 00311\r\nCO COLORADO SPRINGS            \r\n                  TEL=TMOB \r\n+38.938798  -104.702218   3674 \r\nPSAP=CSPD--COLORADO SPRINGS WIRELESS\r\nVERIFY PD\r\nVERIFY FD\r\nVERIFY EMS         \u0003\r\n\u00022000\r\n(666) 390-2149 PBXB 08/10 16:29\r\nCRIMINAL JUSTICE CTR        \r\n      6666       P#666-390-2149\r\nE  LAS VEGAS ST          \r\n                    \r\nSHERIFF               261 00220\r\nCO COLORADO SPRINGS            \r\nSHR - CJC - SHE   TEL=CTLQP\r\n                               \r\nPSAP=CSPD--COLO SPGS      \r\nCOLO SPGSPD      \r\nCOLO SPGS FD      \r\nCOLSPGS FD  \u0003\r\n\u0002200\r\n(777) 390-2149 PBXB 08/10 16:29\r\nCRIMINAL JUSTICE CTR        \r\n      7777       P#719-390-2149\r\nE  LAS VEGAS ST          \r\n                    \r\nSHERIFF               261 00220\r\nCO COLORADO SPRINGS            \r\nSHR - CJC - SHE   TEL=CTLQP\r\n                               \r\nPSAP=CSPD--COLO SPGS      \r\nCOLO SPGSPD      \r\nCOLO SPGS FD      \r\nCOLSPGS FD  \u0003\r\n\u0002E133\u0003\r\n\u0002H\u0003\r\n12345\u0002A1\u0003\u0002H\u0003\u0002B22\u0003\u0002H\u0003\u0002C333\u0003\u0002H\u0003\u0002D4444\u0003\u0002H\u0003\u0002E55555\u0003\u0002H\u000367890\u0002";
            //String Input = "\u0002E133\u0003\r\n\u0002H\u0003\r\n12345\u0002A1\u0003\u0002H\u0003\u0002B22\u0003\u0002H\u0003\u0002C333\u0003\u0002H\u0003\u0002D4444\u0003\u0002H\u0003\u0002E55555\u0003\u0002H\u000367890\u0002";
            //String Input = "12345A1HB22HC333HD4444HE55555H67890";

            byte[] Input_Bytes = Encoding.Default.GetBytes(Input);
            //Console.WriteLine("Byte Array is: " + String.Join(" ", Input_Bytes));

            String Input_String = Encoding.Default.GetString(Input_Bytes).ToUpper();

            Input_String = Regex.Replace(Input_String, @"\r|\n", " ");

            String[] Strip_STX = Input_String.Split('\u0002');

            foreach (String Input_String2 in Strip_STX)
            {

                if (Input_String2.Length != 0 && Input_String2 != "\r\n")
                {
                    String[] Strip_ETX = Input_String2.Split('\u0003');

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

                                    String Phone = CallBack_Wired.Replace(" ", "");

                                    String Caller = RecordX.Substring(39, 28);

                                    String Address_Number = RecordX.Substring(75, 10);
                                    String Address_Street = RecordX.Substring(105, 22);
                                    String Address_Misc = RecordX.Substring(129, 20);
                                    String Address = String.Concat(Address_Number.Trim(), " ", Address_Street, Address_Misc.Trim()).Trim().Replace("  ", " ") + String.Concat(Enumerable.Repeat(" ", 50));

                                    Address = Address.Substring(0, 52);
                                    Address_Misc = String.Concat(Enumerable.Repeat(" ", 22)); ;

                                    String City = RecordX.Substring(187, 28);
                                    String State = RecordX.Substring(184, 2);

                                    String Latitude = RecordX.Substring(246, 12);
                                    String Longitude = RecordX.Substring(258, 12);

                                    String Confidence = RecordX.Substring(269, 7).Trim() + String.Concat(Enumerable.Repeat(" ", 50));
                                    Confidence = Confidence.Substring(0, 7);

                                    if (Class_Of_Service.Trim() == "WRLS" || Class_Of_Service.Trim() == "WPH1" || Class_Of_Service.Trim() == "WPH2")
                                    {
                                        Caller = String.Concat("WIRELESS CALLER (", Carrier.Trim(), ")") + String.Concat(Enumerable.Repeat(" ", 50));
                                        Caller = Caller.Substring(0,28);

                                        Phone = CallBack_Wireless.Replace(" ", ""); ;
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

                                    //Console.WriteLine("Station = " + Station);
                                    //Console.WriteLine("Date_Time = " + Date_Time );
                                    //Console.WriteLine("Class_Of_Service = " + Class_Of_Service);
                                    //Console.WriteLine("Carrier = " + Carrier);
                                    //Console.WriteLine("Phone = " + Phone);
                                    //Console.WriteLine("Caller = " + Caller);
                                    //Console.WriteLine("Address_Misc = " + Address_Misc);
                                    //Console.WriteLine("Address = " + Address);
                                    //Console.WriteLine("City = " + City);
                                    //Console.WriteLine("State = " + State);
                                    //Console.WriteLine("Latitude = " + Latitude);
                                    //Console.WriteLine("Longitude = " + Longitude);
                                    //Console.WriteLine("Confidence = " + Confidence);
                                }
                            }
                            else  if (Type == "H" || Type == "E")
                            {
                                Fixed_String = RecordX.Substring(0, 200);
                            }

                            if (Fixed_String.Length != 0)
                            {
                                Fixed_String = String.Concat("\u0002", Fixed_String, "\u0003");
                                Console.WriteLine(Fixed_String);
                                //Console.WriteLine(Fixed_String + " <" + Fixed_String.Length.ToString() + ">");

                                Output_String = Output_String + Fixed_String;
                            }
                        }
                    }
                }
            }

            Output_Bytes = Encoding.Default.GetBytes(Output_String);

            using (StreamWriter Writer = new StreamWriter("DEBUG.txt"))
            {
                Writer.WriteLine(Output_String);
                //Writer.WriteLine(String.Join(" ", Output_Bytes));
            };

            //String ReadText = File.ReadAllText("DEBUG.txt");
            //Console.WriteLine(ReadText);
        }
    }
}