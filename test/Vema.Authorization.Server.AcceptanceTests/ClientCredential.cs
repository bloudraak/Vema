using System;

namespace Vema.Authorization.Server.AcceptanceTests
{
    public class ClientCredential
    {
        public ClientCredential(string clientId, string clientSecret)
        {
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));
            if (clientSecret == null) throw new ArgumentNullException(nameof(clientSecret));

            ClientSecret = clientSecret;
            ClientId = clientId;
        }

        public string ClientSecret { get; }

        public string ClientId { get; }
    }
}