using Server;
using Standard;
using System;
using System.Net;

namespace NetTap
{
    class Program
    {

        static void Main(String[] Arguments)
        {
            String Destination_IP = null;
            int Destination_Port = 0;
            int Listen_Port = 35263;

            try
            {
                if (Arguments.Length > 2)
                {
                    Destination_IP = Arguments[0];
                    Destination_Port = int.Parse(Arguments[1]);
                    Listen_Port = int.Parse(Arguments[2]);
                }
                else if (Arguments.Length == 2)
                {
                    Destination_IP = Arguments[0];
                    Destination_Port = int.Parse(Arguments[1]);
                }
                else
                {
                    while (Destination_IP == null || Destination_Port == 0 || Listen_Port == 0)
                    {
                        Console.Write("Destination-IP:    ");
                        Destination_IP = Console.ReadLine();

                        Console.Write("Destination-Port:  ");
                        Destination_Port = int.Parse(Console.ReadLine());

                        Console.Write("Listen-Port:       ");
                        Listen_Port = int.Parse(Console.ReadLine());

                        Console.WriteLine();
                    }
                }
            }
            catch
            {
            }

            Listen.Initialize(Destination_IP, Destination_Port, Listen_Port);
        }
    }
}
