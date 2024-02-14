using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThorClient.Engine
{
    /// <summary>
    ///  Defines the data protocol for reading and writing strings on our stream
    /// </summary>
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
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            string outs = streamEncoding.GetString(inBuffer);
            Trace.WriteLine($"[RECV] {outs}", Log.DATA);
            return outs;
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

            Trace.WriteLine($"[SEND] {outString}", Log.DATA);

            return outBuffer.Length + 2;
        }

        #endregion Methods
    }
}
