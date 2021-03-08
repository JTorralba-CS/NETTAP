using Core;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTap
{
    class Program
    {

        static void Main(String[] Arguments)
        {
            try
            {
                IEnumerable<Interface.Extension> DLLs = DLLLoadContext.Initialize("Extension", typeof(Program));
                IEnumerable<Interface.Extension> Priority = DLLs.OrderBy(DLL => DLL.Priority);

                foreach (Interface.Extension DLL in Priority)
                {
                    Console.WriteLine($"{DLL.Priority} {DLL.Name} (Extension Loaded)");
                    DLL.Execute("Jane Smith");
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
