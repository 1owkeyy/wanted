using UnityEngine;
using System.Collections.Generic;

// Place this as an invisible trigger volume positioned between/around a group of enemies.
// On player entry, starts a shared duel against all assigned enemies at once, and moves them
// together to a coordinated engagement position (side-by-side), rather than each enemy
// independently walking to its own closest point relative to the player.
public class GroupEncounterTrigger : MonoBehaviour
{
    [Header("Group Members")]
    [SerializeField] private List<EnemyTrigger> enemies = new List<EnemyTrigger>();

    [Header("Shared Duel Settings")]
    [SerializeField] private float sharedTimerDuration = 4f;

    [Header("Coordinated Positioning")]
    [SerializeField] private float engagementDistance = 3f;  // how far the group's anchor point sits from the player
    [SerializeField] private float spacingBetweenEnemies = 1.5f; // side-to-side gap between grouped enemies

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasTriggered = true;

            foreach (var enemy in enemies)
            {
                enemy.DisableIndividualTrigger();
            }

            MoveGroupToEngagementPositions(other.transform);

            DuelManager.Instance.StartGroupDuel(enemies, sharedTimerDuration);
        }
    }

    private void MoveGroupToEngagementPositions(Transform playerTransform)
    {
        if (enemies.Count == 0) return;

        Vector3 groupAverage = Vector3.zero;
        foreach (var enemy in enemies)
        {
            groupAverage += enemy.transform.position;
        }
        groupAverage /= enemies.Count;

        Vector3 toGroup = groupAverage - playerTransform.position;
        toGroup.y = 0f;
        Vector3 directionToGroup = toGroup.normalized;

        Vector3 anchorPoint = playerTransform.position + directionToGroup * engagementDistance;
        Vector3 sideAxis = Vector3.Cross(Vector3.up, directionToGroup);

        Debug.Log($"[GroupEncounter] Player pos: {playerTransform.position} | Group average: {groupAverage} | Direction to group: {directionToGroup} | Anchor point: {anchorPoint} | Side axis: {sideAxis}");

        int count = enemies.Count;
        float totalWidth = spacingBetweenEnemies * (count - 1);
        float startOffset = -totalWidth * 0.5f;

        // Calculate all candidate slot positions first (before assigning who goes where)
        List<Vector3> slotPositions = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            slotPositions.Add(anchorPoint + sideAxis * (startOffset + i * spacingBetweenEnemies));
        }

        // Greedily assign each enemy to its nearest unclaimed slot, so enemies move to whichever
        // side is closest to their own starting position rather than a fixed index-based slot -
        // this prevents enemies crossing paths through each other to reach a "swapped" target.
        List<int> unclaimedSlotIndices = new List<int>();
        for (int i = 0; i < count; i++) unclaimedSlotIndices.Add(i);

        int[] assignedSlotForEnemy = new int[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 enemyPos = enemies[i].transform.position;
            float bestDist = float.MaxValue;
            int bestSlot = -1;

            foreach (int slotIndex in unclaimedSlotIndices)
            {
                float dist = Vector3.Distance(enemyPos, slotPositions[slotIndex]);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestSlot = slotIndex;
                }
            }

            assignedSlotForEnemy[i] = bestSlot;
            unclaimedSlotIndices.Remove(bestSlot);
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 sideOffsetPos = slotPositions[assignedSlotForEnemy[i]];

            Vector3 enemyStart = enemies[i].transform.position;
            float startDistance = Vector3.Distance(
                new Vector3(enemyStart.x, 0f, enemyStart.z),
                new Vector3(playerTransform.position.x, 0f, playerTransform.position.z)
            );

            Vector3 flatOffsetPos = new Vector3(sideOffsetPos.x, 0f, sideOffsetPos.z);
            Vector3 flatPlayerPos = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
            float offsetDistance = Vector3.Distance(flatOffsetPos, flatPlayerPos);

            float maxAllowedDistance = Mathf.Min(engagementDistance, startDistance);

            Vector3 finalPos;
            bool wasClamped = offsetDistance > maxAllowedDistance;
            if (wasClamped)
            {
                Vector3 dir = (flatOffsetPos - flatPlayerPos).normalized;
                finalPos = playerTransform.position + dir * maxAllowedDistance;
            }
            else
            {
                finalPos = sideOffsetPos;
            }

            float finalDistanceFromPlayer = Vector3.Distance(
                new Vector3(finalPos.x, 0f, finalPos.z),
                flatPlayerPos
            );

            Debug.Log($"[GroupEncounter] Enemy {i} ({enemies[i].name}) | Start pos: {enemyStart} | Start distance: {startDistance:F2} | Side-offset target: {sideOffsetPos} (dist {offsetDistance:F2}) | Clamped: {wasClamped} | maxAllowedDistance: {maxAllowedDistance:F2} | FINAL target: {finalPos} | Final distance from player: {finalDistanceFromPlayer:F2}");

            EnemyApproach approach = enemies[i].Pivot != null ? enemies[i].Pivot.GetComponent<EnemyApproach>() : null;
            if (approach != null)
            {
                approach.PlayApproachToPosition(finalPos);
            }
            else
            {
                Debug.LogWarning($"[GroupEncounter] Enemy {i} ({enemies[i].name}) has no EnemyApproach component on its Pivot - it will NOT move.");
            }
        }
    }
}