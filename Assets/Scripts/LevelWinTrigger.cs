using UnityEngine;

// Place a trigger volume at the north end of the level (past the water tower).
// When the player walks through it, the level is complete.
// Attach this script to a GameObject with a Collider (Is Trigger = true).

public class LevelWinTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        hasTriggered = true;
        UIManager.Instance.ShowWinScreen();
    }
}