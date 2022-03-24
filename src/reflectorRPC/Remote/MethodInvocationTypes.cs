using System.Reflection;

namespace reflectorRPC.Remote;

public class MethodInvocationTypes
{
    public Type TargetType { get; init; }
    public MethodInfo TargetMethodInfo { get; init; }
    public bool MethodReturnsTask { get; set; }
}