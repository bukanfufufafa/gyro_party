using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class mainmenu : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject gamemode;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject credit;
    [SerializeField] private GameObject quit;

    [Header("Pause")]
    [SerializeField] private GameObject pause;

    private bool paused = false;

    private void Awake()
    {
        // Pastikan script ini tetap aktif
        Debug.Log("MAINMENU SCRIPT AKTIF");
    }

    private void Start()
    {
        Time.timeScale = 1f;
        paused = false;

        if (pause != null)
        {
            pause.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC TERDETEKSI");

            if (paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Debug.Log("PAUSE GAME DIPANGGIL");

        paused = true;

        if (pause != null)
        {
            pause.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Debug.Log("RESUME BUTTON DIPANGGIL");

        Time.timeScale = 1f;
        paused = false;

        if (pause != null)
        {
            pause.SetActive(false);
        }
    }

    public void goMenu()
    {
        Debug.Log("GO MENU BUTTON DIPANGGIL");

        Time.timeScale = 1f;
        paused = false;

        SceneManager.LoadScene("mainmenu");
    }

    public void cutting()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("cutting");
    }

    public void fishing()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("fishing");
    }

    public void plane()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("airplane");
    }

    public void exit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}