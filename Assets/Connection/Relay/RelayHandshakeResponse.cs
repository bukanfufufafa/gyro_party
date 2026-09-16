using Newtonsoft.Json;
using Newtonsoft.Json.Converters;



public class RelayHandshakeResponse
{
    [JsonProperty("accepted")]
    public bool Accepted { get; set; }
}