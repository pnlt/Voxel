using System;
using Unity.Netcode;
using UnityEngine;

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
            _instance = null;
    }

    public void AddListerner(EventData eventData, Action<object> action)
    {
        Action<object> events = null;
        if (EventData.eventData.TryGetValue(eventData, out events))
        {
            events += action;
        }
        else
        {
            eventData.RegisterEvent(action);
            events += action;
        }
    }

    public void TriggerEvent(EventData eventData, object message)
    {
        Action<object> events = null;
        if (EventData.eventData.TryGetValue(eventData, out events))
        {
            events.Invoke(message);
        }
    }

    public void RemoveListener(EventData eventData, Action<object> action)
    {
        Action<object> events = null;
        if (EventData.eventData.TryGetValue(eventData, out events))
        {
            events -= action;
            eventData.UnregisterEvent();
        }
    }
}

public static class EventDispatcher
{
    public static void AddListener(this MonoBehaviour listen, EventData eventData, Action<object> action)
    {
        EventManager.Instance.AddListerner(eventData, action);
    }

    public static void TriggerEvent(this MonoBehaviour listener, EventData eventData, object message)
    {
        EventManager.Instance.TriggerEvent(eventData, message);
    }

    public static void RemoveListener(this MonoBehaviour listener, EventData eventData, Action<object> action)
    {
        if (!listener.gameObject.scene.isLoaded)
            return;

        EventManager.Instance.RemoveListener(eventData, action);
    }
}