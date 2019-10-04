using System.Collections.Generic;
using System.Threading.Tasks;

namespace NugetProxy.Core
{
    public interface IPackageDownloadsSource
    {
        Task<Dictionary<string, Dictionary<string, long>>> GetPackageDownloadsAsync();
    }
}
