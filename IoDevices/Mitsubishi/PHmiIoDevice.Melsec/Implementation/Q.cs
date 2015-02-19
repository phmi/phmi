using System;
using System.Collections.Generic;

namespace PHmiIoDevice.Melsec.Implementation
{
    internal class Q: IMelsec
    {
        public void Open()
        {
            _tcpHelper.Open();
        }

        public void Dispose()
        {
            _tcpHelper.Close();
        }

        public int MaxReadLength
        {
            get { return 960; }
        }

        public int MaxWriteLength
        {
            get { return 960; }
        }

        public int MCount
        {
            get { return 8000; }
        }

        public int LCount
        {
            get { return 8000; }
        }

        public int DCount
        {
            get { return 12000; }
        }

        public Q(string address, int port, byte pcNumber, byte networkNumber, int timeout, int messageEndTimeout)
        {
            _pcNumber = pcNumber;
            _networkNumber = networkNumber;
            _tcpHelper = new TcpClientHelper(address, port, timeout, messageEndTimeout);
        }

        private readonly TcpClientHelper _tcpHelper;
        private readonly byte _pcNumber;
        private readonly byte _networkNumber;

        public List<byte> ReadMerkers(int address, int length)
        {
            return ReadBlock(address, length / 16, 0x90);
        }

        public List<byte> ReadLMerkers(int address, int length)
        {
            return ReadBlock(address, length / 16, 0x92);
        }

        public List<byte> ReadRegisters(int address, int length)
        {
            return ReadBlock(address, length, 0xA8);
        }

        private List<byte> ReadBlock(int address, int length, byte code)
        {
            _tcpHelper.Write(
                new byte[]
                    {
                        0x50, 0x00,//Header
                        _networkNumber,//Network number
                        _pcNumber,//PC number
                        0xFF, 0x03,//Dest module I/O number
                        0x00,//Dest module station number
                        0x0C, 0x00,//Request length
                        0x0A, 0x00,//CPU timer
                        0x01, 0x04,//Command
                        0x00, 0x00,//Subcommand
                        (byte)(address & 0xFF), (byte)((address & 0xFF00) >> 8),
                        (byte)((address & 0xFF0000) >> 16),//Address
                        code,
                        (byte)(length & 0xFF), (byte)((length & 0xFF00) >> 8)// number of device points
                    });
            var bytesToReceive = length*2;
            const int headerLength = 11;
            var messageLengthToReceive = bytesToReceive + headerLength;
            var answer = _tcpHelper.Read(messageLengthToReceive);
            if (answer == null)
            {
                throw new Exception("Message not received");
            }
            if (answer.Count < 11)
            {
                throw new Exception("Recieved message size error");
            }

            var responseCode = (int)answer[10];
            responseCode = responseCode << 8;
            responseCode = responseCode | answer[9];
            if (responseCode != 0)
            {
                throw new Exception("Response code 0x" + responseCode.ToString("X"));
            }

            if (answer.Count != messageLengthToReceive)
            {
                throw new Exception("Recieved message size error 1");
            }

            var someLength = (int)answer[8];
            someLength = someLength << 8;
            someLength = someLength | answer[7];
            if (someLength != messageLengthToReceive - 9)
            {
                throw new Exception("Recieved message size error 2");
            }

            var data = new List<byte>(bytesToReceive);
            for (var i = 0; i < bytesToReceive; ++i)
            {
                data.Add(answer[headerLength + i]);
            }
            return data;
        } 

        public void WriteRegisters(int address, List<byte> data)
        {
            var length = data.Count/2;
            var requestLength = 12 + data.Count;
            const int headerLength = 21;
            var msg = new byte[headerLength + data.Count];
            msg[0] = 0x50; msg[1] = 0x00; //Header
            msg[2] = _networkNumber; //Network number
            msg[3] = _pcNumber; //PC number
            msg[4] = 0xFF; msg[5] = 0x03; //Dest module I/O number
            msg[6] = 0x00; //Dest module station number
            msg[7] = (byte)(requestLength & 0xFF);
            msg[8] = (byte)((requestLength & 0xFF00) >> 8); //Request length
            msg[9] = 0x0A; msg[10] = 0x00; //CPU timer
            msg[11] = 0x01; msg[12] = 0x14; //Command
            msg[13] = 0x00; msg[14] = 0x00; //Subcommand
            msg[15] = (byte) (address & 0xFF);
            msg[16] = (byte) ((address & 0xFF00) >> 8);
            msg[17] = (byte) ((address & 0xFF0000) >> 16); //Address
            msg[18] = 0xA8;
            msg[19] = (byte) (length & 0xFF);
            msg[20] = (byte) ((length & 0xFF00) >> 8); // number of device points

            for (var i = 0; i < data.Count; ++i)
                msg[headerLength + i] = data[i];

            _tcpHelper.Write(msg);

            const int messageLengthToReceive = 11;
            var answer = _tcpHelper.Read(messageLengthToReceive);
            if (answer == null)
            {
                throw new Exception("Message not received");
            }
            if (answer.Count != messageLengthToReceive)
            {
                throw new Exception("Recieved message size error");
            }
            var responseCode = (int)answer[10];
            responseCode = responseCode << 8;
            responseCode = responseCode | answer[9];
            if (responseCode != 0)
            {
                throw new Exception("Response code 0x" + responseCode.ToString("X"));
            }
        }

        public void WriteMerker(int address, bool data)
        {
            WriteBit(address, data, 0x90);
        }
        
        public void WriteLMerker(int address, bool data)
        {
            WriteBit(address, data, 0x92);
        }

        private void WriteBit(int address, bool data, byte code)
        {
            var msg = new byte[]
            {
                0x50, 0x00, //Header
                _networkNumber, //Network number
                _pcNumber, //PC number
                0xFF, 0x03, //Dest module I/O number
                0x00, //Dest module station number
                0x0D, 0x00, //request length
                0x0A, 0x00, //CPU timer
                0x01, 0x14, //Command
                0x01, 0x00, //Subcommand
                (byte) (address & 0xFF),
                (byte) ((address & 0xFF00) >> 8),
                (byte) ((address & 0xFF0000) >> 16), //Address
                code,
                0x01, 0x00, // number of device points
                data ? (byte) 0x10 : (byte) 0x00 // data
            };
            _tcpHelper.Write(msg);
            const int messageLengthToReceive = 11;
            var answer = _tcpHelper.Read(messageLengthToReceive);
            if (answer == null)
            {
                throw new Exception("Message not received");
            }
            if (answer.Count != messageLengthToReceive)
            {
                throw new Exception("Recieved message size error");
            }
            var responseCode = (int)answer[10];
            responseCode = responseCode << 8;
            responseCode = responseCode | answer[9];
            if (responseCode != 0)
            {
                throw new Exception("Response code 0x" + responseCode.ToString("X"));
            }
        }
    }
}
