using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

#nullable enable

public class GameplayManager : MonoBehaviour
{
    // Public Properties =========================================================

    public enum GameState
    {
        Init,
        Countdown,
        Live,
        Finished
    }
    public GameState State { get; private set; } = GameState.Init;

    public event EventHandler? OnStartGame;
    public event EventHandler? OnFinishGame;

    public float TimeModifier { get; private set; }

    // Public Properties =========================================================

    // Private Properties =========================================================

    [SerializeField] private Volume volume;
    private ColorAdjustments? colorAdjustments;
    private LensDistortion? lensDistortion;

    private GameObject? tutorialContent;

    private bool isPaused;

    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private GameObject countdownScreen;

    [SerializeField] private GameObject splitFinishScreen;
    [SerializeField] private Texture splitSuccessTexture;
    [SerializeField] private Texture splitFailTexture;

    [SerializeField] private GameObject resultScreen;
    [SerializeField] private GameObject pauseScreen;

    [SerializeField] private SceneTransition transition;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickAudio;
    [SerializeField] private AudioClip countdownTickAudio;
    [SerializeField] private AudioClip countdownGoAudio;
    [SerializeField] private AudioClip failAudio;
    [SerializeField] private AudioClip laughAudio;

    // Private Properties =========================================================

    // Public Functions =========================================================

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMenu();
        }
    }

    public void RegisterGame(GameObject tutorialContent)
    {
        this.tutorialContent = tutorialContent;
    }

    public void InitGame()
    {
        if (State != GameState.Init) return;

        DoPreInitGame();

        transition.TransitionIn(() => DoInitGame());
    }

    public void QuitGame()
    {
        DoQuitGame();
    }

    public void CloseTutorialScreen()
    {
        if (State != GameState.Init) return;

        DoCloseTutorialScreen();
    }

    public void FinishGame(int winner, int scoreFirst, int scoreSecond)
    {
        if (State != GameState.Live) return;

        DoFinishGame(winner, scoreFirst, scoreSecond);
    }

    public void ShowMenu()
    {

        DoShowMenu();
    }

    public void CloseMenu()
    {
        DoCloseMenu();
    }

    // Public Functions =========================================================

    // Private Functions =========================================================

    private void DoPreInitGame()
    {
        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out lensDistortion);
        colorAdjustments!.saturation.overrideState = true;
        colorAdjustments!.saturation.value = -100f;
        lensDistortion!.intensity.overrideState = true;
        lensDistortion!.intensity.value = -0.6f;

        Transform contentContainer = tutorialScreen.transform.Find("Panel").Find("Content");
        Instantiate(tutorialContent, contentContainer);
    }

    private void DoInitGame()
    {
        tutorialScreen.SetActive(true);
        GameObject panel = tutorialScreen.transform.Find("Panel").gameObject;

        LSequence.Create()
            .Append(LMotion.Create(0f, 1f, 0.5f)
                .WithEase(Ease.OutCubic)
                .BindToAlpha(tutorialScreen.GetComponent<CanvasGroup>()))
            .Join(LMotion.Create(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), 1f)
                .WithEase(Ease.OutCubic)
                .BindToLocalScale(panel.transform))
            .Run();
    }

    private void DoCloseTutorialScreen()
    {
        GameObject panel = tutorialScreen.transform.Find("Panel").gameObject;

        audioSource.PlayOneShot(clickAudio);

        LSequence.Create()
            .Append(LMotion.Create(1f, 0f, 0.5f)
                .WithEase(Ease.OutCubic)
                .BindToAlpha(tutorialScreen.GetComponent<CanvasGroup>()))
            .Join(LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.8f, 0.8f, 0.8f), 0.5f)
                .WithEase(Ease.OutCubic)
                .WithOnComplete(() =>
                {
                    tutorialScreen.SetActive(false);
                    StartCountdown();
                })
                .BindToLocalScale(panel.transform))
            .Run();
    }

    private void StartCountdown()
    {
        State = GameState.Countdown;

        countdownScreen.SetActive(true);
        GameObject number = countdownScreen.transform.Find("Number").gameObject;

        number.GetComponent<TextMeshProUGUI>().SetText("3");

        LSequence.Create()
            .Append(LMotion.Create(new Vector3(2f, 2f, 2f), new Vector3(1f, 1f, 1f), 0.25f)
                .WithOnComplete(() => audioSource.PlayOneShot(countdownTickAudio))
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.5f, 0.5f, 0.5f), 1f)
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 0f, 0f), 0.25f)
                .WithOnComplete(() => number.GetComponent<TextMeshProUGUI>().SetText("2"))
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(2f, 2f, 2f), new Vector3(1f, 1f, 1f), 0.25f)
                .WithOnComplete(() => audioSource.PlayOneShot(countdownTickAudio))
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.5f, 0.5f, 0.5f), 1f)
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 0f, 0f), 0.25f)
                .WithOnComplete(() => number.GetComponent<TextMeshProUGUI>().SetText("1"))
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(2f, 2f, 2f), new Vector3(1f, 1f, 1f), 0.25f)
                .WithOnComplete(() => audioSource.PlayOneShot(countdownTickAudio))
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.5f, 0.5f, 0.5f), 1f)
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 0f, 0f), 0.25f)
                .WithOnComplete(() => number.GetComponent<TextMeshProUGUI>().SetText("Go!"))
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(2f, 2f, 2f), new Vector3(1f, 1f, 1f), 0.25f)
                .WithOnComplete(() => audioSource.PlayOneShot(countdownGoAudio, 1.5f))
                .BindToLocalScale(number.transform))
            .Join(
                LMotion.Create(-100f, 0f, 1f)
                    .Bind(x => colorAdjustments!.saturation.value = x)
            )
            .Join(
                LMotion.Create(-0.6f, 0f, 1f)
                    .Bind(x => lensDistortion!.intensity.value = x)
            )
            .Append(LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.5f, 0.5f, 0.5f), 1f)
                .BindToLocalScale(number.transform))
            .Append(LMotion.Create(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 0f, 0f), 0.25f)
                .WithOnComplete(() =>
                {
                    countdownScreen.SetActive(false);
                    StartLive();
                })
                .BindToLocalScale(number.transform))
            .Run();
    }

    private void StartLive()
    {
        State = GameState.Live;

        OnStartGame?.Invoke(this, new EventArgs());
    }

    private void DoFinishGame(int winner, int scoreFirst, int scoreSecond)
    {


        GameObject leftPanel = splitFinishScreen.transform.Find("Left").gameObject;
        GameObject leftImage = splitFinishScreen.transform.Find("Left Image").gameObject;
        GameObject rightPanel = splitFinishScreen.transform.Find("Right").gameObject;
        GameObject rightimage = splitFinishScreen.transform.Find("Right Image").gameObject;

        ColorUtility.TryParseHtmlString("#FF003350", out Color failColor);
        ColorUtility.TryParseHtmlString("#29292950", out Color successColor);

        if (winner == 0)
        {
            leftPanel.GetComponent<Image>().color = successColor;
            leftImage.GetComponent<RawImage>().texture = splitSuccessTexture;
            rightPanel.GetComponent<Image>().color = failColor;
            rightimage.GetComponent<RawImage>().texture = splitFailTexture;
        }
        else if (winner == 1)
        {
            leftPanel.GetComponent<Image>().color = failColor;
            leftImage.GetComponent<RawImage>().texture = splitFailTexture;
            rightPanel.GetComponent<Image>().color = successColor;
            rightimage.GetComponent<RawImage>().texture = splitSuccessTexture;
        }
        else
        {
            leftPanel.GetComponent<Image>().color = successColor;
            leftImage.GetComponent<RawImage>().texture = splitSuccessTexture;
            rightPanel.GetComponent<Image>().color = successColor;
            rightimage.GetComponent<RawImage>().texture = splitSuccessTexture;
        }

        State = GameState.Finished;
        OnFinishGame?.Invoke(this, new EventArgs());

        leftImage.GetComponent<CanvasGroup>().alpha = 0;
        rightimage.GetComponent<CanvasGroup>().alpha = 0;
        splitFinishScreen.SetActive(true);

        LSequence.Create()
            .Append(
                LMotion.Create(0f, 1f, 0.75f)
                    .WithOnComplete(() =>
                    {
                        audioSource.PlayOneShot(failAudio, 1.5f);
                    })
                    .BindToAlpha(splitFinishScreen.GetComponent<CanvasGroup>())
            )
            .Join(
                LMotion.Create(0f, -100f, 2f)
                    .Bind(x => colorAdjustments!.saturation.value = x)
            )
            .Join(
                LMotion.Create(1f, 0f, 2.5f)
                    .Bind(x => TimeModifier = x)
            )
            .Append(
               LMotion.Create(new Vector3(4f, 4f, 4f), new Vector3(1f, 1f, 1f), 1f)
                .WithDelay(2f)
                .WithOnComplete(() =>
                {
                    audioSource.PlayOneShot(countdownTickAudio, 1f);
                    audioSource.PlayOneShot(laughAudio, 1f);
                })
                .WithEase(Ease.OutElastic)
                .BindToLocalScale(leftImage.transform)
            )
            .Join(
               LMotion.Create(0f, 1f, 1f)
                .WithDelay(3f)
                .BindToAlpha(leftImage.GetComponent<CanvasGroup>())
            )
            .Join(
               LMotion.Create(new Vector3(4f, 4f, 4f), new Vector3(1f, 1f, 1f), 1f)
                .WithDelay(3f)
                .WithEase(Ease.OutElastic)
                .BindToLocalScale(rightimage.transform)
            )
            .Join(
               LMotion.Create(0f, 1f, 1f)
                .WithDelay(3f)
                .BindToAlpha(rightimage.GetComponent<CanvasGroup>())
            )
            .Append(
                LMotion.Create(1f, 1f, 2f)
                .WithOnComplete(() =>
                {
                    if (scoreFirst == -1)
                    {
                        DoQuitGame();
                    }
                    else
                    {
                        ShowResultScreen(scoreFirst, scoreSecond);
                    }
                })
                .BindToAlpha(splitFinishScreen.GetComponent<CanvasGroup>())
            )
            .Run();
    }

    private void ShowResultScreen(int scoreFirst, int scoreSecond)
    {
        resultScreen.SetActive(true);
        GameObject panel = resultScreen.transform.Find("Panel").gameObject;
        TextMeshProUGUI firstScore = panel.transform.Find("Score First").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI secondScore = panel.transform.Find("Score Second").GetComponent<TextMeshProUGUI>();

        firstScore.SetText($"{scoreFirst}");
        secondScore.SetText($"{scoreSecond}");

        LSequence.Create()
         .Append(LMotion.Create(0f, 1f, 0.5f)
            .WithEase(Ease.OutCubic)
            .BindToAlpha(resultScreen.GetComponent<CanvasGroup>()))
         .Join(LMotion.Create(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), 1f)
            .WithEase(Ease.OutCubic)
            .BindToLocalScale(panel.transform))
         .Run();
    }

    private void DoShowMenu()
    {
        if (isPaused) return;
        isPaused = true;

        pauseScreen.SetActive(true);
        GameObject panel = pauseScreen.transform.Find("Panel").gameObject;

        LSequence.Create()
            .Append(LMotion.Create(0f, 1f, 0.5f)
                .WithEase(Ease.OutCubic)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToAlpha(pauseScreen.GetComponent<CanvasGroup>()))
            .Join(LMotion.Create(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), 1f)
                .WithEase(Ease.OutCubic)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToLocalScale(panel.transform))
            .Join(
                LMotion.Create(0f, -100f, 1f)
                    .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                    .Bind(x => colorAdjustments!.saturation.value = x)
            )
            .Join(
                LMotion.Create(0f, -0.6f, 1f)
                    .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                    .Bind(x => lensDistortion!.intensity.value = x)
            )
            // .Join(
            //     LMotion.Create(1f, 0.1f, 1f)
            //         .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
            //         .Bind(x => Time.timeScale = x)
            // )
            .Run();
    }

    private void DoCloseMenu()
    {
        // Time.timeScale = 0.1f;
        GameObject panel = pauseScreen.transform.Find("Panel").gameObject;

        audioSource.PlayOneShot(clickAudio);

        LSequence.Create()
            .Append(LMotion.Create(1f, 0f, 0.5f)
                .WithEase(Ease.OutCubic)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToAlpha(pauseScreen.GetComponent<CanvasGroup>()))
            .Join(LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.8f, 0.8f, 0.8f), 0.5f)
                .WithEase(Ease.OutCubic)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToLocalScale(panel.transform))
            .Join(
                LMotion.Create(-100f, -0f, 1f)
                    .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                    .Bind(x => colorAdjustments!.saturation.value = x)
            )
            .Join(
                LMotion.Create(-0.6f, 0f, 1f)
                    .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                    .Bind(x => lensDistortion!.intensity.value = x)
            )
            // .Join(
            //     LMotion.Create(0.1f, 1f, 1f)
            //         .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
            //         .WithOnComplete(() =>
            //         {
            //             isPaused = false;
            //         })
            //         .Bind(x => Time.timeScale = x)
            // )
            .Run();
    }

    private void DoQuitGame()
    {
        transition.TransitionOut(() => SceneManager.LoadScene("mainmenu"));
    }

    // Private Functions =========================================================
}
