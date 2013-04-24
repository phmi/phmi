using System.Collections.Generic;

namespace PHmiIoDevice.Modbus.Implementation
{
    internal abstract class ModbusBase
    {
        public List<byte> ReadCoils(byte slaveAddress, int address, int byteLength, int length)
        {
            return ReadBits(slaveAddress, address, byteLength, length, 0x01);
        }

        public List<byte> ReadContacts(byte slaveAddress, int address, int byteLength, int length)
        {
            return ReadBits(slaveAddress, address, byteLength, length, 0x02);
        }

        public List<byte> ReadBits(
            byte slaveAddress, int address, int byteLength, int length, byte functionCode)
        {
            var pdu = new List<byte>(5) {functionCode};
            pdu.AddRange(ShortToBytes(address));
            pdu.AddRange(ShortToBytes(length));
            return Read(slaveAddress, address, byteLength, pdu);
        }

        public List<byte> ReadInputRegisters(byte slaveAddress, int address, int byteLength, int length)
        {
            return ReadRegisters(slaveAddress, address, byteLength, length, 0x04);
        }

        public List<byte> ReadHoldingRegisters(byte slaveAddress, int address, int byteLength, int length)
        {
            return ReadRegisters(slaveAddress, address, byteLength, length, 0x03);
        }

        private List<byte> ReadRegisters(
            byte slaveAddress, int address, int byteLength, int length, byte functionCode)
        {
            var pdu = new List<byte>(5) {functionCode};
            pdu.AddRange(ShortToBytes(address));
            pdu.AddRange(ShortToBytes(length));
            return Read(slaveAddress, address, byteLength, pdu);
        }

        public bool WriteCoils(byte slaveAddress, int address, List<byte> data, int length)
        {
            var pdu = new List<byte>(6 + data.Count) { 0x0F };
            pdu.AddRange(ShortToBytes(address));
            pdu.AddRange(ShortToBytes(length));
            pdu.Add((byte)data.Count);
            pdu.AddRange(data);
            return Write(slaveAddress, address, data.Count, pdu);
        }

        public bool WriteHoldingRegisters(byte slaveAddress, int address, List<byte> data, int length)
        {
            var pdu = new List<byte>(6 + data.Count) { 0x10 };
            pdu.AddRange(ShortToBytes(address));
            pdu.AddRange(ShortToBytes(length));
            pdu.Add((byte)data.Count);
            pdu.AddRange(data);
            return Write(slaveAddress, address, data.Count, pdu);
        }

        private static IEnumerable<byte> ShortToBytes(int s)
        {
            return new[] { (byte)((s >> 8) & 0xFF), (byte)(s & 0xFF) };
        }

        protected abstract List<byte> Read(
            byte slaveAddress, int address, int byteLength, List<byte> pdu);

        protected abstract bool Write(
            byte slaveAddress, int address, int byteLength, List<byte> pdu);

        public abstract void Open();

        public abstract void Close();
    }
}
