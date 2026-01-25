using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip UIClick;
    //[SerializeField] private AudioSource Music;
    [SerializeField] private AudioSource clickAudioSource;

    private void Awake()
    {
        if (FindObjectsOfType<SoundManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        clickAudioSource.clip = UIClick;
    }
    public void ClickSound()
    {
        clickAudioSource.pitch = 1f + Random.Range(-0.2f, 0.2f);
        clickAudioSource.PlayOneShot(UIClick);
    }
}
