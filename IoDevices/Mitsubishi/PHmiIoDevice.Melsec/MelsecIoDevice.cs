using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using PHmiIoDevice.Melsec.Configuration;
using PHmiIoDevice.Melsec.Implementation;
using PHmiIoDevice.Melsec.WriteInfos;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Melsec
{
    public class MelsecIoDevice : IIoDevice
    {
        private readonly IMelsec _melsec;
        private readonly int _tryCount;

        public MelsecIoDevice(string options)
        {
            _tryCount = 1;
            var config = ConfigHelper.GetConfig(options);
            if (config is FxComConfig)
            {
                var comConfig = (FxComConfig) config;
                _tryCount = comConfig.TryCount;
                var port = new SerialPort(comConfig.PortName)
                {
                    BaudRate = comConfig.BaudRate,
                    DataBits = comConfig.DataBits,
                    Parity = comConfig.Parity,
                    StopBits = comConfig.StopBits
                };
                _melsec = new FxCom(port, comConfig.Timeout, comConfig.MessageEndTimeout);
            }
            else if (config is FxEnetConfig)
            {
                var enetConfig = (FxEnetConfig) config;
                _melsec = new FxEnet(enetConfig.Address, enetConfig.Port, enetConfig.Timeout, enetConfig.MessageEndTimeout);
            }
            else if (config is QConfig)
            {
                var qConfig = (QConfig) config;
                _melsec = new Q(qConfig.Address, qConfig.Port, qConfig.PcNumber, qConfig.NetworkNumber,
                    qConfig.Timeout, qConfig.MessageEndTimeout);
            }
            else
            {
                throw new NotSupportedException(string.Format("Config \"{0}\" is not supported", config.ConfigName));
            }
        }

        public void Dispose()
        {
            _melsec.Dispose();
        }

        public void Open()
        {
            _melsec.Open();
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

        private object[] ReadOnce(ReadParameter[] readParameters)
        {
            if (readParameters == null)
                return null;
            var values = new List<object>();

            var merkers = new List<int>();
            var merkerParameters = new Dictionary<ReadParameter, int>();
            var lMerkers = new List<int>();
            var lMerkerParameters = new Dictionary<ReadParameter, int>();
            var registers = new List<KeyValuePair<int, int>>();
            var registerParameters = new Dictionary<ReadParameter, KeyValuePair<int, int>>();

            foreach (var parameter in readParameters)
            {
                var regex = new Regex(Pattern);
                var match = regex.Match(parameter.Address);
                if (!match.Success)
                {
                    throw new Exception(parameter.Address + " is not a valid device address");
                }

                #region Get device address

                var letter = match.Groups[1].ToString().ToUpper();
                var index = int.Parse(match.Groups[2].ToString());

                int? length;
                if (parameter.ValueType == typeof(bool))
                {
                    if (letter != "M" && letter != "L")
                    {
                        throw new Exception("Type " + parameter.ValueType.Name + " is not supported for" + parameter.Address);
                    }
                    length = 1;
                }
                else
                {
                    if (letter != "D")
                    {
                        throw new Exception("Type " + parameter.ValueType.Name + " is not supported for" + parameter.Address);
                    }
                    length = TypeToRegistersCount(parameter.ValueType);
                    if (length == null)
                    {
                        throw new Exception("Type " + parameter.ValueType.Name + " is not supported for" + parameter.Address);
                    }
                }

                #endregion

                #region Check device index

                var deviceLastIndex = length.Value + index - 1;
                if ((letter == "M" && deviceLastIndex >= _melsec.MCount)
                    ||
                    (letter == "L" && deviceLastIndex >= _melsec.LCount)
                    ||
                    (letter == "D" && deviceLastIndex >= _melsec.DCount))
                {
                    throw new Exception(parameter.Address + ": device index is out of range");
                }

                #endregion

                #region Fill ReadInfo

                switch (letter)
                {
                    case "M":
                        if (!merkers.Contains(index))
                            merkers.Add(index);
                        if (!merkerParameters.ContainsKey(parameter))
                            merkerParameters.Add(parameter, index);
                        break;
                    case "L":
                        if (!lMerkers.Contains(index))
                            lMerkers.Add(index);
                        if (!lMerkerParameters.ContainsKey(parameter))
                            lMerkerParameters.Add(parameter, index);
                        break;
                    case "D":
                        var ir = new KeyValuePair<int, int>(index, length.Value);
                        if (!registers.Contains(ir))
                            registers.Add(ir);
                        if (!registerParameters.ContainsKey(parameter))
                            registerParameters.Add(parameter, ir);
                        break;
                }

                #endregion
            }

            #region Sort

            merkers = merkers.OrderBy(index => index).ToList();
            lMerkers = lMerkers.OrderBy(index => index).ToList();
            registers = registers.OrderBy(r => r.Key).ThenByDescending(r => r.Value).ToList();

            #endregion

            var merkersData = new Dictionary<int, bool>(merkers.Count);
            ReadBits(merkers, merkersData, _melsec.ReadMerkers, "M");
            var lMerkersData = new Dictionary<int, bool>(lMerkers.Count);
            ReadBits(lMerkers, lMerkersData, _melsec.ReadLMerkers, "L");
            var registersData = new Dictionary<KeyValuePair<int, int>, byte[]>(registers.Count);
            ReadRegisters(registers, registersData, _melsec.ReadRegisters);

            foreach (var parameter in readParameters)
            {
                int merkerIndex;
                int lMerkerIndex;
                KeyValuePair<int, int> registerAddr;
                if (merkerParameters.TryGetValue(parameter, out merkerIndex))
                {
                    bool value;
                    if (merkersData.TryGetValue(merkerIndex, out value))
                        values.Add(value);
                    else
                        values.Add(null);
                }
                else if (lMerkerParameters.TryGetValue(parameter, out lMerkerIndex))
                {
                    bool value;
                    if (lMerkersData.TryGetValue(lMerkerIndex, out value))
                        values.Add(value);
                    else
                        values.Add(null);
                }
                else if (registerParameters.TryGetValue(parameter, out registerAddr))
                {
                    byte[] value;
                    values.Add(registersData.TryGetValue(registerAddr, out value)
                                   ? GetValue(value, parameter.ValueType)
                                   : null);
                }
                else
                {
                    values.Add(null);
                }
            }

            return values.ToArray();
        }

        private const string Pattern = @"(^[a-zA-Z]+)([\d]+)$";

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

        private delegate List<byte> ReadDelegate(int address, int length);
        
        private void ReadBits(
            IEnumerable<int> bitsAddresses, IDictionary<int, bool> bitsData, ReadDelegate readDel, string label)
        {
            var readStart = 0;
            var length = 0;
            var addresses = new List<int>(_melsec.MaxReadLength*16);
            foreach (var address in bitsAddresses)
            {
                if (length == 0)
                {
                    readStart = address/16*16;
                    length = 16;
                    addresses.Add(address);
                }
                else
                {
                    if (address >= readStart && address < readStart + length)
                    {
                        addresses.Add(address);
                        continue;
                    }
                    var newAddr = address/16*16;
                    var newLength = newAddr + 16 - readStart;
                    var maxMerkerLength = _melsec.MaxReadLength*16;
                    if (newLength > maxMerkerLength)
                    {
                        ReadBitsFinal(bitsData, readDel, readStart, length, addresses, label);
                        readStart = newAddr;
                        length = 16;
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
            {
                ReadBitsFinal(bitsData, readDel, readStart, length, addresses, label);
            }
        }

        private static void ReadBitsFinal(
            IDictionary<int, bool> bitsData, ReadDelegate readDel, int readStart,
            int length, IEnumerable<int> addresses, string label)
        {
            var bytes = readDel.Invoke(readStart, length);
            if (bytes == null || bytes.Count != length/8)
            {
                throw new Exception(string.Join(Environment.NewLine,
                    string.Format("Error when reading merkers"),
                    string.Format("Address {0}{1}, Length {2}", label, readStart, length)));
            }
            foreach (var address in addresses)
            {
                var index = address - readStart;
                bitsData.Add(address, GetBitFromByte(bytes[index / 8], index % 8));
            }
        }

        private static bool GetBitFromByte(byte b, int index)
        {
            var mask = 1 << index;
            return (b & mask) != 0;
        }

        private void ReadRegisters(
            IEnumerable<KeyValuePair<int, int>> registersAddresses,
            IDictionary<KeyValuePair<int, int>, byte[]> registersData, ReadDelegate readDel)
        {
            var readStart = 0;
            var length = 0;
            var addresses = new List<KeyValuePair<int, int>>(_melsec.MaxReadLength);
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
                    if (newLength > _melsec.MaxReadLength)
                    {
                        ReadRegistersFinal(registersData, readDel, readStart, length, addresses);
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
            {
                ReadRegistersFinal(registersData, readDel, readStart, length, addresses);
            }
        }

        private static void ReadRegistersFinal(
            IDictionary<KeyValuePair<int, int>, byte[]> registersData,
            ReadDelegate readDel, int readStart, int length, IEnumerable<KeyValuePair<int, int>> addresses)
        {
            var bytes = readDel.Invoke(readStart, length);
            if (bytes == null || bytes.Count != length * 2)
            {
                throw new Exception(string.Join(Environment.NewLine,
                    string.Format("Error when reading registers"),
                    string.Format("Address {0}, Length {1}", readStart, length)));
            }
            foreach (var address in addresses)
            {
                var index = (address.Key - readStart) * 2;
                registersData.Add(address, GetBytes(bytes, index, address.Value * 2));
            }
        }

        private static byte[] GetBytes(IList<byte> bytes, int index, int length)
        {
            var result = new byte[length];
            for (var i = 0; i < length; ++i)
            {
                result[i] = bytes[i + index];
            }
            return result;
        }

        public void Write(WriteParameter[] writeParameters)
        {
            for (var i = 1; i <= _tryCount; i++)
            {
                try
                {
                    WriteOnce(writeParameters);
                    return;
                }
                catch
                {
                    if (i >= _tryCount)
                        throw;
                }
            }
        }

        private void WriteOnce(IEnumerable<WriteParameter> writeParameters)
        {
            if (writeParameters == null)
                return;

            var merkers = new List<BitWriteInfo>();
            var lMerkers = new List<BitWriteInfo>();
            var registers = new List<RegisterWriteInfo>();

            var ind = -1;
            foreach (var parameter in writeParameters)
            {
                ind++;
                var regex = new Regex(Pattern);
                var match = regex.Match(parameter.Address);
                if (!match.Success)
                {
                    throw new Exception(parameter.Address + " is not a valid device address");
                }

                #region Get device address

                var letter = match.Groups[1].ToString().ToUpper();
                var index = int.Parse(match.Groups[2].ToString());

                int? length;
                if (parameter.Value is bool)
                {
                    if (letter != "M" && letter != "L")
                    {
                        throw new Exception("Type " + parameter.Value.GetType().Name + " is not supported for" + parameter.Address);
                    }
                    length = 1;
                }
                else
                {
                    if (letter != "D")
                    {
                        throw new Exception("Type " + parameter.Value.GetType().Name + " is not supported for" + parameter.Address);
                    }
                    length = TypeToRegistersCount(parameter.Value.GetType());
                    if (length == null)
                    {
                        throw new Exception("Type " + parameter.Value.GetType().Name + " is not supported for" + parameter.Address);
                    }
                }

                #endregion

                #region Check device index

                var deviceLastIndex = length.Value + index - 1;
                if ((letter == "M" && deviceLastIndex >= _melsec.MCount)
                    ||
                    (letter == "L" && deviceLastIndex >= _melsec.LCount)
                    ||
                    (letter == "D" && deviceLastIndex >= _melsec.DCount))
                {
                    throw new Exception(parameter.Address + ": device index is out of range");
                }

                #endregion

                #region Fill ReadInfo

                switch (letter)
                {
                    case "M":
                        merkers.Add(new BitWriteInfo{Address = index, Index = ind, WriteParameter = parameter});
                        break;
                    case "L":
                        lMerkers.Add(new BitWriteInfo { Address = index, Index = ind, WriteParameter = parameter });
                        break;
                    case "D":
                        var ir = new KeyValuePair<int, int>(index, length.Value);
                        registers.Add(new RegisterWriteInfo{Address = ir, Index = ind, WriteParameter = parameter});
                        break;
                }

                #endregion
            }

            #region Sort

            merkers = merkers.OrderBy(info => info.Address).ToList();
            lMerkers = lMerkers.OrderBy(info => info.Address).ToList();
            registers = registers.OrderBy(r => r.Address.Key).ThenByDescending(r => r.Address.Value).ToList();

            #endregion

            foreach (var merker in merkers)
            {
                _melsec.WriteMerker(merker.Address, merker.WriteParameter.Value as bool? == true);
            }

            foreach (var info in lMerkers)
            {
                _melsec.WriteLMerker(info.Address, info.WriteParameter.Value as bool? == true);
            }

            for (var i = 0; i < registers.Count; ++i)
            {
                var r = registers[i];
                var address = r.Address;
                var bytes = new byte[_melsec.MaxWriteLength*2];
                var length = address.Value;
                var valueBytes = GetBytes(r.WriteParameter.Value);
                for (var j = 0; j < valueBytes.Length; ++j)
                {
                    bytes[j] = valueBytes[j];
                }
                for (var j = 1; i + j < registers.Count; ++j)
                {
                    var h = registers[i + j];
                    if (h.Address.Key <= address.Key + length)
                    {
                        var newLength = h.Address.Key + h.Address.Value - address.Key;
                        if (newLength < _melsec.MaxWriteLength)
                        {
                            var vBytes = GetBytes(h.WriteParameter.Value);
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
                _melsec.WriteRegisters(address.Key, byteList);
            }
        }

        private static byte[] GetBytes(object value)
        {
            return BitConverter.GetBytes((dynamic)value);
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
