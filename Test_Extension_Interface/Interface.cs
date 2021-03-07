using System;

namespace Interface
{
    public interface Extension
    {
        String Name { get; }
        String Description { get; }
        Byte Priority { get; set; }
        int Random_Number { get; set; }

        int Execute(String Data);
    }
}
