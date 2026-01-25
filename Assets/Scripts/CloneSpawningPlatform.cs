using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CloneSpawningPlatform : MonoBehaviour
{

    [Header("Clone")]
    [SerializeField] public ColorRef platformColor;
    [SerializeField] private UnityEvent spawnClone;
    [SerializeField] private GameObject cloneObject;
    [SerializeField] private GameObject cloneHourglass;

    [Header("Recording Timer")]
    [SerializeField] private float recordingDuration = 10f;

    public float timerRemaining;
    public bool timerPaused;

    private bool isPlayerInside;
    private bool isRecording = false;

    public GameObject currentClone;
    private GameObject currentHourglass;
    private List<PlayerInputFrame> currentRecording;

    private Vector3 recordingStartPosition;
    private Quaternion recordingStartRotation;
    private ThirdPersonController player;

    private bool cloneQueuedLastFrame;

    public CloneManager manager;

    [Header("Visuals")]
    [SerializeField] private MeshRenderer[] platformFloorAndShine;
    [SerializeField] private MeshRenderer platformBorder;
    private ColorsSO colors;
    [SerializeField] private GameObject shine;




    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller != null && controller.IsPlayerControlled && manager.isRecording == false)
        {
            isPlayerInside = true;
            player = controller;

            if (manager.isRecording == false)
            {
                player.GetComponent<PlayerColorManager>().ChangeColor(platformColor);
                player.GetComponent<PlayerColorManager>().EnableHourglassSand();
                player.GetComponent<PlayerColorManager>().UpdateTargetValue(1);
                UIManager.instance.UIState(true);
                UIManager.instance.ChangeSliderColor(platformColor);
                UIManager.instance.UpdateSliderValue(1, 1);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller != null && controller.IsPlayerControlled)
        {
            isPlayerInside = false;
            if (manager.isRecording == false)
            {
                player.GetComponent<PlayerColorManager>().ChangeColor(ColorRef.Green);
                player.GetComponent<PlayerColorManager>().DisableHourglassSand();

                UIManager.instance.UIState(false);
            }
            //to do : Dont change player color if recording
        }
    }
    private void Start()
    {
        if (shine != null) shine.SetActive(false);
        colors = Resources.Load<ColorsSO>("ColorsSO");
        if (colors != null) ChangeColor(platformColor);
    }
    private void Update()
    {
        HandleInput();
        UpdateTimer();
    }

    private void HandleInput()
    {
        bool clonePressed = PlayerInputProvider.Instance.CloneQueued;
        bool clonePressedEdge = clonePressed && !cloneQueuedLastFrame;

        if (clonePressedEdge)
        {
            if (isPlayerInside && !isRecording)
            {
                StartRecording();
            }
            else if (isRecording)
            {
                StopRecording();
            }
        }

        cloneQueuedLastFrame = clonePressed;
    }

    private void UpdateTimer()
    {
        if (!isRecording || timerPaused)
            return;

        timerRemaining -= Time.deltaTime;
        UIManager.instance.UpdateSliderValue(timerRemaining, recordingDuration);
        player.GetComponent<PlayerColorManager>().UpdateTargetValue(timerRemaining / recordingDuration);
        if (currentHourglass != null) currentHourglass.GetComponent<PlayerColorManager>().UpdateTargetValue(timerRemaining / recordingDuration);

        if (timerRemaining <= 0f)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        if (manager.isRecording == true) return;
        if (currentRecording != null) currentRecording = null;

        Debug.Log("Started Recording " + platformColor.ToString());

        recordingStartPosition = player.transform.position;
        recordingStartRotation = player.transform.rotation;
        timerRemaining = recordingDuration;
        timerPaused = false;

        CloneInputRecorder.Instance.StartRecording();
        isRecording = true;
        player.SetActiveSpawner(this);
        manager.StartedRecording(this);

    }

    public void StopRecording()
    {
        if (manager.isRecording == false) return;


        Debug.Log("Stoped Recording " + platformColor.ToString());
        if (!isRecording)
            return;

        isRecording = false;

        if (shine != null) shine.SetActive(true);
        manager.StopedRecording(this);

        //Visuals
        CloneInputRecorder.Instance.StopRecording();
        UIManager.instance.UIState(false);

        //Save current recording
        currentRecording = new List<PlayerInputFrame>(CloneInputRecorder.Instance.Frames);

        // Teleport player back
        player.controller.enabled = false;
        player.transform.position = recordingStartPosition;
        player.transform.rotation = recordingStartRotation;
        player.controller.enabled = true;

        player.SetActiveSpawner(null);
        timerPaused = false;

    }

    public void StartPlaying()
    {
        SpawnClone();
    }

    public void DisableClone()
    {
        if (currentClone == null) return;
        Debug.Log("Stoped Playback of " + platformColor.ToString());

        if (currentClone != null)
        {
            Destroy(currentClone);
            currentClone = null;
        }

        if (currentHourglass != null)
        {
            Destroy(currentHourglass);
            currentHourglass = null;
        }

        isRecording = false;
        timerPaused = false;
        timerRemaining = 0f;
    }

    private void SpawnClone()
    {
        if (currentRecording != null)
        {
            Debug.Log("Started Playback of " + platformColor.ToString());
            spawnClone?.Invoke();
            currentClone = Instantiate(cloneObject, recordingStartPosition, recordingStartRotation);
            currentHourglass = Instantiate(cloneHourglass,
                recordingStartPosition * -100,
                Quaternion.identity);
            currentClone.GetComponent<CloneController>().activeSpawner = this;
            currentClone.GetComponent<ThirdPersonController>().SetHourglass(currentHourglass);
            currentClone.GetComponent<PlayerColorManager>().GhostState(true);
            currentClone.GetComponent<PlayerColorManager>().DisableHourglass();
            currentClone.GetComponent<PlayerColorManager>().ChangeColor(platformColor);
            currentHourglass.GetComponent<PlayerColorManager>().ChangeColor(platformColor);
            currentHourglass.GetComponent<PlayerColorManager>().UpdateTargetValue(1);

            currentClone.GetComponent<CloneController>().PassFrames(currentRecording);
            currentClone.GetComponent<CloneController>().StartPlayback();
        }

        // TO DO : CHANGE CLONE COLOR & DISABLE ITS HOURGLASS MODEL
    }

    public void StopEarly()
    {
        StopRecording();
    }

    public void PauseTimer(bool pause)
    {
        timerPaused = pause;
    }

    public void ChangeColor(ColorRef color)
    {
        foreach (MeshRenderer mr in platformFloorAndShine)
        {
            Material mat = mr.material;
            mat.SetColor("_Color", colors.ReturnPlatformColor(color));
        }

        int UVint = 0;
        if (color == ColorRef.Green) UVint = 1;
        if (color == ColorRef.Blue) UVint = 2;
        if (color == ColorRef.Red) UVint = 3;
        if (color == ColorRef.Pink) UVint = 4;
        Material mat2 = platformBorder.material;
        mat2.SetInt("_ColorInt", UVint);

    }
    private void OnDestroy()
    {
        DisableClone();
    }
}
