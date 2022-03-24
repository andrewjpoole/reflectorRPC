using System.Diagnostics;using System.Runtime.CompilerServices;
using reflectorRPC.Client;
using reflectorRPC.TestAppContracts;

Stopwatch _stopWatch = new();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddSingleton<RpcProxyFactory>();
var app = builder.Build();

var loggerFactory = app.Services.GetService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("clientWebApp");

var rpcProxyFactory = app.Services.GetService<RpcProxyFactory>() ?? throw new ApplicationException("RpcProxyFactory not registered");

void WriteLog(HttpContext context, string message)
{
    _stopWatch.Stop();
    logger?.LogInformation($"{message} {_stopWatch.ElapsedMilliseconds}ms");
}

app.MapGet("/", () => "Hello World!");
app.MapGet("/test", async context =>
{
    _stopWatch.Restart();
    
    var remotingTestProxy = rpcProxyFactory.Create<ITestRemoteService>();

    var result = remotingTestProxy.Echo("andrew1");
    WriteLog(context, "Called Echo method on proxy object");
    
    await context.Response.WriteAsync(result);
});
app.MapGet("/test-async", async context =>
{
    _stopWatch.Restart();
    
    var remotingTestProxy = rpcProxyFactory.Create<ITestRemoteService>();

    var result = await remotingTestProxy.EchoAsync("andrew2");
    WriteLog(context, "Called EchoAsync method on proxy object");
    
    await context.Response.WriteAsync(result);
});
app.MapGet("/test-throws", async context =>
{
    var remotingTestProxy = rpcProxyFactory.Create<ITestRemoteService>();

    await remotingTestProxy.MethodThatThrows("andrew3");

    WriteLog(context, "Calling MethodThatThrows method on proxy object");
    
    await context.Response.WriteAsync("called MethodThatThrows()");
});

app.Logger.LogInformation("Client side WebApp starting...");

app.Run();
