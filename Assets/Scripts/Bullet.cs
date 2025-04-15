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

    private Vector2 direction;
    private float speed;
    private float timer;

    public void Launch(Vector2 dir, float spd, BulletSpawner.Chara owner)
    {
        direction = dir.normalized;
        speed = spd;
        ownerType = owner;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (ownerType == BulletSpawner.Chara.Player)
        {
            // 玩家發射的子彈，只打敵人
            EnemyCore enemy = collision.GetComponent<EnemyCore>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else if (ownerType == BulletSpawner.Chara.Enemy)
        {
            // 敵人發射的子彈，只打玩家
            PlayerControl player = collision.GetComponent<PlayerControl>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    public void Stop()
    {
        speed = 0f;
    }

}