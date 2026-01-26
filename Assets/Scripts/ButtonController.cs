using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonController : MonoBehaviour
{
    [Header("When all buttons in this list are pressed")]
    [SerializeField] public List<InteractableButton> buttons;

    [Header("Events")]
    [SerializeField]
    public UnityEvent onRequirementUpdate;
    [SerializeField]
    public UnityEvent onRequirementMet;
    [SerializeField]
    public UnityEvent onRequirementLost;

    private bool requirementMet;
    public int pressedCount = 0;
    private void Awake()
    {
        foreach (var button in buttons)
        {
            button.onStateChanged.AddListener(Recalculate);
        }
        requirementMet = false;
    }

    public void Recalculate()
    {
        //print("recalculating");
        pressedCount = 0;
        int requiredPressed = buttons.Count;

        foreach (var button in buttons)
        {
            if (button != null && button.isPressedNow)
                pressedCount++;
        }

        bool nowMet = pressedCount >= requiredPressed;

        onRequirementUpdate.Invoke();

        if (nowMet)
        {
            //print("Requirement Met");
            if (!requirementMet)
            {
                //print("Open Door");
                requirementMet = true;
                onRequirementMet.Invoke();
            }
        }
        else
        {
            //print("Requirement not Met");
            if (requirementMet)
            {
                //print("Closing Door");
                requirementMet = false;
                onRequirementLost.Invoke();
            }
        }

    }
}
