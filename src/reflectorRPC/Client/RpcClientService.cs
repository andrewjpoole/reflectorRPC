using System.Text;
using Newtonsoft.Json;
using reflectorRPC.Shared;

namespace reflectorRPC.Client;

public class RpcClientService
{
    private const string RpcUri = "rpc";
    private const string RequestMediaType = "application/json";
    public async Task<RpcResponseContent?> CallRemoteMethodAsync(RpcRequestContent requestContent)
    {
        // TODO look this up from Consul/service discovery thing/load balancer etc and cache or use APIM?
        // TODO pull from config
        // TODO need some auth
        // TODO consider adding retry policies
        // TODO use HttpClientFactory and consider scope of this service
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5000"); 
        var response = await client.PostAsync(RpcUri, new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, RequestMediaType));

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<RpcResponseContent>(responseBody);

            return responseContent;
        }
        else
        {
            // log and/or throw a local exception?
            // TODO built in retry policies?
            return null;
        }
    }
}