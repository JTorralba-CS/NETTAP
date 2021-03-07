using System;

namespace Interface
{
    public interface Extension
    {
        public String Name { get; }
        String Description { get; }

        Byte[] BufferX(Byte[] Buffer, int Packet_Size);
    }
}
