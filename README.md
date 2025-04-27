
## TITLE

- 模塊化彈幕遊戲遊戲基礎腳本

## 目錄

- 介紹
- 系統結構
- 腳本功能說明
- 特色功能
- BulletSpawner 使用說明

---

## 介紹

- 這是一個以**時間軸事件驅動**為核心設計的 2D 射擊遊戲架構。
- 強調**模組化設計**，方便擴充子彈行為、敵人行為、事件控制。

---

## 系統結構

- **GameSystem**：總經算遊戲流程、玩家生成、記錄敵人、更新時間軸事件。
- **TimelineManager**：推進遊戲內時間，並根據時間點觸發 `TimelineEvent`事件。
- **Bullet**：子彈的移動與生命週期，處理撞擊後造成傷害，並可以掛載行為腳本。
- **BulletSpawner（可視需要擴充）**：
  - 控制彈幕的生成模式與方向
  - 具備參數化的彈幕生成系統
  - 支援：固定方向、追蹤玩家、隨機散射、掃射、循環掃射、子彈排列等
  - 掃射方向動態反轉，交錯開火，水平/垂直子彈排列
  - 依照清除功能，Spawner消失時自動清除所產生的子彈
- **BulletBehavior 系統（子彈行為模組，可視需要擴充）**：
  - `BehaviorAcceleration`：子彈加速或減速
  - `BehaviorAimAtPlayer`：子彈飄行中纏向玩家
  - `BehaviorOrbitAroundTarget`：子彈繞着目標旋轉
  - `BehaviorRotateDirection`：子彈在空中持續轉動
- **MovementBehavior 系統（敵人移動模組，可視需要擴充）**：
  - `MoveOnFirstFire`：敵人第一次開火後觸發移動
  - `MoveOnFireCount`：敵人達到指定開火次數後移動
  - `MoveAfterSpawnTime`：敵人出生後等待一段時間移動

---

## 腳本功能說明

| 腳本名稱                        | 說明                                   |
| ------------------------------ | ------------------------------------------ |
| `GameSystem`                        | 管理遊戲流程，玩家生命，時間管理 |
| `TimelineManager`                   | 管理後台時間，觸發 `TimelineEvent`事件 |
| `TimelineEvent`                     | 基礎時間軸事件類                         |
| `SpawnEnemyEvent`                   | 指定時間生成敵人                             |
| `SpawnerModifyEvent`                | 修改敵人Spawner內容                             |
| `ConditionalRemoveEnemyEvent`       | 根據條件移除敵人                             |
| `BulletSpawner`                     | 生成子彈，支援多種開火模式               |
| `Bullet`                            | 子彈移動，觸發行為，擊中物件處理         |
| `BulletBehaviorBase`                | 子彈行為基礎類，管理啟動、結束時間        |
| `BehaviorAcceleration`              | 子彈自動加速/減速                   |
| `BehaviorAimAtPlayer`               | 子彈飄行時纏向玩家                    |
| `BehaviorOrbitAroundTarget`         | 子彈繞着目標旋轉                      |
| `BehaviorRotateDirection`           | 子彈持續轉動方向                      |
| `MovementBehaviorBase`              | 敵人移動基礎類                          |
| `MoveOnFirstFire`                   | 敵人開火後觸發移動                    |
| `MoveOnFireCount`                   | 指定開火次數後移動                  |
| `MoveAfterSpawnTime`                | 出生等待時間移動                      |
| `PlayerControl`                     | 玩家控制：移動、受擊、無敵、發射 |

---

## 特色功能

- **模組化子彈行為系統**：自由組合加速、轉動、追蹤、繞旋特效
- **模組化敵人移動系統**：根據生成時間、開火狀態變化行為
- **時間軸系統**：精簡控制敵人生成，行為變化，彈彈切換
- **BulletSpawner超自由度彈彈系統**：
  - 支援固定方向，隨機散射，矩陣排列，掃射，循環掃射
  - 支援掃射反轉、清除生成後子彈等特性

---

## BulletSpawner 使用說明

- **基本配置**：
  - 在物件上掛上 `BulletSpawner` ，指定 `bulletPrefab`彈彈預製

- **重點參數**：
  - `fireInterval`：两次發射的間隔時間
  - `fireCount`：每次發射子彈數
  - `bulletSpeed`：子彈移動速度
  - `fireMode`：發射模式，支援追蹤，隨機，加寬，掃射，循環
  - `alternateRotationDirection`：是否定時切換開火方向
  - `parallelBulletCount`：垂直排列子彈數
  - `horizontalBulletCount`：水平排列子彈數
  - `enableFireAngleInversion`：有無定時對軸旋轉 180度
  - `destroyBulletsOnSpawnerDestroyed`：Spawner消失時，自動刪除所生成後子彈

- **搭配 TimelineEvent 使用**：
  - 可在演出遊戲時，任意變更 BulletSpawner 的開火方式、彈數、橫排、速度、排列操作

https://www.youtube.com/watch?v=W008dkAJYDw
