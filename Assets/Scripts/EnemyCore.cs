using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHP = 100;
    private int currentHP;

    [Header("受擊效果")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;

    private void Start()
    {
        currentHP = maxHP;
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} 受到 {damage} 傷害，剩餘 HP: {currentHP}");

        StartCoroutine(HitEffect());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    IEnumerator HitEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        // TODO: 可播放死亡動畫/特效
        Destroy(gameObject);
    }
}
