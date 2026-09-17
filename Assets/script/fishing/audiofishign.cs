using UnityEngine;

public class audiofishing : MonoBehaviour
{
    [SerializeField] private GameplayManagerProxy gameplayManagerProxy;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bombAudio;
    [SerializeField] private AudioClip sliceAudio;
    [SerializeField] private AudioClip splashAudio;

    void Start()
    {
        gameplayManagerProxy.OnStartGame += (_, _) =>
        {
            audioSource.Play();
        };
    }

    public void PlayBomb()
    {
        audioSource.PlayOneShot(bombAudio, 0.5f);
    }

    public void PlaySlice()
    {
        audioSource.PlayOneShot(sliceAudio, 1f);
    }

    public void PlaySplash()
    {
        audioSource.PlayOneShot(splashAudio, 1f);
    }
}