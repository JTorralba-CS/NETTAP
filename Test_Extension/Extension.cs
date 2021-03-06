using System;

namespace Extension
{
    public class Extension : Interface.Extension
    {
        public string Name { get => "Test_Extension"; }
        public string Description { get => "This is a test extension."; }

        public int Execute(String Data)
        {
            Console.WriteLine("Hi " + Data + "!" + " " + "The current time is " + DateTime.Now.ToString() + ".");
            Console.WriteLine();
            return 0;
        }
    }
}
