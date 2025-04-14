using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("�q�[�Ҍ�")]
    public BulletSpawner.Chara ownerType;
    [Header("���Q")]
    public int damage = 10;
    [Header("�����T��")]
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
            // �߉�ᢎ˓I�q�[�C���œG�l
            EnemyCore enemy = collision.GetComponent<EnemyCore>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else if (ownerType == BulletSpawner.Chara.Enemy)
        {
            // �G�lᢎ˓I�q�[�C���Ŋ߉�
            PlayerControl player = collision.GetComponent<PlayerControl>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}