using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace reflectorRPC.Remote;

public class GenericRpcEndpoint : IGenericRpcEndpoint
{
    public static IHostBuilder CreateHostBuilder(string[] args, IServiceCollection appServices) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureServices(services =>
            {
                foreach (var serviceDescriptor in appServices)
                {
                    // Copy existing non Microsoft services from host app...
                    if (serviceDescriptor.ServiceType.Namespace.StartsWith("Microsoft"))
                        continue;

                    services.Add(serviceDescriptor);
                }
            });

    public GenericRpcEndpoint(IServiceCollection services)
    {
        var host = CreateHostBuilder(new string[]{}, services).Build();
        host.Run();
    }
}