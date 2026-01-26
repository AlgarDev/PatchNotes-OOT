using UnityEngine;
using UnityEngine.Events;

public class CallFunction : MonoBehaviour
{
    [System.Serializable]
    public class NamedEvent
    {
        public string eventName;
        public UnityEvent eventAction;
    }

    [SerializeField]
    private NamedEvent[] events;

    public void CallEvent(string eventName)
    {
        foreach (var namedEvent in events)
        {
            if (namedEvent.eventName == eventName)
            {
                namedEvent.eventAction.Invoke();
                return;
            }
        }
        Debug.LogWarning("Event not found: " + eventName);
    }
}
