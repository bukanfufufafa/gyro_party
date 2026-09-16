using Newtonsoft.Json;

public class RelayConnectControllerRequest
{
    [JsonProperty("tagId")]
    public uint TagId { get; }

    [JsonProperty("index")]
    public byte Index { get; }

    [JsonProperty("candidates")]
    public ICECandidate[] Candidates { get; }

    [JsonProperty("sdp")]
    public string Sdp { get; }

    public RelayConnectControllerRequest(uint tagId, byte index, ICECandidate[] candidates, string sdp)
    {
        TagId = tagId;
        Index = index;
        Candidates = candidates;
        Sdp = sdp;
    }
}