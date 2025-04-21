using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAtPlayerBehavior : BulletBehaviorBase
{
    public float turnSpeed = 360f; // 每秒旋轉角度

    private Transform player;

    protected override void OnStart()
    {
        player = GameSystem.PlayerTransform;
    }

    protected override void Tick(float deltaTime)
    {
        // 若失去參考就再抓一次
        if (player == null)
        {
            player = GameSystem.PlayerTransform;
            if (player == null) return; // 仍然沒抓到就暫停
        }

        Vector2 toPlayer = (player.position - bullet.transform.position).normalized;
        float angleToPlayer = Vector2.SignedAngle(bullet.Direction, toPlayer);
        float maxAngle = turnSpeed * deltaTime;

        float angle = Mathf.Clamp(angleToPlayer, -maxAngle, maxAngle);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        bullet.SetDirection(rotation * bullet.Direction);
    }

    protected override void OnEnd()
    {
        // Optional cleanup
    }
}
