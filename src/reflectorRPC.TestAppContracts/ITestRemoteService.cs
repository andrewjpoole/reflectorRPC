namespace reflectorRPC.TestAppContracts
{
    public interface ITestRemoteService
    {
        string Echo(string message);

        Task<string> EchoAsync(string message);
        
        Task MethodThatThrows(string message);
    }
}