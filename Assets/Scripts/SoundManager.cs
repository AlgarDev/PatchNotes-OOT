using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] static private AudioClip UIClick;
    [SerializeField] static private AudioSource clickAudioSource;

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

    private void Start()
    {
        clickAudioSource.clip = UIClick;
    }

    static public void ClickSound()
    {
        clickAudioSource.pitch = 1f + Random.Range(-0.2f, 0.2f);
        clickAudioSource.PlayOneShot(UIClick);
    }
}
