using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using PHmiIoDevice.Modbus.BytesConverters;
using PHmiIoDevice.Modbus.Configuration;
using PHmiIoDevice.Modbus.Implementation;
using PHmiIoDevice.Modbus.ReadInfos;
using PHmiIoDevice.Modbus.WriteInfos;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Modbus
{
    public class ModbusIoDevice: IIoDevice
    {
        private readonly ModbusBase _mb;
        private readonly byte _defaultAddress;
        private readonly IBytesToRegistersBytesConverter _bytesToRegistersConverter;
        private readonly int _tryCount;

        public ModbusIoDevice(string options)
        {
            var config = ConfigHelper.GetConfig(options);
            _defaultAddress = config.DefaultAddress;
            switch (config.BytesOrder)
            {
                case BytesOrder.HL:
                    _bytesToRegistersConverter = new HLBytesConverter();
                    break;
                case BytesOrder.LH:
                    _bytesToRegistersConverter = new LHBytesConverter();
                    break;
                default:
                    throw new NotSupportedException(string.Format("Bytes order {0} is not supported", config.BytesOrder));
            }
            var comConfig = config as ComConfig;
            if (comConfig != null)
            {
                _tryCount = comConfig.TryCount;
                var port = new SerialPort(comConfig.PortName)
                {
                    BaudRate = comConfig.BaudRate,
                    DataBits = comConfig.DataBits,
                    Parity = comConfig.Parity,
                    StopBits = comConfig.StopBits
                };
                var portHelper = new SerialPortHelper(port, comConfig.Timeout, comConfig.MessageEndTimeout);
                if (comConfig is RtuConfig)
                {
                    _mb = new ModbusRTU(portHelper);
                }
                else if (comConfig is AsciiConfig)
                {
                    _mb = new ModbusASCII(portHelper);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Config \"{0}\" is not supported", config.ConfigName));
                }
                return;
            }
            var enetConfig = config as EnetConfig;
            if (enetConfig != null)
            {
                _tryCount = 1;
                var tcpHelper = new TcpClientHelper(enetConfig.Address, enetConfig.Port, config.Timeout, config.MessageEndTimeout);
                if (enetConfig is RTUviaTCPConfig)
                {
                    _mb = new ModbusRTU(tcpHelper);
                }
                else if (enetConfig is AsciiViaTcpConfig)
                {
                    _mb = new ModbusASCII(tcpHelper);
                }
                else if (enetConfig is TcpConfig)
                {
                    _mb = new ModbusTCP(tcpHelper);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Config \"{0}\" is not supported", config.ConfigName));
                }
                return;
            }
            throw new NotSupportedException(string.Format("Config \"{0}\" is not supported", config.ConfigName));
        }

        public void Open()
        {
            _mb.Open();
        }

        public void Dispose()
        {
            _mb.Close();
        }

        private const string DevicePattern = @"^([0-9]{1,3}:){0,1}([0,1,3,4])([0-9]{1,5})$";

        private void GetParametersReadInfo(
            IEnumerable<ReadParameter> parameters,
            out Dictionary<byte, SlaveReadInfo> infos,
            out List<KeyValuePair<ReadParameter, byte?>> paramSlaveReadAddresses)
        {
            infos = new Dictionary<byte, SlaveReadInfo>();
            var readParameters = parameters as ReadParameter[] ?? parameters.ToArray();
            paramSlaveReadAddresses = new List<KeyValuePair<ReadParameter, byte?>>(readParameters.Count());
            foreach (var parameter in readParameters)
            {
                var regex = new Regex(DevicePattern);
                var match = regex.Match(parameter.Address);
                if (!match.Success)
                {
                    throw new Exception(parameter.Address + " is not a valid device address");
                }

                #region Get slave address

                var slaveAddress = _defaultAddress;
                var match1 = match.Groups[1].ToString();
                if (!string.IsNullOrEmpty(match1) && match1.Length > 1)
                {
                    int i;
                    if (int.TryParse(match1.Substring(0, match1.Length - 1), out i))
                    {
                        if (i >= byte.MinValue && i <= byte.MaxValue)
                        {
                            slaveAddress = (byte)i;
                        }
                        else
                        {
                            throw new Exception(parameter.Address + ": slave address incorrect");
                        }
                    }
                }

                paramSlaveReadAddresses.Add(new KeyValuePair<ReadParameter, byte?>(parameter, slaveAddress));

                #endregion

                #region Get device address

                ushort deviceAddress;
                if (!ushort.TryParse(match.Groups[3].ToString(), out deviceAddress) || deviceAddress == 0)
                {
                    throw new Exception(parameter.Address + ": device index not supported");
                }
                deviceAddress -= 1;
                var match2 = match.Groups[2].ToString();
                int? length;
                if (parameter.ValueType == typeof(bool))
                {
                    if (match2 != "0" && match2 != "1")
                    {
                        throw new Exception(
                            "Type " + parameter.ValueType.Name + " is not supported for" + parameter.Address);
                    }
                    length = 1;
                }
                else
                {
                    if (match2 != "3" && match2 != "4")
                    {
                        throw new Exception(
                            "Type " + parameter.ValueType.Name + " is not supported for" + parameter.Address);
                    }
                    length = TypeToRegistersCount(parameter.ValueType);
                    if (length == null)
                    {
                        throw new Exception(
                            "Type " + parameter.ValueType.Name + " is not supported for" + parameter.Address);
                    }
                }

                #endregion

                #region Check device index

                var deviceLength = length.Value;
                if (deviceLength + deviceAddress - 1 > ushort.MaxValue)
                {
                    throw new Exception(parameter.Address + ": device index is out of range");
                }

                #endregion

                #region ReadInfo

                SlaveReadInfo readInfo;
                if (!infos.TryGetValue(slaveAddress, out readInfo))
                {
                    readInfo = new SlaveReadInfo();
                    infos.Add(slaveAddress, readInfo);
                }

                #endregion

                #region Fill ReadInfo

                switch (match2)
                {
                    case "0":
                        if (!readInfo.Coils.Contains(deviceAddress))
                            readInfo.Coils.Add(deviceAddress);
                        readInfo.CoilsAddresses.Add(parameter, deviceAddress);
                        break;
                    case "1":
                        if (!readInfo.Contacts.Contains(deviceAddress))
                            readInfo.Contacts.Add(deviceAddress);
                        readInfo.ContactsAddresses.Add(parameter, deviceAddress);
                        break;
                    case "3":
                        var ir = new KeyValuePair<ushort, int>(deviceAddress, deviceLength);
                        if (!readInfo.InputRegisters.Contains(ir))
                            readInfo.InputRegisters.Add(ir);
                        readInfo.InputRegistersAddresses.Add(parameter, ir);
                        break;
                    case "4":
                        var hr = new KeyValuePair<ushort, int>(deviceAddress, deviceLength);
                        if (!readInfo.HoldingRegisters.Contains(hr))
                            readInfo.HoldingRegisters.Add(hr);
                        readInfo.HoldingRegistersAddresses.Add(parameter, hr);
                        break;
                }

                #endregion
            }
        }

        private static void SortReadInfoDevices(SlaveReadInfo readInfo)
        {
            readInfo.Coils.Sort();
            readInfo.Contacts.Sort();
            Comparison<KeyValuePair<ushort, int>> comparison =
                (c1, c2) =>
                {
                    if (c1.Key > c2.Key)
                        return 1;
                    if (c1.Key < c2.Key)
                        return -1;
                    if (c1.Value > c2.Value)
                        return -1;
                    if (c1.Value < c2.Value)
                        return 1;
                    return 0;
                };
            readInfo.InputRegisters.Sort(comparison);
            readInfo.HoldingRegisters.Sort(comparison);
        }

        public object[] Read(ReadParameter[] readParameters)
        {
            for (var i = 1; i <= _tryCount; i++)
            {
                try
                {
                    return ReadOnce(readParameters);
                }
                catch
                {
                    if (i >= _tryCount)
                        throw;
                }
            }
            throw new NotSupportedException();
        }

        public object[] ReadOnce(ReadParameter[] readParameters)
        {
            if (readParameters == null)
                return null;
            var values = new List<object>();
            
            Dictionary<byte, SlaveReadInfo> readInfos;
            List<KeyValuePair<ReadParameter, byte?>> readParamSlaveAddresses;
            GetParametersReadInfo(readParameters, out readInfos, out readParamSlaveAddresses);

            var result = new Dictionary<byte, SlaveReadResult>();
            foreach (var slaveReadInfo in readInfos)
            {
                var slaveAddress = slaveReadInfo.Key;
                var r = new SlaveReadResult();
                result.Add(slaveAddress, r);
                var readInfo = slaveReadInfo.Value;
                SortReadInfoDevices(readInfo);

                r.CoilsData = new Dictionary<ushort, bool>(readInfo.Coils.Count);
                r.ContactsData = new Dictionary<ushort, bool>(readInfo.Contacts.Count);
                r.InputRegistersData = new Dictionary<KeyValuePair<ushort, int>, byte[]>(readInfo.InputRegisters.Count);
                r.HoldingRegistersData = new Dictionary<KeyValuePair<ushort, int>, byte[]>(readInfo.HoldingRegisters.Count);
                ReadBits(readInfo.Coils, slaveAddress, r.CoilsData, _mb.ReadCoils, "discrete coils");
                ReadBits(readInfo.Contacts, slaveAddress, r.ContactsData, _mb.ReadContacts, "discrete contacts");
                ReadRegisters(readInfo.InputRegisters, slaveAddress, r.InputRegistersData,
                    _mb.ReadInputRegisters, "input registers");
                ReadRegisters(readInfo.HoldingRegisters, slaveAddress, r.HoldingRegistersData,
                    _mb.ReadHoldingRegisters, "holding registers");
            }

            foreach (var k in readParamSlaveAddresses)
            {
                if (k.Value == null)
                {
                    values.Add(null);
                    continue;
                }
                var slaveAddress = k.Value.Value;
                SlaveReadResult r;
                if (!result.TryGetValue(slaveAddress, out r))
                {
                    values.Add(null);
                    continue;
                }
                SlaveReadInfo slaveReadInfo;
                if (readInfos.TryGetValue(slaveAddress, out slaveReadInfo))
                {
                    var readParameter = k.Key;
                    ushort bitAddress;
                    KeyValuePair<ushort, int> registerAddress;
                    if (slaveReadInfo.CoilsAddresses.TryGetValue(readParameter, out bitAddress))
                    {
                        bool value;
                        if (r.CoilsData.TryGetValue(bitAddress, out value))
                        {
                            values.Add(value);
                        }
                        else
                        {
                            values.Add(null);
                        }
                    }
                    else if (slaveReadInfo.ContactsAddresses.TryGetValue(readParameter, out bitAddress))
                    {
                        bool value;
                        if (r.ContactsData.TryGetValue(bitAddress, out value))
                        {
                            values.Add(value);
                        }
                        else
                        {
                            values.Add(null);
                        }
                    }
                    else if (slaveReadInfo.InputRegistersAddresses.TryGetValue(readParameter, out registerAddress))
                    {
                        byte[] value;
                        values.Add(r.InputRegistersData.TryGetValue(registerAddress, out value)
                                       ? GetValue(value, readParameter.ValueType)
                                       : null);
                    }
                    else if (slaveReadInfo.HoldingRegistersAddresses.TryGetValue(readParameter, out registerAddress))
                    {
                        byte[] value;
                        values.Add(r.HoldingRegistersData.TryGetValue(registerAddress, out value)
                                       ? GetValue(value, readParameter.ValueType)
                                       : null);
                    }
                }
                else
                {
                    values.Add(null);
                }
            }

            return values.ToArray();
        }

        private static object GetValue(byte[] bytes, Type type)
        {
            if (type == typeof(short))
            {
                return BitConverter.ToInt16(bytes, 0);
            }
            if (type == typeof(int))
            {
                return BitConverter.ToInt32(bytes, 0);
            }
            if (type == typeof(float))
            {
                return BitConverter.ToSingle(bytes, 0);
            }
            if (type == typeof(ushort))
            {
                return BitConverter.ToUInt16(bytes, 0);
            }
            if (type == typeof(uint))
            {
                return BitConverter.ToUInt32(bytes, 0);
            }
            if (type == typeof(double))
            {
                return BitConverter.ToDouble(bytes, 0);
            }
            return null;
        }

        private static bool GetBitFromByte(byte b, int index)
        {
            var mask = 1 << index;
            return (b & mask) != 0;
        }

        private const int MaxReadBytesLength = 250;
        private const int MaxReadBitsLength = 256;

        private delegate List<byte> ReadDelegate(
            byte slaveAddres, int address, int byteLength, int length);

        private static void ReadBits(
            IEnumerable<ushort> bitsAddresses,
            byte slaveAddress,
            IDictionary<ushort, bool> bitsData,
            ReadDelegate readDel,
            string dataName)
        {
            var readStart = 0;
            var length = 0;
            const int maxLength = MaxReadBitsLength;
            var addresses = new List<ushort>(maxLength);
            foreach (var address in bitsAddresses)
            {
                if (length == 0)
                {
                    readStart = address;
                    length = 1;
                    addresses.Add(address);
                }
                else
                {
                    if (address >= readStart && address < readStart + length)
                    {
                        addresses.Add(address);
                        continue;
                    }
                    var newLength = address - readStart + 1;
                    if (newLength > maxLength)
                    {
                        ReadBitsFinal(
                            slaveAddress, bitsData, readDel, dataName, readStart, length, addresses);
                        readStart = address;
                        length = 1;
                        addresses.Clear();
                    }
                    else
                    {
                        if (length < newLength)
                            length = newLength;
                    }
                    addresses.Add(address);
                }
            }
            if (length > 0)
                ReadBitsFinal(slaveAddress, bitsData, readDel, dataName, readStart, length, addresses);
        }

        private static void ReadBitsFinal(
            byte slaveAddress,
            IDictionary<ushort, bool> bitsData,
            ReadDelegate readDel,
            string dataName,
            int readStart,
            int length,
            IEnumerable<ushort> addresses)
        {
            var byteLength = length / 8;
            if (length % 8 != 0)
                ++byteLength;
            var bytes = readDel.Invoke(slaveAddress, readStart, byteLength, length);
            if (bytes == null || bytes.Count != byteLength)
            {
                throw new Exception(
                    string.Format("Error when reading {0}. SlaveAddress: {1}, Address {2}, Length {3}",
                    dataName, slaveAddress, readStart, length));
            }
            foreach (var address in addresses)
            {
                var index = address - readStart;
                bitsData.Add(address, GetBitFromByte(bytes[index / 8], index % 8));
            }
        }

        private void ReadRegisters(
            IEnumerable<KeyValuePair<ushort, int>> registersAddresses,
            byte slaveAddress,
            IDictionary<KeyValuePair<ushort, int>, byte[]> registersData,
            ReadDelegate readDel,
            string dataName)
        {
            var readStart = 0;
            var length = 0;
            const int maxLength = MaxReadBytesLength/2;
            var addresses = new List<KeyValuePair<ushort, int>>(maxLength);
            foreach (var regAddress in registersAddresses)
            {
                var address = regAddress.Key;
                if (length == 0)
                {
                    readStart = address;
                    length = regAddress.Value;
                    addresses.Add(regAddress);
                }
                else
                {
                    if (address >= readStart && address + regAddress.Value <= readStart + length)
                    {
                        addresses.Add(regAddress);
                        continue;
                    }
                    var newLength = address - readStart + regAddress.Value;
                    if (newLength > maxLength)
                    {
                        ReadRegistersFinal(
                            slaveAddress, registersData, 
                            readDel, dataName, readStart, length, addresses);
                        readStart = address;
                        length = regAddress.Value;
                        addresses.Clear();
                    }
                    else
                    {
                        if (length < newLength)
                            length = newLength;
                    }
                    addresses.Add(regAddress);
                }
            }
            if (length > 0)
                ReadRegistersFinal(
                    slaveAddress, registersData, 
                    readDel, dataName, readStart, length, addresses);
        }

        private void ReadRegistersFinal(
            byte slaveAddress,
            IDictionary<KeyValuePair<ushort, int>, byte[]> registersData,
            ReadDelegate readDel,
            string dataName,
            int readStart,
            int length,
            IEnumerable<KeyValuePair<ushort, int>> addresses)
        {
            var byteLength = length * 2;
            var bytes = readDel.Invoke(slaveAddress, readStart, byteLength, length);
            if (bytes == null || bytes.Count != byteLength)
            {
                throw new Exception(
                    string.Format("Error when reading {0}. SlaveAddress: {1}, Address {2}, Length {3}",
                    dataName, slaveAddress, readStart, length));
            }
            foreach (var address in addresses)
            {
                var index = (address.Key - readStart) * 2;
                registersData.Add(address, _bytesToRegistersConverter.GetBytes(bytes, index, address.Value));
            }
        }

        private const int MaxWriteBytesLength = 246;

        public void Write(WriteParameter[] writeParameters)
        {
            for (var i = 1; i <= _tryCount; i++)
            {
                try
                {
                    WriteOnce(writeParameters);
                }
                catch
                {
                    if (i >= _tryCount)
                        throw;
                }
            }
        }

        public void WriteOnce(WriteParameter[] writeParameters)
        {
            if (writeParameters == null)
                return;
            var result = new bool[writeParameters.Count()];

            Dictionary<byte, SlaveWriteInfo> writeInfos;
            GetParametersWriteInfo(writeParameters, out writeInfos);

            foreach (var slaveWriteInfo in writeInfos)
            {
                var slaveAddress = slaveWriteInfo.Key;
                var writeInfo = slaveWriteInfo.Value;
                SortWriteInfoDevices(writeInfo);
                for (var i = 0; i < writeInfo.Coils.Count; ++i)
                {
                    var coil = writeInfo.Coils[i];
                    var address = coil.Address;
                    const int maxLength = MaxWriteBytesLength * 8;
                    var bools = new List<bool>(maxLength) { coil.WriteParameter.Value as bool? == true };
                    for (var j = 1; j < maxLength && i + j < writeInfo.Coils.Count; ++j)
                    {
                        var c = writeInfo.Coils[i + j];
                        if (c.Address == address + j)
                            bools.Add(c.WriteParameter.Value as bool? == true);
                        else
                            break;
                    }
                    if (_mb.WriteCoils(slaveAddress, address, BoolsToBytes(bools), bools.Count))
                    {
                        for (var j = i; j < bools.Count + i; ++j)
                        {
                            result[writeInfo.Coils[j].Index] = true;
                        }
                    }
                    i += bools.Count - 1;
                }

                for (var i = 0; i < writeInfo.HoldingRegisters.Count; ++i)
                {
                    var hr = writeInfo.HoldingRegisters[i];
                    var address = hr.Address;
                    const int maxLength = MaxWriteBytesLength / 2;
                    var bytes = new byte[MaxWriteBytesLength];
                    var length = address.Value;
                    var valueBytes = GetBytes(hr.WriteParameter.Value);
                    valueBytes = _bytesToRegistersConverter.GetBytes(valueBytes, 0, hr.Address.Value);
                    for (var j = 0; j < valueBytes.Length; ++j)
                    {
                        bytes[j] = valueBytes[j];
                    }
                    var oldI = i;
                    for (var j = 1; i + j < writeInfo.HoldingRegisters.Count; ++j)
                    {
                        var h = writeInfo.HoldingRegisters[i + j];
                        if (h.Address.Key <= address.Key + length)
                        {
                            var newLength = h.Address.Key + h.Address.Value - address.Key;
                            if (newLength < maxLength)
                            {
                                var vBytes = GetBytes(h.WriteParameter.Value);
                                vBytes = _bytesToRegistersConverter.GetBytes(vBytes, 0, h.Address.Value);
                                for (var k = 0; k < vBytes.Length; ++k)
                                {
                                    bytes[k + (h.Address.Key - address.Key) * 2] = vBytes[k];
                                }
                                if (newLength > length)
                                    length = newLength;
                                ++i;
                                continue;
                            }
                        }
                        break;
                    }
                    var byteList = new List<byte>(length * 2);
                    for (var j = 0; j < length * 2; ++j)
                    {
                        byteList.Add(bytes[j]);
                    }
                    if (_mb.WriteHoldingRegisters(slaveAddress, address.Key, byteList, length))
                    {
                        for (var j = oldI; j <= i; ++j)
                        {
                            result[writeInfo.HoldingRegisters[j].Index] = true;
                        }
                    }
                }
            }
        }

        private static byte[] GetBytes(object value)
        {
            return BitConverter.GetBytes((dynamic)value);
        }

        private static List<byte> BoolsToBytes(IList<bool> bools)
        {
            var length = bools.Count / 8;
            if (bools.Count % 8 != 0)
                length++;
            var bytes = new List<byte>(length);
            for (var i = 0; i < length; ++i)
            {
                var b = 0;
                var index = i*8;
                var mask = 1;
                if (i == length - 1)
                    for (var j = 0; j < bools.Count - index; ++j)
                    {
                        if (bools[index + j])
                            b = b | mask;
                        mask = mask << 1;
                    }
                else
                    for (var j = 0; j < 8; ++j)
                    {
                        if (bools[index + j])
                            b = b | mask;
                        mask = mask << 1;
                    }
                bytes.Add((byte)b);
            }
            return bytes;
        }

        private void GetParametersWriteInfo(
            IEnumerable<WriteParameter> parameters, out Dictionary<byte, SlaveWriteInfo> infos)
        {
            infos = new Dictionary<byte, SlaveWriteInfo>();
            var index = -1;
            foreach (var parameter in parameters)
            {
                ++index;
                var regex = new Regex(DevicePattern);
                var match = regex.Match(parameter.Address);
                if (!match.Success)
                {
                    throw new Exception(parameter.Address + " is not a valid device address");
                }

                #region Get slave address

                var slaveAddress = _defaultAddress;
                var match1 = match.Groups[1].ToString();
                if (!string.IsNullOrEmpty(match1) && match1.Length > 1)
                {
                    int i;
                    if (int.TryParse(match1.Substring(0, match1.Length - 1), out i))
                    {
                        if (i >= byte.MinValue && i <= byte.MaxValue)
                        {
                            slaveAddress = (byte)i;
                        }
                        else
                        {
                            throw new Exception(parameter.Address + ": slave address incorrect");
                        }
                    }
                }

                #endregion

                #region Get device address

                ushort deviceAddress;
                if (!ushort.TryParse(match.Groups[3].ToString(), out deviceAddress) || deviceAddress == 0)
                {
                    throw new Exception(parameter.Address + ": device index not supported");
                }
                deviceAddress -= 1;
                var match2 = match.Groups[2].ToString();
                int? length;
                if (parameter.Value is bool)
                {
                    if (match2 != "0" && match2 != "1")
                    {
                        throw new Exception("Type " + parameter.Value.GetType().Name + " is not supported for " + parameter.Address);
                    }
                    length = 1;
                }
                else
                {
                    if (match2 != "3" && match2 != "4")
                    {
                        throw new Exception("Type " + parameter.Value.GetType().Name + " is not supported for " + parameter.Address);
                    }
                    length = TypeToRegistersCount(parameter.Value.GetType());
                    if (length == null)
                    {
                        throw new Exception("Type " + parameter.Value.GetType().Name + " is not supported for " + parameter.Address);
                    }
                }

                #endregion

                #region Check device index

                var deviceLength = length.Value;
                if (deviceLength + deviceAddress - 1 > ushort.MaxValue)
                {
                    throw new Exception(parameter.Address + ": device index is out of range");
                }

                #endregion

                #region WriteInfo

                SlaveWriteInfo writeInfo;
                if (!infos.TryGetValue(slaveAddress, out writeInfo))
                {
                    writeInfo = new SlaveWriteInfo();
                    infos.Add(slaveAddress, writeInfo);
                }

                #endregion

                #region Fill WriteInfo

                switch (match2)
                {
                    case "0":
                        var coil = new CoilWriteInfo
                                       {
                                           Address = deviceAddress,
                                           Index = index,
                                           WriteParameter = parameter
                                       };
                        writeInfo.Coils.Add(coil);
                        break;
                    case "4":
                        var hr = new HoldingRegisterWriteInfo
                                     {
                                         Address = new KeyValuePair<ushort, int>(deviceAddress, deviceLength),
                                         Index = index,
                                         WriteParameter = parameter
                                     };
                        writeInfo.HoldingRegisters.Add(hr);
                        break;
                }

                #endregion
            }
        }

        private static void SortWriteInfoDevices(SlaveWriteInfo info)
        {
            Comparison<CoilWriteInfo> coilComparison =
                (c1, c2) =>
                    {
                        if (c1.Address > c2.Address)
                            return 1;
                        if (c1.Address < c2.Address)
                            return -1;
                        return 0;
                    };
            info.Coils.Sort(coilComparison);
            Comparison<HoldingRegisterWriteInfo> hrComparison =
                (c1, c2) =>
                {
                    if (c1.Address.Key > c2.Address.Key)
                        return 1;
                    if (c1.Address.Key < c2.Address.Key)
                        return -1;
                    if (c1.Address.Value > c2.Address.Value)
                        return -1;
                    if (c1.Address.Value < c2.Address.Value)
                        return 1;
                    return 0;
                };
            info.HoldingRegisters.Sort(hrComparison);
        }

        private static int? TypeToRegistersCount(Type type)
        {
            if (type == typeof(short))
                return 1;
            if (type == typeof(int))
                return 2;
            if (type == typeof(float))
                return 2;
            if (type == typeof(ushort))
                return 1;
            if (type == typeof(uint))
                return 2;
            if (type == typeof(double))
                return 4;
            return null;
        }
    }
}