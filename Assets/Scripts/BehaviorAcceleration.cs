using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationBehavior : BulletBehaviorBase
{
    public enum Mode
    {
        Linear,         // 等差變化
        Curve           // 使用動畫曲線
    }

    [Header("加速度設定")]
    public Mode accelerationMode = Mode.Linear;

    public float acceleration = 5f;              // 每秒變化速度（線性用）
    public AnimationCurve accelerationCurve;     // 曲線模式（0~1）

    private float initialSpeed;
    private float duration;
    private float elapsed = 0f;

    protected override void OnStart()
    {
        initialSpeed = bullet != null ? bullet.GetSpeed() : 0f;
        duration = Mathf.Max(endTime - startTime, 0.001f); // 防止除以 0
        elapsed = 0f;
    }

    protected override void Tick(float deltaTime)
    {
        if (bullet == null) return;

        elapsed += deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        switch (accelerationMode)
        {
            case Mode.Linear:
                float deltaSpeed = acceleration * elapsed;
                bullet.SetSpeed(initialSpeed + deltaSpeed);
                break;

            case Mode.Curve:
                float curveValue = accelerationCurve.Evaluate(t); // 0~1
                bullet.SetSpeed(initialSpeed + curveValue * acceleration); // 用 curve 控制變化量（乘上最大加速值）
                break;
        }
    }

    protected override void OnEnd()
    {
        // 可選：是否鎖定最終速度？目前保留最後速度不變
    }
}
