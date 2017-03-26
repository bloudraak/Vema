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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Xunit.Abstractions;

namespace Vema.Authorization.Portal.AcceptanceTests 
{
    public class SmokeTests : IDisposable
    {
        private readonly TestServer _server;
        private HttpClient _client;
        private HttpMessageHandler _handler;
        private readonly ITestOutputHelper output;

        public SmokeTests(ITestOutputHelper output)
        {
            this.output = output;
            var webHostBuilder = new WebHostBuilder().UseStartup<Startup>();
            _server = new TestServer(webHostBuilder);
            _handler = _server.CreateHandler();
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task HomePage()
        {
            // Arrange
            var requestUri = "/";

            // Act
            var response = await _client.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Vema Authorization Server", content);
        }

        [Fact]
        public async Task OpenIdConfiguration()
        {
            // Arrange

            // Act
            var authority = "http://localhost:5000";
            DiscoveryClient client = new DiscoveryClient(authority, _handler);
            var disco = await client.GetAsync();

            // Assert
            Assert.Equal(null, disco.Error);
            Assert.Equal("http://localhost:5000/connect/authorize", disco.AuthorizeEndpoint);
            Assert.Equal("http://localhost:5000/connect/checksession", disco.CheckSessionIframe);
            Assert.Equal("http://localhost:5000/connect/endsession", disco.EndSessionEndpoint);
            Assert.Equal("http://localhost:5000/connect/introspect", disco.IntrospectionEndpoint);
            Assert.Equal("http://localhost:5000", disco.Issuer);
            Assert.Equal("http://localhost:5000/.well-known/openid-configuration/jwks", disco.JwksUri);
            Assert.Equal(null, disco.RegistrationEndpoint);
            Assert.Equal("http://localhost:5000/connect/revocation", disco.RevocationEndpoint);
            Assert.Equal("http://localhost:5000/connect/token", disco.TokenEndpoint);
            Assert.Equal("http://localhost:5000/connect/userinfo", disco.UserInfoEndpoint);
        }

        public void Dispose()
        {
            _server?.Dispose();
            _client?.Dispose();
        }
    }
}