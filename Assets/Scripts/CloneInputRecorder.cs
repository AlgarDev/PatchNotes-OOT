using System.Collections.Generic;
using UnityEngine;

public class CloneInputRecorder : MonoBehaviour
{
    [field:SerializeField]
    public List<PlayerInputFrame> Frames = new();

    public bool IsRecording { get; private set; }
    public static CloneInputRecorder Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void StartRecording()
    {
        Frames.Clear();
        IsRecording = true;
        Debug.Log("Started Recording");
    }

    public void StopRecording()
    {
        IsRecording = false;
        Debug.Log("Stopped Recording");
    }
    public void Record(PlayerInputFrame frame)
    {
        if (!IsRecording) return;
        Frames.Add(frame);
        //print(Frames.Count);
    }
}
