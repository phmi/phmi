namespace PHmiIoDevice.Modbus.Configuration
{
    public class TcpConfig: EnetConfig
    {
        public const string Name = "Tcp";

        public TcpConfig() : base(Name)
        {
        }
    }
}
