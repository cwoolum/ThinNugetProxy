using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Protocol.Models;
using NuGet.Versioning;

namespace BaGet.Core.Content
{
    /// <summary>
    /// Implements the NuGet Package Content resource. Supports read-through caching.
    /// Tracks state in a database (<see cref="IPackageService"/>) and stores packages
    /// using <see cref="IPackageStorageService"/>.
    /// </summary>
    public class DatabasePackageContentService : IPackageContentService
    {
        private readonly IMirrorService _mirror;


        public DatabasePackageContentService(
            IMirrorService mirror)
        {
            _mirror = mirror ?? throw new ArgumentNullException(nameof(mirror));

        }

        public async Task<PackageVersionsResponse> GetPackageVersionsOrNullAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            // First, attempt to find all package versions using the upstream source.
            var versions = await _mirror.FindPackageVersionsOrNullAsync(id, cancellationToken);


            return new PackageVersionsResponse
            {
                Versions = versions
                    .Select(v => v.ToNormalizedString())
                    .Select(v => v.ToLowerInvariant())
                    .ToList()
            };
        }

        public async Task<Stream> GetPackageContentStreamOrNullAsync(
            string id,
            NuGetVersion version,
            CancellationToken cancellationToken = default)
        {
            // Allow read-through caching if it is configured.
            return await _mirror.MirrorAsync(id, version, cancellationToken);
        }

        public async Task<Stream> GetPackageManifestStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
        {
            // Allow read-through caching if it is configured.
            return await _mirror.MirrorAsync(id, version, cancellationToken);


        }

        public async Task<Stream> GetPackageReadmeStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
        {
            // Allow read-through caching if it is configured.
            return await _mirror.MirrorAsync(id, version, cancellationToken);
        }
    }
}
