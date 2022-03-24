using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using reflectorRPC.Caching;
using reflectorRPC.Shared;

namespace reflectorRPC.Remote;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRemoteRpcService, RemoteRpcService>();
        services.AddSingleton<IDictionaryCache<string,MethodInvocationTypes>>(new DictionaryCache<string, MethodInvocationTypes>());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Hello from generic RPC endpoint");
            });

            endpoints.MapPost("/rpc", async context =>
            {
                var rpcService = (IRemoteRpcService)context.RequestServices.GetService(typeof(IRemoteRpcService))! ??
                                 throw new ApplicationException("IRemoteRpcService service is not registered in DI");
                
                var requestContentJson = await new StreamReader(context.Request.Body).ReadToEndAsync();
                var requestContent = JsonConvert.DeserializeObject<RpcRequestContent>(requestContentJson);

                var responseContent = rpcService.Process(requestContent);
                var responseContentJson = JsonConvert.SerializeObject(responseContent);

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseContentJson);
            });
        });
    }
}