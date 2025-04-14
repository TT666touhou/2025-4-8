using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("qœ[˜ÒŒ¹")]
    public BulletSpawner.Chara ownerType;
    [Header("ŠQ")]
    public int damage = 10;
    [Header("¶–½TŠú")]
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
            // Šß‰Æá¢Ë“Iqœ[C‘ü‘Å“Gl
            EnemyCore enemy = collision.GetComponent<EnemyCore>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else if (ownerType == BulletSpawner.Chara.Enemy)
        {
            // “Glá¢Ë“Iqœ[C‘ü‘ÅŠß‰Æ
            PlayerControl player = collision.GetComponent<PlayerControl>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}