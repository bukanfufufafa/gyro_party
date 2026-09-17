using UnityEngine;

public class CuttingAudioBootstrap : MonoBehaviour
{
    [SerializeField] private GameplayManagerProxy gameplayManagerProxy;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bombAudio;

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
}