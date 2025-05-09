﻿using System.Collections;
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
        MouseAim
    }
    public enum Chara
    {
        Player,
        Enemy
    }

    [Header("角色設定")]
    public Chara charaType = Chara.Enemy;

    [Header("開火啟用控制")]
    [Tooltip("是否允許這個 Spawner 開火（false 將完全禁止開火）")]
    public bool canFire = true;

    [Header("射擊參數")]
    public GameObject bulletPrefab;
    public float fireInterval = 1f;
    public int fireCount = 1;
    public float bulletSpeed = 5f;
    public float minAngle = -15f;
    public float maxAngle = 15f;
    public int fireCountVariance = 0;        // 子彈數量 ±變化
    public float angleJitter = 0f;           // 每發子彈 ±角度誤差（即使 accuracy = 0 也會有抖動）
    public FireMode fireMode = FireMode.FixedDirection;
    public Vector2 fixedDirection = Vector2.down;
    public float globalAngleOffset = 0f;
    [Tooltip("子彈初始生成與 Spawner 的距離（0 表示重疊）")]
    public float spawnOffsetDistance = 0f;
    [Tooltip("-1 表示無限次")]
    public int maxFireCycles = -1; // -1 = infinite
    public float cooldownAfterCycle = 3f;
    public bool fireImmediately = true;
    [Header("方向切換設定")]
    public bool alternateRotationDirection = false;
    public float rotationToggleInterval = 2f;

    [Header("子彈組（平行線）設定")]
    [Tooltip("每個發射方向的子彈數（最少 1）")]
    public int parallelBulletCount = 1;
    [Tooltip("每顆子彈之間的垂直間隔（單位：世界單位）")]
    public float parallelSpacing = 0.1f;
    [Tooltip("子彈組的角度發散總量（0 表示平行）")]
    public float parallelAngleSpread = 0f;
    [Tooltip("子彈組中每顆子彈的生成延遲時間（由中心向外）")]
    public float parallelSpawnDelay = 0f;
    [Tooltip("是否讓中心的子彈最先生成（勾選代表由內往外）")]
    public bool centerFiresFirst = true;

    [Header("子彈組（水平方向）設定")]
    [Tooltip("水平方向的子彈數（最少 1）")]
    public int horizontalBulletCount = 1;
    [Tooltip("水平方向子彈之間的間隔（單位：世界單位）")]
    public float horizontalSpacing = 0.1f;
    [Tooltip("水平方向子彈的生成延遲（由中間向兩側）")]
    public float horizontalSpawnDelay = 0f;
    [Tooltip("是否讓中間的子彈先生成")]
    public bool horizontalCenterFiresFirst = true;


    [Header("旋轉掃射參數（僅 SwingSweep / LoopSweep 使用）")]
    public float sweepSpeed = 30f; // 每秒轉動的角度

    public enum CircleSpawnType
    {
        Instant,    // 直接生成在圓上
        FlyOut      // 從中心飛到圓上再停下來
    }

    [Header("開火方向反轉設定")]
    [Tooltip("是否啟用定時反轉開火角度")]
    public bool enableFireAngleInversion = false;

    [Tooltip("每隔幾秒反轉一次角度（180 度）")]
    public float fireAngleInversionInterval = 3f;


    [Header("子彈管理")]
    [Tooltip("當此 Spawner 被摧毀時，是否一併摧毀由它產生的子彈")]
    public bool destroyBulletsOnSpawnerDestroyed = false;

    // 紀錄此 Spawner 生成的子彈
    private List<GameObject> spawnedBullets = new List<GameObject>();



    private float fireTimer = 0f;
    private Transform player;
    private float sweepAngle = 0f;
    private float sweepDirection = 1f; // 1 = 正向，-1 = 反向
    private int fireCycleCount = 0;
    private bool isCoolingDown = false;
    private float inversionTimer = 0f;
    private bool isAngleInverted = false;
    private bool currentRotationClockwise = true;
    private float rotationToggleTimer = 0f;



    void Start()
    {
        player = GameSystem.PlayerTransform; // 用 GameSystem 的靜態參考

        // OrbitBehavior 設定預設方向
        var orbit = bulletPrefab != null ? bulletPrefab.GetComponent<OrbitAroundTargetBehavior>() : null;
        if (orbit != null)
        {
            currentRotationClockwise = orbit.clockwise;
        }

        fireTimer = fireImmediately ? fireInterval : 0f;
    }




    void Update()
    {
        if (player == null && (fireMode == FireMode.AimAtPlayer || fireMode == FireMode.EvenSpreadToPlayer))
        {
            player = GameSystem.PlayerTransform;
        }


        if (isCoolingDown) return;
        if (!canFire) return;

        fireTimer += Time.deltaTime;


        bool readyToFire= fireTimer >= fireInterval && (maxFireCycles == -1 || fireCycleCount < maxFireCycles);


        // 每 rotationToggleInterval 秒切換一次方向（不會影響已存在子彈）
        if (alternateRotationDirection)
        {
            rotationToggleTimer += Time.deltaTime;
            if (rotationToggleTimer >= rotationToggleInterval)
            {
                currentRotationClockwise = !currentRotationClockwise;
                rotationToggleTimer = 0f;
            }
        }

        // 🟡 玩家角色：必須按下空白鍵才可開火
        if (charaType == Chara.Player)
        {
            if (Input.GetKey(KeyCode.Space) && readyToFire)
            {
                fireTimer = 0f;
                Fire();
            }
        }
        // 🔴 敵人角色：自動開火
        else if (charaType == Chara.Enemy)
        {
            if (readyToFire)
            {
                fireTimer = 0f;
                Fire();
            }
        }

        //==========================================================//

        // 處理冷卻：若達到上限，開始冷卻
        if (maxFireCycles != -1 && fireCycleCount >= maxFireCycles)
        {
            StartCoroutine(RestartAfterCooldown());
        }

        //===========================================================//

        // 控制角度反轉邏輯
        if (enableFireAngleInversion)
        {
            inversionTimer += Time.deltaTime;
            if (inversionTimer >= fireAngleInversionInterval)
            {
                inversionTimer = 0f;
                isAngleInverted = !isAngleInverted;
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
                float sweepRange = maxAngle - minAngle;
                float halfSweep = sweepRange / 2f;

                if (sweepAngle > halfSweep)
                {
                    sweepAngle = halfSweep;
                    sweepDirection = -1f;
                }
                else if (sweepAngle < -halfSweep)
                {
                    sweepAngle = -halfSweep;
                    sweepDirection = 1f;
                }

            }

            if (fireMode == FireMode.LoopSweep)
            {
                // 持續遞增，循環角度
                if (sweepAngle > (maxAngle - minAngle) / 2f)
                {
                    sweepAngle = -(maxAngle - minAngle) / 2f;
                }

            }
        }

    }

    private IEnumerator RestartAfterCooldown()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(cooldownAfterCycle);
        fireCycleCount = 0;
        isCoolingDown = false;
    }

    void Fire()
    {
        fireCycleCount++;

        int actualFireCount = fireCount + Random.Range(-fireCountVariance, fireCountVariance + 1);
        actualFireCount = Mathf.Max(1, actualFireCount); // 至少發一發

        

        
        
         for (int i = 0; i < actualFireCount; i++)
         {
            Vector2 dir = GetFireDirection(i, actualFireCount).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x); // 對應的垂直方向

            // 發散角度控制
            int subCount = Mathf.Max(1, parallelBulletCount);
            float totalSpan = (subCount - 1) * parallelSpacing;
            float angleStep = (subCount > 1) ? parallelAngleSpread / (subCount - 1) : 0f;

            for (int j = 0; j < subCount; j++)
            {
                float offsetAmount = j * parallelSpacing - totalSpan / 2f;
                Vector2 spawnCenter = (Vector2)transform.position + dir * spawnOffsetDistance;
                Vector2 verticalPos = spawnCenter + perp * offsetAmount;

                float spreadAngle = -parallelAngleSpread / 2f + angleStep * j;
                Quaternion spreadRot = Quaternion.Euler(0, 0, spreadAngle);
                Vector2 finalDir = spreadRot * dir;

                float verticalDelay = GetParallelDelay(j, subCount);

                // 水平方向處理
                int hCount = Mathf.Max(1, horizontalBulletCount);
                float hTotalSpan = (hCount - 1) * horizontalSpacing;
                for (int h = 0; h < hCount; h++)
                {
                    float hOffset = h * horizontalSpacing - hTotalSpan / 2f;
                    Vector2 hPerp = dir; // 水平往前/後延伸，使用 dir 的方向為基礎
                    Vector2 finalPos = verticalPos + hPerp * hOffset;

                    float hDelay = GetHorizontalDelay(h, hCount);
                    float totalDelay = verticalDelay + hDelay;

                    StartCoroutine(SpawnDelayedBullet(totalDelay, finalPos, finalDir));
                }
            }


        }
    }

    private float GetHorizontalDelay(int index, int count)
    {
        int centerIndex = (count - 1) / 2;
        int distanceFromCenter = Mathf.Abs(index - centerIndex);

        if (horizontalCenterFiresFirst)
            return horizontalSpawnDelay * distanceFromCenter;
        else
        {
            int maxDistance = (count - 1) / 2;
            return horizontalSpawnDelay * (maxDistance - distanceFromCenter);
        }
    }


    private float GetParallelDelay(int index, int count)
    {
        int centerIndex = (count - 1) / 2;
        int distanceFromCenter = Mathf.Abs(index - centerIndex);

        if (centerFiresFirst)
        {
            return parallelSpawnDelay * distanceFromCenter; // 原本：中心先出
        }
        else
        {
            // 反過來：離中心越近延遲越大（邊邊先出）
            int maxDistance = (count - 1) / 2;
            return parallelSpawnDelay * (maxDistance - distanceFromCenter);
        }
    }


    private IEnumerator SpawnDelayedBullet(float delay, Vector2 position, Vector2 direction)
    {
        yield return new WaitForSeconds(delay);
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        spawnedBullets.Add(bullet);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Launch(direction, bulletSpeed, charaType);
            ApplyRotationDirection(bulletScript);
        }
    }
    void OnDestroy()
    {
        if (destroyBulletsOnSpawnerDestroyed)
        {
            foreach (var bullet in spawnedBullets)
            {
                if (bullet != null)
                    Destroy(bullet);
            }
        }
    }


    private void ApplyRotationDirection(Bullet bullet)
    {
        if (bullet == null) return;

        RotateDirectionBehavior rotateBehavior = bullet.GetComponent<RotateDirectionBehavior>();
        if (rotateBehavior != null)
        {
            rotateBehavior.clockwise = currentRotationClockwise;
        }

        OrbitAroundTargetBehavior orbit = bullet.GetComponent<OrbitAroundTargetBehavior>();
        if (orbit != null)
        {
            orbit.clockwise = currentRotationClockwise;
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
                baseAngle = Random.Range(minAngle, maxAngle);
                baseAngle =baseAngle - 90f;
                break;

            case FireMode.EvenSpread:
                float step = (maxAngle - minAngle) / count;
                float angleOffset = minAngle + step / 2f + step * index;

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
                    float stepL = (maxAngle - minAngle) / count;
                    float angleOffsetL = minAngle + stepL / 2f + stepL * index;


                    baseAngle = centerAngle + angleOffsetL;
                    break;
                }


            case FireMode.EvenSpreadToPlayer:
                if (player != null)
                {
                    float stepE = (maxAngle - minAngle) / count;
                    float angleOffsetE = minAngle + stepE / 2f + stepE * index;

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
        float finalAngle = baseAngle + jitter + globalAngleOffset;

        // 加入角度反轉邏輯
        if (isAngleInverted)
            finalAngle += 180f;

        return Quaternion.Euler(0, 0, finalAngle) * Vector2.down;

    }

}