using System;
using System.Collections.Generic;

public class EventManager : BaseSingleton<EventManager>
{
    // 무파라미터 이벤트
    private Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

    // 파라미터 이벤트
    private Dictionary<string, Delegate> eventDictionaryWithParams = new Dictionary<string, Delegate>();

    // ============================
    // 무파라미터 이벤트
    // ============================
    public void Subscribe(string eventName, Action listener)
    {
        if (eventDictionary.ContainsKey(eventName))
            eventDictionary[eventName] += listener;
        else
            eventDictionary[eventName] = listener;
    }

    public void Unsubscribe(string eventName, Action listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;
            if (eventDictionary[eventName] == null)
                eventDictionary.Remove(eventName);
        }
    }

    public void Publish(string eventName)
    {
        if (eventDictionary.ContainsKey(eventName))
            eventDictionary[eventName]?.Invoke();
    }

    // ============================
    // 파라미터 있는 이벤트 (제네릭)
    // ============================
    public void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (eventDictionaryWithParams.ContainsKey(eventName))
            eventDictionaryWithParams[eventName] = (Action<T>)eventDictionaryWithParams[eventName] + listener;
        else
            eventDictionaryWithParams[eventName] = listener;
    }

    public void Unsubscribe<T>(string eventName, Action<T> listener)
    {
        if (eventDictionaryWithParams.ContainsKey(eventName))
        {
            eventDictionaryWithParams[eventName] = (Action<T>)eventDictionaryWithParams[eventName] - listener;
            if (eventDictionaryWithParams[eventName] == null)
                eventDictionaryWithParams.Remove(eventName);
        }
    }

    public void Publish<T>(string eventName, T param)
    {
        if (eventDictionaryWithParams.ContainsKey(eventName))
        {
            (eventDictionaryWithParams[eventName] as Action<T>)?.Invoke(param);
        }
    }
}