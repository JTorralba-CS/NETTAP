using System;

namespace Extension
{
    public class Extension : Interface.Extension
    {
        public string Name { get => "Time"; }
        public string Description { get => "Display current time."; }

        public int Execute(String Data)
        {
            Console.WriteLine("Hi " + "Senior Night" + "!");
            Console.WriteLine();
            Console.WriteLine("The current time is " + DateTime.Now.ToString() + ".");
            Console.WriteLine();
            return 0;
        }
    }
}
