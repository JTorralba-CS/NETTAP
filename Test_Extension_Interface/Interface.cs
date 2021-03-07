using System;

namespace Interface
{
    public interface Extension
    {
        String Name {get;}
        String Description {get;}
        int Priority { get; set;}
        int Execute(String Data);
    }
}
