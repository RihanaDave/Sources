using System;
using System.IO;
using System.Text;

namespace GPAS.Utility
{
    public class StreamUtility
    {
        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public byte[] ReadFully(Stream input)
        {

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public MemoryStream ConvertStreamToMemoryStream(Stream stream)
        {
            MemoryStream memoryStream = new MemoryStream();

            if (stream != null)
            {
                byte[] buffer = ReadFully(stream);

                if (buffer != null)
                {
                    var binaryWriter = new BinaryWriter(memoryStream);
                    binaryWriter.Write(buffer);
                }
            }
            return memoryStream;
        }

        public byte[] ReadStreamAsBytesArray(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public string ByteArrayToStringUtf8(byte[] value)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetString(value);
        }

        public byte[] StringToByteArrayUtf8(string value)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(value);
        }
        
        public string GetStringFromStream(MemoryStream ms)
        {
            byte[] bb = ReadStreamAsBytesArray(ms);
            string ss = Encoding.Default.GetString(bb);
            return ss;
        }
    }
}