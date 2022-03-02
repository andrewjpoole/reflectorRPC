namespace reflectorRPC.Shared;

public class RpcResponseContent
{
    public string TypeName { get; set; } = "";
    public string MethodName { get; set; } = "";
    public dynamic Result { get; set; }
    public Exception? Exception { get; set; }
}