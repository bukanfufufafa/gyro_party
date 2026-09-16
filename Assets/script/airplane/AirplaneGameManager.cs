using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class AirplaneGameManager : MonoBehaviour
{
    // Public Properties =========================================================
    // Public Properties =========================================================

    // Private Properties =========================================================

    [SerializeField] private GameplayManagerProxy gameplayManagerProxy;

    [SerializeField] private GameObject routeIndicator;
    [SerializeField] private GameObject airplaneIcon;

    // Private Properties =========================================================

    // Public Functions =========================================================

    void Start()
    {
        gameplayManagerProxy.OnStartGame += OnStartGame;
        gameplayManagerProxy.OnFinishGame += OnFinishGame;

        _ = TestSetupRelay();
    }

    void Update()
    {

    }

    void OnDestroy()
    {
        _ = RelayService.Instance.Shutdown();
    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    private async UniTask TestSetupRelay()
    {
        PlayerPrefs.SetString("RelayUrl", "coke-bonfire-vaguely.ngrok-free.dev");
        RelayService.Instance.OnConnectControllerRequest += (_, result) => result(true);
        await RelayService.Instance.Setup();
        Controller controller = ControllerService.Instance.GetController(0);
        controller.OnChannelOpen += (_, _) =>
        {
            controller.SetSensorControl(true);
        };
    }

    private void OnStartGame(object sender, EventArgs e)
    {
        Vector3 routeIndicatorPos = routeIndicator.transform.position;

        routeIndicator.SetActive(true);

        LSequence.Create()
            .Append(
                LMotion.Create(new Vector3(routeIndicatorPos.x, routeIndicatorPos.y + 300, routeIndicatorPos.z), routeIndicatorPos, 0.75f)
                    .WithEase(Ease.OutCubic)
                    .BindToPosition(routeIndicator.transform)
            )
            .Run();

        LevelManager.Instance.StartGame();
    }

    private void OnFinishGame(object sender, EventArgs e)
    {
        LevelManager.Instance.StopGame();
    }

    // Private Functions =========================================================
}
