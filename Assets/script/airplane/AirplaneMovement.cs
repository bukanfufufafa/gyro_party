using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class AirplaneMovement : MonoBehaviour
{
    // Private Properties =========================================================

    [SerializeField] private uint index;
    [SerializeField] private GameplayManagerProxy gameplayManagerProxy;

    [SerializeField] private GameObject airplane;

    private Controller? controller;

    // Private Properties =========================================================

    // Public Functions =========================================================

    void Awake()
    {
        gameplayManagerProxy.OnStartGame += OnStartGame;
    }

    void Start()
    {
    }

    void Update()
    {

    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    private void OnStartGame(object sender, EventArgs e)
    {
        controller = ControllerService.Instance.GetController((byte)index);
        controller.CallibratePosition();
        controller.OnSensorChanged += OnSensorChanged;
    }

    private void OnFinishGame(object sender, EventArgs e)
    {
        controller!.OnSensorChanged -= OnSensorChanged;
    }

    private void OnSensorChanged(object sender, ControllerSensorData data)
    {
        var rotation = airplane.transform.rotation;
        airplane.transform.rotation = Quaternion.Slerp(rotation, Quaternion.Euler(rotation.x, rotation.y, data.Rotation.eulerAngles.z), 25f);
    }

    // Private Functions =========================================================
}
