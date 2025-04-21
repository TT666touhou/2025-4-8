using UnityEngine;
using System.Reflection;

public class MoveOnFirstFire : MovementBehaviorBase
{
    [Header("重複觸發設定")]
    public bool repeatable = false;

    [Header("方向變化設定")]
    public bool rotateDirection = false;
    public int rotateInterval = 1;
    public float rotateAngle = 90f;

    private int moveCount = 0;
    private bool hasTriggered = false;
    private FieldInfo fireCycleField;
    private int lastFireCycle = -1;

    protected override void Start()
    {
        base.Start();
        if (observedSpawner != null)
        {
            fireCycleField = typeof(BulletSpawner).GetField("fireCycleCount", BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }

    void Update()
    {
        if (observedSpawner == null || fireCycleField == null)
            return;

        int fireCycleCount = (int)fireCycleField.GetValue(observedSpawner);

        if (fireCycleCount != lastFireCycle)
        {
            lastFireCycle = fireCycleCount;

            if (!hasTriggered || repeatable)
            {
                BeginMove();
                moveCount++;

                // 方向旋轉判定
                if (rotateDirection && rotateInterval > 0 && moveCount % rotateInterval == 0)
                {
                    // 將 targetOffset 旋轉
                    targetOffset = Quaternion.Euler(0, 0, rotateAngle) * targetOffset;
                }

                hasTriggered = true;
            }
        }

        if (hasTriggered)
        {
            UpdateMove();

            // 非重複模式下只跑一次
            if (!repeatable && elapsed >= moveDuration)
                hasTriggered = false;
        }
    }
}
