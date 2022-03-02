using reflectorRPC.Shared;

namespace reflectorRPC.Remote;

public interface IRemoteRpcService
{
    RpcResponseContent Process(RpcRequestContent requestContent);
}