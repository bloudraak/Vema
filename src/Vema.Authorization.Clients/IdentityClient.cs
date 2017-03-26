using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Vema.Authorization.Server.AcceptanceTests;

namespace Vema.Authorization.Clients
{
    public class IdentityClient : IIdentityClient
    {
        private readonly HttpClient _httpClient;

        public IdentityClient(TokenCredential tokenCredential, Uri baseAddress, HttpMessageHandler handler = null)
        {
            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = baseAddress;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenCredential.Scheme, tokenCredential.Token);
        }

        public async Task<string> GetAsync()
        {
            var message = await _httpClient.GetAsync("/identity");
            message.EnsureSuccessStatusCode();
            var s = await message.Content.ReadAsStringAsync();
            return s;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}