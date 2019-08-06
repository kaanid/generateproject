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

        IAsync ClientOne { get; }
    }

    public partial class ThriftClient : IThriftClient
    {
        private static string visitAppName = "App";
        private const string configFileName = "Configs/Fanews.UserManage.ThriftService.json";
        private readonly ILogger<ThriftClient> _logger;
        private readonly IConfiguration _configuration;
        public static ThriftClientConfig config { private set; get; }

        public ThriftClient(ILogger<ThriftClient> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _instance = this;
        }

        private static ThriftClient _instance;
        public static ThriftClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    ILogger<ThriftClient> _log = new LoggerFactory()
                    .AddConsole()
                    .AddDebug()
                    .CreateLogger<ThriftClient>();

                    var _config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile("appsettings.Development.json", true)
                    .AddJsonFile("appsettings.Production.json", true)
                    .AddJsonFile(configFileName, false, false)
                    .Build();

                    _instance = new ThriftClient(_log, _config);
                }
                return _instance;
            }
        }

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

            SetFreeEvent();
        }

        private void LoadConf()
        {
            if (config != null)
            {
                return;
            }

            visitAppName = _configuration["AppName"]?.ToString() ?? visitAppName;
            config = _configuration.Get<ThriftClientConfig>();
            if (config == null)
            {
                throw new ArgumentNullException("ThriftClientConfig");
            }

            SetFreeEvent();
        }

        private static void SetFreeEvent()
        {
            $dllname$.$serviceclassname$.Client.ExcetinedEvent += (method, clinet) =>
            {
                ClientStartup.SetFree(typeof(Client), clinet);
            };
        }

        public Task<IAsync> ClientAsync
        {
            get
            {
                LoadConf();

                return ClientStartup.GetByPoolAsync<Client, IAsync>(config, visitAppName, _log);
            }
        }

        public IAsync Client
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

        public IAsync ClientOne
        {
            get
            {
                LoadConf();
                return ClientStartup.Get<Client>(config);
            }
        }
    }
}
