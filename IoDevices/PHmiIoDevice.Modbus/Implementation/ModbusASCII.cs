using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PHmiIoDevice.Modbus.Implementation
{
    internal class ModbusASCII: ModbusBase
    {
        private readonly IHelper _portHelper;

        public ModbusASCII(IHelper portHelper)
        {
            _portHelper = portHelper;
        }

        protected override List<byte> Read(
            byte slaveAddress, int address, int byteLength, List<byte> pdu)
        {
            var adu = CreateAdu(slaveAddress, pdu);
            _portHelper.Write(GetAscii(adu));
            var packetLength = (byteLength + 4)*2 + 3;
            var responseAscii = _portHelper.Read(packetLength);
            if (responseAscii == null || responseAscii.Count < 11)
            {
                throw new Exception("No response");
            }
            var response = GetBytes(responseAscii.ToArray());
            var lrc = GetLrc(response, 0, response.Count - 1);
            if (lrc != response[response.Count - 1])
            {
                throw new Exception("LRC error");
            }
            if (slaveAddress != response[0])
            {
                throw new Exception("Slave address does not match");
            }

            if (response.Count == 4)
            {
                var error = string.Format(
                    "Error: function code = {0}, exception code = {1}",
                    ByteToHex(response[1]),
                    ByteToHex(response[2]));
                throw new Exception(error);
            }

            if (responseAscii.Count != packetLength)
            {
                throw new Exception("Unexpected packet length");
            }

            if (pdu[0] != response[1])
            {
                throw new Exception("Function code does not match");
            }

            if (byteLength != response[2])
            {
                throw new Exception("Bytes count does not match");
            }
            var dataBytesLength = response.Count - 4;
            if (byteLength != dataBytesLength)
            {
                throw new Exception("Data bytes count does not match");
            }

            var result = new List<byte>(byteLength);
            for (var i = 3; i < byteLength + 3; ++i)
            {
                result.Add(response[i]);
            }

            return result;
        }

        protected override bool Write(
            byte slaveAddress, int address, int byteLength, List<byte> pdu)
        {
            var adu = CreateAdu(slaveAddress, pdu);
            _portHelper.Write(GetAscii(adu));
            var responseAscii = _portHelper.Read(17);
            if (responseAscii == null || responseAscii.Count < 11)
            {
                throw new Exception("No response");
            }
            var response = GetBytes(responseAscii.ToArray());
            var lrc = GetLrc(response, 0, response.Count - 1);
            if (lrc != response[response.Count - 1])
            {
                throw new Exception("LRC error");
            }
            if (slaveAddress != response[0])
            {
                throw new Exception("Slave address does not match");
            }

            if (response.Count == 4)
            {
                var error = string.Format(
                    "Error: function code = {0}, exception code = {1}",
                    ByteToHex(response[1]),
                    ByteToHex(response[2]));
                throw new Exception(error);
            }

            if (responseAscii.Count != 17)
            {
                throw new Exception("Unexpected packet length");
            }

            if (pdu[0] != response[1])
            {
                throw new Exception("Function code does not match");
            }

            if (pdu[1] != response[2] || pdu[2] != response[3])
            {
                throw new Exception("Head coil number does not match");
            }

            if (pdu[3] != response[4] || pdu[4] != response[5])
            {
                throw new Exception("Write length does not match");
            }

            return true;
        }

        public override void Open()
        {
            _portHelper.Open();
        }

        public override void Close()
        {
            _portHelper.Close();
        }

        private static List<byte> CreateAdu(byte slaveAddress, ICollection<byte> pdu)
        {
            var adu = new List<byte>(pdu.Count + 2) { slaveAddress };
            adu.AddRange(pdu);
            adu.Add(GetLrc(adu, 0, adu.Count));
            return adu;
        }

        private static byte GetLrc(IList<byte> message, int startIndex, int length)
        {
            var summ = 0;
            for (var i = startIndex; i < startIndex + length; ++i)
            {
                summ += message[i];
            }
            var bitReversal = (summ ^ 0xFF)&0xFF;
            var result = (bitReversal + 1)&0xFF;
            return (byte)result;
        }

        private static byte[] GetAscii(ICollection<byte> adu)
        {
            var ascii = new List<byte>(adu.Count*2 + 3){0x3A};
            foreach (var b in adu)
            {
                ascii.AddRange(ByteToAscii(b));
            }
            ascii.Add(0x0D);
            ascii.Add(0x0A);
            return ascii.ToArray();
        }

        private static readonly UTF8Encoding Encoder = new UTF8Encoding();

        private const byte ZeroByte = 0x30;

        private static IEnumerable<byte> ByteToAscii(byte b)
        {
            var bytes = Encoder.GetBytes(b.ToString("X"));
            return bytes.Length == 1 ? new[] {ZeroByte, bytes[0]} : bytes;
        }

        private static List<byte> GetBytes(byte[] ascii)
        {
            var bytes = new List<byte>((ascii.Length - 3)/2);
            for (var i = 1; i < ascii.Length - 2; i += 2)
            {
                bytes.Add(byte.Parse(Encoder.GetString(ascii, i, 2), NumberStyles.AllowHexSpecifier));
            }
            return bytes;
        }

        private static string ByteToHex(byte b)
        {
            var s = b.ToString("X");
            while (s.Length < 2)
            {
                s = "0" + s;
            }
            return "0x" + s;
        }
    }
}
