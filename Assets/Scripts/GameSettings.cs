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

    [SerializeField]
    public Slider volumeSlider;
    [SerializeField]
    public Slider sensitivitySlider;
    [SerializeField]
    public Toggle musicToggle;
    [SerializeField]
    public Toggle invertToggle;

    static private bool doOnce;
    private void Awake()
    {   
        if (!doOnce)
        {
            volume = 0.5f;
            sensitivity = 1f;
            musicEnabled = true;
            cameraInverted = false;
            doOnce = true;
        }
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    private void Start()
    {
        float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("volume", Mathf.Log10(clampedVolume) * 20f);

        volumeSlider.SetValueWithoutNotify(volume);
        sensitivitySlider.SetValueWithoutNotify(sensitivity);
        musicToggle.isOn = musicEnabled;
        invertToggle.isOn = cameraInverted;
    }

    public void ChangeMusicToggle(bool checkValue)
    {
        musicEnabled = checkValue;
        SetMusicVolume(musicEnabled);
    }
    public void InvertCameraToggle(bool checkValue)
    {
        cameraInverted = checkValue;
        print(cameraInverted);
    }
    public void ChangeVolumeSlider(float value)
    {
        volume = value;

        float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("volume", Mathf.Log10(clampedVolume) * 20f);

    }
    public void ChangeSensitivitySlider(float value)
    {
        sensitivity = value;
    }
    private void SetMusicVolume(bool isEnabled)
    {
        audioMixer.SetFloat("MusicVolume", isEnabled ? 0f : -80f);
    }
}
