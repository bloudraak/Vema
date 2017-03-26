#region License

// The MIT License (MIT)
// 
// Copyright (c) 2017 Werner Strydom
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vema.Authorization.Clients;
using Xunit;
using Xunit.Abstractions;

namespace Vema.Authorization.Server.AcceptanceTests
{
    public class IdentityTests : IDisposable
    {
        public void Dispose()
        {
            _authorizationServer?.Dispose();
            _authorizationServer = null;
            _testServer?.Dispose();
            _testServer = null;
        }

        private TestServer _authorizationServer;
        private TestServer _testServer;
        private readonly ITestOutputHelper _output;

        public IdentityTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        private TestServer AuthorizationServer
        {
            get
            {
                if (_authorizationServer == null)
                {
                    var builder = new WebHostBuilder()
                        .UseStartup<Portal.Startup>();

                    builder.ConfigureServices(c =>
                    {
                        var provider = c.BuildServiceProvider();
                        var loggerFactory = provider.GetService<ILoggerFactory>();
                        loggerFactory.AddXunit(_output);
                    });

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
                        ApiName = "api1"
                    };

                    builder.ConfigureServices(c => c.AddSingleton(options));

                    builder.ConfigureServices(c =>
                    {
                        var provider = c.BuildServiceProvider();
                        var loggerFactory = provider.GetService<ILoggerFactory>();
                        loggerFactory.AddXunit(_output);
                    });

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
            var client = new IdentityClient(tokenCredential, TestServer.BaseAddress, TestServer.CreateHandler());

            // Act
            var s = await client.GetAsync();

            // Assert
            Assert.NotNull(s);
        }
    }
}