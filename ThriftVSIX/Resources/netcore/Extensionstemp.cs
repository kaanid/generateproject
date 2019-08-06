using Fanews.Thrift.ThriftBase;
using Microsoft.Extensions.DependencyInjection;

namespace $dllname$
{
    public static class Extensions
    {
        public static IServiceCollection Add$serviceclassname$Thrift(this IServiceCollection services)
        {
            services.AddSingleton<IThriftClient, ThriftClient>();
            services.AddTransient(_ => _.GetRequiredService<IThriftClient>().ClientOne);
            return services;
        }

        public static IServiceCollection Add$serviceclassname$Thrift(this IServiceCollection services, ThriftClientConfig config)
        {
            $dllname$.ThriftClient.Init(config);
            services.AddSingleton<IThriftClient, ThriftClient>();
            services.AddTransient(_ => _.GetRequiredService<IThriftClient>().ClientOne);
            return services;
        }

    }
}
