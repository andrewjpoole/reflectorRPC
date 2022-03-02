using System.Dynamic;
using System.Reflection;
using ImpromptuInterface;
using reflectorRPC.Shared;

namespace reflectorRPC.Client
{
    public class RcpProxyFactory
    {
        public static T Create<T>()
        {
            // return an DynamicObject which traps and redirects all method invocations, serialises the args and calls an API before returning the result
            var targetType = typeof(T);
            dynamic proxyObject = new ProxyObject(targetType);
            T result = Impromptu.ActLike(proxyObject);

            return result;
        }
    }

    public class ProxyObject : DynamicObject
    {
        private readonly Type _type;
        public ProxyObject(Type type)
        {
            _type = type;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            try
            {
                var targetMethodInfo = _type.GetMethod(binder.Name) ??
                                       throw new ApplicationException($"Cant find method named {binder.Name} on the Type named {_type.Name}");
                
                var requestContent = new RpcRequestContent
                {
                    TypeName = _type.Name,
                    AssemblyQualifiedTypeName = _type.AssemblyQualifiedName ?? _type.Name,
                    MethodName = binder.Name,
                    MethodReturnsTask = MethodReturnsTask(targetMethodInfo),
                    Args = args?.ToList()
                };

                var rpcClientService = new RpcClientService();

                var responseContent = rpcClientService.CallRemoteMethodAsync(requestContent).Result;

                if (responseContent is null)
                    throw new ApplicationException("Error calling remote service");

                // Check for exceptions and re-throw for existing client code to handle
                if (responseContent.Exception is not null)
                    throw new AggregateException(new List<Exception> { responseContent.Exception });

                if (MethodReturnsTask(targetMethodInfo))
                {
                    var finalReturnTypeFromTask = MethodReturnsTaskOf(targetMethodInfo);
                    result = Task.FromResult(DynamicConvert(responseContent.Result, finalReturnTypeFromTask));
                }
                else
                {
                    result = DynamicConvert(responseContent.Result, targetMethodInfo.ReturnType);
                }

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private bool MethodReturnsTask(MethodInfo method)
        {
            return method.ReturnType.Name == "Task`1";
        }

        private Type MethodReturnsTaskOf(MethodInfo method)
        {
            var returnType = method.ReturnType.GenericTypeArguments[0];
            return returnType;
        }

        private dynamic DynamicConvert(dynamic source, Type dest)
        {
            return Convert.ChangeType(source, dest);
        }
    }
}