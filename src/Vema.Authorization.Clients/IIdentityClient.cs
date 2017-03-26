using System;
using System.Threading.Tasks;

namespace Vema.Authorization.Clients
{
    public interface IIdentityClient : IDisposable
    {
        Task<string> GetAsync();
    }
}