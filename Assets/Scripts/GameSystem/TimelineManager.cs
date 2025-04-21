using System.Collections.Generic;
using UnityEngine;

public class TimelineManager
{
    private float currentTime = 0f;
    private readonly SortedList<float, List<TimelineEvent>> eventQueue = new();

    public void AddEvent(TimelineEvent evt)
    {
        if (!eventQueue.ContainsKey(evt.triggerTime))
            eventQueue[evt.triggerTime] = new List<TimelineEvent>();

        eventQueue[evt.triggerTime].Add(evt);
    }

    public void Update(float deltaTime)
    {
        currentTime += deltaTime;
        var keysToExecute = new List<float>();

        foreach (var kvp in eventQueue)
        {
            if (kvp.Key <= currentTime)
                keysToExecute.Add(kvp.Key);
            else
                break;
        }

        foreach (float key in keysToExecute)
        {
            foreach (var evt in eventQueue[key])
            {
                evt.Execute();
            }
            eventQueue.Remove(key);
        }
    }
}