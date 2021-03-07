using System;

namespace Test_Extension_2
{
    public class Extension : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }
        public int Random_Number { get; set; }

        public Extension()
        {
            Name = "Test_Extension_2";
            Description = "This is a test extension.";
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
