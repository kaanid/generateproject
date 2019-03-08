using Fanews.Thrift.ThriftBase;
using Fanews.Thrift.ThriftClientNet45;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Thrift.Protocol;
using static $dllname$.$serviceclassname$;

namespace $dllname$
{
    public interface IThriftClient
    {
        Iface C { get; }
    }

    public class ThriftClient:IThriftClient
    {
        private static string visitAppName = "App";
        private const string configFileName= "Configs/$dllname$Service.json";

        public static ThriftClientConfig config {private set;get;}

        public static void Init(ThriftClientConfig thriftClientConfig,string formAppName="App")
        {
            if (config != null)
                return;

            if (thriftClientConfig == null)
                throw new ArgumentNullException(nameof(thriftClientConfig),"thriftClientConfig is null");

            if (string.IsNullOrWhiteSpace(thriftClientConfig.Name) || thriftClientConfig.Port == 0)
                throw new ArgumentNullException("thriftClientConfig.Name is null or  thriftClientConfig.Port is null");

            if (thriftClientConfig.Consul==null && string.IsNullOrWhiteSpace(thriftClientConfig.IPHost))
            {
                throw new ArgumentNullException("Consul is null && IPHost is null");
            }

            config = thriftClientConfig;
            visitAppName = formAppName;

            SetFreeEvent();
        }

        private static void LoadConf()
        {
            if (config != null)
                return;

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", configFileName);
                if (!File.Exists(filePath))
                {
                    throw new ArgumentNullException($"fild nofound path:{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName)} path:{filePath}");
                }
            }

            config = ThriftConfig.GetConfig(filePath);
            if (config == null)
            {
                throw new ArgumentNullException($"config is null path:{filePath}");
            }

            visitAppName = ConfigurationManager.AppSettings.Get("AppName") ?? visitAppName;

            SetFreeEvent();
        }

        private static void SetFreeEvent()
    {
            $dllname$.$serviceclassname$.Client.ExcetinedEvent += (method, clinet) =>
            {
                ClientStartup.SetFree(typeof(Client), clinet);
            };
        }

        private static Task<Client> ClientAsync
        {
            get
            {
                LoadConf();

                return ClientStartup.GetByPoolAsync<Client>(config, visitAppName, (model) => {
                    if (model == null)
                        return false;

                    var client = model.Client as Client;
                    if (client == null)
                        return false;

                    client.OutputProtocol.WriteMessageBegin(new TMessage("livecheck", TMessageType.Oneway, 0));
                    client.OutputProtocol.WriteMessageEnd();
                    client.OutputProtocol.Transport.Flush();

                    return true;
                });
            }
        }

        public static Iface Client
        {
            get
            {
                return Task.Run(async () => {
                    var m= await ClientAsync;
                    return m;
                }).Result;
            }
        }

        public Iface C
        {
            get
            {
                return Client;
            }
        }

        public static Client ClientOne
        {
            get
            {
                LoadConf();
                return ClientStartup.Get<Client>(config);
            }
        }
    }
}
