using System.Text.Json.Serialization;

namespace PSTimeTracker.Update
{
    public record VersionInfo
    {
        [JsonPropertyName("version")]
        public string Version { get; init; }
        [JsonPropertyName("description")]
        public string Description { get; init; }
    }
}
