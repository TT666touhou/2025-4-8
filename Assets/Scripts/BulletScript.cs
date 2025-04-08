using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public int damage = 10;
    public float bulletSpeed = 10f;
    public float maxDistance = 10f;
    private Vector3 direction;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += direction * bulletSpeed * Time.deltaTime;

        float traveledDistance = Vector3.Distance(transform.position, startPosition);
        if (traveledDistance >= maxDistance)
        {
            Destroy(gameObject); // ”ò‘¾‰“©‰äç÷šÊ
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ¦æ“¾›”•û¥”Û—L TargetScript
        TargetScript target = collision.GetComponent<TargetScript>();
        if (target != null)
        {
            target.TakeDamage(damage); // JŒŒ
            Destroy(gameObject);       // ™ˆœqœ[
        }
    }
}
