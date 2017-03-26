using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vema.Authorization.Server.AcceptanceTests
{
    public interface IAuthenticationContext
    {
        string Authority { get; }

        Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential credential, CancellationToken cancellationToken = default(CancellationToken));
    }
}