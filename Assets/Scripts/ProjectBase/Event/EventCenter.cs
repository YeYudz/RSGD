using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


public interface IEventInfo
{
    string EventType { get; }
}

public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;
    public string EventType => typeof(T).Name;

    public EventInfo( UnityAction<T> action)
    {
        actions += action;
    }
}

public class EventInfo : IEventInfo
{
    public UnityAction actions;
    public string EventType => "NoParam";

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}


/// <summary>
/// 事件中心 单例模式对象
/// 1.Dictionary
/// 2.委托
/// 3.观察者设计模式
/// 4.泛型
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    //key —— 事件的名字（比如：怪物死亡，玩家死亡，通关 等等）
    //value —— 对应的是 监听这个事件 对应的委托函数们
    public Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();
    public EventCenter()
    {
        Debug.Log($"EventCenter 构造，实例 Hash: {GetHashCode()}");
    }
    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="name">事件的名字</param>
    /// <param name="action">准备用来处理事件 的委托函数</param>
    public void AddEventListener<T>(string name, UnityAction<T> action)
    {
        if (eventDic.TryGetValue(name, out var existing))
        {
            if (!(existing is EventInfo<T>))
            {
                Debug.LogError(
                    $"[EventCenter] 事件名 [{name}] 已注册为 [{existing.EventType}]，" +
                    $"不能再用 [{typeof(T).Name}] 重复注册！"
                );
                return;
            }

        (existing as EventInfo<T>).actions += action;
            return;
        }

        eventDic.Add(name, new EventInfo<T>(action));
    }

    /// <summary>
    /// 监听不需要参数传递的事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void AddEventListener(string name, UnityAction action)
    {
        if (eventDic.TryGetValue(name, out var existing))
        {
            if (!(existing is EventInfo))
            {
                Debug.LogError(
                    $"[EventCenter] 事件名 [{name}] 已注册为 [{existing.EventType}]，" +
                    $"不能再注册无参事件！"
                );
                return;
            }

        (existing as EventInfo).actions += action;
            return;
        }

        eventDic.Add(name, new EventInfo(action));
    }


    /// <summary>
    /// 移除对应的事件监听
    /// </summary>
    /// <param name="name">事件的名字</param>
    /// <param name="action">对应之前添加的委托函数</param>
    public void RemoveEventListener<T>(string name, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions -= action;
    }

    /// <summary>
    /// 移除不需要参数的事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void RemoveEventListener(string name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
        {
            var info = eventDic[name] as EventInfo;
            if (info != null && info.actions != null) // 加一层判空
            {
                info.actions -= action;
            }
        }
    }

    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="name">哪一个名字的事件触发了</param>
    public void EventTrigger<T>(string name, T info)
    {
        if (!eventDic.TryGetValue(name, out var evt))
            return;

        if (evt is EventInfo<T> e)
        {
            e.actions?.Invoke(info);
        }
        else
        {
            Debug.LogError(
                $"[EventCenter] 触发事件 [{name}] 时，签名不匹配！" +
                $"期望：{typeof(T).Name}，实际：{evt.EventType}"
            );
        }
    }

    /// <summary>
    /// 事件触发（不需要参数的）
    /// </summary>
    /// <param name="name"></param>
    public void EventTrigger(string name)
    {
        if (!eventDic.TryGetValue(name, out var evt))
            return;

        if (evt is EventInfo e)
        {
            e.actions?.Invoke();
        }
        else
        {
            Debug.LogError(
                $"[EventCenter] 触发无参事件 [{name}] 时，发现它是有参事件！" +
                $"实际类型：{evt.EventType}"
            );
        }
    }

    /// <summary>
    /// 清空事件中心
    /// 主要用在 场景切换时
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
    public void PrintAllEvents()
    {
        foreach (var kv in eventDic)
        {
            Debug.Log($"[Event] {kv.Key} → {kv.Value.EventType}");
        }
    }
}
