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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Vema.Authorization.Portal.AcceptanceTests
{
    public class SmokeTests : IDisposable
    {
        public SmokeTests()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000");
        }

        private readonly HttpClient _client;

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
            var requestUri = "/.well-known/openid-configuration";

            // Act
            var response = await _client.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            JsonSerializer serializer = new JsonSerializer();
            dynamic doc = serializer.Deserialize(new JsonTextReader(new StringReader(content)));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("http://localhost:5000", (string)doc.issuer);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}