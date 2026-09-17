using System;
using System.Collections;
using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject logo;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioIn;
    [SerializeField] private AudioClip audioOut;

    public void TransitionIn(Action OnComplete)
    {
        panel.SetActive(true);

        // logo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        LSequence.Create()
            .Append(
                LMotion.Create(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(2f, 2f, 2f), 0.5f)
                    .WithEase(Ease.InCubic)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToLocalScale(logo.transform)
            )
            .Append(
                LMotion.Create(new Vector3(2f, 2f, 2f), new Vector3(0.5f, 0.5f, 0.5f), 0.5f)
                    .WithEase(Ease.OutCubic)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToLocalScale(logo.transform)
            )
            .Append(
                LMotion.Create(0f, -1920f, 0.5f)
                    .WithDelay(1f)
                    .WithEase(Ease.OutCubic)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() =>
                    {
                        panel.SetActive(false);
                        OnComplete();
                    })
                    .BindToAnchoredPositionX(panel.transform as RectTransform)
            )
            .Join(
                LMotion.Create(0f, 0f, 0.1f)
                    .WithDelay(1f)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() => audioSource.PlayOneShot(audioIn, 2f))
                    .Bind(x => { })
            )
            .Run();
    }

    public void TransitionOut(Action OnComplete)
    {
        panel.SetActive(true);

        LSequence.Create()
            .Append(
                LMotion.Create(0f, 0f, 0.1f)
                    .WithDelay(1f)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() => audioSource.PlayOneShot(audioOut, 2f))
                    .Bind(x => { })
            )
            .Append(
                LMotion.Create(-1920f, 0f, 0.5f)
                    .WithEase(Ease.OutCubic)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(OnComplete)
                    .BindToAnchoredPositionX(panel.transform as RectTransform)
            )
            .Run();
    }
}
