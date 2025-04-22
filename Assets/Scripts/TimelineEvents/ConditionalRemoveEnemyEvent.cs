using UnityEngine;

public class ConditionalRemoveEnemyEvent : TimelineEvent
{
    public string targetEnemyName;
    public float maxExistTime = -1f; // -1 代表不檢查時間
    public string dependencyEnemyName; // 若指定的敵人被移除，則一併移除此敵人

    public override void Execute()
    {
        GameSystem system = GameObject.FindObjectOfType<GameSystem>();
        if (system == null) return;

        GameObject target = system.GetActiveEnemies().Find(e => e != null && e.name == targetEnemyName);
        if (target == null) return;

        bool shouldRemove = false;

        if (maxExistTime > 0)
        {
            float spawnTime = target.GetComponent<EnemySpawnTime>()?.spawnTime ?? -1f;
            if (spawnTime > 0 && system.GetGameTime() - spawnTime >= maxExistTime)
                shouldRemove = true;
        }

        if (!string.IsNullOrEmpty(dependencyEnemyName))
        {
            GameObject dependency = system.GetActiveEnemies().Find(e => e != null && e.name == dependencyEnemyName);
            if (dependency == null)
                shouldRemove = true;
        }

        if (shouldRemove)
        {
            system.UnregisterEnemy(target);
            GameObject.Destroy(target);
        }
    }
}

