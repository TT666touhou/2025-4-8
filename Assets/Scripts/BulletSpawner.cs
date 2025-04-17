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
    public float minAngle = -15f;
    public float maxAngle = 15f;
    public int fireCountVariance = 0;        // 子彈數量 ±變化
    public float angleJitter = 0f;           // 每發子彈 ±角度誤差（即使 accuracy = 0 也會有抖動）
    public FireMode fireMode = FireMode.FixedDirection;
    public Vector2 fixedDirection = Vector2.down;
    [Tooltip("-1 表示無限次")]
    public int maxFireCycles = -1; // -1 = infinite
    public float cooldownAfterCycle = 3f;
    public bool fireImmediately = true;
    [Header("方向切換設定")]
    public bool alternateRotationDirection = false;
    public float rotationToggleInterval = 2f;

    private bool currentRotationClockwise = true;
    private float rotationToggleTimer = 0f;


    [Header("旋轉掃射參數（僅 SwingSweep / LoopSweep 使用）")]
    public float sweepSpeed = 30f; // 每秒轉動的角度

    public enum CircleSpawnType
    {
        Instant,    // 直接生成在圓上
        FlyOut      // 從中心飛到圓上再停下來
    }


    private float fireTimer = 0f;
    private Transform player;
    private float sweepAngle = 0f;
    private float sweepDirection = 1f; // 1 = 正向，-1 = 反向
    private int fireCycleCount = 0;
    private bool isCoolingDown = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // 🔁 根據 prefab 中 OrbitAroundTargetBehavior 的 clockwise 設定初始化 spawner 預設方向
        var orbit = bulletPrefab != null ? bulletPrefab.GetComponent<OrbitAroundTargetBehavior>() : null;
        if (orbit != null)
        {
            currentRotationClockwise = orbit.clockwise;
        }

        // 控制是否立即開火
        if (!fireImmediately)
            fireTimer = 0f;
        else
            fireTimer = fireInterval; // 強制讓第一次可立即進入 Fire()
    }



    void Update()
    {
        if (isCoolingDown) return;

        fireTimer += Time.deltaTime;


        bool canFire = fireTimer >= fireInterval && (maxFireCycles == -1 || fireCycleCount < maxFireCycles);


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
            if (Input.GetKey(KeyCode.Space) && canFire)
            {
                fireTimer = 0f;
                Fire();
            }
        }
        // 🔴 敵人角色：自動開火
        else if (charaType == Chara.Enemy)
        {
            if (canFire)
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
             Vector2 dir = GetFireDirection(i, actualFireCount);
             GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
             Bullet bulletScript = bullet.GetComponent<Bullet>();
             if (bulletScript != null)
             {
                 bulletScript.Launch(dir.normalized, bulletSpeed, charaType);
                 ApplyRotationDirection(bulletScript);
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
        float finalAngle = baseAngle + jitter;

        return Quaternion.Euler(0, 0, finalAngle) * Vector2.down;
    }

}