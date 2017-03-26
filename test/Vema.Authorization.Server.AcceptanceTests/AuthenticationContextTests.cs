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
using IdentityServer4.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Vema.Authorization.Server.AcceptanceTests
{
    public class AuthenticationContextTests
    {
        private TestServer authorizationServer;

        [Fact]
        public async Task AcquireTokenAsync_using_client_credentials_should_return_accesstoken()
        {
            // Arrange
            var handler = AuthorizationServer.CreateHandler();
            var authority = AuthorizationServer.BaseAddress.ToString();
            var authenticationContext = new AuthenticationContext(authority, handler);
            var resource = "api1";
            var credential = new ClientCredential("client", "secret");

            // Act
            var response = await authenticationContext.AcquireTokenAsync(resource, credential);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.AccessToken);
        }

        [Fact]
        public async Task AcquireTokenAsync_using_invalid_client_credentials_should_throw()
        {
            // Arrange
            var handler = AuthorizationServer.CreateHandler();
            var authority = AuthorizationServer.BaseAddress.ToString();
            var authenticationContext = new AuthenticationContext(authority, handler);
            var resource = "api1";
            var credential = new ClientCredential("client", "invalid-password");

            // Act
            AuthenticationException actual = null;
            try
            {
                await authenticationContext.AcquireTokenAsync(resource, credential);
            }
            catch (AuthenticationException e)
            {
                actual = e;
            }

            // Assert
            Assert.NotNull(actual);
            Assert.Equal("invalid_client", actual.Message);
            Assert.Equal("invalid_client", actual.ErrorCode);
        }


        private TestServer AuthorizationServer
        {
            get
            {
                if (this.authorizationServer == null)
                {
                    var authorizationWebHostBuilder = new WebHostBuilder().UseStartup<Portal.Startup>();
                    this.authorizationServer = new TestServer(authorizationWebHostBuilder);
                }
                return this.authorizationServer;
            }
        }
    }
}