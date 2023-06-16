using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ViewApiUpdateInfo {
    [JsonProperty]
    [JsonPropertyName("version_code")]
    public long VersionCode { get; set; }

    [JsonProperty]
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonProperty]
    [JsonPropertyName("changelog")]
    public string? Changelog { get; set; }
}