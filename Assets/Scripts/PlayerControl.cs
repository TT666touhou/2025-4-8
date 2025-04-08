using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public int moveSpeed = 5;
    public GameObject bulletPrefab; //  bullet prefab
    public Transform firePoint;     // 射出位置（可為角色中心或武器位置）

    public float fireRate = 0.1f;
    public float nextFireTime = 0f;

    public int hp = 100;
    public SpriteRenderer Sprite;

    private Color originalColor;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = Sprite.color;
        originalPosition = transform.position;
        originalScale = transform.localScale;
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

        shoot();
    }

    void shoot() {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time+fireRate;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector3 shootDir = mouseWorldPos - transform.position;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.GetComponent<BulletScript>().SetDirection(shootDir);
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name} 被擊中，失去 {damage} HP，剩餘 HP: {hp}");

        StartCoroutine(HitEffect());

        if (hp <= 0)
        {
            Debug.Log("玩家死亡！");
            Destroy(gameObject); // 你可以改成觸發動畫或重生等
        }
    }

    IEnumerator HitEffect()
    {
        Sprite.color = Color.red;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f),
                0);
            transform.position = originalPosition + offset;

            float scaleShake = Random.Range(-0.005f, 0.005f);
            transform.localScale = originalScale + new Vector3(scaleShake, scaleShake, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Sprite.color = originalColor;
        transform.position = originalPosition;
        transform.localScale = originalScale;
    }
}
