using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

#nullable enable

public class ControllerService
{
    // Singleton =========================================================
    private static readonly Lazy<ControllerService> _lazyInstance =
       new Lazy<ControllerService>(() => new ControllerService());
    public static ControllerService Instance => _lazyInstance.Value;
    private ControllerService() { }
    // Singleton =========================================================

    // Public Properties =========================================================

    // Public Properties =========================================================

    // Private Properties =========================================================

    private RTCConfiguration config;
    private List<RTCIceCandidate> iceCandidates = new();
    private RTCSessionDescription? sdpDescription;
    private Controller[] controllers = new Controller[2];

    // Private Properties =========================================================

    // Public Functions =========================================================

    /// <summary>
    /// Run this for setting up ControllerService.
    /// </summary>
    public async UniTask<(ReadOnlyCollection<RTCIceCandidate> IceCandidates, string Sdp)> Setup()
    {
        WebRTC.Initialize();

        config = default;
        config.iceServers = new[] {
            new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } }
        };
        config.iceTransportPolicy = RTCIceTransportPolicy.All;

        RTCPeerConnection peer1 = new RTCPeerConnection(ref config);
        RTCPeerConnection peer2 = new RTCPeerConnection(ref config);
        {
            var (GeneralChannel, SensorChannel) = await GatherICECandidates(peer1);
            if (GeneralChannel == null || SensorChannel == null)
            {
                throw new Exception("ControllerService: Channel gagal dibuat untuk Controller 1");
            }
            Controller controller1 = new Controller(peer1, GeneralChannel, SensorChannel);
            controllers[0] = controller1;
        }
        {
            var (GeneralChannel, SensorChannel) = await CreateOffer(peer2);
            if (GeneralChannel == null || SensorChannel == null)
            {
                throw new Exception("ControllerService: Channel gagal dibuat untuk Controller 2");
            }
            Controller controller2 = new Controller(peer2, GeneralChannel, SensorChannel);
            controllers[1] = controller2;
        }

        return (IceCandidates: iceCandidates.AsReadOnly(), Sdp: sdpDescription!.Value.sdp);
    }

    /// <summary>
    /// Get the Controller index in which isn't promoted yet.
    /// </summary>
    /// <returns>The Controller index, or -1 if all Controller has been promoted.</returns>
    public sbyte GetNonReadyControllerIndex()
    {
        return (sbyte)Array.FindIndex(controllers, x => x.State == Controller.ControllerState.None);
    }

    /// <summary>
    /// Get Controller.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Controller GetController(byte index)
    {
        return controllers[index];
    }

    public void Shutdown()
    {
        foreach (Controller? controller in controllers)
        {
            controller?.Shutdown();
        }
    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    private async UniTask<(RTCDataChannel? GeneralChannel, RTCDataChannel? SensorChannel)> GatherICECandidates(RTCPeerConnection peer)
    {
        iceCandidates.Clear();

        var tcs = new UniTaskCompletionSource<(RTCDataChannel? GeneralChannel, RTCDataChannel? SensorChannel)>();
        RTCDataChannel? generalChannel = null;
        RTCDataChannel? sensorChannel = null;

        peer.OnIceCandidate = candidate =>
        {
            Debug.Log($"ControllerService: Got ICE Candidate: {candidate.Candidate}");

            if (tcs.Task.Status == UniTaskStatus.Pending && !string.IsNullOrEmpty(candidate.Candidate))
            {
                iceCandidates.Add(candidate);
            }
        };
        peer.OnIceGatheringStateChange = state =>
        {
            Debug.Log($"ControllerService: ICE gathering state change: {state}");

            if (state == RTCIceGatheringState.Complete)
            {
                tcs.TrySetResult((GeneralChannel: generalChannel, SensorChannel: sensorChannel));
            }
        };

        // Create SDP offer.
        var (GeneralChannel, SensorChannel) = await CreateOffer(peer);
        generalChannel = GeneralChannel;
        sensorChannel = SensorChannel;

        // Give 5 seconds limit for gathering ICE candidates.
        var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(5));
        var completedTask = await UniTask.WhenAny(tcs.Task, timeoutTask);
        if (completedTask.hasResultLeft)
        {
            Debug.Log($"ControllerService: Gathering candidate beres setelah timeout");
        }
        else
        {
            Debug.LogWarning($"ControllerService: Gathering candidate belum beres, keburu distop oleh timeout");
            tcs.TrySetResult((GeneralChannel: generalChannel, SensorChannel: sensorChannel));
        }

        return await tcs.Task;
    }

    private async UniTask<(RTCDataChannel? GeneralChannel, RTCDataChannel? SensorChannel)> CreateOffer(RTCPeerConnection peer)
    {
        RTCDataChannel generalChannel = peer.CreateDataChannel("general");
        RTCDataChannel sensorChannel = peer.CreateDataChannel("sensor", new RTCDataChannelInit
        {
            ordered = false,
            maxRetransmits = 0
        });

        RTCSessionDescriptionAsyncOperation offerOp = peer.CreateOffer();
        await offerOp.ToUniTask();
        if (offerOp.IsError)
        {
            Debug.LogError($"ControllerService: Gagal membuat offer: {offerOp.Error.message}");
            return (null, null);
        }

        RTCSessionDescription offer = offerOp.Desc;
        var setLocalOp = peer.SetLocalDescription(ref offer);
        await setLocalOp.ToUniTask();
        if (setLocalOp.IsError)
        {
            Debug.LogError($"ControllerService: Gagal men-set local description: {setLocalOp.Error.message}");
        }

        if (!string.IsNullOrEmpty(offer.sdp) && sdpDescription == null)
        {
            sdpDescription = offer;
        }

        return (GeneralChannel: generalChannel, SensorChannel: sensorChannel);
    }

    // Private Functions =========================================================
}