using UnityEngine;
using System.Collections;

// Generic "knocked backward and falls" reaction - usable on either PlayerPivot or EnemyPivot.
// Replaces the old EnemyKnockback with a shared, reusable version.
public class FallReaction : MonoBehaviour
{
    [Header("Fall Settings")]
    [SerializeField] private float fallAngle = 80f;
    [SerializeField] private float fallDuration = 0.35f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float holdBeforeDeactivate = 0.4f;
    [SerializeField] private bool deactivateAfterFall = true;
    [SerializeField] private bool disableCollidersOnFall = true; // true for enemies, false for player // false for the player, since the level shouldn't disappear

    // sourceTransform = whoever fired the shot, used to determine fall direction (away from source).
    public void PlayFall(Transform sourceTransform, System.Action onComplete)
    {
        // Disable all colliders immediately so dead enemies can't re-trigger duels
        // or block the player while falling. Covers both the pivot and capsule child.
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        StartCoroutine(FallRoutine(sourceTransform, onComplete));
    }

    private IEnumerator FallRoutine(Transform sourceTransform, System.Action onComplete)
    {
        // Disable all colliders immediately so the fallen enemy doesn't block movement or re-trigger duels
        if (disableCollidersOnFall)
        {
            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        Vector3 awayFromSource = (transform.position - sourceTransform.position);
        awayFromSource.y = 0f;
        awayFromSource.Normalize();

        Vector3 rotationAxis = Vector3.Cross(Vector3.up, awayFromSource);

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(fallAngle, rotationAxis) * startRotation;

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            float eased = fallCurve.Evaluate(t);

            transform.rotation = Quaternion.Slerp(startRotation, endRotation, eased);
            yield return null;
        }

        transform.rotation = endRotation;

        yield return new WaitForSecondsRealtime(holdBeforeDeactivate);

        if (deactivateAfterFall)
            gameObject.SetActive(false);

        onComplete?.Invoke();
    }

    // Allows resetting rotation cleanly on level restart, if ever reused without a scene reload.
    public void ResetRotation()
    {
        StopAllCoroutines();
        transform.rotation = Quaternion.identity;
    }
}