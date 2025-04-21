using UnityEngine;

public abstract class MovementBehaviorBase : MonoBehaviour
{
    [Header("通用設定")]
    public string targetSpawnerName;
    public Vector2 targetOffset = Vector2.zero;
    public float moveDuration = 1f;

    // 共享位置
    public static Vector3 currentPosition;

    protected BulletSpawner observedSpawner;
    protected Vector3 startPosition;
    protected float elapsed = 0f;
    protected bool isMoving = false;

    protected virtual void Start()
    {
        Transform spawnerTransform = transform.Find(targetSpawnerName);
        if (spawnerTransform != null)
            observedSpawner = spawnerTransform.GetComponent<BulletSpawner>();
        else
            Debug.LogWarning($"{name} 找不到指定子物件 {targetSpawnerName}");

        currentPosition = transform.position;
    }

    protected void BeginMove()
    {
        isMoving = true;
        startPosition = currentPosition;
        elapsed = 0f;
    }

    protected void UpdateMove()
    {
        if (!isMoving) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / moveDuration);
        transform.position = Vector3.Lerp(startPosition, startPosition + (Vector3)targetOffset, t);

        if (t >= 1f)
        {
            isMoving = false;
            currentPosition = transform.position;
        }
    }
}
