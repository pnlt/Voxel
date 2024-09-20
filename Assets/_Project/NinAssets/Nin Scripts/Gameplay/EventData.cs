using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "newEvent",menuName = "Event")]
public class EventData : ScriptableObject
{
    public static Dictionary<EventData, Action<object>> eventData =
        new Dictionary<EventData, Action<object>>();

    public void RegisterEvent(Action<object> action)
    {
        if (!eventData.ContainsKey(this))
        {
            eventData.Add(this, action);
        }
    }

    public void UnregisterEvent()
    {
        eventData.Remove(this);
    }
}
