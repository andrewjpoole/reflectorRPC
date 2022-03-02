// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using reflectorRPC.Remote;
using reflectorRPC.RemoteTestApp;
using reflectorRPC.TestAppContracts;

Console.WriteLine("Hello, World!");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<ITestRemoteService, TestRemoteService>(); // this is the service we want to make remote calls to
        // TODO create extension method to register a Service and specify which methods are allowed to be remoted to for security purposes?
        
        services.AddGenericRpcEndpoint(); // this adds the services to listen and proxy calls from remote clients
        // TODO announce/register this instance Uri in Consul/load balancer/service discovery thing etc for service discovery by clients

    })
    .Build();

// wake up the genericRpcEndpoint, TODO this should possibly be an IHostedService etc at some point
var x = host.Services.GetService(typeof(IGenericRpcEndpoint));

await host.StartAsync(CancellationToken.None);

Console.WriteLine("Remote Service running");
Console.ReadKey();

