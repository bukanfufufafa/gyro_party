using System;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.WebRTC;
using UnityEngine;

#nullable enable

public class Controller
{
    // Public Properties =========================================================

    public enum ControllerState
    {
        None,
        Live
    }
    public ControllerState State { get; set; } = ControllerState.None;
    public RTCPeerConnection Peer { get; }

    public Quaternion Rotation { get; private set; }

    // Public Properties =========================================================

    // Private Properties =========================================================

    private bool isSensorEnabled;
    private RTCDataChannel? generalChannel;
    private RTCDataChannel? sensorChannel;

    private Quaternion rotationCalibration = Quaternion.identity;
    private bool rotationCalibrationRequest = false;

    // Private Properties =========================================================

    public event EventHandler? OnPromoted;
    public event EventHandler? OnChannelOpen;
    public event EventHandler? OnShutdown;
    public event EventHandler<ControllerSensorData>? OnSensorChanged;

    // Public Functions =========================================================

    public Controller(RTCPeerConnection peer, RTCDataChannel generalChannel, RTCDataChannel sensorChannel)
    {
        Peer = peer;

        this.generalChannel = generalChannel;
        this.sensorChannel = sensorChannel;

        generalChannel.OnMessage = ProcessGeneralMessages;
        sensorChannel.OnMessage = ProcessSensorMessages;

        generalChannel.OnOpen = () =>
        {
            Debug.Log("Controller: General channel open");
            if (sensorChannel.ReadyState == RTCDataChannelState.Open)
                OnChannelOpen?.Invoke(this, new EventArgs());
        };
        sensorChannel.OnOpen = () =>
        {
            Debug.Log("Controller: Sensor channel open");
            if (generalChannel.ReadyState == RTCDataChannelState.Open)
                OnChannelOpen?.Invoke(this, new EventArgs());
        };
    }

    public async UniTask PromoteController(RTCIceCandidateInit[] candidates, string sdp)
    {
        RTCSessionDescription remoteAnswer = new RTCSessionDescription
        {
            type = RTCSdpType.Answer,
            sdp = sdp
        };
        var setRemoteOp = Peer.SetRemoteDescription(ref remoteAnswer);
        await setRemoteOp.ToUniTask();

        foreach (var candidate in candidates)
        {
            Peer.AddIceCandidate(new RTCIceCandidate(candidate));
        }

        State = ControllerState.Live;
        OnPromoted?.Invoke(this, new EventArgs());
    }

    /*  public void SetupDataChannel(RTCDataChannel general, RTCDataChannel sensor)
     {
         if (State == ControllerState.Closed) throw new Exception("Controller: SetupDataChannel tapi mode closed");

         generalChannel = general;
         sensorChannel = sensor;

         generalChannel.OnMessage = ProcessGeneralMessages;
         sensorChannel.OnMessage = ProcessSensorMessages;

         generalChannel.OnOpen = () =>
         {
             Debug.Log("Controller: General channel open");
         };
         sensorChannel.OnOpen = () =>
         {
             Debug.Log("Controller: Sensor channel open");
         };
     } */

    public void SetSensorControl(bool enable)
    {
        if (enable && isSensorEnabled)
        {
            throw new Exception("Controller: Mencoba menyalakan Sensor tapi udah nyala");
        }
        else if (!enable && !isSensorEnabled)
        {
            throw new Exception("Controller: Mencoba mematikan Sensor tapi udah mati");
        }

        ControllerSensorControlRequest request = new(enable);
        SendGeneralMessage(ControllerGeneralMessageType.SensorControl, request);
    }

    public void CallibratePosition()
    {
        rotationCalibrationRequest = true;
    }

    public void Shutdown()
    {
        Debug.LogWarning("Controller: Shutdown, Index");
        if (State != ControllerState.Live)
        {
            Debug.LogWarning("Controller: Mau coba matiin tapi belum masuk mode Live");
            return;
        }

        generalChannel?.Close();
        sensorChannel?.Close();

        Peer.Close();

        State = ControllerState.None;

        OnShutdown?.Invoke(this, new EventArgs());
    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    // -- General Channel -------------------------------------------

    private void ProcessGeneralMessages(byte[] message)
    {

    }

    private void SendGeneralMessage<T>(ControllerGeneralMessageType type, T value)
    {
        if (generalChannel == null) throw new Exception("Controller: Kirim pesan General tapi data channelnya null");
        if (generalChannel.ReadyState != RTCDataChannelState.Open) throw new Exception("Controller: Kirim pesan General tapi belum open");

        Debug.Log($"Controller: Mengirim pesan General {type}");

        byte[] serializedBytes;
        if (value != null)
        {
            string serialized = JsonConvert.SerializeObject(value);
            serializedBytes = Encoding.UTF8.GetBytes(serialized);
        }
        else
        {
            serializedBytes = Array.Empty<byte>();
        }

        byte[] buffer = new byte[1 + sizeof(ushort) + serializedBytes.Count()];
        BitConverter.TryWriteBytes(buffer.AsSpan(0, 2), (byte)type);
        serializedBytes.CopyTo(buffer, 1 + sizeof(ushort));

        generalChannel!.Send(buffer);
    }

    // -- General Channel -------------------------------------------

    // -- Sensor Channel --------------------------------------------

    private void ProcessSensorMessages(byte[] message)
    {
        try
        {
            float qX, qY, qZ, qW;
            float oX, oY, oZ;

            qX = BitConverter.ToSingle(message, sizeof(float) * 0);
            qY = BitConverter.ToSingle(message, sizeof(float) * 1);
            qZ = BitConverter.ToSingle(message, sizeof(float) * 2);
            qW = BitConverter.ToSingle(message, sizeof(float) * 3);

            oX = BitConverter.ToSingle(message, sizeof(float) * 4);
            oY = BitConverter.ToSingle(message, sizeof(float) * 5);
            oZ = BitConverter.ToSingle(message, sizeof(float) * 6);

            // Debug.Log($"Controller: Rotasi {qX} {qY} {qZ} {qW}");

            UpdateSensor(qX, qY, qZ, qW, oX, oY, oZ);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Controller: Failed processing sensor message: {ex}");
        }
    }

    private void UpdateSensor(float qX, float qY, float qZ, float qW, float oX, float oY, float oZ)
    {
        if (rotationCalibrationRequest)
        {
            RecenterRotation(qX, qY, qZ, qW);
            rotationCalibrationRequest = false;
        }
        Quaternion unityRotation = new Quaternion(qX, qZ, qY, -qW);
        Quaternion postureCorrection = Quaternion.Euler(90f, 0f, 0f);
        Quaternion targetRotation = rotationCalibration * (unityRotation * postureCorrection);
        Rotation = Quaternion.Slerp(
            Rotation,
            targetRotation,
            Time.deltaTime * 25f
        );

        Vector3 offset = new(oX, oY, oZ);

        OnSensorChanged?.Invoke(this, new ControllerSensorData(Rotation, offset));
    }

    private void RecenterRotation(float qX, float qY, float qZ, float qW)
    {
        Quaternion unityRotation = new Quaternion(qX, qZ, qY, -qW);
        Quaternion postureCorrection = Quaternion.Euler(90f, 0f, 0f);
        Quaternion currentRawRotation = unityRotation * postureCorrection;

        // Calculate the offset needed to make the current rotation equal to identity (0,0,0)
        rotationCalibration = Quaternion.Inverse(currentRawRotation);
    }

    // -- Sensor Channel --------------------------------------------

    // Private Functions =========================================================
}