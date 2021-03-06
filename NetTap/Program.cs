using Core;
using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetTap
{
    class Program
    {

        static void Main(String[] Arguments)
        {
            try
            {
                IEnumerable<Interface.Extension> DLLs = DLLLoadContext.Initialize("Extension", typeof(Program));

                foreach (Interface.Extension DLL in DLLs)
                {
                    Console.WriteLine($"{DLL.Name} (Extension Loaded)");
                    Console.WriteLine();
                }

                //Interface.Extension Extension = DLLs.FirstOrDefault(DLL => DLL.Name == "Test_Extension");
                //if (Extension == null)
                //{
                //    Console.WriteLine("Extension not found.");
                //}
                //else
                //{
                //    Extension.Execute("Jane Smith");
                //}                   
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }

            Syntax.Check(Arguments);
        }
    }
}
