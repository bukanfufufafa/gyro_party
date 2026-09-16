using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject gamemode;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject credit;
    [SerializeField] private GameObject quit;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void cutting()
    {
        SceneManager.LoadScene("cutting");
    }

    public void fishing()
    {
        SceneManager.LoadScene("fishing");
    }

    public void plane()
    {
        SceneManager.LoadScene("airplane");
    }
}
