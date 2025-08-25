using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class WTF
    {
        private static BlockingCollection<Parameters> WTFQueue;      

        public WTF(String file, Byte[] bytes)
        {
            if (WTFQueue == null)
            {
                WTFQueue = new BlockingCollection<Parameters>();

                Task.Run(() => WTFExecute());
            }
            else
            {
                WTFQueue.Add(new Parameters
                {
                    File = file,
                    Bytes = bytes
                });
            }
        }

        private async void WTFExecute()
        {
            while (!WTFQueue.IsCompleted)
            {
                var Log = WTFQueue.Take();             

                try
                {
                    using (FileStream Target = new FileStream(Log.File, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 32768, useAsync: true))
                    {
                        await Target.WriteAsync(Log.Bytes, 0, Log.Bytes.Length);
                    }
                }
                catch (Exception E)
                {
                    StringBuilder Detail_String = new StringBuilder(0);

                    int Detail_Length = 0;

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
        }
    }

    public class Parameters
    {
        public String File;
        public Byte[] Bytes;
    }
}
