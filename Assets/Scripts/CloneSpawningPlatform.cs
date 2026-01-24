using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CloneSpawningPlatform : MonoBehaviour
{

    [Header("Clone")]
    [SerializeField] private ColorRef platformColor;
    [SerializeField] private UnityEvent spawnClone;
    [SerializeField] private GameObject cloneObject;
    [SerializeField] private GameObject cloneHourglass;

    [Header("Recording Timer")]
    [SerializeField] private float recordingDuration = 10f;

    public float timerRemaining;
    public bool timerPaused;

    private bool isPlayerInside;
    private bool isRecording = false;

    private GameObject currentClone;
    private GameObject currentHourglass;
    private List<PlayerInputFrame> currentRecording;

    private Vector3 recordingStartPosition;
    private Quaternion recordingStartRotation;
    private ThirdPersonController player;

    private bool cloneQueuedLastFrame;

    [Header("Visuals")]
    [SerializeField] private MeshRenderer[] platformFloorAndShine;
    [SerializeField] private MeshRenderer platformBorder;
    private ColorsSO colors;
    [SerializeField] private GameObject shine;


    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller != null && controller.IsPlayerControlled)
        {
            isPlayerInside = true;
            player = controller;
            player.GetComponent<PlayerColorManager>().ChangeColor(platformColor);
            player.GetComponent<PlayerColorManager>().EnableHourglassSand();
            player.GetComponent<PlayerColorManager>().UpdateTargetValue(1);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller != null && controller.IsPlayerControlled)
        {
            isPlayerInside = false;
            player.GetComponent<PlayerColorManager>().ChangeColor(ColorRef.Green);
            player.GetComponent<PlayerColorManager>().DisableHourglassSand();
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

        if (timerRemaining <= 0f)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        // Start playback on existing clones
        foreach (var clone in FindObjectsOfType<CloneController>())
        {
            clone.StartPlayback();
        }

        SpawnClone();

        recordingStartPosition = player.transform.position;
        recordingStartRotation = player.transform.rotation;
        timerRemaining = recordingDuration;
        timerPaused = false;

        CloneInputRecorder.Instance.StartRecording();
        isRecording = true;
    }

    private void StopRecording()
    {
        if (!isRecording)
            return;

        if (shine != null) shine.SetActive(true);
        CloneInputRecorder.Instance.StopRecording();
        isRecording = false;

        currentRecording = new List<PlayerInputFrame>(CloneInputRecorder.Instance.Frames);

        var cloneController = currentClone.GetComponent<CloneController>();
        cloneController.PassFrames(currentRecording);

        // Teleport player back
        player.controller.enabled = false;
        player.transform.position = recordingStartPosition;
        player.transform.rotation = recordingStartRotation;
        player.controller.enabled = true;
    }

    private void SpawnClone()
    {
        Debug.Log("Clone spawned");
        spawnClone?.Invoke();
        currentClone = Instantiate(cloneObject, CloneInputRecorder.Instance.gameObject.transform.position, CloneInputRecorder.Instance.gameObject.transform.rotation);
        currentHourglass = Instantiate(cloneHourglass,
            new Vector3(CloneInputRecorder.Instance.gameObject.transform.position.x,
            CloneInputRecorder.Instance.gameObject.transform.position.y - 100,
            CloneInputRecorder.Instance.gameObject.transform.position.z),
            CloneInputRecorder.Instance.gameObject.transform.rotation);
        currentClone.GetComponent<ThirdPersonController>().SetHourglass(currentHourglass);
        // TO DO : CHANGE CLONE COLOR & DISABLE ITS HOURGLASS MODEL
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
}
