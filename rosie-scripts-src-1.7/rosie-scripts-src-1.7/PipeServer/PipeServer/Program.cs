using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeServer
{
    [Serializable]
    public class Client
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string PhoneNo { get; set; }
    }
    internal class Program
    {
        //static NamedPipeClientStream _converterStream;
        static NamedPipeServerStream _resultStream;
        static bool _convertProcessCompleted = false;
        static bool _resultProcessCompleted = false;
        public static List<Client> DefaultData { get; set; }
        public static List<Client> ResultData { get; set; }


        static void Main(string[] args)
        {
            ResultData = new List<Client>();
            GenerateDefaultData();

            Thread resultPipeThread = new Thread(ResultPipe);
            resultPipeThread.Start();

            Thread converterPipeThread = new Thread(() => ConverterPipe(DefaultData));
            converterPipeThread.Start();
            resultPipeThread.Join();





            var d = Console.ReadLine();

            while (d != "end")
            {
                Console.WriteLine(">" + d);
                d = Console.ReadLine();
            }

            //if (_converterStream != null)
            //    _converterStream.Close();

            if (_resultStream != null)
                _resultStream.Close();
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool WaitNamedPipe(string name, int timeout);

        public static bool NamedPipeDoesNotExist(string pipeName)
        {
            try
            {
                int timeout = 0;
                string normalizedPath = System.IO.Path.GetFullPath(
                 string.Format(@"\\.\pipe\{0}", pipeName));
                bool exists = WaitNamedPipe(normalizedPath, timeout);
                if (!exists)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 0) // pipe does not exist
                        return true;
                    else if (error == 2) // win32 error code for file not found
                        return true;
                    // all other errors indicate other issues
                }
                return false;
            }
            catch (Exception ex)
            {
                return true; // assume it exists
            }
        }

        /// <summary>  
        /// Process default data and send it to pipe.  
        /// </summary>  
        /// <param name="defaultData"></param>  
        static void ConverterPipe(List<Client> defaultData)
        {
            while (true)//endless loop
            {
                // New Server NamedPipeClientStream Instance
                NamedPipeClientStream _converterStream = new NamedPipeClientStream("ThorClientThorImagePipe");

                if (NamedPipeDoesNotExist("ThorClientThorImagePipe"))
                {
                    //Console.WriteLine("[Bilal] Named Pipe Does Not Exist");
                    //sleep to lessen CPU load
                    System.Threading.Thread.Sleep(500);
                    continue;
                }

                _converterStream.Connect();

                Console.WriteLine("ClientServer connected.");


                StreamString ss = new StreamString(_converterStream);
                string res = ss.ReadString();

                Console.WriteLine($"[Bilal] client server says ........{res}");

               
                _converterStream.Close();

            }
        }

        /// <summary>  
        /// Create final result from default data.  
        /// </summary>  
        static void ResultPipe()
        {
            bool send = true;
            while (true)
            {
                Console.WriteLine("Waiting for connection on named pipe mypipe [ThorImageThorClientPipe]");
                _resultStream = new NamedPipeServerStream("ThorImageThorClientPipe");
                _resultStream.WaitForConnection();

                Console.WriteLine("Got Connection");

                //StreamWriter m_pipeServerWriter = new StreamWriter(_resultStream);
                //m_pipeServerWriter.AutoFlush = true;

                //m_pipeServerWriter.WriteLine("Hello.");
                //m_pipeServerWriter.Flush();

                Thread.Sleep(2000);

                if (send)
                {
                    string msg = @"Remote~Local~Establish~bilal";

                    byte[] bytes = Encoding.Unicode.GetBytes(msg);

                    List<byte> data = new List<byte>();

                    ushort dataL = (ushort)bytes.Length;

                    data.Add((byte)((dataL & 0xFF00) >> 8));
                    data.Add((byte)(dataL & 0x00FF));

                    foreach (byte b in bytes)
                        data.Add(b);

                    _resultStream.Write(data.ToArray(), 0, data.Count);
                }

                Thread.Sleep(2000);

                StreamString ss = new StreamString(_resultStream);
                string res = ss.ReadString();

                if (res.Contains("Remote~Local~Establish"))
                    send = false;

                Console.WriteLine($"[Bilal] client says ........{res}");

                //while (_resultProcessCompleted == false)
                //{
                //    if (_convertProcessCompleted)
                //    {
                //        _resultProcessCompleted = true;
                //        break;
                //    }
                //    IFormatter formatter = new BinaryFormatter();
                //    Client clientReceived = (Client)formatter.Deserialize(_resultStream);
                //    Thread.Sleep(1);
                //    ResultData.Add(clientReceived);
                //}

                //while (true) {
                Thread.Sleep(500);
                //}

                _resultStream.Close();
            }
        }

        /// <summary>  
        /// Generates default data to transfer.  
        /// </summary>  
        private static void GenerateDefaultData()
        {
            DefaultData = new List<Client>();
            for (int i = 0; i < 500; i++)
            {
                Client client = new Client();
                client.Id = i;
                client.Name = "Client " + i.ToString();
                client.PhoneNo = "00111" + i.ToString();
                DefaultData.Add(client);
            }
        }
    }

    public class StreamString
    {
        #region Fields

        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamString"/> class.
        /// </summary>
        /// <param name="ioStream">The io stream.</param>
        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            int len = 0;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="outString">The out string.</param>
        /// <returns></returns>
        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }

        #endregion Methods
    }
}
