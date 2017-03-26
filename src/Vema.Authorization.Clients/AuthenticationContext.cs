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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Vema.Authorization.Clients
{
    public class AuthenticationContext : IAuthenticationContext
    {
        public AuthenticationContext(string authority, HttpMessageHandler handler = null)
        {
            if (authority == null)
                throw new ArgumentNullException(nameof(authority));

            Authority = authority;
            Handler = handler;
        }

        private HttpMessageHandler Handler { get; }

        public string Authority { get; }

        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (clientCredential == null) throw new ArgumentNullException(nameof(clientCredential));

            var tokenEndpoint = new Uri(new Uri(Authority), "/connect/token");
            var client = new TokenClient(tokenEndpoint.ToString(), clientCredential.ClientId, clientCredential.ClientSecret, Handler);
            var response = await client.RequestClientCredentialsAsync(resource, cancellationToken: cancellationToken);
            if (response.IsError)
                throw new AuthenticationException(response.Error, response.ErrorDescription ?? response.Error);
            return new AuthenticationResult(response);
        }

        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential, UserCredential userCredential,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (clientCredential == null) throw new ArgumentNullException(nameof(clientCredential));
            if (userCredential == null) throw new ArgumentNullException(nameof(userCredential));

            var tokenEndpoint = new Uri(new Uri(Authority), "/connect/token");
            var client = new TokenClient(tokenEndpoint.ToString(), clientCredential.ClientId, clientCredential.ClientSecret, Handler);
            var response = await client.RequestResourceOwnerPasswordAsync(
                userCredential.UserName, 
                userCredential.Password,
                resource, 
                cancellationToken: cancellationToken);

            if (response.IsError)
                throw new AuthenticationException(response.Error, response.ErrorDescription ?? response.Error);

            return new AuthenticationResult(response);
        }
    }
}