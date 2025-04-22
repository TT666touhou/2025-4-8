using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("移動速度")]
    public int moveSpeed = 5;
    public int slowSpeed = 1;

    [Header("血量設定")]
    public int hp = 100;

    [Header("Sprite 設定")]
    public SpriteRenderer Sprite;
    [Header("受擊效果")]
    public Color hitColor = Color.red;

    private Color originalColor;

    [Header("無敵狀態")]
    public float invincibleDuration = 1.5f;
    private bool isInvincible = false;

    // ========= 限制移動範圍 ========= //
    private Vector2 minBoundary = new Vector2(-8f, -5f);
    private Vector2 maxBoundary = new Vector2(0f, 5f);

    void Start()
    {
        originalColor = Sprite.color;
        StartCoroutine(InvincibilityEffect());
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir.y += 1;
        if (Input.GetKey(KeyCode.S)) moveDir.y -= 1;
        if (Input.GetKey(KeyCode.A)) moveDir.x -= 1;
        if (Input.GetKey(KeyCode.D)) moveDir.x += 1;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? slowSpeed : moveSpeed;
        Vector3 nextPos = transform.position + moveDir.normalized * currentSpeed * Time.deltaTime;

        // 限制移動區域
        nextPos.x = Mathf.Clamp(nextPos.x, minBoundary.x, maxBoundary.x);
        nextPos.y = Mathf.Clamp(nextPos.y, minBoundary.y, maxBoundary.y);

        transform.position = nextPos;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        hp -= damage;

        StartCoroutine(HitEffect());

        if (hp <= 0)
        {
            Debug.Log("玩家死亡！");
            FindObjectOfType<GameSystem>()?.OnPlayerDeath();
            Destroy(gameObject);
        }
    }

    IEnumerator HitEffect()
    {
        Sprite.color = hitColor;

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

    IEnumerator InvincibilityEffect()
    {
        isInvincible = true;
        float timer = 0f;
        bool fade = false;

        while (timer < invincibleDuration)
        {
            float blinkSpeed = Mathf.Lerp(20f, 2f, timer / invincibleDuration);
            float blinkInterval = 1f / blinkSpeed;

            // 切換透明度
            Color color = Sprite.color;
            color.a = fade ? 0.5f : 1f;
            Sprite.color = color;
            fade = !fade;

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // 恢復正常透明度
        Color finalColor = Sprite.color;
        finalColor.a = 1f;
        Sprite.color = finalColor;

        isInvincible = false;
    }

    public void PlayEntryAnimation(Vector3 targetPosition, float duration)
    {
        StartCoroutine(EntryAnimationCoroutine(targetPosition, duration));
    }

    private IEnumerator EntryAnimationCoroutine(Vector3 targetPosition, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        isInvincible = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;

        // 開始無敵閃爍效果
        StartCoroutine(InvincibilityEffect());
    }
}
