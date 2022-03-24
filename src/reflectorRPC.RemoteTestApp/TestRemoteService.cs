using Microsoft.Extensions.Logging;
using reflectorRPC.TestAppContracts;

namespace reflectorRPC.RemoteTestApp
{
    public class TestRemoteService : ITestRemoteService
    {
        private readonly ILogger<TestRemoteService> _logger;

        public TestRemoteService(ILogger<TestRemoteService> logger)
        {
            _logger = logger;

            _logger.LogInformation("TestRemoteService ctor called");
        }

        public async Task<string> EchoAsync(string message)
        {
            _logger.LogInformation($"EchoAsync called with {message}");

            return $"Echoing(async) {message} @{DateTime.Now}";
        }

        public string Echo(string message)
        {
            _logger.LogInformation($"Echo called with {message}");

            return $"Echoing {message} @{DateTime.Now}";
        }

        public Task MethodThatThrows(string message)
        {
            _logger.LogInformation($"MethodThatThrows called with {message}");

            throw new ApplicationException($"Oops an exception was thrown for {message}!");
        }
    }
}
