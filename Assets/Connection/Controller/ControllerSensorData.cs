using System;
using UnityEngine;

public class ControllerSensorData : EventArgs
{
    public Quaternion Rotation { get; }
    public Vector3 Offset { get; }

    public ControllerSensorData(Quaternion rotation, Vector3 offset)
    {
        Rotation = rotation;
        Offset = offset;
    }
}