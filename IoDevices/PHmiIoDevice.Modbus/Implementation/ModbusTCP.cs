using System;
using System.Collections.Generic;

namespace PHmiIoDevice.Modbus.Implementation
{
    internal class ModbusTCP: ModbusBase
    {
        private readonly IHelper _clientHelper;

        public ModbusTCP(IHelper clientHelper)
        {
            _clientHelper = clientHelper;
        }

        protected override List<byte> Read(
            byte slaveAddress, int address, int byteLength, List<byte> pdu)
        {
            var adu = CreateAdu(slaveAddress, pdu);
            _clientHelper.Write(adu);
            var response = _clientHelper.Read(byteLength + 9);
            if (response == null || response.Count < 9)
            {
                throw new Exception("No response");
            }

            var packLength = response.Count - 6;
            var packLengthByteH = (byte)(packLength >> 8 & 0xFF);
            var packLengthByteL = (byte)(packLength & 0xFF);
            if (response[4] != packLengthByteH || response[5] != packLengthByteL)
            {
                throw new Exception("Packet length does not match");
            }
            
            if (slaveAddress != response[6])
            {
                throw new Exception("Slave address does not match");
            }

            if (response.Count == 9)
            {
                var error = string.Format(
                    "Error: function code = {0}, exception code = {1}",
                    ByteToHex(response[7]),
                    ByteToHex(response[8]));
                throw new Exception(error);
            }

            if (response.Count != byteLength + 9)
            {
                throw new Exception("Unexpected packet length");
            }

            if (pdu[0] != response[7])
            {
                throw new Exception("Function code does not match");
            }

            if (byteLength != response[8])
            {
                throw new Exception("Bytes count does not match");
            }
            var dataBytesLength = response.Count - 9;
            if (byteLength != dataBytesLength)
            {
                throw new Exception("Data bytes count does not match");
            }

            var result = new List<byte>(byteLength);
            for (var i = 9; i < byteLength + 9; ++i)
            {
                result.Add(response[i]);
            }

            return result;
        }

        protected override bool Write(
            byte slaveAddress, int address, int byteLength, List<byte> pdu)
        {
            var adu = CreateAdu(slaveAddress, pdu);
            _clientHelper.Write(adu);
            var response = _clientHelper.Read(12);
            if (response == null || response.Count < 9)
            {
                throw new Exception("No response");
            }

            var packLength = response.Count - 6;
            var packLengthByteH = (byte)(packLength >> 8 & 0xFF);
            var packLengthByteL = (byte)(packLength & 0xFF);
            if (response[4] != packLengthByteH || response[5] != packLengthByteL)
            {
                throw new Exception("Packet length does not match");
            }
            
            if (slaveAddress != response[6])
            {
                throw new Exception("Slave address does not match");
            }

            if (response.Count == 9)
            {
                var error = string.Format(
                    "Error: function code = {0}, exception code = {1}",
                    ByteToHex(response[7]),
                    ByteToHex(response[8]));
                throw new Exception(error);
            }

            if (response.Count != 12)
            {
                throw new Exception("Unexpected packet length");
            }

            if (pdu[0] != response[7])
            {
                throw new Exception("Function code does not match");
            }

            if (pdu[1] != response[8] || pdu[2] != response[9])
            {
                throw new Exception("Head coil number does not match");
            }

            if (pdu[3] != response[10] || pdu[4] != response[11])
            {
                throw new Exception("Write length does not match");
            }

            return true;
        }

        public override void Open()
        {
            _clientHelper.Open();
        }

        public override void Close()
        {
            _clientHelper.Close();
        }

        private static byte[] CreateAdu(byte slaveAddress, ICollection<byte> pdu)
        {
            var length = pdu.Count + 1;
            var lengthByteH = (byte) (length >> 8 & 0xFF);
            var lengthByteL = (byte) (length & 0xFF);
            var adu = new List<byte>(pdu.Count + 7) { 0x00, 0x00, 0x00, 0x00, lengthByteH, lengthByteL, slaveAddress };
            adu.AddRange(pdu);
            return adu.ToArray();
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
