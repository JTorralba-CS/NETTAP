using Core;
using System;
using System.Net;

namespace CCC
{
    public class CCC : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }

        public CCC()
        {
            Name = "CCC";
            Description = "This is the CCC extension.";
            Priority = 10;
        }

        public int Execute(IPEndPoint Source, IPEndPoint Destination, ref Byte[] Packet, ref int Packet_Size)
        {
            Byte[] Mutant = new Byte[0];

            int Index = 0;

            while (Index < (Packet_Size))
            {
                if (Packet[Index] != 10)
                {
                    if (Packet[Index] == 44)
                    {
                        // Convert COMMA To SPACEBAR
                        Mutant = Append.Byte(Mutant, 32);
                    }
                    else
                    {
                        Mutant = Append.Byte(Mutant, Packet[Index]);
                    }

                    try
                    {
                        // ETX Check
                        if (Packet[Index] == 3)
                        {
                            Packet[Index + 1] = 10;

                            // Add EOL
                            Mutant = Append.Byte(Mutant, 13);
                            Mutant = Append.Byte(Mutant, 10);
                        }
                    }
                    catch (Exception E)
                    {
                        Log.Terminal("CCC", E.Message);
                    }
                }

                Index++;
            }

            Array.Reverse(Mutant, 0, Mutant.Length);

            Packet = Mutant;
            Packet_Size = Mutant.Length;

            return 0;
        }
    }
}
