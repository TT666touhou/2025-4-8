using UnityEngine;

public class MoveOnFireCount : MovementBehaviorBase
{
    public int triggerFireCount = 3;
    public bool allowRepeat = false;
    public float rotateOffsetPerStep = 0f;
    public int rotationStepInterval = 1;

    private bool hasTriggered = false;
    private int moveStepCount = 0;

    void Update()
    {
        if (isMoving)
        {
            UpdateMove();
            return;
        }

        if (observedSpawner != null && observedSpawner.fireCount == triggerFireCount)
        {
            if (!hasTriggered || allowRepeat)
            {
                RotateOffsetIfNeeded();
                BeginMove();

                hasTriggered = true;
                moveStepCount++;
            }
        }
        else
        {
            if (!allowRepeat)
                hasTriggered = false; // 為了避免卡住，需要在條件消失後釋放
        }
    }

    private void RotateOffsetIfNeeded()
    {
        if (rotateOffsetPerStep == 0f || rotationStepInterval <= 0) return;

        if (moveStepCount > 0 && moveStepCount % rotationStepInterval == 0)
        {
            float angle = rotateOffsetPerStep;
            float radians = angle * Mathf.Deg2Rad;

            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            Vector2 rotated = new Vector2(
                targetOffset.x * cos - targetOffset.y * sin,
                targetOffset.x * sin + targetOffset.y * cos
            );

            targetOffset = rotated;
        }
    }
}
