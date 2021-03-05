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
                String[] DLLPaths = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Extension\", "*.dll");

                IEnumerable<Interface.Extension> DLLs = DLLPaths.SelectMany(DLLPath =>
                {
                    Assembly DLLAssembly = DLLLoadContext.LoadDLL(DLLPath, typeof(Program));
                    return DLLLoadContext.CreateCommands(DLLAssembly);
                }).ToList();

                foreach (Interface.Extension DLL in DLLs)
                {
                    Console.WriteLine($"{DLL.Name} - {DLL.Description}");
                    Console.WriteLine();
                    DLL.Execute("Jane Doe");
                }

                //foreach (String Argument in Arguments)
                //{
                //    Interface.Extension DLL = DLLs.FirstOrDefault(DLL => DLL.Name == Argument);

                //    if (DLL == null)
                //    {
                //        Console.WriteLine("Invalid extension");
                //        return;
                //    }

                //    DLL.Execute("Jane Smith");
                //}
            }
            catch (Exception E)
            {
                //Console.WriteLine(E);
            }

            //Syntax.Check(Arguments);
        }
    }
}
