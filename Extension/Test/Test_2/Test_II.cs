using System;

namespace Test_Extension_2
{
    public class Test_II : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }
        public int Random_Number { get; set; }

        public Test_II()
        {
            Name = "Test_II";
            Description = "This is Test_II extension.";
            Priority = 100;
            Random_Number = 7299;
        }

        public int Execute(String Data)
        {
            Random RG = new Random();

            Console.WriteLine("Bye " + Data + "!" + " " + "The current time is " + DateTime.Now.ToString() + ".");

            Random_Number = RG.Next();
            return 0;
        }
    }
}
