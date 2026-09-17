using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.WebRTC;
using UnityEngine;

#nullable enable

public class RelayService
{
    // Singleton =========================================================
    private static readonly ThreadSafeResettableLazy<RelayService> _lazyInstance =
       new ThreadSafeResettableLazy<RelayService>(() => new RelayService());

    public static RelayService Instance => _lazyInstance.Value;
    private RelayService() { }
    // Singleton =========================================================

    // Public Properties =========================================================

    public enum RelayState
    {
        None,
        Connecting,
        Handshaking,
        Live
    }
    public RelayState State { get; private set; } = RelayState.None;

    public Action<byte, Action<bool>>? OnConnectControllerRequest;

    // Public Properties =========================================================

    // Private Properties =========================================================

    private const int WS_FRAME_SIZE = 1024 * 4; // 4 KB.

    private Uri? url;
    private ClientWebSocket? ws;
    private CancellationTokenSource? wsSetupCts;
    private Action<bool>? wsSetupOnResult;

    private Dictionary<ushort, Action<RelayMessage>> messageListeners = new();

    // Private Properties =========================================================

    // Public Functions =========================================================

    public async UniTask Reset()
    {
        await Shutdown();
        ControllerService.Instance.Reset();
        _lazyInstance.Reset();
    }

    public async UniTask<bool> Setup()
    {
        Debug.Log("RelayService: Called");
        if (State != RelayState.None)
        {
            Debug.LogWarning("RelayService: Called even though not in None state");
            return false;
        }

        State = RelayState.Connecting;
        var tcs = new UniTaskCompletionSource<bool>();
        wsSetupOnResult = result => tcs.TrySetResult(result);

        string prefRelayUrl = PlayerPrefs.GetString("RelayUrl", "");
        if (string.IsNullOrEmpty(prefRelayUrl))
        {
            throw new Exception("URL Server Relay masih kosong, harap isi dahulu");
        }

        // Set up websocket.
        url = new Uri($"wss://{prefRelayUrl}/pc");
        ws = new ClientWebSocket();
        wsSetupCts = new CancellationTokenSource();
        await ws.ConnectAsync(url, wsSetupCts.Token);

        _ = ProcessMessages();

        // Send handshake request.
        State = RelayState.Handshaking;
        var connectionData = await ControllerService.Instance.Setup();
        if (wsSetupCts.IsCancellationRequested)
        {
            if (!tcs.TrySetResult(false))
            {
                return false;
            }
        }

        if (string.IsNullOrEmpty(connectionData[0].Sdp) || string.IsNullOrEmpty(connectionData[1].Sdp))
        {
            Debug.LogWarning("RelayService: SDP Description kosong");
            await Shutdown();
            if (!tcs.TrySetResult(false))
            {
                return false;
            }
        }

        var request = new RelayHandshakeRequest(
            connectionData[0].IceCandidates.Select(x => new ICECandidate(x.Candidate, x.SdpMid, x.SdpMLineIndex ?? 0)).ToArray(),
            connectionData[1].IceCandidates.Select(x => new ICECandidate(x.Candidate, x.SdpMid, x.SdpMLineIndex ?? 0)).ToArray(),
            connectionData[0].Sdp,
            connectionData[1].Sdp
        );
        _ = SendJson((ushort)RelaySystemMessageId.Handshake, request, wsSetupCts.Token);

        return await tcs.Task;
    }

    public async UniTask Shutdown()
    {
        /* Debug.Log("RelayService: Shutdown!");
        State = RelayState.None;

        if (ws!.State == WebSocketState.Connecting)
        {
            wsSetupCts!.Cancel();
            ws.Abort();
            ws.Dispose();
        }
        else if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseSent || ws.State == WebSocketState.CloseReceived)
        {
            // ws.Abort();

            try
            {
                using var wsCloseCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutdown", wsCloseCancellation.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"RelayService: Gagal menutup koneksi RelayService: {ex}");
            }
            finally
            {
                ws.Dispose();
            }
        } */

        Debug.Log("RelayService: Shutdown");
        State = RelayState.None;

        if (wsSetupCts != null && !wsSetupCts.IsCancellationRequested)
        {
            wsSetupCts.Cancel();
        }
        if (ws == null) return;

        if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
        {
            try
            {
                using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutdown", closeCts.Token);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"RelayService: Gagal menutup koneksi: {ex}");
                ws.Abort();
            }
        }
        else
        {
            ws.Abort();
        }

        ws.Dispose();
        ws = null;
        wsSetupCts?.Dispose();
        wsSetupCts = null;

        ControllerService.Instance.Shutdown();
    }

