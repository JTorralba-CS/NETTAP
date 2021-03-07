using System;

namespace Test_Extension_2
{
    public class Extension : Interface.Extension
    {
        public string Name { get => "Test_Extension_2"; }
        public string Description { get => "This is a test extension."; }
        public Byte Priority { get; set; }
        public int Random_Number { get; set; }

        public Extension()
        {
            Priority = 107;
            Random_Number = 7299;
        }

        public int Execute(String Data)
        {
            Random RG = new Random();

            Console.WriteLine("Bye " + Data + "!" + " " + "The current time is " + DateTime.Now.ToString() + ".");
            Console.WriteLine();

            Random_Number = RG.Next();
            return 0;
        }
    }
}
