using UnityEngine;
using System.Collections;

// Attach to EnemyPivot (NOT the capsule) - moving the capsule directly would desync it from
// its pivot, breaking FallReaction's rotation-around-feet logic.
public class EnemyApproach : MonoBehaviour
{
    [Header("Approach Settings")]
    [SerializeField] private float engagementDistance = 2f; // should be smaller than your trigger radius, so there's room to visibly step closer
    [SerializeField] private float approachDuration = 0.4f;
    [SerializeField] private AnimationCurve approachCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine activeRoutine;

    public void PlayApproach(Transform playerTransform)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ApproachRoutine(playerTransform));
    }

    // Used for coordinated group approaches - walks directly to a pre-calculated shared position,
    // instead of independently calculating its own angle-based point on the engagement circle.
    public void PlayApproachToPosition(Vector3 targetPosition)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ApproachToPositionRoutine(targetPosition));
    }

    private IEnumerator ApproachToPositionRoutine(Vector3 targetPosition)
    {
        Vector3 startPos = transform.position;
        targetPosition.y = startPos.y;

        Debug.Log($"[EnemyApproach] {gameObject.name} starting coordinated approach. From: {startPos} To: {targetPosition}");

        float elapsed = 0f;
        while (elapsed < approachDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / approachDuration);
            float eased = approachCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, targetPosition, eased);
            yield return null;
        }

        transform.position = targetPosition;
        Debug.Log($"[EnemyApproach] {gameObject.name} finished coordinated approach. Final position: {transform.position}");
        activeRoutine = null;
    }

    private IEnumerator ApproachRoutine(Transform playerTransform)
    {
        Vector3 toPlayer = (playerTransform.position - transform.position);
        toPlayer.y = 0f;
        float currentDistance = toPlayer.magnitude;
        Vector3 directionToPlayer = toPlayer.normalized;

        // Only approach if currently farther away than the target engagement distance.
        // Otherwise stay put (don't walk away or through the player).
        if (currentDistance <= engagementDistance)
        {
            activeRoutine = null;
            yield break;
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = playerTransform.position - directionToPlayer * engagementDistance;
        endPos.y = startPos.y; // preserve enemy's own height, only move on the ground plane

        float elapsed = 0f;
        while (elapsed < approachDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / approachDuration);
            float eased = approachCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        transform.position = endPos;
        activeRoutine = null;
    }
}