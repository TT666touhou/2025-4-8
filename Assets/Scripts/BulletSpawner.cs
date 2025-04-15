using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public enum FireMode
    {
        FixedDirection,
        AimAtPlayer,
        RandomSpread,
        EvenSpread,
        EvenSpreadToPlayer,
        SwingSweep,
        LoopSweep,
        Circle,
        MouseAim
    }
    public enum Chara
    {
        Player,
        Enemy
    }

    [Header("角色設定")]
    public Chara charaType = Chara.Enemy;


    [Header("射擊參數")]
    public GameObject bulletPrefab;
    public float fireInterval = 1f;
    public int fireCount = 1;
    public float bulletSpeed = 5f;
    public float accuracy = 15f; // 角度範圍
    public int fireCountVariance = 0;        // 子彈數量 ±變化
    public float angleJitter = 0f;           // 每發子彈 ±角度誤差（即使 accuracy = 0 也會有抖動）
    public FireMode fireMode = FireMode.FixedDirection;
    public Vector2 fixedDirection = Vector2.down;

    [Header("旋轉掃射參數（僅 SwingSweep / LoopSweep 使用）")]
    public float sweepSpeed = 30f; // 每秒轉動的角度

    public enum CircleSpawnType
    {
        Instant,    // 直接生成在圓上
        FlyOut      // 從中心飛到圓上再停下來
    }

    [Header("Circle 模式專用參數")]
    public float circleRadius = 2f;
    public CircleSpawnType circleSpawnType = CircleSpawnType.Instant;
    public float flyOutTime = 0.5f; // 飛到指定位置花費的時間


    private float fireTimer = 0f;
    private Transform player;
    private float sweepAngle = 0f;
    private float sweepDirection = 1f; // 1 = 正向，-1 = 反向

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        fireTimer += Time.deltaTime;

        // 🟡 玩家角色：必須按下空白鍵才可開火
        if (charaType == Chara.Player)
        {
            if (Input.GetKey(KeyCode.Space) && fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                Fire();
            }
        }
        // 🔴 敵人角色：自動開火
        else if (charaType == Chara.Enemy)
        {
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                Fire();
            }
        }

        //===========================================================//

        // 控制 Sweep 角度更新
        if (fireMode == FireMode.SwingSweep || fireMode == FireMode.LoopSweep)
        {
            sweepAngle += sweepSpeed * sweepDirection * Time.deltaTime;

            if (fireMode == FireMode.SwingSweep)
            {
                // 超過左右界限時反向
                if (sweepAngle > accuracy / 2f)
                {
                    sweepAngle = accuracy / 2f;
                    sweepDirection = -1f;
                }
                else if (sweepAngle < -accuracy / 2f)
                {
                    sweepAngle = -accuracy / 2f;
                    sweepDirection = 1f;
                }
            }

            if (fireMode == FireMode.LoopSweep)
            {
                // 持續遞增，循環角度
                if (sweepAngle > accuracy / 2f)
                {
                    sweepAngle = -accuracy / 2f;
                }
            }
        }

    }

    void Fire()
    {
        int actualFireCount = fireCount + Random.Range(-fireCountVariance, fireCountVariance + 1);
        actualFireCount = Mathf.Max(1, actualFireCount); // 至少發一發

        if (fireMode == FireMode.Circle)
        {
            for (int i = 0; i < actualFireCount; i++)
            {
                float angle = 360f / actualFireCount * i;
                Vector2 offset = Quaternion.Euler(0, 0, angle) * Vector2.up * circleRadius;
                Vector2 targetPos = (Vector2)transform.position + offset;

                GameObject bullet;

                if (circleSpawnType == CircleSpawnType.Instant)
                {
                    // 直接在圓上生成
                    bullet = Instantiate(bulletPrefab, targetPos, Quaternion.identity);
                    Bullet bulletScript = bullet.GetComponent<Bullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.Launch(Vector2.zero, 0f, charaType); // 靜止
                    }
                }
                else if (circleSpawnType == CircleSpawnType.FlyOut)
                {
                    // 從中心飛出到目標點
                    bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                    Bullet bulletScript = bullet.GetComponent<Bullet>();
                    if (bulletScript != null)
                    {
                        Vector2 flyDir = (targetPos - (Vector2)transform.position).normalized;
                        float distance = Vector2.Distance(transform.position, targetPos);
                        float speedToReach = distance / flyOutTime;

                        bulletScript.Launch(flyDir, speedToReach, charaType);
                        bulletScript.StartCoroutine(StopBulletAfterTime(bulletScript, flyOutTime));
                    }
                }
            }
        }

        else
        {
            for (int i = 0; i < actualFireCount; i++)
            {
                Vector2 dir = GetFireDirection(i, actualFireCount);
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.Launch(dir.normalized, bulletSpeed, charaType);
                }
            }
        }
    }

    private IEnumerator StopBulletAfterTime(Bullet bullet, float time)
    {
        yield return new WaitForSeconds(time);
        if (bullet != null)
        {
            bullet.Stop();
        }
    }


    Vector2 GetFireDirection(int index, int count)
    {
        float baseAngle = 0f;

        switch (fireMode)
        {
            case FireMode.FixedDirection:
                baseAngle = Vector2.SignedAngle(Vector2.down, fixedDirection);
                break;

            case FireMode.AimAtPlayer:
                if (player != null)
                    baseAngle = Vector2.SignedAngle(Vector2.down, (player.position - transform.position).normalized);
                break;

            case FireMode.MouseAim:
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                baseAngle = Vector2.SignedAngle(Vector2.down, (mouseWorldPos - transform.position).normalized);
                break;

            case FireMode.RandomSpread:
                baseAngle = Random.Range(-accuracy / 2f, accuracy / 2f);
                break;

            case FireMode.EvenSpread:
                float step = accuracy / count;
                float angleOffset = -accuracy / 2f + step / 2f + step * index;

                float baseAngleEven = Vector2.SignedAngle(Vector2.down, fixedDirection);
                baseAngle = baseAngleEven + angleOffset;
                break;

            case FireMode.SwingSweep:
            case FireMode.LoopSweep:
                {
                    float baseSweepAngle = Vector2.SignedAngle(Vector2.down, fixedDirection);

                    // ⬇ 中心掃射角（由 sweepAngle 控制）
                    float centerAngle = baseSweepAngle + sweepAngle;

                    // ⬇ 平均分散角度（與 EvenSpread 相同）
                    float step2 = accuracy / count;
                    float angleOffset2 = -accuracy / 2f + step2 / 2f + step2 * index;

                    baseAngle = centerAngle + angleOffset2;
                    break;
                }


            case FireMode.EvenSpreadToPlayer:
                if (player != null)
                {
                    float stepE = accuracy / count;
                    float angleOffsetE = -accuracy / 2f + stepE / 2f + stepE * index;
                    float angleToPlayerE = Vector2.SignedAngle(Vector2.down, (player.position - transform.position).normalized);
                    baseAngle = angleToPlayerE + angleOffsetE;
                }
                break;

            default:
                baseAngle = 0f;
                break;
        }

        // 加入隨機誤差（angleJitter）
        float jitter = Random.Range(-angleJitter, angleJitter);
        float finalAngle = baseAngle + jitter;

        return Quaternion.Euler(0, 0, finalAngle) * Vector2.down;
    }

}