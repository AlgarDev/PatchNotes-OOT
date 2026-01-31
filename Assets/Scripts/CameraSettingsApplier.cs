using Cinemachine;
using UnityEngine;

public class CameraSettingsApplier : MonoBehaviour
{
    private CinemachineFreeLook freeLook;
    bool inverted;
    private float baseXSpeed;
    private float baseYSpeed;

    void Awake()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
    }

    void Start()
    {
        baseXSpeed = freeLook.m_XAxis.m_MaxSpeed;
        baseYSpeed = freeLook.m_YAxis.m_MaxSpeed;

        inverted = GameSettings.cameraInverted;
        freeLook.m_YAxis.m_InvertInput = inverted;
    }
    void Update()
    {
        inverted = GameSettings.cameraInverted;
        if (freeLook.m_YAxis.m_InvertInput != inverted)
        {
            freeLook.m_YAxis.m_InvertInput = inverted;
        }

        if (GameSettings.sensitivity != 0)
        {

            freeLook.m_YAxis.m_MaxSpeed = baseYSpeed * GameSettings.sensitivity;
            freeLook.m_XAxis.m_MaxSpeed = baseXSpeed * GameSettings.sensitivity;
        }


    }
}