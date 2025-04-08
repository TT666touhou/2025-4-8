using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{   
    public int hp = 30;
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
        
    }
    

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name} éÛìû {damage} èùäQÅCôîÈP HP: {hp}");

        StartCoroutine(HitEffect());

        if (hp <= 0)
        {
            Destroy(gameObject); // ååó üd 0ÅCôàèúñ⁄ïW
        }
    }

    IEnumerator HitEffect() {

        Sprite.color = Color.red;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration) {

            Vector3 offset = new Vector3(
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f),
                0);
            transform.position = offset+originalPosition;

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
