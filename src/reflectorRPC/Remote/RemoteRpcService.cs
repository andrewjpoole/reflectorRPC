using System.Reflection;
using reflectorRPC.Shared;

namespace reflectorRPC.Remote;

public class RemoteRpcService : IRemoteRpcService
{
    private readonly IServiceProvider _serviceProvider;

    public RemoteRpcService(IServiceProvider serviceProvider) // TODO may need a strategy pattern/plugin thing for resolving services from DI framework of choice?
    {
        _serviceProvider = serviceProvider;
    }

    public RpcResponseContent Process(RpcRequestContent requestContent)
    {
        try
        {
            var targetType = Type.GetType(requestContent.AssemblyQualifiedTypeName) ??
                             throw new ApplicationException($"Cant find Type named {requestContent.TypeName}");

            var targetService = _serviceProvider.GetService(targetType) ??
                                throw new ApplicationException($"Cant find instance of {requestContent.TypeName} registered in the DI container");

            var targetMethodInfo = targetType.GetMethod(requestContent.MethodName) ??
                                   throw new ApplicationException($"Cant find method named {requestContent.MethodName} on the Type named {requestContent.TypeName}");

            object? result;
            if (requestContent.MethodReturnsTask)
            {
                result = InvokeAsync(targetType, targetService, targetMethodInfo, requestContent.Args.ToArray()).GetAwaiter().GetResult(); // TODO determine best way to call this async method
            }
            else
            {
                result = targetMethodInfo.Invoke(targetService, requestContent.Args.ToArray());
            }

            return new RpcResponseContent
            {
                TypeName = requestContent.TypeName,
                MethodName = requestContent.MethodName,
                Result = result
            };
        }
        catch (Exception e)
        {
            return new RpcResponseContent
            {
                TypeName = requestContent.TypeName,
                MethodName = requestContent.MethodName,
                Exception = e
            };
        }
    }

    private async Task<object> InvokeAsync(Type targetType, object targetService, MethodInfo targetMethodInfo, object?[] args)
    {
        var task = (Task)targetType.InvokeMember(targetMethodInfo.Name, BindingFlags.InvokeMethod, null, targetService, args);
        await task;

        var resultProperty = task.GetType().GetProperty("Result");
        var result = resultProperty.GetValue(task);

        return result;
    }
}

