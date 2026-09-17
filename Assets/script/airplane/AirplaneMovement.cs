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

    bool isStarted = false;

    // Private Properties =========================================================

    // Public Functions =========================================================

    void Awake()
    {
        gameplayManagerProxy.OnStartGame += OnStartGame;
        gameplayManagerProxy.OnFinishGame += OnFinishGame;
    }

    void Start()
    {
    }

    void Update()
    {
        if (!isStarted) return;

        if (index == 0)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
               airplane.transform.Rotate(0f, 0f, 1f * 200f * Time.deltaTime);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
               airplane.transform.Rotate(0f, 0f, -1f * 200f * Time.deltaTime);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
               airplane.transform.Rotate(0f, 0f, 1f * 200f * Time.deltaTime);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                airplane.transform.Rotate(0f, 0f, -1f * 200f * Time.deltaTime);
            }
        }

    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    private void OnStartGame(object sender, EventArgs e)
    {
        isStarted = true;
    }

    private void OnFinishGame(object sender, EventArgs e)
    {
        isStarted = false;
    }

    private void OnSensorChanged(object sender, ControllerSensorData data)
    {
        var rotation = airplane.transform.rotation;
        airplane.transform.rotation = Quaternion.Slerp(rotation, Quaternion.Euler(rotation.x, rotation.y, data.Rotation.eulerAngles.z - 90f), 25f);
    }

    // Private Functions =========================================================
}
