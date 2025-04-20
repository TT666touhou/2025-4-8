using UnityEngine;

public class SpawnerModifyEvent : TimelineEvent
{
    public string targetEnemyName;          // 敵人 GameObject 名稱（例如 "Enemy(Clone)"）
    public string spawnerObjectName;        // 掛有 Spawner 的子物件名稱
    public System.Action<BulletSpawner> configAction;

    public override void Execute()
    {
        GameSystem system = GameObject.FindObjectOfType<GameSystem>();
        if (system == null) return;

        foreach (var enemy in system.GetActiveEnemies())
        {
            if (enemy != null && enemy.name == targetEnemyName)
            {
                Transform spawnerTransform = enemy.transform.Find(spawnerObjectName);
                if (spawnerTransform != null)
                {
                    var spawner = spawnerTransform.GetComponent<BulletSpawner>();
                    if (spawner != null)
                    {
                        configAction?.Invoke(spawner);
                    }
                    else
                    {
                        Debug.LogWarning($"在 {spawnerObjectName} 找不到 BulletSpawner");
                    }
                }
                else
                {
                    Debug.LogWarning($"找不到 {spawnerObjectName} 子物件於 {enemy.name}");
                }
                break; // 成功就不再往下找
            }
        }
    }
}
