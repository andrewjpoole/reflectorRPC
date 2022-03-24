namespace reflectorRPC.Shared;

public class RpcRequestContent
{
    public string AssemblyQualifiedTypeName { get; set; } = "";
    public string TypeName { get; set; } = ""; // TODO could move this to the request path? so rpc/bacs/IRemoteService in APIM?
    public string MethodName { get; set; } = "";
    public List<object?>? Args { get; set; } = new();
}