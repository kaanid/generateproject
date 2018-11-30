using Fanews.Thrift.ThriftBase;
using Fanews.Thrift.ThriftClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static $dllname$.$serviceclassname$;

namespace $dllname$
{
    public interface IThriftClient
    {
        IAsync C { get; }
    }
    public partial class ThriftClient
    {
        private static string visitAppName = "App";
        private const string configFileName= "configs/$dllname$Service.json";
        private static ILogger<ThriftClient> _log = new LoggerFactory()
            .AddConsole()
            .AddDebug()
            .CreateLogger<ThriftClient>();
        public static ThriftClientConfig config {private set;get;}

        public static void Init(ThriftClientConfig thriftClientConfig, string formAppName = "App")
        {
            if (config != null)
                return;

            if (thriftClientConfig == null)
                throw new ArgumentNullException(nameof(thriftClientConfig), "thriftClientConfig is null");

            if (string.IsNullOrWhiteSpace(thriftClientConfig.Name) || thriftClientConfig.Port == 0)
                throw new ArgumentNullException("thriftClientConfig.Name is null or  thriftClientConfig.Port is null");

            if (thriftClientConfig.Consul == null && string.IsNullOrWhiteSpace(thriftClientConfig.IPHost))
            {
                throw new ArgumentNullException("Consul is null && IPHost is null");
            }

            config = thriftClientConfig;
            visitAppName = formAppName;
        }

        private static void LoadConf()
        {
            if (config != null)
            {
                return ;
            }

            var _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile(configFileName, false, false)
                .Build();

            visitAppName = _config["AppName"]?.ToString() ?? visitAppName;
            config = _config.Get<ThriftClientConfig>();
            if (config == null)
            {
                throw new ArgumentNullException("ThriftClientConfig");
            }
        
        }

        public static Task<IAsync> ClientAsync
        {
            get
            {
                LoadConf();

                return ClientStartup.GetByCache<Client, IAsync>(config, visitAppName, true, _log);
            }
        }

        public static IAsync Client
        {
            get
            {
                return ClientAsync.GetAwaiter().GetResult();
            }
        }

        public IAsync C
        {
            get
            {
                return Client;
            }
        }

        public static IAsync ClientOne
        {
            get
            {
                LoadConf();
                return ClientStartup.Get<Client>(config);
            }
        }
}
}
