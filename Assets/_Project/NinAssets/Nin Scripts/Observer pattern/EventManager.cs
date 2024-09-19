using System;
using System.Collections.Generic;
using UnityEngine;

public enum Event
{
    APPLY_EFFECT
}

public class EventManager : MonoBehaviour
{
    private static EventManager _instance;
    public static EventManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(_instance);
        }
    }

    public static Dictionary<Event, Action<object, Component>> listEvent =
        new Dictionary<Event, Action<object, Component>>();

    public void AddListener(Event eventArgs, Action<object, Component> callback)
    {
        if (!listEvent.TryGetValue(eventArgs, out var callingEvent))
        {
            listEvent.Add(eventArgs, callback);
        }
        listEvent[eventArgs] += callback;
    }

    public void Invoke(Event eventArgs, object t = null, Component sender = null)
    {
        if (listEvent.TryGetValue(eventArgs, out var callingEvent))
            listEvent[eventArgs].Invoke(t, sender);
    }

    public void RemoveListener(Event eventArgs, Action<object, Component> callback)
    {
        if (!listEvent.TryGetValue(eventArgs, out var callingEvent))
            return;

        listEvent[eventArgs] -= callback;
    }
    
    
}
