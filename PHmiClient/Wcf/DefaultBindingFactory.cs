using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace PHmiClient.Wcf
{
    internal class DefaultBindingFactory : IDefaultBindingFactory
    {
        public Binding Create()
        {
            return new WSHttpBinding
                {
                    CloseTimeout = TimeSpan.FromSeconds(10),
                    OpenTimeout = TimeSpan.FromSeconds(20),
                    ReceiveTimeout = TimeSpan.FromSeconds(10),
                    SendTimeout = TimeSpan.FromSeconds(10),
                    BypassProxyOnLocal = false,
                    TransactionFlow = false,
                    HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                    MaxBufferPoolSize = 524288,
                    MaxReceivedMessageSize = 16777216,
                    MessageEncoding = WSMessageEncoding.Text,
                    TextEncoding = Encoding.UTF8,
                    UseDefaultWebProxy = true,
                    AllowCookies = false,
                    ReaderQuotas = new XmlDictionaryReaderQuotas
                        {
                            MaxDepth = 32,
                            MaxStringContentLength = 8192,
                            MaxArrayLength = 524288,
                            MaxBytesPerRead = 4096,
                            MaxNameTableCharCount = 16384
                        },
                    ReliableSession = new OptionalReliableSession
                        {
                            Ordered = true,
                            InactivityTimeout = TimeSpan.FromMinutes(10),
                            Enabled = false
                        },
                    Security = new WSHttpSecurity
                        {
                            Mode = SecurityMode.Message,
                            Transport = new HttpTransportSecurity
                                {
                                    ClientCredentialType = HttpClientCredentialType.Windows,
                                    ProxyCredentialType = HttpProxyCredentialType.None,
                                    Realm = ""
                                },
                            Message = new NonDualMessageSecurityOverHttp
                                {
                                    ClientCredentialType = MessageCredentialType.Windows,
                                    NegotiateServiceCredential = true,
                                    AlgorithmSuite =
                                        System.ServiceModel.Security.SecurityAlgorithmSuite.Default
                                }
                        }
                };
        }
    }
}
