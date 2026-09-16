using System;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    [SerializeField]
    private GameObject loadingIndicator;

    public async void SetupRelay()
    {
        RelayService.Instance.OnConnectControllerRequest = (byte index, Action<bool> accept) =>
        {
            accept(true);
        };

        loadingIndicator.SetActive(true);
        await RelayService.Instance.Setup();
        loadingIndicator.SetActive(false);
    }

    public void SetRelayUrl(string value)
    {
        PlayerPrefs.SetString("RelayUrl", value);
    }

    public void EnableSensor(int index)
    {
        Controller controller = ControllerService.Instance.GetController((byte)index);
        // controller.OnSensorChanged += (_, data) =>
        // {
        //     Debug.Log($"TestUI: Rotasi {data.Rotation} Offset {data.Offset}");
        // };
        controller.SetSensorControl(true);
    }

    void OnDestroy()
    {
        RelayService.Instance.Shutdown().GetAwaiter().GetResult();
    }
}