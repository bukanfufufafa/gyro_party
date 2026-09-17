using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

#nullable enable

public class ControllerService
{
    // Singleton =========================================================
    private static readonly ThreadSafeResettableLazy<ControllerService> _lazyInstance =
       new ThreadSafeResettableLazy<ControllerService>(() => new ControllerService());
    public static ControllerService Instance => _lazyInstance.Value;
    private ControllerService() { }
    // Singleton =========================================================

    private RTCConfiguration config;
    private Controller[] controllers = new Controller[2];

    // Create a struct to hold per-peer connection data
    public struct PeerConnectionData
    {
        public ReadOnlyCollection<RTCIceCandidate> IceCandidates;
        public string Sdp;
    }

    public void Reset()
    {
        Shutdown();
        _lazyInstance.Reset();
    }

    /// <summary>
    /// Run this for setting up ControllerService.
    /// Returns an array containing the connection data for BOTH controllers.
    /// </summary>
    public async UniTask<PeerConnectionData[]> Setup()
    {
        WebRTC.Initialize();

        config = default;
        config.iceServers = new[] {
            new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } }
        };
        config.iceTransportPolicy = RTCIceTransportPolicy.All;

        PeerConnectionData[] connectionData = new PeerConnectionData[2];

        // Setup Controller 1
        RTCPeerConnection peer1 = new RTCPeerConnection(ref config);
        var peer1Result = await GatherICECandidates(peer1);
        if (peer1Result.GeneralChannel == null || peer1Result.SensorChannel == null)
            throw new Exception("ControllerService: Channel gagal dibuat untuk Controller 1");

        controllers[0] = new Controller(peer1, peer1Result.GeneralChannel, peer1Result.SensorChannel);
        connectionData[0] = new PeerConnectionData
        {
            IceCandidates = peer1Result.Candidates.AsReadOnly(),
            Sdp = peer1Result.Sdp
        };

        // Setup Controller 2
        RTCPeerConnection peer2 = new RTCPeerConnection(ref config);
        var peer2Result = await GatherICECandidates(peer2);
        if (peer2Result.GeneralChannel == null || peer2Result.SensorChannel == null)
            throw new Exception("ControllerService: Channel gagal dibuat untuk Controller 2");

        controllers[1] = new Controller(peer2, peer2Result.GeneralChannel, peer2Result.SensorChannel);
        connectionData[1] = new PeerConnectionData
        {
            IceCandidates = peer2Result.Candidates.AsReadOnly(),
            Sdp = peer2Result.Sdp
        };

        return connectionData;
    }

    public sbyte GetNonReadyControllerIndex()
    {
        return (sbyte)Array.FindIndex(controllers, x => x.State == Controller.ControllerState.None);
    }

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

    // Notice we now return the Sdp and Candidates scoped entirely to this specific peer execution
    private async UniTask<(RTCDataChannel? GeneralChannel, RTCDataChannel? SensorChannel, string Sdp, List<RTCIceCandidate> Candidates)> GatherICECandidates(RTCPeerConnection peer)
    {
        List<RTCIceCandidate> localCandidates = new List<RTCIceCandidate>();
        var tcs = new UniTaskCompletionSource<bool>();

        peer.OnIceCandidate = candidate =>
        {
            Debug.Log($"ControllerService: Got ICE Candidate: {candidate.Candidate}");
            if (tcs.Task.Status == UniTaskStatus.Pending && !string.IsNullOrEmpty(candidate.Candidate))
            {
                localCandidates.Add(candidate);
            }
        };

        peer.OnIceGatheringStateChange = state =>
        {
            Debug.Log($"ControllerService: ICE gathering state change: {state}");
            if (state == RTCIceGatheringState.Complete)
            {
                tcs.TrySetResult(true);
            }
        };

        // Create SDP offer
        var (GeneralChannel, SensorChannel, Sdp) = await CreateOffer(peer);

        // Wait for ICE gathering with 5-second timeout
        var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(5));
        var completedTask = await UniTask.WhenAny(tcs.Task, timeoutTask);
        
        if (completedTask.hasResultLeft)
            Debug.Log("ControllerService: Gathering candidate beres");
        else
            Debug.LogWarning("ControllerService: Gathering candidate timeout");

        return (GeneralChannel, SensorChannel, Sdp, localCandidates);
    }

    private async UniTask<(RTCDataChannel GeneralChannel, RTCDataChannel SensorChannel, string Sdp)> CreateOffer(RTCPeerConnection peer)
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
            throw new Exception($"ControllerService: Gagal membuat offer: {offerOp.Error.message}");
        }

        RTCSessionDescription offer = offerOp.Desc;
        var setLocalOp = peer.SetLocalDescription(ref offer);
        await setLocalOp.ToUniTask();
        
        if (setLocalOp.IsError)
        {
            throw new Exception($"ControllerService: Gagal men-set local description: {setLocalOp.Error.message}");
        }

        return (generalChannel, sensorChannel, offer.sdp);
    }
}