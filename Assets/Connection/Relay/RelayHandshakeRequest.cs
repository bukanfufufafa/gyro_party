

using Newtonsoft.Json;

public class RelayHandshakeRequest
{
    [JsonProperty("candidates1")]
    public ICECandidate[] Candidates1;

    [JsonProperty("candidates2")]
    public ICECandidate[] Candidates2;

    [JsonProperty("sdp1")]
    public string Sdp1;

    [JsonProperty("sdp2")]
    public string Sdp2;

    public RelayHandshakeRequest(ICECandidate[] candidates1, ICECandidate[] candidates2, string sdp1, string sdp2)
    {
        Candidates1 = candidates1;
        Candidates2 = candidates2;
        Sdp1 = sdp1;
        Sdp2 = sdp2;
    }
}