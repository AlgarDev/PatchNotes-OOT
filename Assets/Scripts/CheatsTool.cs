using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class CheatsTool : EditorWindow
{

    // Add a menu item to open the tool window
    [MenuItem("Window/CheatsTool")]
    public static void ShowWindow()
    {
        GetWindow<CheatsTool>("CheatsTool");
    }

    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Start Recording"))
            {
                CloneInputRecorder.Instance.StartRecording();
            }
            if (GUILayout.Button("Stop Recording"))
            {
                CloneInputRecorder.Instance.StopRecording();
            }
            if (GUILayout.Button("Start Clones"))
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    Debug.LogWarning("No player with tag 'Player' found!");
                    return;
                }
                player.gameObject.SetActive(false);

                // Find all clones
                CloneController[] clones = FindObjectsOfType<CloneController>();
                foreach (var clone in clones)
                {
                    clone.Initialize(CloneInputRecorder.Instance.Frames);
                    Debug.Log("CLONE CONTROLLLLLAA");
                }

                Debug.Log($"Started {clones.Length} clones playback.");
            }
            

        }
        else
        {
            GUILayout.Label("Actions hidden while game isn't playing :3", EditorStyles.boldLabel);
        }


    }




}
#endif