namespace reflectorRPC.Shared;

public class RpcRequestContent
{
    public string AssemblyQualifiedTypeName { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string MethodName { get; set; } = "";
    public bool MethodReturnsTask { get; init; }
    public List<object?>? Args { get; set; } = new();
}