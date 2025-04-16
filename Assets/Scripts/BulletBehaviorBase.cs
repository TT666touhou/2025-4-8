using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletBehaviorBase : MonoBehaviour
{
    public float startTime = 0f;
    public float endTime = Mathf.Infinity;

    protected Bullet bullet;
    protected bool isActive = false;

    public virtual void Initialize(Bullet b)
    {
        bullet = b;
        isActive = false;
    }

    public void ManualUpdate(float currentTime, float deltaTime)
    {
        bool shouldBeActive = currentTime >= startTime && currentTime <= endTime;

        if (shouldBeActive && !isActive)
        {
            OnStart();
            isActive = true;
        }
        if (!shouldBeActive && isActive)
        {
            OnEnd();
            isActive = false;
        }

        if (isActive)
        {
            Tick(deltaTime);
        }
    }

    protected virtual void OnStart() { }
    protected virtual void OnEnd() { }
    protected abstract void Tick(float deltaTime);
}
