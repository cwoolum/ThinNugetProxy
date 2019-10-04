using System.ComponentModel.DataAnnotations;

namespace BaGet.Core
{
    public class BaGetOptions
    {
        /// <summary>
        /// The application root URL for usage in reverse proxy scenarios.
        /// </summary>
        public string PathBase { get; set; }

        /// <summary>
        /// If true, disables package pushing, deleting, and relisting.
        /// </summary>
        public bool IsReadOnlyMode { get; set; } = true;

        [Required]
        public SearchOptions Search { get; set; }

        [Required]
        public MirrorOptions Mirror { get; set; }
    }
}
