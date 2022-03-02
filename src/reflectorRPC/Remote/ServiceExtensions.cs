using Microsoft.Extensions.DependencyInjection;

namespace reflectorRPC.Remote
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddGenericRpcEndpoint(this IServiceCollection services)
        {
            services.AddSingleton<IGenericRpcEndpoint>(new GenericRpcEndpoint(services));
            
            return services;
        }
    }
}
