using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonController : MonoBehaviour
{
    [Header("When all buttons in this list are pressed")]
    [SerializeField] private List<InteractableButton> buttons;

    [Header("Events")]
    [SerializeField]
    public UnityEvent<int> onRequirementUpdate;
    [SerializeField]
    public UnityEvent onRequirementMet;
    [SerializeField]
    public UnityEvent onRequirementLost;

    private bool requirementMet;
    int pressedCount = 0;
    private void Awake()
    {
        foreach (var button in buttons)
        {
            button.onStateChanged.AddListener(Recalculate);
        }
    }

    public void Recalculate()
    {
        print("recalculating");

        int requiredPressed = buttons.Count;

        foreach (var button in buttons)
        {
            if (button != null && button.IsPressed)
                pressedCount++; //this shit isn't reseting nigga
        }

        //onRequirementUpdate.Invoke(pressedCount);

        bool nowMet = pressedCount >= requiredPressed;

        if (nowMet && !requirementMet)
        {
            requirementMet = true;
            onRequirementMet.Invoke();
            print("bounce on it king");
        }
        else if (!nowMet && requirementMet)
        {
            requirementMet = false;
            onRequirementLost.Invoke();
            print("unbounce on it king");

        }
    }
}
