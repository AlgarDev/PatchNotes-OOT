using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioClip UIClick;
    [SerializeField] private AudioSource clickAudioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void ClickSound()
    {
        if (Instance == null)
            return;

        Instance.PlayClick();
    }

    private void PlayClick()
    {
        clickAudioSource.pitch = 1f + Random.Range(-0.2f, 0.2f);
        clickAudioSource.PlayOneShot(UIClick);
    }
}
