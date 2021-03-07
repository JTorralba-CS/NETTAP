using System;

namespace Interface
{
    public interface Extension
    {
        String Name {get;}
        String Description {get;}

        int Execute(String Data);
    }
}