    public ushort GenerateID()
    {
        System.Random random = new System.Random();
        return (ushort)random.Next((ushort)RelaySystemMessageId.END, ushort.MaxValue);
    }

    public async UniTask SendJson<T>(ushort id, T? value, CancellationToken token)
    {
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
        buffer[0] = 1; // JSON type.
        BitConverter.GetBytes(id).CopyTo(buffer, 1);
        serializedBytes.CopyTo(buffer, 1 + sizeof(ushort));

        await ws!.SendAsync(buffer, WebSocketMessageType.Binary, true, token);
    }

    public async UniTask SendBinary(ushort id, byte[] value, CancellationToken token)
    {
        byte[] buffer = new byte[1 + sizeof(ushort) + value.Count()];

        buffer[0] = 1; // JSON type.
        BitConverter.GetBytes(id).CopyTo(buffer, 1);
        value.CopyTo(buffer, 1 + sizeof(ushort));

        await ws!.SendAsync(buffer, WebSocketMessageType.Binary, true, token);
    }

    public void AddListener(ushort id, Action<RelayMessage> callback)
    {
        messageListeners.Add(id, callback);
    }

    public void RemoveListener(ushort id)
    {
        if (messageListeners.ContainsKey(id))
        {
            messageListeners.Remove(id);
        }
    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    private async UniTask ProcessMessages()
    {
        Debug.Log("RelayService: Process message");

        while (ws!.State == WebSocketState.Open && !wsSetupCts!.IsCancellationRequested)
        {
            Debug.Log("RelayService: Processing message");

            try
            {
                byte[] buffer = new byte[WS_FRAME_SIZE];

                WebSocketReceiveResult result;
                while (true)
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), wsSetupCts.Token);
                    if (!result.EndOfMessage)
                    {
                        Array.Resize(ref buffer, buffer.Count() * 2);
                    }
                    else
                    {
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Complete the close handshake if initiated by the server
                        if (ws.State == WebSocketState.CloseReceived)
                        {
                            await Shutdown();
                        }
                        break;
                    }
                }

                // Check the type of the message.
                if (buffer[0] == 1) // JSON.
                {
                    // Get id and deserialized message.
                    ushort id = BitConverter.ToUInt16(buffer, sizeof(byte));
                    int sliceStart = sizeof(byte) + sizeof(ushort), sliceEnd = buffer.Count() - 1;
                    string json = Encoding.UTF8.GetString(buffer.AsSpan().Slice(sliceStart, sliceEnd - sliceStart));
                    RelayJsonMessage message = new RelayJsonMessage(id, json);

                    // Handle the message according to its id.
                    if (id == (ushort)RelaySystemMessageId.Handshake) // Handshake Response.
                    {
                        Debug.Log("RelayService: Dapat response Handshake");
                        RelayHandshakeResponse response = message.Json<RelayHandshakeResponse>();
                        await HandleHandshakeMessage(response);
                    }
                    else if (id == (ushort)RelaySystemMessageId.ConnectController) // Connect Controller Request.
                    {
                        Debug.Log("RelayService: Dapat request ConnectController");
                        RelayConnectControllerRequest request = message.Json<RelayConnectControllerRequest>();
                        HandleConnectControllerMessage(request);
                    }
                    else if (id == (ushort)RelaySystemMessageId.DisconnectController) // Disconnect Controller Request.
                    {
                        Debug.Log("RelayService: Dapat request ConnectController");
                        RelayConnectControllerRequest request = message.Json<RelayConnectControllerRequest>();
                        HandleConnectControllerMessage(request);
                    }
                    else // Regular message.
                    {
                        if (messageListeners.ContainsKey(id))
                        {
                            messageListeners[id].Invoke(message);
                        }
                        else
                        {
                            Debug.LogWarning("RelayService: Dapat message dari Relay tapi tidak ada listener dengan ID yang sama");
                        }
                    }
                }
                else if (buffer[0] == 2) // Raw Binary.
                {
                    // Get id and message.
                    ushort id = BitConverter.ToUInt16(buffer, sizeof(byte));
                    int sliceStart = sizeof(byte) + sizeof(ushort), sliceEnd = buffer.Count() - 1;
                    RelayBinaryMessage message = new RelayBinaryMessage(id, buffer.AsSpan().Slice(sliceStart, sliceEnd - sliceStart).ToArray());

                    // Handle the message according to its id.
                    if (messageListeners.ContainsKey(id))
                    {
                        messageListeners[id].Invoke(message);
                    }
                    else
                    {
                        Debug.LogWarning("RelayService: Dapat message dari Relay tapi tidak ada listener dengan ID yang sama");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("RelayService: Operation canceled");

                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"RelayService: Dapat error saat memproses message: {ex}");
            }
        }
    }

    private async UniTask HandleHandshakeMessage(RelayHandshakeResponse message)
    {
        if (State != RelayState.Handshaking)
        {
            Debug.LogWarning("RelayService: Dapat pesan Handshake dari Relay tapi bukan dalam mode Handshaking");
            return;
        }

        if (!message.Accepted)
        {
            Debug.LogError("RelayService: Handshake ditolak");

            await Shutdown();

            wsSetupOnResult!(false);
            wsSetupOnResult = null;
            return;
        }

        Debug.Log("RelayService: Handshake berhasil");

        State = RelayState.Live;
        wsSetupOnResult!(true);
        wsSetupOnResult = null;
    }

    private void HandleConnectControllerMessage(RelayConnectControllerRequest request)
    {
        if (State != RelayState.Live)
        {
            Debug.LogWarning("RelayService: Dapat request ConnectController dari Relay tapi belum masuk mode Live");
            return;
        }

        Controller controller = ControllerService.Instance.GetController(request.Index);

        // Check if the Controller the Relay requested on the slot has been promoted which should not happen.
        if (controller.State == Controller.ControllerState.Live)
        {
            Debug.LogWarning("RelayService: Relay request ConnectController dengan slot yang sudah dipakai");
            var response = new RelayConnectControllerResponse(request.TagId, true, RelayConnectControllerResponse.Reason.Bug);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            _ = SendJson((ushort)RelaySystemMessageId.ConnectController, response, cts.Token);
            return;
        }

        // Ask user to accept the Controller or not.
        OnConnectControllerRequest!(request.Index, async (isAccepted) =>
        {
            if (isAccepted)
            {
                var response = new RelayConnectControllerResponse(request.TagId, true, RelayConnectControllerResponse.Reason.None);
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await SendJson((ushort)RelaySystemMessageId.ConnectController, response, cts.Token);

                if (cts.IsCancellationRequested)
                {
                    Debug.LogError("RelayService: Gagal mengirim response ConnectController");
                }
                else
                {
                    await controller.PromoteController(
                        request.Candidates.Select(x => new RTCIceCandidateInit
                        {
                            candidate = x.Candidate,
                            sdpMid = x.SdpMid,
                            sdpMLineIndex = x.SdpMLineIndex
                        }).ToArray(),
                        request.Sdp);
                }
            }
            else
            {
                var response = new RelayConnectControllerResponse(request.TagId, true, RelayConnectControllerResponse.Reason.User);
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                _ = SendJson((ushort)RelaySystemMessageId.ConnectController, response, cts.Token);
            }
        });
    }

    private void HandleDisconnectControllerRequest(RelayDisconnectControllerRequest request)
    {
        
    }

    // Private Functions =========================================================

    ~RelayService()
    {
        Debug.Log("RelayService: Destructor called");
    }
}