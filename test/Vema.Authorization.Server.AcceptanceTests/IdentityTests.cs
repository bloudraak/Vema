using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Vema.Authorization.Clients;
using Xunit;

namespace Vema.Authorization.Server.AcceptanceTests
{
    public class IdentityTests
    {
        private TestServer _authorizationServer;
        private TestServer _testServer;

        private TestServer AuthorizationServer
        {
            get
            {
                if (_authorizationServer == null)
                {
                    var builder = new WebHostBuilder()
                        .UseStartup<Portal.Startup>();

                    _authorizationServer = new TestServer(builder);
                }
                return _authorizationServer;
            }
        }

        private TestServer TestServer
        {
            get
            {
                if (_testServer == null)
                {
                    var builder = new WebHostBuilder()
                        .UseStartup<Startup>();

                    var options = new IdentityServerAuthenticationOptions
                    {
                        Authority = AuthorizationServer.BaseAddress.ToString(),
                        JwtBackChannelHandler = AuthorizationServer.CreateHandler(),
                        IntrospectionDiscoveryHandler = AuthorizationServer.CreateHandler(),
                        IntrospectionBackChannelHandler = AuthorizationServer.CreateHandler(),
                        RequireHttpsMetadata = false,
                        ApiName = "api1",
                    };

                    builder.ConfigureServices(c => c.AddSingleton(options));

                    _testServer = new TestServer(builder);
                }
                return _testServer;
            }
        }

        private async Task<TokenCredential> CreateTokenCredentialAsync()
        {
            var accessToken = await AccessTokenAsync();
            var tokenCredential = new TokenCredential(accessToken);
            return tokenCredential;
        }

        private async Task<string> AccessTokenAsync()
        {
            var handler = AuthorizationServer.CreateHandler();
            var authority = AuthorizationServer.BaseAddress.ToString();
            var authenticationContext = new AuthenticationContext(authority, handler);
            var resource = "api1";
            var credential = new ClientCredential("client", "secret");
            var response = await authenticationContext.AcquireTokenAsync(resource, credential);
            return response.AccessToken;
        }

        [Fact]
        public async Task Get_should_return_ok()
        {
            // Arrange
            var tokenCredential = await CreateTokenCredentialAsync();

            var httpClient = new HttpClient(TestServer.CreateHandler());
            var baseAddress = TestServer.BaseAddress;
            httpClient.BaseAddress = baseAddress;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenCredential.Scheme,
                tokenCredential.Token);

            // Act
            var message = await httpClient.GetAsync("/identity");
            message.EnsureSuccessStatusCode();
            var s = await message.Content.ReadAsStringAsync();

            // Assert
            Assert.NotNull(s);
        }
    }
}