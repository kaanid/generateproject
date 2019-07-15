using Fanews.Thrift.ThriftBase;
using Microsoft.Extensions.DependencyInjection;

namespace $dllname$
{
    public static class Extensions
    {
        public static IServiceCollection AddThrift(this IServiceCollection services)
        {
            services.AddTransient(_ => $dllname$.ThriftClient.Client);
            return services;
        }

        public static IServiceCollection AddThrift(this IServiceCollection services, ThriftClientConfig config)
    {
            $dllname$.ThriftClient.Init(config);
            services.AddTransient(_ => $dllname$.ThriftClient.Client);
            return services;
        }

    }
}
