using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using LitMotion;
using LitMotion.Extensions;
using Cysharp.Threading.Tasks;
using System;
using Unity.WebRTC;
using UnityEngine.UI;
using UnityEngine.Audio;

public class mainmenu : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject gamemode;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject credit;
    [SerializeField] private GameObject quit;

    [Header("Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicVolumeSilder;
    [SerializeField] private SceneTransition transition;

    // [SerializeField] private GameObject relayConnectingScreen;
    // [SerializeField] private GameObject relayFailedScreen;
    // [SerializeField] private GameObject connectControllerScreen;
    // [SerializeField] private GameObject callibrateControllerScreen;

    private string pendingScene = "";

    // private bool paused = false;

    // private bool connectingController = false;
    // private Action stopConnectingController;

    private void Awake()
    {
        // Pastikan script ini tetap aktif
        Debug.Log("MAINMENU SCRIPT AKTIF");

        musicVolumeSilder.value = PlayerPrefs.GetFloat("MusicVolume", 0f);

        // if (RelayService.Instance.State == RelayService.RelayState.None)
        // {
        //     _ = StartRelay();
        // }
    }

    // private async UniTask StartRelay()
    // {
    //     Debug.Log("MainMenu: Setup Relay");
    //     relayConnectingScreen.SetActive(true);
    //     PlayerPrefs.SetString("RelayUrl", "coke-bonfire-vaguely.ngrok-free.dev");
    //     bool result = await RelayService.Instance.Setup();
    //     relayConnectingScreen.SetActive(false);
    //     if (!result)
    //     {
    //         DoShowRelayFailedScreen();
    //     }
    //     else
    //     {
    //         RelayService.Instance.OnConnectControllerRequest += (_, callback) =>
    //         {
    //             if (!connectingController) callback(false);
    //             callback(true);
    //         };
    //     }
    // }

    private void Start()
    {
        transition.TransitionIn(() => {});
    }

    public void cutting()
    {
        pendingScene = "cutting";
        // if (!CheckControllers()) return;

        transition.TransitionOut(() => SceneManager.LoadScene(pendingScene));
    }

    public void fishing()
    {
        pendingScene = "fishing";
        // if (!CheckControllers()) return;

        transition.TransitionOut(() => SceneManager.LoadScene(pendingScene));
    }

    public void plane()
    {
        pendingScene = "airplane";
        // if (!CheckControllers()) return;

        transition.TransitionOut(() => SceneManager.LoadScene(pendingScene));
    }

    public void exit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        audioMixer.SetFloat("MusicVolume", value);
    }

    // public void CloseRelayFailedScreen()
    // {
    //     Application.Quit();
    // }

    // public void CloseConnectControllerScreen()
    // {
    //     DoCloseConnectControllerScreen();
    // }

    // public void GoToCallibration()
    // {
    //     DoCloseConnectControllerScreen();
    //     DoShowCallibrateControllerScreen();
    // }

    // public void CloseCallibrateControllerScreen()
    // {
    //     DoCloseCallibrateControllerScreen();
    // }

    // ============================================================================================

    // private bool CheckControllers()
    // {
    //     sbyte index = ControllerService.Instance.GetNonReadyControllerIndex();
    //     if (index > -1)
    //     {
    //         ShowConnectControllerScreen();

    //         connectingController = true;

    //         GameObject check1 = connectControllerScreen.transform.Find("Panel").Find("Check First").gameObject;
    //         GameObject check2 = connectControllerScreen.transform.Find("Panel").Find("Check Second").gameObject;

    //         Controller controller1 = ControllerService.Instance.GetController(0);
    //         Controller controller2 = ControllerService.Instance.GetController(1);

    //         bool attemptToCallibration()
    //         {
    //             if (controller1.State == Controller.ControllerState.Live && controller2.State == Controller.ControllerState.Live)
    //             {
    //                 DoShowCallibrateControllerScreen();
    //                 stopConnectingController = null;
    //                 return true;
    //             }
    //             else
    //             {
    //                 return false;
    //             }
    //         }

    //         void controllerConnect1(object sender, EventArgs e)
    //         {
    //             check1.SetActive(true);
    //             if (!attemptToCallibration())
    //             {
    //                 controller1.OnPromoted -= controllerConnect1;
    //                 controller1.OnShutdown -= controllerDisconnect1;
    //             }
    //         }
    //         void controllerDisconnect1(object sender, EventArgs e)
    //         {
    //             check1.SetActive(false);
    //         }
    //         void controllerConnect2(object sender, EventArgs e)
    //         {
    //             check2.SetActive(true);
    //             if (!attemptToCallibration())
    //             {
    //                 controller2.OnPromoted -= controllerConnect2;
    //                 controller2.OnShutdown -= controllerDisconnect2;
    //             }
    //         }
    //         void controllerDisconnect2(object sender, EventArgs e)
    //         {
    //             check2.SetActive(false);
    //         }

    //         if (controller1.State != Controller.ControllerState.Live)
    //         {
    //             check1.SetActive(false);
    //             controller1.OnPromoted += controllerConnect1;
    //             controller1.OnShutdown += controllerDisconnect1;
    //         }
    //         else
    //         {
    //             check1.SetActive(true);
    //         }
    //         if (controller2.State != Controller.ControllerState.Live)
    //         {
    //             check2.SetActive(false);
    //             controller2.OnPromoted += controllerConnect2;
    //             controller2.OnShutdown += controllerDisconnect2;
    //         }
    //         else
    //         {
    //             check2.SetActive(true);
    //         }

    //         stopConnectingController = () =>
    //         {
    //             controller1.OnPromoted -= controllerConnect1;
    //             controller1.OnShutdown -= controllerDisconnect1;
    //             controller2.OnPromoted -= controllerConnect2;
    //             controller2.OnShutdown -= controllerDisconnect2;
    //             stopConnectingController = null;
    //         };

    //         attemptToCallibration();

    //         return false;
    //     }
    //     else
    //     {
    //         return true;
    //     }
    // }

    // private void ShowConnectControllerScreen()
    // {
    //     connectControllerScreen.SetActive(true);
    //     GameObject panel = connectControllerScreen.transform.Find("Panel").gameObject;

    //     LSequence.Create()
    //         .Append(LMotion.Create(0f, 1f, 0.5f)
    //             .WithEase(Ease.OutCubic)
    //             .BindToAlpha(connectControllerScreen.GetComponent<CanvasGroup>()))
    //         .Join(LMotion.Create(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), 1f)
    //             .WithEase(Ease.OutCubic)
    //             .BindToLocalScale(panel.transform))
    //         .Run();
    // }

    // private void DoCloseConnectControllerScreen()
    // {
    //     stopConnectingController();

    //     GameObject panel = connectControllerScreen.transform.Find("Panel").gameObject;

    //     LSequence.Create()
    //         .Append(LMotion.Create(1f, 0f, 0.5f)
    //             .WithEase(Ease.OutCubic)
    //             .BindToAlpha(connectControllerScreen.GetComponent<CanvasGroup>()))
    //         .Join(LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.8f, 0.8f, 0.8f), 0.5f)
    //             .WithEase(Ease.OutCubic)
    //             .WithOnComplete(() =>
    //             {
    //                 connectControllerScreen.SetActive(false);
    //             })
    //             .BindToLocalScale(panel.transform))
    //         .Run();
    // }

    // private void DoShowRelayFailedScreen()
    // {
    //     relayFailedScreen.SetActive(true);
    //     GameObject panel = relayFailedScreen.transform.Find("Panel").gameObject;

    //     LSequence.Create()
    //         .Append(LMotion.Create(0f, 1f, 0.5f)
    //             .WithEase(Ease.OutCubic)
    //             .BindToAlpha(relayFailedScreen.GetComponent<CanvasGroup>()))
    //         .Join(LMotion.Create(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), 1f)
    //             .WithEase(Ease.OutCubic)
    //             .BindToLocalScale(panel.transform))
    //         .Run();
    // }

    // private void DoShowCallibrateControllerScreen()
    // {
    //     callibrateControllerScreen.SetActive(true);
    //     GameObject panel = callibrateControllerScreen.transform.Find("Panel").gameObject;

    //     LSequence.Create()
    //         .Append(LMotion.Create(0f, 1f, 0.5f)
    //             .WithEase(Ease.OutCubic)
    //             .BindToAlpha(callibrateControllerScreen.GetComponent<CanvasGroup>()))
    //         .Join(LMotion.Create(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), 1f)
    //             .WithEase(Ease.OutCubic)
    //             .BindToLocalScale(panel.transform))
    //         .Run();
    // }

    // private void DoCloseCallibrateControllerScreen()
    // {
    //     Controller controller1 = ControllerService.Instance.GetController(0);
    //     Controller controller2 = ControllerService.Instance.GetController(1);

    //     if (!CheckControllers())
    //     {
    //         controller1.CallibratePosition();
    //         controller2.CallibratePosition();
    //         SceneManager.LoadScene(pendingScene);
    //     }
    //     else
    //     {
    //         callibrateControllerScreen.SetActive(false);
    //     }
    // }
}