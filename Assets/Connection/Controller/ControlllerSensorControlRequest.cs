using Newtonsoft.Json;

public class ControllerSensorControlRequest
{
    [JsonProperty("enable")]
    public bool Enable { get; }
    
    public ControllerSensorControlRequest(bool enable)
    {
        Enable = enable;
    }
}