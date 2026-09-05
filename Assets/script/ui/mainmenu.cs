using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainmenu : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject gamemode;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject credit;
    [SerializeField] private GameObject quit;
    
    void Start()
    {
        if (gamemode != null)
        {
            gamemode.SetActive(false);
        }
        if (settings != null)
        {
            settings.SetActive(false);
        }
        if (credit != null)
        {
            credit.SetActive(false);
        }
        if (quit != null)
        {
            quit.SetActive(false);
        }

        activateGamemode();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void activateGamemode()
    {
        if(gamemode != null)
        {
            gamemode.SetActive(true);
            settings.SetActive(false);
            credit.SetActive(false);
            quit.SetActive(false);
        }
    }

    public void activateSetting()
    {
        if (settings != null)
        {
            gamemode.SetActive(false);
            settings.SetActive(true);
            credit.SetActive(false);
            quit.SetActive(false);
        }
    }

    public void activateCredit()
    {
        if (credit != null)
        {
            gamemode.SetActive(false);
            settings.SetActive(false);
            credit.SetActive(true);
            quit.SetActive(false);
        }
    }

    public void activateQuit()
    {
        if (quit != null)
        {
            gamemode.SetActive(false);
            settings.SetActive(false);
            credit.SetActive(false);
            quit.SetActive(true);
        }
    }

}
