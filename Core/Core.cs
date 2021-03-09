using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Constant
    {
        public static readonly String CR = Convert.ToChar(13).ToString();
        public static readonly String CRLF = Convert.ToChar(13).ToString() + Convert.ToChar(10).ToString();
        public static readonly String LF = Convert.ToChar(10).ToString();
    }

    public static class Log
    {
        public static StringBuilder Detail(String General, String Specific)
        {
            StringBuilder Detail_String = new StringBuilder(0);
            int Detail_Length = 0;

            if (General.Length + Specific.Length != 0)
            {
                Detail_String.Append(DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + General);

                switch (Specific.Length)
                {
                    case 0:
                        break;
                    case 1:
                        Detail_String.Append(" " + Specific.Length + " Byte");
                        break;
                    default:
                        Detail_String.Append(" " + Specific.Length + " Bytes");
                        break;
                }

                Detail_Length = Detail_String.Length;

                if (Specific.Length != 0)
                {
                    Detail_String.Append(Constant.CRLF);
                    Detail_String.Append("".PadLeft(Detail_Length, '-'));
                    Detail_String.Append(Constant.CRLF);
                    Detail_String.Append(Specific);
                }

                Detail_String = Detail_String.Replace(Constant.CRLF, Constant.CR);
                Detail_String = Detail_String.Replace(Constant.LF, Constant.CR);
                Detail_String = Detail_String.Replace(Constant.CR, Constant.CRLF);
            }

            return Detail_String;
        }

        public static void Terminal(String General, String Specific)
        {
            Console.WriteLine(Detail(General, Specific));
            Console.WriteLine();
        }

        public static void Terminal(String General)
        {
            Terminal(General, "");
        }

        public static void Terminal(String General, Byte[] Specific, int Specific_Size)
        {
            Terminal(General, System.Text.Encoding.ASCII.GetString(Specific, 0, Specific_Size));
        }

        public static async Task Write_To_File(String File, Byte[] Bytes)
        {
            StringBuilder Detail_String = new StringBuilder(0);
            int Detail_Length = 0;

            try
            {
                using (FileStream Target = new FileStream(File, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 32768, useAsync: true))
                {
                    await Target.WriteAsync(Bytes, 0, Bytes.Length);
                }
            }
            catch (Exception E)
            {
                Detail_String.Append(DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " Exception: Write_To_File");
                Detail_Length = Detail_String.Length;

                Detail_String.Append(Constant.CRLF);
                Detail_String.Append("".PadLeft(Detail_Length, '-'));
                Detail_String.Append(Constant.CRLF);
                Detail_String.Append(E.Message);
                Detail_String.Append(Constant.CRLF);
                Detail_String.Append(Constant.CRLF);
                Console.Write(Detail_String);
            }
        }

        public static void File(String Path, String General, String Specific)
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\" + Path))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + Path);
            }

            StringBuilder Detail_String = Detail(General, Specific);
            Detail_String.Append(Constant.CRLF);
            Detail_String.Append(Constant.CRLF);

            Write_To_File(Directory.GetCurrentDirectory() + @"\" + Path + @"\" + "LOG.txt", Encoding.ASCII.GetBytes(Detail_String.ToString())).ConfigureAwait(false);
        }

        public static void File(String Path, String General)
        {
            File(Path, General, "");
        }

        public static void File(String Path, String General, Byte[] Specific, int Specific_Size)
        {
            File(Path, General, System.Text.Encoding.ASCII.GetString(Specific, 0, Specific_Size));
        }
    }

    public static class Network
    {
        public static String[] IP(Byte Version)
        {
            IPAddress[] IPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            List<String> IP = new List<String>();

            for (int I = IPAddress.Length; --I >= 0;)
            {
                switch (Version)
                {
                    case 4:
                        if (IPAddress[I].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            IP.Add(IPAddress[I].ToString());
                        }
                        break;
                    case 6:
                        if (IPAddress[I].AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            IP.Add(IPAddress[I].ToString());
                        }
                        break;
                    default:
                        IP.Add(IPAddress[I].ToString());
                        break;
                }
            }

            return IP.ToArray();
        }
    }

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

    public static class Append
    {
        public static Byte[] Byte(Byte[] Byte_Array, Byte Byte)
        {
            Byte[] Byte_Array_New = new Byte[Byte_Array.Length + 1];

            Byte_Array.CopyTo(Byte_Array_New, 1);
            Byte_Array_New[0] = Byte;

            return Byte_Array_New;
        }
    }
}
