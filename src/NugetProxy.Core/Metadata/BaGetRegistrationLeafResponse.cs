using NugetProxy.Protocol.Models;
using Newtonsoft.Json;

namespace NugetProxy.Core
{
    /// <summary>
    /// NugetProxy's extensions to a registration leaf response. These additions
    /// are not part of the official protocol.
    /// </summary>
    public class NugetProxyRegistrationLeafResponse : RegistrationLeafResponse
    {
        [JsonProperty("downloads")]
        public long Downloads { get; set; }
    }
}
