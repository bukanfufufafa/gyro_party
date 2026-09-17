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

    [SerializeField] private AudioSource audioSource;

    // Private Properties =========================================================

    // Public Functions =========================================================

    void Start()
    {
        gameplayManagerProxy.OnStartGame += OnStartGame;
        gameplayManagerProxy.OnFinishGame += OnFinishGame;

        // _ = TestSetupRelay();
    }

    void Update()
    {

    }

    void OnDestroy()
    {
        // _ = RelayService.Instance.Shutdown();
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

        audioSource.Play();
    }

    private void OnFinishGame(object sender, EventArgs e)
    {
        levelManager1.StopGame();
        levelManager2.StopGame();
    }

    // Private Functions =========================================================
}
