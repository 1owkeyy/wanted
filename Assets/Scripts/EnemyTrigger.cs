using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string enemyName = "Outlaw";

    [Header("Pivot (assign the parent EnemyPivot transform)")]
    [SerializeField] private Transform pivot;

    [Header("Duel Difficulty")]
    [SerializeField] private float arcWidth = 12f;
    [SerializeField] private float timerDuration = 3f;

    [Header("Group Membership")]
    [SerializeField] private bool isPartOfGroupEncounter = false;

    [Header("Wanted Guy")]
    [SerializeField] private bool isWantedGuy = false; // check this on Billy's prefab instance only

    private bool hasTriggered = false;
    private bool individualTriggerDisabled = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || individualTriggerDisabled || isPartOfGroupEncounter) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasTriggered = true;
            DuelManager.Instance.StartDuel(this);
        }
    }

    public void DisableIndividualTrigger()
    {
        individualTriggerDisabled = true;
    }

    // Called by DuelManager's HandleWinSequence when this enemy is defeated
    public void OnDefeated()
    {
        if (isWantedGuy)
        {
            GameState.SetWantedGuyDefeated();
        }
    }

    public string EnemyName => enemyName;
    public Transform Pivot => pivot;
    public float ArcWidth => arcWidth;
    public float TimerDuration => timerDuration;

    public void ResetTrigger()
    {
        hasTriggered = false;
        individualTriggerDisabled = false;
    }
}