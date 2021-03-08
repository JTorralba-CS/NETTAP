using System;

namespace Extension
{
    public class Test_I : Interface.Extension
    {
        public string Name { get; }
        public string Description { get; }
        public Byte Priority { get; set; }
        public int Random_Number { get; set; }

        public Test_I()
        {
            Name = "Test_I";
            Description = "This is the Test_I extension.";
            Priority = 200;
            Random_Number = 7299;
        }

        public int Execute(String Data)
        {
            Random RG = new Random();

            Console.WriteLine("Hi " + Data + "!" + " " + "The current time is " + DateTime.Now.ToString() + ".");
 
            Random_Number = RG.Next();
            return 0;
        }
    }
}
