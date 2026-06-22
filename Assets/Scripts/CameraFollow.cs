using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float fixedHeight = 15f; // matches your camera's Y position

    private List<Transform> duelEnemyTransforms = new List<Transform>();
    private bool isDuelFraming;

    void LateUpdate()
    {
        Vector3 targetPosition;

        if (isDuelFraming && duelEnemyTransforms.Count > 0)
        {
            // Frame the midpoint between the player and the average position of all active enemies.
            // With one enemy in the list, this is identical to the old single-enemy midpoint behavior.
            Vector3 enemyAverage = Vector3.zero;
            int validCount = 0;

            foreach (var enemyTransform in duelEnemyTransforms)
            {
                if (enemyTransform == null) continue; // skip any that were destroyed/deactivated mid-duel
                enemyAverage += enemyTransform.position;
                validCount++;
            }

            if (validCount > 0)
            {
                enemyAverage /= validCount;
                Vector3 midpoint = (playerTransform.position + enemyAverage) * 0.5f;
                targetPosition = new Vector3(midpoint.x, fixedHeight, midpoint.z);
            }
            else
            {
                targetPosition = new Vector3(playerTransform.position.x, fixedHeight, playerTransform.position.z);
            }
        }
        else
        {
            // Normal exploration: just follow the player
            targetPosition = new Vector3(playerTransform.position.x, fixedHeight, playerTransform.position.z);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * followSpeed);
    }

    // ----- Public calls, wired into DuelManager -----

    // Solo duel - single enemy
    public void BeginDuelFraming(Transform enemyTransform)
    {
        duelEnemyTransforms.Clear();
        duelEnemyTransforms.Add(enemyTransform);
        isDuelFraming = true;
    }

    // Group duel - frame on the average of all enemies involved
    public void BeginGroupDuelFraming(List<Transform> enemyTransforms)
    {
        duelEnemyTransforms.Clear();
        duelEnemyTransforms.AddRange(enemyTransforms);
        isDuelFraming = true;
    }

    public void EndDuelFraming()
    {
        duelEnemyTransforms.Clear();
        isDuelFraming = false;
    }
}