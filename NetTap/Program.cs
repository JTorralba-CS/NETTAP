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
                String[] DLLPaths = new String[]
                {
                    //@"Extension\bin\Debug\net5.0\Extension.dll",
                    Directory.GetCurrentDirectory() + @"\Extension\" + "Extension.dll"
                };

                IEnumerable<Interface.Extension> Commands = DLLPaths.SelectMany(DLLPath =>
                {
                    Assembly DLLAssembly = DLLLoadContext.LoadDLL(DLLPath, typeof(Program));
                    return DLLLoadContext.CreateCommands(DLLAssembly);
                }).ToList();

                if (Arguments.Length == 0)
                {
                    foreach (Interface.Extension Command in Commands)
                    {
                        Console.WriteLine($"{Command.Name} - {Command.Description}");
                    }
                }
                else
                {
                    foreach (String CommandName in Arguments)
                    {
                        Console.WriteLine($"{CommandName}:");
                        Console.WriteLine();

                        Interface.Extension Command = Commands.FirstOrDefault(C => C.Name == CommandName);

                        if (Command == null)
                        {
                            Console.WriteLine("Invalid Command");
                            return;
                        }

                        Command.Execute("Jane Smith");
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }

            //Syntax.Check(Arguments);
        }
    }
}
