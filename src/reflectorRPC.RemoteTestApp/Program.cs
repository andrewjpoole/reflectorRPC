// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using reflectorRPC.Remote;
using reflectorRPC.RemoteTestApp;
using reflectorRPC.TestAppContracts;

Console.WriteLine("Remote side TestApp starting...");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<ITestRemoteService, TestRemoteService>(); 

        // Add the services to listen and proxy calls from remote clients
        services.AddGenericRpcEndpoint(); 

    })
    .Build();

// wake up the genericRpcEndpoint, TODO this should possibly be an IHostedService etc at some point
var x = host.Services.GetService(typeof(IGenericRpcEndpoint));

await host.StartAsync(CancellationToken.None);

Console.WriteLine("Remote side TestApp running");
Console.ReadKey();

