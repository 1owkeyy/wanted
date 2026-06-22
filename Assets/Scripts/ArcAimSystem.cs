using UnityEngine;
using System;
using System.Collections.Generic;

public class ArcAimSystem : MonoBehaviour
{
    [Header("Arc Settings")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float arcWidthDefault = 12f;
    [SerializeField] private float maxArcAngle = 60f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.05f;

    [Header("References")]
    [SerializeField] private Transform arcVisual;
    [SerializeField] private Transform playerTransform;

    private float currentAngle;
    private float targetAngle;
    private float angleVelocity;

    private float timer;
    private float originalTimerDuration;
    private bool isActive;

    private class DuelTarget
    {
        public Transform transform;
        public float arcWidth;
        public EnemyTrigger source;
    }

    private List<DuelTarget> activeTargets = new List<DuelTarget>();

    public float TimerProgress01 => originalTimerDuration > 0f ? Mathf.Clamp01(timer / originalTimerDuration) : 0f;
    public bool IsActive => isActive;

    // Fired exactly once per shot/timeout, with full result info:
    // wasHit: did this specific shot land
    // hitEnemy: which enemy was hit (null if missed)
    // duelComplete: true if this was the last target (won) or a miss/timeout (lost) - duel is over
    public event Action<bool, EnemyTrigger, bool> OnShotResolved;

    void Start()
    {
        if (arcVisual != null)
            arcVisual.gameObject.SetActive(false);
    }

    public void BeginDuel(EnemyTrigger enemy, float duration)
    {
        BeginGroupDuel(new List<EnemyTrigger> { enemy }, duration);
    }

    public void BeginGroupDuel(List<EnemyTrigger> enemies, float duration)
    {
        timer = duration;
        originalTimerDuration = duration;
        currentAngle = 0f;
        targetAngle = 0f;
        angleVelocity = 0f;
        isActive = true;

        activeTargets.Clear();
        foreach (var enemy in enemies)
        {
            activeTargets.Add(new DuelTarget
            {
                transform = enemy.transform,
                arcWidth = enemy.ArcWidth > 0f ? enemy.ArcWidth : arcWidthDefault,
                source = enemy
            });
        }

        if (arcVisual != null)
            arcVisual.gameObject.SetActive(true);
    }

    // Call from DuelManager after a partial hit to grant the "sigh of relief" time refund.
    public void RefundTime(float percentOfOriginal)
    {
        timer = Mathf.Min(timer + originalTimerDuration * percentOfOriginal, originalTimerDuration);
    }

    void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;

        HandleAimInput();

        if (Input.GetMouseButtonDown(0))
        {
            ResolveShot();
            return;
        }

        if (timer <= 0f)
        {
            isActive = false;
            FinishDuel(false, null);
        }
    }

    private void HandleAimInput()
    {
        float mouseDeltaX = Input.GetAxisRaw("Mouse X");
        targetAngle = Mathf.Clamp(targetAngle + mouseDeltaX * mouseSensitivity, -maxArcAngle, maxArcAngle);

        currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref angleVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        if (arcVisual != null)
            arcVisual.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    private float AngleToTarget(Transform target)
    {
        Vector3 toTarget = target.position - playerTransform.position;
        toTarget.y = 0f;
        return Vector3.SignedAngle(playerTransform.forward, toTarget, Vector3.up);
    }

    private void ResolveShot()
    {
        DuelTarget bestMatch = null;
        float bestDifference = float.MaxValue;

        foreach (var target in activeTargets)
        {
            float angleToThis = AngleToTarget(target.transform);
            float difference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, angleToThis));

            if (difference < bestDifference)
            {
                bestDifference = difference;
                bestMatch = target;
            }
        }

        bool hit = bestMatch != null && bestDifference <= bestMatch.arcWidth * 0.5f;

        if (hit)
        {
            activeTargets.Remove(bestMatch);
            bool allCleared = activeTargets.Count == 0;

            if (allCleared)
            {
                isActive = false;
            }

            FinishDuel(true, bestMatch.source, allCleared);
        }
        else
        {
            isActive = false;
            FinishDuel(false, null, true);
        }
    }

    private void FinishDuel(bool wasHit, EnemyTrigger hitEnemy, bool duelComplete = true)
    {
        if (duelComplete && arcVisual != null)
            arcVisual.gameObject.SetActive(false);

        OnShotResolved?.Invoke(wasHit, hitEnemy, duelComplete);

        if (duelComplete)
            activeTargets.Clear();
    }
}