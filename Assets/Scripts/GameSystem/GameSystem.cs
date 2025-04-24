using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameSystem : MonoBehaviour
{
    [Header("玩家設定")]
    public GameObject playerPrefab;
    public int maxLives = 3;

    [Header("敵人 Prefab 設定")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    private GameObject currentPlayer;
    public static Transform PlayerTransform { get; private set; }
    private int currentLives;
    private float gameTime;

    [Header("管理中敵人")]
    private List<GameObject> activeEnemies = new List<GameObject>();

    private TimelineManager timelineManager = new TimelineManager();

    void Start()
    {
        currentLives = maxLives;
        SpawnPlayer();

        // 使用第一個敵人 prefab（需在 Inspector 中指派）
        if (enemyPrefabs.Count > 0)
        {
            AddTimelineEvent(new SpawnEnemyEvent
            {
                triggerTime = 2f,
                enemyPrefab = enemyPrefabs[0],
                position = new Vector2(-4f,14.5f),
                hp = 40000
            });
        }
        AddTimelineEvent(new SpawnerModifyEvent
        {
            triggerTime = 6f,
            targetEnemyName = "Jackie(Clone)",
            spawnerObjectName = "Jackie_BulletSpawner_1",
            configAction = (s) =>
            {

                s.canFire = false;
            }
        });

        AddTimelineEvent(new SpawnerModifyEvent
        {
            triggerTime = 6f,
            targetEnemyName = "Jackie(Clone)",
            spawnerObjectName = "Jackie_BulletSpawner_2",
            configAction = (s) =>
            {

                s.canFire = true;
            }
        });
        AddTimelineEvent(new SpawnEnemyEvent
        {
            triggerTime = 0f,
            enemyPrefab = enemyPrefabs[1],
            position = new Vector2(-4f, 5.5f),
            hp = 100
        });
        /*
        AddTimelineEvent(new SpawnerModifyEvent
        {
            triggerTime = 2f,
            targetEnemyName = "Jackie(Clone)",
            spawnerObjectName = "Jackie_BulletSpawner_2",
            configAction = (s) =>
            {

                s.fireCount = 5;
                s.fireMode = BulletSpawner.FireMode.EvenSpreadToPlayer;
                s.bulletSpeed = 6f;
                s.fireInterval = 1f;
                s.canFire = false;
            }
        });
        */
        AddTimelineEvent(new SpawnEnemyEvent
        {
            triggerTime = 200f,
            enemyPrefab = enemyPrefabs[1],
            position = new Vector2(-10f, 3.5f),
            hp = 40000
        });

        AddTimelineEvent(new ConditionalRemoveEnemyEvent
        {
            triggerTime = 10f,
            targetEnemyName = "Enemy(Clone)",
            maxExistTime = 7f
        });

        /* EXAMPLE
        AddTimelineEvent(new ConditionalRemoveEnemyEvent
        {
            triggerTime = 10f,
            targetEnemyName = "Enemy(Clone)",
            dependencyEnemyName = "Boss(Clone)"
        });
        */
    }

    void Update()
    {
        gameTime += Time.deltaTime;
        timelineManager.Update(Time.deltaTime);
    }

    public void AddTimelineEvent(TimelineEvent evt)
    {
        timelineManager.AddEvent(evt);
    }

    // ========= 玩家管理 ========= //
    public void OnPlayerDeath()
    {
        currentLives--;

        if (currentLives >= 0)
        {
            Debug.Log($"玩家復活，剩餘殘機：{currentLives}");
            SpawnPlayer();
        }
        else
        {
            Debug.Log("Game Over！");
            // TODO: 顯示結束畫面
        }
    }

    public void SpawnPlayer()
    {
        Vector3 entryPos = new Vector3(-4f, -6f, 0f);
        currentPlayer = Instantiate(playerPrefab, entryPos, Quaternion.identity);
        PlayerTransform = currentPlayer.transform;

        // 啟動登場動畫
        var control = currentPlayer.GetComponent<PlayerControl>();
        if (control != null)
        {
            control.PlayEntryAnimation(new Vector3(-4f, -2f, 0f), 1.5f);
        }


    }

    // ========= 敵人管理 ========= //
    public void RegisterEnemy(GameObject enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }

    public List<GameObject> GetActiveEnemies() => activeEnemies;
    public float GetGameTime() => gameTime;
} // END GameSystem.cs