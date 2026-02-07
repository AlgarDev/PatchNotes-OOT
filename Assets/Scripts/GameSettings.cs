using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameSettings : MonoBehaviour
{
    private static GameSettings _instance;
    public static GameSettings Instance { get { return _instance; } }

    [SerializeField]
    private AudioMixer audioMixer;

    static public bool musicEnabled;
    static public bool cameraInverted;
    static public float volume;
    static public float sensitivity;

    static private bool doOnce;

    //[SerializeField]
    //public Slider volumeSlider;
    //[SerializeField]
    //public Slider sensitivitySlider;
    //[SerializeField]
    //public Toggle musicToggle;
    //[SerializeField]
    //public Toggle invertToggle;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (!doOnce)
        {
            volume = 0.5f;
            sensitivity = 1f;
            musicEnabled = true;
            cameraInverted = false;
            doOnce = true;
        }
    }
    private void Start()
    {
        float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("volume", Mathf.Log10(clampedVolume) * 20f);

        //volumeSlider.SetValueWithoutNotify(volume);
        //sensitivitySlider.SetValueWithoutNotify(sensitivity);
        //musicToggle.isOn = musicEnabled;
        //invertToggle.isOn = cameraInverted;
    }

    public static void ChangeMusicToggle(bool checkValue)
    {
        if (Instance == null)
            return;

        musicEnabled = checkValue;
        Instance.ApplyMusic();
    }
    public static void ChangeVolumeSlider(float value)
    {
        if (Instance == null)
            return;

        volume = value;
        Instance.ApplyVolume();

    }
    public static void InvertCameraToggle(bool checkValue)
    {
        cameraInverted = checkValue;
        print(cameraInverted);
    }
    public static void ChangeSensitivitySlider(float value)
    {
        sensitivity = value;
    }

    private void ApplyVolume()
    {
        float clamped = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("volume", Mathf.Log10(clamped) * 20f);
    }

    private void ApplyMusic()
    {
        audioMixer.SetFloat("MusicVolume", musicEnabled ? 0f : -80f);
    }
}
