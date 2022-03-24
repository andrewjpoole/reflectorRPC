using System.Reflection;
using reflectorRPC.Caching;
using reflectorRPC.Shared;

namespace reflectorRPC.Remote;

public class RemoteRpcService : IRemoteRpcService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDictionaryCache<string,MethodInvocationTypes> _methodInvocationTypeCache;

    public RemoteRpcService(IServiceProvider serviceProvider, IDictionaryCache<string, MethodInvocationTypes> methodInvocationTypeCache) // TODO may need a strategy pattern/plugin thing for resolving services from DI framework of choice?
    {
        _serviceProvider = serviceProvider;
        _methodInvocationTypeCache = methodInvocationTypeCache;
    }

    public RpcResponseContent Process(RpcRequestContent requestContent)
    {
        try
        {
            var methodInvocationType = _methodInvocationTypeCache.Fetch(requestContent.MethodName, () =>
            {
                var targetType = Type.GetType(requestContent.AssemblyQualifiedTypeName) ??
                                 throw new ApplicationException($"Cant find Type named {requestContent.TypeName}");

                var targetMethodInfo = targetType.GetMethod(requestContent.MethodName) ??
                                       throw new ApplicationException($"Cant find method named {requestContent.MethodName} on the Type named {requestContent.TypeName}");

                var methodInvocationType = new MethodInvocationTypes
                {
                    TargetType = targetType,
                    TargetMethodInfo = targetMethodInfo,
                    MethodReturnsTask = MethodReturnsTask(targetMethodInfo)
                };

                return methodInvocationType;
            });

            var targetService = _serviceProvider.GetService(methodInvocationType.TargetType) ??
                                throw new ApplicationException($"Cant find instance of {requestContent.TypeName} registered in the DI container");
            
            object? result;
            if (methodInvocationType.MethodReturnsTask)
            {
                result = InvokeAsync(methodInvocationType.TargetType, targetService, methodInvocationType.TargetMethodInfo, requestContent.Args.ToArray()).GetAwaiter().GetResult(); // TODO determine best way to call this async method
            }
            else
            {
                result = methodInvocationType.TargetMethodInfo.Invoke(targetService, requestContent.Args.ToArray());
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

    private bool MethodReturnsTask(MethodInfo method)
    {
        return method.ReturnType.Name == "Task`1";
    }
}