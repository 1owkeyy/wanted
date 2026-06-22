using UnityEngine;
using UnityEngine.EventSystems;

// Attach to the FTUEPanel's root Image component so it catches clicks anywhere on the panel.
// Requires the Image to have Raycast Target = true (default for UI Images).

public class FTUEClickDismiss : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        UIManager.Instance.OnFTUEDismissed();
    }
}