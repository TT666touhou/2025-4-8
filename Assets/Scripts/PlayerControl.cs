using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("移動速度")]
    public int moveSpeed = 5;
    [Header("血量設定")]
    public int hp = 100;
    public SpriteRenderer Sprite;
    [Header("受擊效果")]
    public Color hitColor = Color.red;

    private Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = Sprite.color;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDir.y += 1;
        if (Input.GetKey(KeyCode.S))
            moveDir.y -= 1;
        if (Input.GetKey(KeyCode.A))
            moveDir.x -= 1;
        if (Input.GetKey(KeyCode.D))
            moveDir.x += 1;

        transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;


    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name} 被擊中，失去 {damage} HP，剩餘 HP: {hp}");

        StartCoroutine(HitEffect());

        if (hp <= 0)
        {
            Debug.Log("玩家死亡！");
            Destroy(gameObject);
        }
    }

    IEnumerator HitEffect()
    {
        Sprite.color = Color.red;

        Transform visual = Sprite.transform;
        Vector3 originalLocalPos = visual.localPosition;
        Vector3 originalScale = visual.localScale;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f),
                0);
            visual.localPosition = originalLocalPos + offset;

            float scaleShake = Random.Range(-0.005f, 0.005f);
            visual.localScale = originalScale + new Vector3(scaleShake, scaleShake, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Sprite.color = originalColor;
        visual.localPosition = originalLocalPos;
        visual.localScale = originalScale;
    }

}
