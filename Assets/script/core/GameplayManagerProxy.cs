using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

public class GameplayManagerProxy : MonoBehaviour
{
    // Public Properties =========================================================



    public event EventHandler? OnStartGame;
    public event EventHandler? OnFinishGame;

    // Public Properties =========================================================

    // Private Properties =========================================================

    private GameplayManager gameplayManager;

    // Private Properties =========================================================

    // Public Functions =========================================================

    void Start()
    {
        StartCoroutine(OnStart());
    }

    public void FinishGame(int winner)
    {
        Debug.LogWarning($"Winner {winner}");
        gameplayManager.FinishGame(winner, -1, -1);
    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    private IEnumerator OnStart()
    {
        SceneManager.LoadScene("gameplayroot", LoadSceneMode.Additive);

        yield return null;

        gameplayManager = GameObject.Find("Gameplay Root Canvas").GetComponent<GameplayManager>();
        gameplayManager.RegisterGame();
        gameplayManager.OnStartGame += OnStartGame;
        gameplayManager.OnFinishGame += OnFinishGame;
        gameplayManager.InitGame();
    }

    // Private Functions =========================================================
}