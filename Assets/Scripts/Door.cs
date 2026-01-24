using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private GameObject doorCheckPrefab;
    [SerializeField] private float doorCheckSpacing;
    [SerializeField] private float doorSize = 4.1f;
    private List<DoorCheck> doorChecks = new List<DoorCheck>();

    void Start()
    {
        // TO DO: Spawn based on connected buttons
        //SpawnDoorCheck(3);
    }


    private void SetDoorCheckStates(int numberOfChecks)
    {
        if (numberOfChecks > doorChecks.Count) numberOfChecks = doorChecks.Count;
        for (int i = 0; i < numberOfChecks; i++)
        {
            if (doorChecks.Count < i) doorChecks[i].ChangeState(true);
        }

    }


    private void SpawnDoorCheck(int numberOfChecks)
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
