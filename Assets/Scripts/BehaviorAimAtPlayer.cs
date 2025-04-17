using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAtPlayerBehavior : BulletBehaviorBase
{
    public float turnSpeed = 360f; // 每秒旋轉角度

    private Transform player;

    protected override void OnStart()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected override void Tick(float deltaTime)
    {
        if (player == null) return;

        Vector2 toPlayer = (player.position - bullet.transform.position).normalized;
        float angleToPlayer = Vector2.SignedAngle(bullet.Direction, toPlayer);
        float maxAngle = turnSpeed * deltaTime;

        float angle = Mathf.Clamp(angleToPlayer, -maxAngle, maxAngle);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        bullet.SetDirection(rotation * bullet.Direction);
    }

    protected override void OnEnd()
    {
        // Optionally reset anything
    }
}
