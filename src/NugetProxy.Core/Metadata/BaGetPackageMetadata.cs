using System.Collections.Generic;
using NugetProxy.Protocol.Models;
using Newtonsoft.Json;

namespace NugetProxy.Core
{
    /// <summary>
    /// NugetProxy's extensions to the package metadata model. These additions
    /// are not part of the official protocol.
    /// </summary>
    public class NugetProxyPackageMetadata : PackageMetadata
    {
        [JsonProperty("downloads")]
        public long Downloads { get; set; }

        [JsonProperty("hasReadme")]
        public bool HasReadme { get; set; }

        [JsonProperty("packageTypes")]
        public IReadOnlyList<string> PackageTypes { get; set; }

        [JsonProperty("repositoryUrl")]
        public string RepositoryUrl { get; set; }

        [JsonProperty("repositoryType")]
        public string RepositoryType { get; set; }
    }
}
