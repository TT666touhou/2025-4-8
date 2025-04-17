using System.Collections.Generic;
using UnityEngine;

public class RotateDirectionBehavior : BulletBehaviorBase
{
    public enum RotationMode
    {
        Instant,    // 瞬間轉向
        Gradual     // 漸進轉向
    }

    [Header("旋轉設定")]
    public RotationMode rotationMode = RotationMode.Gradual;

    public bool clockwise = true;           // true 順時針, false 逆時針
    public float totalRotation = 90f;       // 總共要旋轉的角度（度數）
    public float rotationSpeed = 90f;       // 每秒旋轉角度（僅適用 Gradual）

    private float rotatedSoFar = 0f;
    private bool finished = false;

    protected override void OnStart()
    {
        rotatedSoFar = 0f;
        finished = false;

        if (rotationMode == RotationMode.Instant)
        {
            float angle = clockwise ? -totalRotation : totalRotation;
            RotateBulletDirection(angle);
            finished = true;
        }
    }

    protected override void Tick(float deltaTime)
    {
        if (finished || bullet == null || rotationMode == RotationMode.Instant) return;

        float angleStep = rotationSpeed * deltaTime;
        if (!clockwise)
            angleStep = -angleStep;

        // 避免超轉
        float remaining = totalRotation - Mathf.Abs(rotatedSoFar);
        float applied = Mathf.Clamp(angleStep, -remaining, remaining);

        RotateBulletDirection(applied);
        rotatedSoFar += Mathf.Abs(applied);

        if (rotatedSoFar >= Mathf.Abs(totalRotation))
        {
            finished = true;
        }
    }

    private void RotateBulletDirection(float angle)
    {
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);
        bullet.SetDirection(rot * bullet.Direction);
    }

    protected override void OnEnd()
    {
        // 行為完成或時間到了後結束，不需重設方向
    }
}