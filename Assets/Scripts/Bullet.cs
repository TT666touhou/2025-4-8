using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子彈來源")]
    public BulletSpawner.Chara ownerType;
    [Header("傷害")]
    public int damage = 10;
    [Header("生命週期")]
    public float lifeTime = 5f;

    private float timer = 0f;
    private Vector2 direction;
    private float speed;

    public Vector2 Direction => direction;
    private BulletBehaviorBase[] behaviors;

    private bool hasLaunched = false;

    void Awake()
    {
        // 若為靜態子彈（如從 Scene 或預製中產生而未手動 Launch）
        if (!hasLaunched)
        {
            // 預設靜止
            direction = Vector2.zero;
            speed = 0f;
            InitializeBehaviors();
        }
    }

    public void Launch(Vector2 dir, float spd, BulletSpawner.Chara owner)
    {
        direction = dir.normalized;
        speed = spd;
        ownerType = owner;
        timer = 0f;
        hasLaunched = true;
        InitializeBehaviors();
    }

    private void InitializeBehaviors()
    {
        behaviors = GetComponents<BulletBehaviorBase>();
        foreach (var b in behaviors)
        {
            b.Initialize(this);
        }
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        transform.Translate(direction * speed * deltaTime);
        timer += deltaTime;

        if (behaviors != null)
        {
            foreach (var b in behaviors)
            {
                b.ManualUpdate(timer, deltaTime);
            }
        }

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 newDir)
    {
        direction = newDir.normalized;
    }

    public void SetSpeed(float spd)
    {
        speed = spd;
    }
    public float GetSpeed()
    {
        return speed;
    }

    public void Stop()
    {
        speed = 0f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (ownerType == BulletSpawner.Chara.Player)
        {
            var enemy = collision.GetComponent<EnemyCore>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else if (ownerType == BulletSpawner.Chara.Enemy)
        {
            var player = collision.GetComponent<PlayerControl>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}