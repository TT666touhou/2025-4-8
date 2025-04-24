using UnityEngine;

public class MoveAfterSpawnTime : MovementBehaviorBase
{
    [Header("�\ᢎ��Ԑݒ�")]
    public float triggerAfterSeconds = 2f;

    [Header("�d���\ᢐݒ�")]
    public bool repeatable = false;

    [Header("�����̉��ݒ�")]
    public bool rotateDirection = false;
    public int rotateInterval = 1;
    public float rotateAngle = 90f;

    private float timeSinceSpawn = 0f;
    private int moveCount = 0;
    private bool hasTriggered = false;

    protected override void Start()
    {
        base.Start();
        timeSinceSpawn = 0f;
    }

    void Update()
    {
        if (isMoving)
        {
            UpdateMove();
            return;
        }

        if (!repeatable && hasTriggered)
            return;

        timeSinceSpawn += Time.deltaTime;

        if (timeSinceSpawn >= triggerAfterSeconds)
        {
            BeginMove();
            moveCount++;
            hasTriggered = true;
            timeSinceSpawn = 0f;

            // �������z
            if (rotateDirection && rotateInterval > 0 && moveCount % rotateInterval == 0)
            {
                targetOffset = Quaternion.Euler(0, 0, rotateAngle) * targetOffset;
            }
        }
    }

}
