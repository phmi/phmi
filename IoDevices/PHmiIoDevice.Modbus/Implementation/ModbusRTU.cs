using System;
using System.Collections.Generic;

namespace PHmiIoDevice.Modbus.Implementation
{
    internal class ModbusRTU: ModbusBase
    {
        private readonly IHelper _portHelper;

        public ModbusRTU(IHelper portHelper)
        {
            _portHelper = portHelper;
        }

        protected override List<byte> Read(
            byte slaveAddress, int address, int byteLength, List<byte> pdu)
        {
            var adu = CreateAdu(slaveAddress, pdu);
            _portHelper.Write(adu);
            var response = _portHelper.Read(byteLength + 5);
            if (response == null || response.Count < 5)
            {
                throw new Exception("No response");
            }
            var crc = GetCrc(response, 0, response.Count - 2);
            if (crc[0] != response[response.Count - 2] || crc[1] != response[response.Count - 1])
            {
                throw new Exception("CRC error");
            }
            if (slaveAddress != response[0])
            {
                throw new Exception("Slave address does not match");
            }

            if (response.Count == 5)
            {
                var error = string.Format(
                    "Error: function code = {0}, exception code = {1}",
                    ByteToHex(response[1]),
                    ByteToHex(response[2]));
                throw new Exception(error);
            }

            if (response.Count != byteLength + 5)
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
            var dataBytesLength = response.Count - 5;
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

        protected override bool Write(byte slaveAddress, int address, int byteLength, List<byte> pdu)
        {
            var adu = CreateAdu(slaveAddress, pdu);
            _portHelper.Write(adu);
            var response = _portHelper.Read(8);
            if (response == null || response.Count < 5)
            {
                throw new Exception("No response");
            }
            var crc = GetCrc(response, 0, response.Count - 2);
            if (crc[0] != response[response.Count - 2] || crc[1] != response[response.Count - 1])
            {
                throw new Exception("CRC error");
            }
            if (slaveAddress != response[0])
            {
                throw new Exception("Slave address does not match");
            }

            if (response.Count == 5)
            {
                var error = string.Format(
                    "Error: function code = {0}, exception code = {1}",
                    ByteToHex(response[1]),
                    ByteToHex(response[2]));
                throw new Exception(error);
            }

            if (response.Count != 8)
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

        private static string ByteToHex(byte b)
        {
            var s = b.ToString("X");
            while (s.Length < 2)
            {
                s = "0" + s;
            }
            return "0x" + s;
        }

        private static byte[] CreateAdu(byte slaveAddress, ICollection<byte> pdu)
        {
            var adu = new List<byte>(pdu.Count + 3) {slaveAddress};
            adu.AddRange(pdu);
            adu.AddRange(GetCrc(adu, 0, adu.Count));
            return adu.ToArray();
        }

        private static byte[] GetCrc(IList<byte> message, int startIndex, int length)
        {
            var crc = 0xFFFF;
            for (var i = startIndex; i < startIndex + length; ++i)
            {
                var messageByte = message[i];
                if (i < startIndex)
                    continue;
                if (i - startIndex >= length)
                    break;
                crc = crc ^ messageByte;
                var shiftCounter = 0;
                do
                {
                    bool lsb;
                    do
                    {
                        lsb = (crc & 1) > 0;
                        crc = (crc >> 1) & 0x7FFF;
                        shiftCounter++;
                        if (shiftCounter == 8)
                        {
                            break;
                        }
                    } while (!lsb);
                    if (lsb)
                        crc = crc ^ 0xA001;
                } while (shiftCounter < 8);
            }
            return new[] { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
        }
    }
}
