using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class scorecut : MonoBehaviour
{
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;

    public KnifeCut player1;
    public KnifeCut player2;

  

    // Start is called before the first frame update
    void Start()
    {
        player1Text.text = "0";
        player2Text.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        player1Text.text = "" + player1.score;
        player2Text.text = "" + player2.score;
    }

  
}
