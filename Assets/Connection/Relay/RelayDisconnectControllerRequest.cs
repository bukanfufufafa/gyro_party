

using Newtonsoft.Json;

public class RelayDisconnectControllerRequest
{
    [JsonProperty("tagId")]
    public uint TagId { get; set; }
}