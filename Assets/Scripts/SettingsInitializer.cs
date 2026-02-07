using UnityEngine;
using UnityEngine.UI;

public class SettingsInitializer : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle invertToggle;

    private void Start()
    {
        volumeSlider.SetValueWithoutNotify(GameSettings.volume);
        sensitivitySlider.SetValueWithoutNotify(GameSettings.sensitivity);
        musicToggle.isOn = GameSettings.musicEnabled;
        invertToggle.isOn = GameSettings.cameraInverted;
    }
}
