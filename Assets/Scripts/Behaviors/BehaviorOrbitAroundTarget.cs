using System.Collections;
using UnityEngine;

public class OrbitAroundTargetBehavior : BulletBehaviorBase
{
    public enum OrbitSpawnType { Instant, FlyOut }
    public enum FlyOutStyle { Linear, Swirl }
    public enum OrbitTargetType { Player, Enemy }

    [Header("目標設定")]
    public OrbitTargetType targetType = OrbitTargetType.Enemy;
    public string targetName = "Enemy(Clone)";
    public string childObjectName = "OrbitCenter";

    [Header("繞行設定")]
    public OrbitSpawnType spawnType = OrbitSpawnType.Instant;
    public FlyOutStyle flyOutStyle = FlyOutStyle.Linear;
    public float flyOutTime = 0.5f;
    public float orbitSpeed = 90f;
    public float orbitRadius = 2f;
    public float swirlSpeed = 360f;
    public bool clockwise = true;

    private Transform orbitCenter;
    private float currentAngle;
    private bool isOrbiting = false;

    protected override void OnStart()
    {
        if (bullet != null) bullet.Stop();
        TrySetOrbitCenter(spawnType);
    }

    protected override void Tick(float deltaTime)
    {
        if (orbitCenter == null)
        {
            TrySetOrbitCenter(OrbitSpawnType.FlyOut); // 玩家復活後重新飛向目標
            return;
        }

        if (!isOrbiting || bullet == null) return;

        float angleDelta = orbitSpeed * deltaTime * (clockwise ? -1f : 1f);
        currentAngle += angleDelta;
        bullet.transform.position = GetOrbitPosition(currentAngle);
    }

    protected override void OnEnd()
    {
        if (bullet != null) bullet.Stop();
    }

    private void TrySetOrbitCenter(OrbitSpawnType flyMode)
    {
        Transform newCenter = FindOrbitCenter();
        if (newCenter == null) return;

        orbitCenter = newCenter;
        Vector2 offset = bullet.transform.position - orbitCenter.position;
        currentAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

        if (flyMode == OrbitSpawnType.Instant)
        {
            bullet.transform.position = GetOrbitPosition(currentAngle);
            isOrbiting = true;
        }
        else
        {
            Vector2 orbitPos = GetOrbitPosition(currentAngle);
            if (flyOutStyle == FlyOutStyle.Linear)
                bullet.StartCoroutine(FlyToOrbitLinear(orbitPos));
            else
                bullet.StartCoroutine(FlyToOrbitSwirl(currentAngle));
        }
    }

    private Transform FindOrbitCenter()
    {
        if (targetType == OrbitTargetType.Player)
        {
            var player = GameSystem.PlayerTransform;
            if (player == null) return null;

            var child = player.Find(childObjectName);
            return child != null ? child : null;
        }

        if (targetType == OrbitTargetType.Enemy)
        {
            var system = GameObject.FindObjectOfType<GameSystem>();
            if (system != null)
            {
                foreach (var enemy in system.GetActiveEnemies())
                {
                    if (enemy != null && enemy.name == targetName)
                    {
                        var child = enemy.transform.Find(childObjectName);
                        if (child != null) return child;
                    }
                }
            }
        }

        return null;
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

            float swirlDelta = swirlSpeed * Time.deltaTime * (clockwise ? -1f : 1f);
            currentSwirlAngle += swirlDelta;

            bullet.transform.position = GetOrbitPositionAtAngle(currentSwirlAngle, radius);
            yield return null;
        }

        currentAngle = currentSwirlAngle;
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
