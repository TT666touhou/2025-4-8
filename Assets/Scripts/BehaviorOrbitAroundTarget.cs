using System.Collections;
using UnityEngine;

public class OrbitAroundTargetBehavior : BulletBehaviorBase
{
    public enum OrbitSpawnType
    {
        Instant,    // 立即出現在軌道上
        FlyOut      // 從原地飛向軌道
    }

    [Header("旋轉設定")]
    public string orbitTargetTag = "OrbitCenter"; // 用 tag 自動找中心
    public OrbitSpawnType spawnType = OrbitSpawnType.Instant;
    public float flyOutTime = 0.5f;

    public float orbitSpeed = 90f;    // 每秒旋轉角度
    public float orbitRadius = 2f;    // 繞行半徑
    public bool clockwise = true;

    private float currentAngle = 0f;
    private Transform orbitCenter;
    private bool isOrbiting = false;

    public enum FlyOutStyle
    {
        Linear,     // 原方式：直線移動
        Swirl       // 螺旋方式：漸漸加入繞行角度
    }

    [Header("飛出方式")]
    public FlyOutStyle flyOutStyle = FlyOutStyle.Linear;

    [Tooltip("Swirl 飛行時的角速度 (度/秒)，僅在 Swirl 模式啟用")]
    public float swirlSpeed = 360f;


    protected override void OnStart()
    {
        // 找中心物件
        if (orbitCenter == null)
        {
            GameObject[] candidates = GameObject.FindGameObjectsWithTag(orbitTargetTag);
            foreach (GameObject obj in candidates)
            {
                if (obj.GetComponent<BulletSpawner>() != null)
                {
                    orbitCenter = obj.transform;
                    break;
                }
            }
        }

        if (orbitCenter == null || bullet == null)
            return;

        Vector2 offset = (Vector2)bullet.transform.position - (Vector2)orbitCenter.position;
        currentAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

        // 根據 spawn type 判斷初始行為
        if (spawnType == OrbitSpawnType.Instant)
        {
            SetBulletToOrbitPosition();
            isOrbiting = true;
        }
        else if (spawnType == OrbitSpawnType.FlyOut)
        {
            Vector2 orbitPos = GetOrbitPosition(currentAngle);
            if (flyOutStyle == FlyOutStyle.Linear)
                bullet.StartCoroutine(FlyToOrbitLinear(orbitPos));
            else
                bullet.StartCoroutine(FlyToOrbitSwirl(targetAngle: currentAngle));

        }
    }

    protected override void Tick(float deltaTime)
    {
        if (!isOrbiting || orbitCenter == null || bullet == null) return;

        float angleDelta = orbitSpeed * deltaTime * (clockwise ? -1f : 1f);
        currentAngle += angleDelta;

        bullet.transform.position = GetOrbitPosition(currentAngle);
    }

    private void SetBulletToOrbitPosition()
    {
        bullet.transform.position = GetOrbitPosition(currentAngle);
    }

    private Vector2 GetOrbitPosition(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
        return (Vector2)orbitCenter.position + offset;
    }

    private IEnumerator FlyToOrbitLinear(Vector2 targetPos)
    {
        Vector2 startPos = bullet.transform.position;
        float elapsed = 0f;

        while (elapsed < flyOutTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flyOutTime);
            bullet.transform.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        bullet.transform.position = targetPos;
        isOrbiting = true;
    }

    private IEnumerator FlyToOrbitSwirl(float targetAngle)
    {
        Vector2 startOffset = bullet.transform.position - orbitCenter.position;
        float startRadius = startOffset.magnitude;
        float startAngle = Mathf.Atan2(startOffset.y, startOffset.x) * Mathf.Rad2Deg;

        float currentSwirlAngle = startAngle;
        float elapsed = 0f;

        while (elapsed < flyOutTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flyOutTime);

            float radius = Mathf.Lerp(startRadius, orbitRadius, t);

            // ✅ 累加角度（受 swirlSpeed 控制）
            float swirlDelta = swirlSpeed * Time.deltaTime * (clockwise ? -1f : 1f);
            currentSwirlAngle += swirlDelta;

            bullet.transform.position = GetOrbitPositionAtAngle(currentSwirlAngle, radius);

            yield return null;
        }

        currentAngle = currentSwirlAngle; // 用最終 swirl 角度繼續主繞行
        bullet.transform.position = GetOrbitPosition(currentAngle);
        isOrbiting = true;
    }


    private Vector2 GetOrbitPositionAtAngle(float angleDeg, float radius)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        return (Vector2)orbitCenter.position + offset;
    }

}
