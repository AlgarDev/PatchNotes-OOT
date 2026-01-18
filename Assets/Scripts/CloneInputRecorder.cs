using System.Collections.Generic;
using UnityEngine;

public class CloneInputRecorder : MonoBehaviour
{
    public readonly List<PlayerInputFrame> Frames = new();

    public bool IsRecording { get; private set; }

    public void StartRecording()
    {
        Frames.Clear();
        IsRecording = true;
    }

    public void StopRecording()
    {
        IsRecording = false;
    }

    public void Record(PlayerInputFrame frame)
    {
        if (!IsRecording) return;
        Frames.Add(frame);
    }
}
