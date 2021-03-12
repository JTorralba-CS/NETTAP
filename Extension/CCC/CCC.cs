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

        public int Execute(ref IPEndPoint Source, ref IPEndPoint Destination, ref Byte[] Packet)
        {
            Byte[] Mutant = new Byte[0];

            int Index = 0;

            for (int Counter = Packet.Length; --Counter > 0;)
            {
                Index = Packet.Length - Counter;

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
                                Packet[Index + 1] = 10;
                            }
                            catch
                            {
                            }
                            break;
                        default:
                            Mutant = Append.Byte(Mutant, Packet[Index]);
                            break;
                    }
                }
            }

            Array.Reverse(Mutant, 0, Mutant.Length);
            Packet = Mutant;

            return 0;
        }
    }
}
