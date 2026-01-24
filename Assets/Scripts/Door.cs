using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private GameObject doorObject;
    [SerializeField] private GameObject doorCheckPrefab;
    [SerializeField] private float doorCheckSpacing;
    [SerializeField] private float doorSize = 4.1f;
    private List<DoorCheck> doorChecks = new List<DoorCheck>();
    private Animator doorAnimator;
    [SerializeField] private float openDoorY = 8.22f;
    private float currentDoorYTarget;
    private Coroutine moveDoorCoroutine;
    [SerializeField] private float doorSpeed = 0.2f;

    void Start()
    {
        doorAnimator = GetComponent<Animator>();
        SpawnDoorCheck(GetComponent<ButtonController>().buttons.Count);
    }

    public void OpenDoor()
    {
        if (doorObject == null) return;
        if (moveDoorCoroutine != null) StopCoroutine(moveDoorCoroutine);
        currentDoorYTarget = openDoorY;
        moveDoorCoroutine = StartCoroutine(MoveDoor());

    }

    public void CloseDoor()
    {
        if (doorObject == null) return;
        if (moveDoorCoroutine != null) StopCoroutine(moveDoorCoroutine);
        currentDoorYTarget = 0;
        moveDoorCoroutine = StartCoroutine(MoveDoor());
    }

    private IEnumerator MoveDoor()
    {
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = new Vector3(
            startPosition.x,
            currentDoorYTarget,
            startPosition.z
        );

        float distance = Mathf.Abs(targetPosition.y - startPosition.y);
        float duration = distance / doorSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Smooth interpolation using Lerp
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null; // Wait for next frame
        }

        // Ensure exact final position
        transform.localPosition = targetPosition;
        moveDoorCoroutine = null;
    }


    public void UpdateCheckState()
    {
        int numberOfChecks = GetComponent<ButtonController>().pressedCount;

        if (numberOfChecks > doorChecks.Count) numberOfChecks = doorChecks.Count;
        for (int i = 0; i < numberOfChecks; i++)
        {
            if (doorChecks.Count < i) doorChecks[i].ChangeState(true);
        }

    }

    public void SpawnDoorCheck(int numberOfChecks)
    {
        float space = (doorSize * 2 - doorCheckSpacing) / numberOfChecks;

        for (int i = 0; i < numberOfChecks; i++)
        {
            float x = -doorSize + doorCheckSpacing + (space * i);
            Vector3 location = new Vector3(x, 0, 0);
            GameObject doorCheck = Instantiate(doorCheckPrefab);
            doorCheck.transform.parent = transform;
            doorCheck.transform.localPosition = location;
            if (doorCheck.GetComponent<DoorCheck>()) doorChecks.Add(doorCheck.GetComponent<DoorCheck>());
        }
    }
}
