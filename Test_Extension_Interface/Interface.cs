using System;

namespace Interface
{
    public interface Extension
    {
        public String Name {get;}
        String Description {get;}

        int Execute(String Data);
    }
}
