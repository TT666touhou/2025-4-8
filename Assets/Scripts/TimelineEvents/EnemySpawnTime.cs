using UnityEngine;

public class EnemySpawnTime : MonoBehaviour
{
    public float spawnTime;

    void Start()
    {
        spawnTime = GameObject.FindObjectOfType<GameSystem>()?.GetGameTime() ?? 0f;
    }
}
