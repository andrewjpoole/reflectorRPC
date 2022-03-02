using reflectorRPC.Client;
using reflectorRPC.TestAppContracts;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/test", async context =>
{
    var remotingTestProxy = RcpProxyFactory.Create<ITestRemoteService>();
    
    var result = remotingTestProxy.Echo("andrew1");
    
    await context.Response.WriteAsync(result);
});
app.MapGet("/test-async", async context =>
{
    var remotingTestProxy = RcpProxyFactory.Create<ITestRemoteService>();

    var result = await remotingTestProxy.EchoAsync("andrew2");

    await context.Response.WriteAsync(result);
});
app.MapGet("/test-throws", async context =>
{
    var remotingTestProxy = RcpProxyFactory.Create<ITestRemoteService>();

    await remotingTestProxy.MethodThatThrows("andrew3");

    await context.Response.WriteAsync("called MethodThatThrows()");
});

app.Run();
