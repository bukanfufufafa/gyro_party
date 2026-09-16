using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public class RelayConnectControllerResponse
{
    public enum Reason
    {
        [EnumMember(Value = "none")]
        None,
        [EnumMember(Value = "user")]
        User,
        [EnumMember(Value = "bug")]
        Bug
    }

    [JsonProperty("tagId")]
    public uint TagId;

    [JsonProperty("accepted")]
    public bool Accepted { get; }

    [JsonProperty("index")]
    public byte Index { get; }

    [JsonProperty("rejectionReason"), JsonConverter(typeof(StringEnumConverter))]
    public Reason RejectionReason { get; }

    public RelayConnectControllerResponse(uint tagId, bool accepted, Reason rejectionReason)
    {
        TagId = tagId;
        Accepted = accepted;
        RejectionReason = rejectionReason;
    }
}