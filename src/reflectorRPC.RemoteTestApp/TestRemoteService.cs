using reflectorRPC.TestAppContracts;

namespace reflectorRPC.RemoteTestApp
{
    public class TestRemoteService : ITestRemoteService
    {
        public async Task<string> EchoAsync(string message)
        {
            return $"Echoing(async) {message} @{DateTime.Now}";
        }

        public string Echo(string message)
        {
            return $"Echoing {message} @{DateTime.Now}";
        }

        public Task MethodThatThrows(string message)
        {
            throw new ApplicationException($"Oops an exception was thrown for {message}!");
        }
    }
}
