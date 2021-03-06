using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NugetProxy.Protocol;
using NugetProxy.Protocol.Models;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace NugetProxy.Core
{
    public class MirrorService : IMirrorService
    {
        private readonly NuGetClient _upstreamClient;
        private readonly ILogger<MirrorService> _logger;

        public MirrorService(
            NuGetClient upstreamClient,
            ILogger<MirrorService> logger)
        {
            _upstreamClient = upstreamClient ?? throw new ArgumentNullException(nameof(upstreamClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<NuGetVersion>> FindPackageVersionsOrNullAsync(
            string id,
            CancellationToken cancellationToken)
        {
            var upstreamVersions = await _upstreamClient.ListPackageVersionsAsync(id, includeUnlisted: true, cancellationToken);
            if (!upstreamVersions.Any())
            {
                return null;
            }


            return upstreamVersions.ToList();
        }

        public async Task<IReadOnlyList<Package>> FindPackagesOrNullAsync(string id, CancellationToken cancellationToken)
        {
            var items = await _upstreamClient.GetPackageMetadataAsync(id, cancellationToken);
            if (!items.Any())
            {
                return null;
            }

            var upstreamPackages = items.Select(ToPackage);


            return upstreamPackages.ToList();
        }

        public async Task<FileStream> MirrorAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {

            _logger.LogInformation(
                "Package {PackageId} {PackageVersion} does not exist locally. Indexing from upstream feed...",
                id,
                version);

            return await IndexFromSourceAsync(id, version, cancellationToken);
        }

        private Package ToPackage(PackageMetadata metadata)
        {
            return new Package
            {
                Id = metadata.PackageId,
                Version = metadata.ParseVersion(),
                Authors = ParseAuthors(metadata.Authors),
                Description = metadata.Description,
                Downloads = 0,
                HasReadme = false,
                Language = metadata.Language,
                Listed = metadata.IsListed(),
                MinClientVersion = metadata.MinClientVersion,
                Published = metadata.Published.UtcDateTime,
                RequireLicenseAcceptance = metadata.RequireLicenseAcceptance,
                Summary = metadata.Summary,
                Title = metadata.Title,
                IconUrl = ParseUri(metadata.IconUrl),
                LicenseUrl = ParseUri(metadata.LicenseUrl),
                ProjectUrl = ParseUri(metadata.ProjectUrl),
                PackageTypes = new List<PackageType>(),
                RepositoryUrl = null,
                RepositoryType = null,
                Tags = metadata.Tags.ToArray(),

                Dependencies = FindDependencies(metadata)
            };
        }

        private Uri ParseUri(string uriString)
        {
            if (uriString == null) return null;

            if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                return null;
            }

            return uri;
        }

        private string[] ParseAuthors(string authors)
        {
            if (string.IsNullOrEmpty(authors)) return new string[0];

            return authors
                .Split(new[] { ',', ';', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToArray();
        }

        private List<PackageDependency> FindDependencies(PackageMetadata package)
        {
            if ((package.DependencyGroups?.Count ?? 0) == 0)
            {
                return new List<PackageDependency>();
            }

            return package.DependencyGroups
                .SelectMany(FindDependenciesFromDependencyGroup)
                .ToList();
        }

        private IEnumerable<PackageDependency> FindDependenciesFromDependencyGroup(DependencyGroupItem group)
        {
            // NugetProxy stores a dependency group with no dependencies as a package dependency
            // with no package id nor package version.
            if ((group.Dependencies?.Count ?? 0) == 0)
            {
                return new[]
                {
                    new PackageDependency
                    {
                        Id = null,
                        VersionRange = null,
                        TargetFramework = group.TargetFramework
                    }
                };
            }

            return group.Dependencies.Select(d => new PackageDependency
            {
                Id = d.Id,
                VersionRange = d.Range,
                TargetFramework = group.TargetFramework
            });
        }

        private async Task<FileStream> IndexFromSourceAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Attempting to mirror package {PackageId} {PackageVersion}...",
                id,
                version);

            FileStream packageStream = null;

            try
            {
                using (var stream = await _upstreamClient.GetPackageStreamAsync(id, version, cancellationToken))
                {
                    packageStream = await stream.AsTemporaryFileStreamAsync();
                }

            }
            catch (PackageNotFoundException)
            {
                _logger.LogWarning(
                    "Failed to download package {PackageId} {PackageVersion}",
                    id,
                    version);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Failed to mirror package {PackageId} {PackageVersion}",
                    id,
                    version);
            }


            return packageStream;

        }
    }
}
