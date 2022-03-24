using System.Dynamic;
using System.Reflection;
using ImpromptuInterface;
using reflectorRPC.Caching;
using reflectorRPC.Shared;

namespace reflectorRPC.Client
{
    public class RpcProxyFactory
    {
        private readonly IDictionaryCache<Type, ProxyObject> _proxyObjectCache = new DictionaryCache<Type,ProxyObject>();

        public T Create<T>()
        {
            var targetType = typeof(T);
            
            T proxyObject = _proxyObjectCache.Fetch(targetType, () => {
                dynamic proxyObject = new ProxyObject(targetType);
                return proxyObject;
            }).ActLike();

            return proxyObject;
        }
    }

    public class ProxyObject : DynamicObject
    {
        private readonly Type _type;

        private readonly IDictionaryCache<string, MethodInfo> _methodInfoCache = new DictionaryCache<string, MethodInfo>();

        public ProxyObject(Type type)
        {
            _type = type;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            try
            {
                var targetMethodInfo = _methodInfoCache.Fetch($"{_type.Name}.{binder.Name}", () => _type.GetMethod(binder.Name)) ??
                                       throw new ApplicationException($"Cant find method named {binder.Name} on the Type named {_type.Name}");
                
                var requestContent = new RpcRequestContent
                {
                    TypeName = _type.Name,
                    AssemblyQualifiedTypeName = _type.AssemblyQualifiedName ?? _type.Name,
                    MethodName = binder.Name,
                    Args = args?.ToList()
                };

                var rpcClientService = new RpcClientService(); // TODO use IoC to get this

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