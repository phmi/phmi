using System;
using System.Collections.Generic;

namespace PHmiIoDevice.Melsec.Implementation
{
    internal interface IMelsec: IDisposable
    {
        void Open();

        List<byte> ReadMerkers(int address, int length);

        List<byte> ReadLMerkers(int address, int length);

        List<byte> ReadRegisters(int address, int length);

        void WriteRegisters(int address, List<byte> data);

        void WriteMerker(int address, bool data);

        void WriteLMerker(int address, bool data);

        int MaxReadLength { get; }

        int MaxWriteLength { get; }

        int MCount { get; }

        int LCount { get; }

        int DCount { get; }
    }
}
