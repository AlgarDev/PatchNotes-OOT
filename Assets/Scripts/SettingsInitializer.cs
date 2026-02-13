using UnityEngine;
using UnityEngine.UI;

public class SettingsInitializer : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle invertToggle;

    private void OnEnable()
    {
        volumeSlider?.SetValueWithoutNotify(GameSettings.volume);
        sensitivitySlider?.SetValueWithoutNotify(GameSettings.sensitivity);
        if (musicToggle != null)
            musicToggle.isOn = GameSettings.musicEnabled;
        if (invertToggle)
            invertToggle.isOn = GameSettings.cameraInverted;


    }
}
