using System;

namespace Test_Extension_2
{
    public class Extension : Interface.Extension
    {
        public string Name { get => "Test_Extension_2"; }
        public string Description { get => "This is a test extension."; }
        public int Priority { get; set; }

        public Extension()
        {
            Priority = 1;
        }


        public int Execute(String Data)
        {
            Console.WriteLine("Bye " + Data + "!" + " " + "The current time is " + DateTime.Now.ToString() + ".");
            Console.WriteLine();
            return 0;
        }
    }
}
