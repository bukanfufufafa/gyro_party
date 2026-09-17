using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class football : MonoBehaviour
{
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;

    public GameObject ball;
    public GameObject goalplayer1;
    public GameObject goalplayer2;

    private int scorePlayer1 = 0;
    private int scorePlayer2 = 0;


    private Vector3 initialBallPosition;
    private Quaternion initialBallRotation;

    // [SerializeField] private GameplayManagerProxy gameplayManagerProxy;
    


    void Start()
    {
        GameplayManagerProxy gameplayManagerProxy = GameObject.Find("Gameplay Manager Proxy").GetComponent<GameplayManagerProxy>();
        gameplayManagerProxy.OnStartGame += (_, _) =>
      {
          player1Text.text = "0";
          player2Text.text = "0";


          initialBallPosition = ball.transform.position;
          initialBallRotation = ball.transform.rotation;
      };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == goalplayer1)
        {
            scorePlayer2++;
            player2Text.text = "" + scorePlayer2;

            ResetBall();
        }

        if (other.gameObject == goalplayer2)
        {
            scorePlayer1++;
            player1Text.text = "" + scorePlayer1;

            ResetBall();
        }
    }

    void ResetBall()
    {
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;


        ball.transform.position = initialBallPosition;
        ball.transform.rotation = initialBallRotation;
    }
}