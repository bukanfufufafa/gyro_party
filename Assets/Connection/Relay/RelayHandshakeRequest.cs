

using Newtonsoft.Json;

public class RelayHandshakeRequest
{
    [JsonProperty("candidates")]
    public ICECandidate[] Candidates;

    [JsonProperty("sdp")]
    public string Sdp;

    public RelayHandshakeRequest(ICECandidate[] candidates, string sdp)
    {
        Candidates = candidates;
        Sdp = sdp;
    }
}