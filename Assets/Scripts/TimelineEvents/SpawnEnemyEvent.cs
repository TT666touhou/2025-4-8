using UnityEngine;

public class SpawnEnemyEvent : TimelineEvent
{
    public GameObject enemyPrefab;
    public Vector2 position;
    public int hp = 100;

    public override void Execute()
    {
        GameObject enemy = GameObject.Instantiate(enemyPrefab, position, Quaternion.identity);
        var core = enemy.GetComponent<EnemyCore>();
        if (core != null)
        {
            core.GetType()
                .GetField("maxHP", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                ?.SetValue(core, hp);
        }

        GameSystem system = GameObject.FindObjectOfType<GameSystem>();
        system?.RegisterEnemy(enemy);
    }
}