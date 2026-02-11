using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneManager : MonoBehaviour
{
    [SerializeField] List<CloneSpawningPlatform> platforms = new List<CloneSpawningPlatform>();
    [SerializeField] List<MovingPlatform> movingPlatforms = new List<MovingPlatform>();
    public bool isRecording = false;
    [SerializeField] ThirdPersonController thirdPersonController;
    public ColorRef currentColor;

    public void Start()
    {
        foreach (var platform in platforms)
        {
            platform.manager = this;
        }
    }

    public void StartedRecording(CloneSpawningPlatform recordingPlatform)
    {
        isRecording = true;
        currentColor = recordingPlatform.platformColor;
        foreach (var platform in platforms)
        {
            if (recordingPlatform != platform)
            {
                platform.StartPlaying();
            }

        }
        foreach (var movingPlatform in movingPlatforms)
        {
            movingPlatform.RestartPlatform();

        }
    }

    public void StopedRecording(CloneSpawningPlatform recordingPlatform)
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.ResetGrab();
        }
        else
        {
            print("dick");
        }

            foreach (var platform in platforms)
            {
                if (recordingPlatform != platform)
                {
                    platform.DisableClone();
                }

            }


        //if (thirdPersonController.controllingHourglass == true)
        //{
        //    thirdPersonController.ToggleHourglassControl();
        //}

        isRecording = false;
    }
}
