using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Core
{
    public class DLLLoadContext : AssemblyLoadContext
    {
        private static AssemblyDependencyResolver _Resolver;

        public DLLLoadContext(String DLLPath)
        {
            _Resolver = new AssemblyDependencyResolver(DLLPath);
        }

        protected override Assembly Load(AssemblyName AssemblyName)
        {
            String AssemblyPath = _Resolver.ResolveAssemblyToPath(AssemblyName);
            if (AssemblyPath != null)
            {
                return LoadFromAssemblyPath(AssemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(String UnManagedDLLName)
        {
            String LibraryPath = _Resolver.ResolveUnmanagedDllToPath(UnManagedDLLName);
            if (LibraryPath != null)
            {
                return LoadUnmanagedDllFromPath(LibraryPath);
            }

            return IntPtr.Zero;
        }

        public static Assembly LoadDLL(String RelativePath, Type Program)
        {
            String Root = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(
                                Path.GetDirectoryName(Program.Assembly.Location)))))));

            String DLLLocation = Path.GetFullPath(Path.Combine(Root, RelativePath.Replace('\\', Path.DirectorySeparatorChar)));

            //Console.WriteLine($"Extension = {DLLLocation}");
            //Console.WriteLine();

            DLLLoadContext LoadContext = new DLLLoadContext(DLLLocation);

            return LoadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(DLLLocation)));
        }

        public static IEnumerable<Interface.Extension> Integrate(Assembly Assembly)
        {
            int Count = 0;

            foreach (Type Type in Assembly.GetTypes())
            {
                if (typeof(Interface.Extension).IsAssignableFrom(Type))
                {
                    Interface.Extension Result = Activator.CreateInstance(Type) as Interface.Extension;
                    if (Result != null)
                    {
                        Count++;
                        yield return Result;
                    }
                }
            }

            if (Count == 0)
            {
                String AvailableTypes = String.Join(",", Assembly.GetTypes().Select(T => T.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements ICommand in {Assembly} from {Assembly.Location}.\n" +
                    $"Available types: {AvailableTypes}");
            }
        }

        public static IEnumerable<Interface.Extension> Initialize(String ExtensionFolder, Type Program)
        {
            String[] DLLPaths = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\" + ExtensionFolder + @"\", "*.dll");

            IEnumerable<Interface.Extension> DLLs = DLLPaths.SelectMany(DLLPath =>
            {
                Assembly DLLAssembly = DLLLoadContext.LoadDLL(DLLPath, Program);
                return DLLLoadContext.Integrate(DLLAssembly);
            }).ToList();

            return DLLs;
        }
    }
}
