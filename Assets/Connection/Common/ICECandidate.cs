using Newtonsoft.Json;

public struct ICECandidate
{
    [JsonProperty("candidate")]
    public string Candidate { get; }

    [JsonProperty("sdpMid")]
    public string SdpMid { get; }

    [JsonProperty("sdpMLineIndex")]
    public int SdpMLineIndex { get; }

    public ICECandidate(string candidate, string sdpMid, int sdpMLineIndex)
    {
        Candidate = candidate;
        SdpMid = sdpMid;
        SdpMLineIndex = sdpMLineIndex;
    }
}