using System;
using UnityEngine;
using UnityEngine.Events;
public class TriggerEvents : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onExit;
    [SerializeField] private GameObject GOToCheck = null;
    [SerializeField] private string tagToCheck = "";
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GOToCheck || (!string.IsNullOrEmpty(tagToCheck) && other.CompareTag(tagToCheck)))
        {
            onEnter.Invoke();
            print("entered");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == GOToCheck || (!string.IsNullOrEmpty(tagToCheck) && other.CompareTag(tagToCheck)))
        {
            onExit.Invoke();

        }
    }
}
