using UnityEngine;
using Newtonsoft.Json;
using System;

#nullable enable

public class RelayJsonMessage : RelayMessage
{
    public string RawJson { get; }

    public RelayJsonMessage(ushort id, string rawJson) : base(id)
    {
        RawJson = rawJson;
    }

    public T Json<T>()
    {
        T? deserialized = JsonConvert.DeserializeObject<T>(RawJson) ?? throw new Exception($"RelayJsonMessage: Failed deserializing JSON with type ${typeof(T)}");
        return deserialized;
    }
}