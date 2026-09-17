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

    [SerializeField] private LevelManager levelManager1;
    [SerializeField] private LevelManager levelManager2;

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

    public void OnAirplaneProgressChanged(float progress)
    {
        RectTransform routeRect = routeIndicator.GetComponent<RectTransform>();
        float routeWidth = routeRect.rect.width;

        RectTransform airplaneRect = airplaneIcon.GetComponent<RectTransform>();
        float targetAirplaneLeft = (progress * (routeWidth - 48)) + 28;
        airplaneRect.anchoredPosition = new Vector2(targetAirplaneLeft, airplaneRect.anchoredPosition.y);
    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    private async UniTask TestSetupRelay()
    {
        PlayerPrefs.SetString("RelayUrl", "coke-bonfire-vaguely.ngrok-free.dev");
        RelayService.Instance.OnConnectControllerRequest += (_, result) => result(true);
        await RelayService.Instance.Setup();
        Controller controller1 = ControllerService.Instance.GetController(0);
        controller1.OnChannelOpen += (_, _) =>
        {
            Debug.LogWarning("Controller 1 Channel Open");
            controller1.SetSensorControl(true);
        };
        controller1.OnPromoted += (_, _) =>
        {
            Debug.LogWarning("Controller 1 Promoted");
        };
        Controller controller2 = ControllerService.Instance.GetController(1);
        controller2.OnPromoted += (_, _) =>
        {
            Debug.LogWarning("Controller 2 Promoted");
        };
        controller2.OnChannelOpen += (_, _) =>
        {
            Debug.LogWarning("Controller 2 Channel Open");
            controller2.SetSensorControl(true);
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

        levelManager1.StartGame();
        levelManager2.StartGame();
    }

    private void OnFinishGame(object sender, EventArgs e)
    {
        levelManager1.StopGame();
        levelManager2.StopGame();
    }

    // Private Functions =========================================================
}
