using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PHmiIoDevice.Melsec.Implementation
{
    internal abstract class FxBase: IMelsec
    {
        public abstract void Open();

        public abstract void Dispose();

        protected abstract void Write(byte[] message);

        protected abstract IList<byte> Read(int length);

        protected abstract char FirstChar { get; }

        public List<byte> ReadMerkers(int address, int length)
        {
            return Read((0x4400 + address/16)*2, length/8);
        }

        public List<byte> ReadLMerkers(int address, int length)
        {
            throw new NotImplementedException();
        }

        public List<byte> ReadRegisters(int address, int length)
        {
            return Read((0x2000 + address)*2, length*2);
        }

        private List<byte> Read(int dataType, int dataSize)
        {
            var message = new StringBuilder();
            message.Append(FirstChar);
            message.Append("E0");
            message.Append(IncludeFirstZero(dataType.ToString("X"), 5));
            message.Append(IncludeFirstZero(dataSize.ToString("X"), 2));

            message.Append((char)3);
            {
                var summ = 0;
                for (var i = 1; i < message.Length; i++)
                {
                    summ += (byte) message[i];
                }
                var checkSumm = IncludeFirstZero(((byte) (summ & 0xFF)).ToString("X"), 2);
                message.Append(checkSumm);
            }
            
            Write(Encoder.GetBytes(message.ToString()));

            var bytes = Read(dataSize*2 + 4);

            if (bytes == null || bytes.Count < 5)
            {
                throw new Exception("Recieved message size error");
            }
            
            {
                var summ = 0;
                var endIndex = 0;
                for (var i = 1; i < bytes.Count; i++)
                {
                    summ += bytes[i];
                    if (bytes[i] == (char)3)
                    {
                        endIndex = i;
                        break;
                    }
                }
                var checkSumm = IncludeFirstZero(((byte)(summ & 0xFF)).ToString("X"), 2);
                if (bytes.Count - endIndex < 3 || endIndex == 0)
                {
                    throw new Exception("Recieved message size error 2");
                }
                if (bytes[endIndex + 1] != checkSumm[0] || bytes[endIndex + 2] != checkSumm[1])
                {
                    throw new Exception("Check summ error");
                }
                var data = new List<byte>(dataSize);
                for (var i = 0; i < endIndex / 2; i++)
                {
                    if (i + 1 >= endIndex)
                        continue;
                    var index = i * 2 + 1;
                    var str = Encoder.GetString(new[] { bytes[index], bytes[index + 1] });
                    data.Add(byte.Parse(str, NumberStyles.AllowHexSpecifier));
                }
                return data;
            }
        }

        private static readonly UTF8Encoding Encoder = new UTF8Encoding();

        private static string IncludeFirstZero(string number, int count)
        {
            var s = number;
            while (s.Length < count)
            {
                s = "0" + s;
            }
            return s;
        }

        public void WriteRegisters(int address, List<byte> data)
        {
            var message = new StringBuilder();
            message.Append(FirstChar);
            message.Append("E1");
            var dataType = (0x2000 + address) * 2;
            message.Append(IncludeFirstZero(dataType.ToString("X"), 5));
            message.Append(IncludeFirstZero(data.Count.ToString("X"), 2));
            for (var i = 0; i < data.Count/2; i++)
            {
                message.Append(IncludeFirstZero(data[i * 2].ToString("X"), 2));
                message.Append(IncludeFirstZero(data[i * 2 + 1].ToString("X"), 2));
            }

            message.Append((char)3);
            var summ = 0;
            for (var i = 1; i < message.Length; i++)
            {
                summ += (byte)message[i];
            }
            var checkSumm = IncludeFirstZero(((byte)(summ & 0xFF)).ToString("X"), 2);
            message.Append(checkSumm);

            Write(Encoder.GetBytes(message.ToString()));

            var bytes = Read(1);
            if (bytes.Count == 1 && bytes[0] == 0x06)
            {
                return;
            }

            var error = "Error";
            if (bytes.Count > 0)
            {
                error += " code " + bytes[0];
            }
            throw new Exception(error);
        }

        public void WriteMerker(int address, bool data)
        {
            var message = new StringBuilder();
            message.Append(FirstChar);

            message.Append(data ? "E7" : "E8");
            var dataType = 0x4000 + address;
            var str = IncludeFirstZero(dataType.ToString("X"), 4);
            message.Append(str.Substring(2, 2));
            message.Append(str.Substring(0, 2));

            message.Append((char)3);
            var summ = 0;
            for (var i = 1; i < message.Length; i++)
            {
                summ += (byte)message[i];
            }
            var checkSumm = IncludeFirstZero(((byte)(summ & 0xFF)).ToString("X"), 2);
            message.Append(checkSumm);

            Write(Encoder.GetBytes(message.ToString()));

            var bytes = Read(1);
            if (bytes.Count == 1 && bytes[0] == 0x06)
            {
                return;
            }

            var error = "Error";
            if (bytes.Count > 0)
            {
                error += " code " + bytes[0];
            }
            throw new Exception(error);
        }

        public void WriteLMerker(int address, bool data)
        {
            throw new NotImplementedException();
        }

        public int MaxReadLength
        {
            get { return 127; }
        }

        public int MaxWriteLength
        {
            get { return 63; }
        }

        public int MCount
        {
            get { return 7680; }
        }

        public int LCount
        {
            get { return 0; }
        }

        public int DCount
        {
            get { return 8000; }
        }
    }
}
