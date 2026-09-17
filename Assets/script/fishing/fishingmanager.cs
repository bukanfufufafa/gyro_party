using System.Collections.Generic;
using UnityEngine;

public class FishingGameManager : MonoBehaviour
{
    [Header("Fish")]
    public List<FishRandomMovement> fishList = new List<FishRandomMovement>();

    [Header("Player Score")]
    public fishing player1;
    public fishing player2;

    [Header("Result Panel")]
    public GameObject panelPlayer1Menang;
    public GameObject panelPlayer2Menang;
    public GameObject panelSeri;

    private bool gameFinished = false;


    private void Start()
    {
        // Cari semua ikan yang ada di scene
        FishRandomMovement[] allFish =
            FindObjectsOfType<FishRandomMovement>();

        fishList.AddRange(allFish);


        // Sembunyikan semua panel
        if (panelPlayer1Menang != null)
            panelPlayer1Menang.SetActive(false);

        if (panelPlayer2Menang != null)
            panelPlayer2Menang.SetActive(false);

        if (panelSeri != null)
            panelSeri.SetActive(false);
    }


    private void Update()
    {
        if (gameFinished)
            return;

        CheckFishRemaining();
    }


    private void CheckFishRemaining()
    {
        // Hapus ikan yang sudah Destroy
        fishList.RemoveAll(fish => fish == null);


        // Kalau masih ada ikan
        if (fishList.Count > 0)
            return;


        // Semua ikan sudah habis
        EndGame();
    }


    private void EndGame()
    {
        gameFinished = true;


        int scorePlayer1 = 0;
        int scorePlayer2 = 0;


        // Ambil score P1
        if (player1 != null)
        {
            scorePlayer1 = player1.score;
        }


        // Ambil score P2
        if (player2 != null)
        {
            scorePlayer2 = player2.score;
        }


        Debug.Log(
            "Game Selesai | " +
            "P1: " + scorePlayer1 +
            " | P2: " + scorePlayer2
        );


        // ==================================
        // PLAYER 1 MENANG
        // ==================================

        if (scorePlayer1 > scorePlayer2)
        {
            ShowPlayer1Win();
        }


        // ==================================
        // PLAYER 2 MENANG
        // ==================================

        else if (scorePlayer2 > scorePlayer1)
        {
            ShowPlayer2Win();
        }


        // ==================================
        // SERI
        // ==================================

        else
        {
            ShowDraw();
        }
    }


    private void ShowPlayer1Win()
    {
        if (panelPlayer1Menang != null)
        {
            panelPlayer1Menang.SetActive(true);
        }

        Debug.Log("PLAYER 1 MENANG!");
    }


    private void ShowPlayer2Win()
    {
        if (panelPlayer2Menang != null)
        {
            panelPlayer2Menang.SetActive(true);
        }

        Debug.Log("PLAYER 2 MENANG!");
    }


    private void ShowDraw()
    {
        if (panelSeri != null)
        {
            panelSeri.SetActive(true);
        }

        Debug.Log("HASIL SERI!");
    }
}