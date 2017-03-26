using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Vema.Authorization.Server.AcceptanceTests
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

        public string Authority { get; }

        private HttpMessageHandler Handler { get; }

        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential credential, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (credential == null) throw new ArgumentNullException(nameof(credential));

            var tokenEndpoint = new Uri(new Uri(Authority), "/connect/token");
            var client = new TokenClient(tokenEndpoint.ToString(), credential.ClientId, credential.ClientSecret, Handler);
            var response = await client.RequestClientCredentialsAsync(resource, cancellationToken: cancellationToken);
            if (response.IsError)
            {
                throw new AuthenticationException(response.Error, response.ErrorDescription ?? response.Error);
            }
            return new AuthenticationResult(response);
        }
    }
}